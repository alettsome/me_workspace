using Microsoft.EntityFrameworkCore;
using Microsoft.Data.Sqlite;
using me_workspace.Web.Data;
using me_workspace.Web.Data.Entities;
using me_workspace.Web.Features.Chat;
using me_workspace.Web.Features.Context;
using me_workspace.Web.Features.Files;
using me_workspace.Web.Features.Journal;
using me_workspace.Web.Features.Memory;
using me_workspace.Web.Features.System;
using me_workspace.Web.Features.Voice;
using me_workspace.Web.Infrastructure.Llm;
using me_workspace.Web.Infrastructure.Speech;

var builder = WebApplication.CreateBuilder(args);

builder.WebHost.UseUrls(builder.Configuration["urls"] ?? "http://127.0.0.1:5078");
builder.Logging.ClearProviders();
builder.Logging.AddConsole();
builder.Logging.AddDebug();

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
builder.Services.AddSingleton<ILlmClient, LocalLlmClient>();
builder.Services.AddSingleton<ISpeechToTextClient, LocalSpeechToTextClient>();
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
    SeedDefaultMemoryItems(db);
}

var journalService = app.Services.GetRequiredService<JournalService>();
journalService.EnsureStorage();

app.MapGet("/api/health", () => Results.Ok(new
{
    status = "ok",
    mode = "offline-local",
    speech = "local-draft",
    llm = "local-adapter"
}));

app.MapChatEndpoints();
app.MapFileEndpoints();
app.MapJournalEndpoints();
app.MapSystemEndpoints();

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
    const string sql = """
        ALTER TABLE "Conversations" ADD COLUMN "JournalEntryId" TEXT NULL;
        """;

    try
    {
        db.Database.ExecuteSqlRaw(sql);
    }
    catch (SqliteException ex) when (ex.SqliteErrorCode == 1 &&
                                     ex.Message.Contains("duplicate column name", StringComparison.OrdinalIgnoreCase))
    {
    }
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
