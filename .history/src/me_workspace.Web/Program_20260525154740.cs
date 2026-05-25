using Microsoft.EntityFrameworkCore;
using Microsoft.Data.Sqlite;
using me_workspace.Web.Data;
using me_workspace.Web.Data.Entities;
using me_workspace.Web.Features.Chat;
using me_workspace.Web.Features.Chunking;
using me_workspace.Web.Features.Context;
using me_workspace.Web.Features.Extraction;
using me_workspace.Web.Features.Files;
using me_workspace.Web.Features.Journal;
using me_workspace.Web.Features.Memory;
using me_workspace.Web.Features.Pipeline;
using me_workspace.Web.Features.Processing;
using me_workspace.Web.Features.Sources;
using me_workspace.Web.Features.System;
using me_workspace.Web.Features.Voice;
using me_workspace.Web.Infrastructure.Llm;
using me_workspace.Web.Infrastructure.Speech;
using me_workspace.Web.Infrastructure.Workspace;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

var workspaceOptions = new WorkspaceOptions();
builder.Configuration.GetSection(WorkspaceOptions.SectionName).Bind(workspaceOptions);
var runtimeLogDirectory = Path.Combine(workspaceOptions.RuntimeRoot, workspaceOptions.LogsFolderName);
Directory.CreateDirectory(runtimeLogDirectory);

builder.WebHost.UseUrls(builder.Configuration["urls"] ?? "http://127.0.0.1:5078");
builder.Host.UseSerilog((_, _, loggerConfiguration) => loggerConfiguration
    .WriteTo.Console()
    .WriteTo.Debug()
    .WriteTo.File(
        Path.Combine(runtimeLogDirectory, "me_workspaces-.log"),
        rollingInterval: RollingInterval.Day,
        shared: true));
builder.Services.Configure<WorkspaceOptions>(builder.Configuration.GetSection(WorkspaceOptions.SectionName));
builder.Services.Configure<SpeechEngineOptions>(builder.Configuration.GetSection(SpeechEngineOptions.SectionName));

builder.Services.AddDbContext<AppDbContext>(options =>
{
    var dataDirectory = ResolveDataDirectory(builder.Environment.ContentRootPath);
    Directory.CreateDirectory(dataDirectory);

    var databasePath = Path.Combine(dataDirectory, "me_workspace.db");
    options.UseSqlite($"Data Source={databasePath}");
});

builder.Services.AddScoped<ChatService>();
builder.Services.AddScoped<FileContextService>();
builder.Services.AddScoped<MemoryService>();
builder.Services.AddScoped<SystemService>();
builder.Services.AddSingleton<ContextService>();
builder.Services.AddSingleton<JournalService>();
builder.Services.AddSingleton<VoiceService>();
builder.Services.AddSingleton<WorkspaceLayoutService>();
builder.Services.AddHostedService<WorkspaceBootstrapHostedService>();
builder.Services.AddHostedService<BackgroundProcessingService>();
builder.Services.AddSingleton<PipelineQueue>();
builder.Services.AddScoped<SourceRegistryService>();
builder.Services.AddScoped<ExtractionService>();
builder.Services.AddScoped<ChunkingService>();
builder.Services.AddScoped<ProcessingPipelineService>();
builder.Services.AddScoped<SourceFileMover>();
builder.Services.AddSingleton<ILlmClient, LocalLlmClient>();
builder.Services.AddSingleton<OfflineSpeechEngineRunner>();
builder.Services.AddSingleton<ISpeechToTextClient, LocalSpeechToTextClient>();
builder.Services.AddSingleton<IStreamingSpeechToTextClient, LocalStreamingSpeechToTextClient>();
builder.Services.AddEndpointsApiExplorer();

var app = builder.Build();

app.UseDefaultFiles();
app.UseStaticFiles();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    db.Database.EnsureCreated();
    EnsureMemoryTable(db);
    EnsureMessageFileContextTable(db);
    EnsureConversationJournalEntryColumn(db);
    EnsureSourceTables(db);
    EnsureProcessingNotificationsTable(db);
    SeedDefaultMemoryItems(db);
}

var journalService = app.Services.GetRequiredService<JournalService>();
journalService.EnsureStorage();
var workspaceLayout = app.Services.GetRequiredService<WorkspaceLayoutService>();

app.MapGet("/api/health", (OfflineSpeechEngineRunner speechEngineRunner) => Results.Ok(new
{
    status = "ok",
    mode = "offline-local",
    speech = speechEngineRunner.IsConfigured ? "whispercpp-ready" : "local-session-scaffold",
    llm = "local-adapter",
    workspaceRoot = workspaceLayout.RuntimeRoot
}));

app.MapChatEndpoints();
app.MapFileEndpoints();
app.MapJournalEndpoints();
app.MapSourceEndpoints();
app.MapProcessingEndpoints();
app.MapSystemEndpoints();
app.MapVoiceEndpoints();

app.Run();

static string ResolveDataDirectory(string contentRootPath)
{
    var currentDirectoryCandidate = Path.Combine(Directory.GetCurrentDirectory(), "App_Data");
    var workspaceCandidate = Path.GetFullPath(Path.Combine(contentRootPath, "..", "..", "App_Data"));
    var localContentRootCandidate = Path.Combine(contentRootPath, "App_Data");

    var existingCandidate = new[]
    {
        currentDirectoryCandidate,
        workspaceCandidate,
        localContentRootCandidate
    }.FirstOrDefault(Directory.Exists);

    return existingCandidate ?? workspaceCandidate;
}

static void EnsureMemoryTable(AppDbContext db)
{
    const string sql = """
        CREATE TABLE IF NOT EXISTS "MemoryItems" (
            "Id" TEXT NOT NULL CONSTRAINT "PK_MemoryItems" PRIMARY KEY,
            "Key" TEXT NOT NULL,
            "Content" TEXT NOT NULL,
            "Pinned" INTEGER NOT NULL,
            "CreatedUtc" TEXT NOT NULL,
            "UpdatedUtc" TEXT NOT NULL
        );
        """;

    db.Database.ExecuteSqlRaw(sql);
}

static void EnsureMessageFileContextTable(AppDbContext db)
{
    const string tableSql = """
        CREATE TABLE IF NOT EXISTS "MessageFileContexts" (
            "Id" TEXT NOT NULL CONSTRAINT "PK_MessageFileContexts" PRIMARY KEY,
            "MessageId" TEXT NOT NULL,
            "RelativePath" TEXT NOT NULL,
            "ContentSnippet" TEXT NOT NULL,
            "CreatedUtc" TEXT NOT NULL,
            CONSTRAINT "FK_MessageFileContexts_Messages_MessageId" FOREIGN KEY ("MessageId") REFERENCES "Messages" ("Id") ON DELETE CASCADE
        );
        """;

    const string indexSql = """
        CREATE INDEX IF NOT EXISTS "IX_MessageFileContexts_MessageId" ON "MessageFileContexts" ("MessageId");
        """;

    db.Database.ExecuteSqlRaw(tableSql);
    db.Database.ExecuteSqlRaw(indexSql);
}

static void EnsureConversationJournalEntryColumn(AppDbContext db)
{
    const string existsSql = """
        SELECT COUNT(*) FROM pragma_table_info('Conversations') WHERE name = 'JournalEntryId';
        """;

    var exists = db.Database.SqlQueryRaw<long>(existsSql).AsEnumerable().FirstOrDefault() > 0;
    if (!exists)
    {
        const string sql = """
            ALTER TABLE "Conversations" ADD COLUMN "JournalEntryId" TEXT NULL;
            """;

        db.Database.ExecuteSqlRaw(sql);
    }
}

static void EnsureSourceTables(AppDbContext db)
{
    const string sourcesSql = """
        CREATE TABLE IF NOT EXISTS "Sources" (
            "Id" TEXT NOT NULL CONSTRAINT "PK_Sources" PRIMARY KEY,
            "SourceKey" TEXT NOT NULL,
            "Title" TEXT NOT NULL,
            "SourceType" TEXT NOT NULL,
            "RightsLabel" TEXT NOT NULL,
            "OriginalRelativePath" TEXT NOT NULL,
            "CurrentStage" TEXT NOT NULL,
            "Status" TEXT NOT NULL,
            "CreatedUtc" TEXT NOT NULL,
            "UpdatedUtc" TEXT NOT NULL
        );
        """;

    const string sourceKeyIndexSql = """
        CREATE UNIQUE INDEX IF NOT EXISTS "IX_Sources_SourceKey" ON "Sources" ("SourceKey");
        """;

    const string sourceFilesSql = """
        CREATE TABLE IF NOT EXISTS "SourceFiles" (
            "Id" TEXT NOT NULL CONSTRAINT "PK_SourceFiles" PRIMARY KEY,
            "SourceId" TEXT NOT NULL,
            "RelativePath" TEXT NOT NULL,
            "FileRole" TEXT NOT NULL,
            "CreatedUtc" TEXT NOT NULL,
            CONSTRAINT "FK_SourceFiles_Sources_SourceId" FOREIGN KEY ("SourceId") REFERENCES "Sources" ("Id") ON DELETE CASCADE
        );
        """;

    const string sourceFilesIndexSql = """
        CREATE INDEX IF NOT EXISTS "IX_SourceFiles_SourceId" ON "SourceFiles" ("SourceId");
        """;

    const string sourceTagsSql = """
        CREATE TABLE IF NOT EXISTS "SourceTags" (
            "Id" TEXT NOT NULL CONSTRAINT "PK_SourceTags" PRIMARY KEY,
            "SourceId" TEXT NOT NULL,
            "Tag" TEXT NOT NULL,
            CONSTRAINT "FK_SourceTags_Sources_SourceId" FOREIGN KEY ("SourceId") REFERENCES "Sources" ("Id") ON DELETE CASCADE
        );
        """;

    const string sourceTagsIndexSql = """
        CREATE INDEX IF NOT EXISTS "IX_SourceTags_SourceId" ON "SourceTags" ("SourceId");
        """;

    const string processingJobsSql = """
        CREATE TABLE IF NOT EXISTS "ProcessingJobs" (
            "Id" TEXT NOT NULL CONSTRAINT "PK_ProcessingJobs" PRIMARY KEY,
            "SourceId" TEXT NOT NULL,
            "JobType" TEXT NOT NULL,
            "Status" TEXT NOT NULL,
            "StartedUtc" TEXT NOT NULL,
            "CompletedUtc" TEXT NULL,
            "ErrorMessage" TEXT NULL,
            CONSTRAINT "FK_ProcessingJobs_Sources_SourceId" FOREIGN KEY ("SourceId") REFERENCES "Sources" ("Id") ON DELETE CASCADE
        );
        """;

    const string processingJobsIndexSql = """
        CREATE INDEX IF NOT EXISTS "IX_ProcessingJobs_SourceId" ON "ProcessingJobs" ("SourceId");
        """;

    const string chunksSql = """
        CREATE TABLE IF NOT EXISTS "Chunks" (
            "Id" TEXT NOT NULL CONSTRAINT "PK_Chunks" PRIMARY KEY,
            "SourceId" TEXT NOT NULL,
            "ChunkIndex" INTEGER NOT NULL,
            "SectionTitle" TEXT NULL,
            "PageReference" TEXT NULL,
            "TextPath" TEXT NOT NULL,
            "CharacterCount" INTEGER NOT NULL,
            "TokenCount" INTEGER NOT NULL,
            "Status" TEXT NOT NULL,
            "CreatedUtc" TEXT NOT NULL,
            CONSTRAINT "FK_Chunks_Sources_SourceId" FOREIGN KEY ("SourceId") REFERENCES "Sources" ("Id") ON DELETE CASCADE
        );
        """;

    const string chunksIndexSql = """
        CREATE INDEX IF NOT EXISTS "IX_Chunks_SourceId" ON "Chunks" ("SourceId");
        """;

    db.Database.ExecuteSqlRaw(sourcesSql);
    db.Database.ExecuteSqlRaw(sourceKeyIndexSql);
    db.Database.ExecuteSqlRaw(sourceFilesSql);
    db.Database.ExecuteSqlRaw(sourceFilesIndexSql);
    db.Database.ExecuteSqlRaw(sourceTagsSql);
    db.Database.ExecuteSqlRaw(sourceTagsIndexSql);
    db.Database.ExecuteSqlRaw(processingJobsSql);
    db.Database.ExecuteSqlRaw(processingJobsIndexSql);
    db.Database.ExecuteSqlRaw(chunksSql);
    db.Database.ExecuteSqlRaw(chunksIndexSql);
}

static void EnsureProcessingNotificationsTable(AppDbContext db)
{
    const string notificationsSql = """
        CREATE TABLE IF NOT EXISTS "ProcessingNotifications" (
            "Id" TEXT NOT NULL CONSTRAINT "PK_ProcessingNotifications" PRIMARY KEY,
            "SourceId" TEXT NULL,
            "FileName" TEXT NOT NULL,
            "Status" TEXT NOT NULL,
            "Message" TEXT NULL,
            "ChunksCreated" INTEGER NULL,
            "CharactersProcessed" INTEGER NULL,
            "ProcessingTimeMs" INTEGER NULL,
            "CreatedUtc" TEXT NOT NULL,
            "IsRead" INTEGER NOT NULL DEFAULT 0,
            CONSTRAINT "FK_ProcessingNotifications_Sources_SourceId" FOREIGN KEY ("SourceId") REFERENCES "Sources" ("Id") ON DELETE CASCADE
        );
        """;

    const string sourceIdIndexSql = """
        CREATE INDEX IF NOT EXISTS "IX_ProcessingNotifications_SourceId" ON "ProcessingNotifications" ("SourceId");
        """;

    const string createdUtcIndexSql = """
        CREATE INDEX IF NOT EXISTS "IX_ProcessingNotifications_CreatedUtc" ON "ProcessingNotifications" ("CreatedUtc");
        """;

    const string isReadIndexSql = """
        CREATE INDEX IF NOT EXISTS "IX_ProcessingNotifications_IsRead" ON "ProcessingNotifications" ("IsRead");
        """;

    db.Database.ExecuteSqlRaw(notificationsSql);
    db.Database.ExecuteSqlRaw(sourceIdIndexSql);
    db.Database.ExecuteSqlRaw(createdUtcIndexSql);
    db.Database.ExecuteSqlRaw(isReadIndexSql);
}

static void SeedDefaultMemoryItems(AppDbContext db)
{
    if (db.MemoryItems.Any())
    {
        return;
    }

    db.MemoryItems.AddRange(
    [
        new MemoryItem
        {
            Key = "mode",
            Content = "me_workspace runs as a local-first assistant shell on 127.0.0.1.",
            Pinned = true
        },
        new MemoryItem
        {
            Key = "storage",
            Content = "Chats are stored in SQLite under App_Data/me_workspace.db.",
            Pinned = true
        },
        new MemoryItem
        {
            Key = "priority",
            Content = "Keep the end-to-end loop wired before improving individual components.",
            Pinned = true
        }
    ]);

    db.SaveChanges();
}
