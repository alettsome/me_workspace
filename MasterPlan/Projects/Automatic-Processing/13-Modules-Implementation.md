# Modules Implementation - Automatic Processing

## Code Structure

---

## Module Map

```
src/me_workspace.Web/
├── Features/
│   ├── Processing/
│   │   ├── BackgroundProcessingService.cs       [Core Service]
│   │   ├── ProcessingNotification.cs            [Entity]
│   │   ├── ProcessingEndpoints.cs               [API]
│   │   └── ProcessingPipelineService.cs         [Chunking]
│   ├── Sources/
│   │   └── SourceRegistryService.cs             [Registration]
│   └── Chunking/
│       └── ChunkingService.cs                   [Algorithm]
├── Data/
│   ├── AppDbContext.cs                          [EF Core Context]
│   └── Entities/
│       ├── Source.cs
│       ├── SourceFile.cs
│       ├── Chunk.cs
│       └── ProcessingNotification.cs
└── Program.cs                                   [Startup]

tools/
└── ExtractPdfText/
    ├── ExtractPdfText.csproj
    └── Program.cs                               [PDF Extraction]

docs/
└── automatic-processing.md                      [Documentation]
```

---

## Core Modules

### **1. BackgroundProcessingService**

**Location**: `src/me_workspace.Web/Features/Processing/BackgroundProcessingService.cs`

**Purpose**: Orchestrates the entire automatic processing pipeline

**Implements**: `IHostedService`

**Key Methods**:

```csharp
public class BackgroundProcessingService : IHostedService
{
    private FileSystemWatcher _watcher;
    private readonly IServiceProvider _services;
    private readonly IWebHostEnvironment _environment;
    private readonly ILogger<BackgroundProcessingService> _logger;

    // Lifecycle
    public Task StartAsync(CancellationToken cancellationToken)
    {
        var inboxPath = Path.Combine(runtimeRoot, "01-Inbox");
        _watcher = new FileSystemWatcher(inboxPath)
        {
            Filter = "*.pdf",
            NotifyFilter = NotifyFilters.FileName | NotifyFilters.LastWrite
        };
        _watcher.Created += OnCreated;
        _watcher.Changed += OnCreated;
        _watcher.EnableRaisingEvents = true;
        _logger.LogInformation("Background file watcher started...");
        return Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        _watcher?.Dispose();
        return Task.CompletedTask;
    }

    // Event Handler
    private void OnCreated(object sender, FileSystemEventArgs e)
    {
        _ = Task.Run(async () => await ProcessFileAsync(e.FullPath));
    }

    // Pipeline Orchestration
    private async Task ProcessFileAsync(string filePath)
    {
        _logger.LogInformation("🔄 Processing started for: {FileName}", Path.GetFileName(filePath));
        
        var sw = Stopwatch.StartNew();
        
        // Step 1: Wait for file ready
        await WaitForFileReadyAsync(filePath);
        
        // Step 2: Extract PDF
        var textPath = await ExtractPdfTextAsync(filePath);
        if (textPath == null) return;
        
        // Step 3: Register source
        var sourceId = await RegisterSourceAsync(filePath, textPath);
        
        // Step 4: Chunk content
        var result = await ChunkSourceAsync(sourceId);
        
        // Step 5: Create notification
        await SendNotificationAsync(Path.GetFileName(filePath), result, sw.ElapsedMilliseconds);
        
        _logger.LogInformation("Processing complete! ✅");
    }

    // Helper Methods
    private async Task WaitForFileReadyAsync(string path)
    {
        int maxRetries = 10;
        int retryDelayMs = 500;
        
        for (int i = 0; i < maxRetries; i++)
        {
            try
            {
                using var stream = File.Open(path, FileMode.Open, FileAccess.Read, FileShare.None);
                return;
            }
            catch (IOException)
            {
                await Task.Delay(retryDelayMs);
            }
        }
        
        throw new IOException($"File not ready after {maxRetries} attempts");
    }

    private async Task<string?> ExtractPdfTextAsync(string pdfPath)
    {
        _logger.LogInformation("📄 Extracting text from: {FileName}", Path.GetFileName(pdfPath));
        
        var extractToolPath = Path.GetFullPath(Path.Combine(
            _environment.ContentRootPath, "..", "..", 
            "tools", "ExtractPdfText", "bin", "Debug", "net8.0", "ExtractPdfText.exe"
        ));
        
        var process = new Process
        {
            StartInfo = new ProcessStartInfo
            {
                FileName = extractToolPath,
                Arguments = $"\"{pdfPath}\"",
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true
            }
        };
        
        process.Start();
        await process.WaitForExitAsync();
        
        if (process.ExitCode != 0)
        {
            _logger.LogError("❌ PDF extraction failed");
            return null;
        }
        
        var textPath = pdfPath + ".txt";
        _logger.LogInformation("✓ Extracted text to: {FileName}", Path.GetFileName(textPath));
        return textPath;
    }

    private async Task<Guid> RegisterSourceAsync(string pdfPath, string textPath)
    {
        using var scope = _services.CreateScope();
        var registry = scope.ServiceProvider.GetRequiredService<SourceRegistryService>();
        
        var sourceId = await registry.RegisterAsync(new SourceRegistrationRequest
        {
            Title = Path.GetFileNameWithoutExtension(pdfPath),
            SourceType = "pdf",
            RightsLabel = "personal",
            OriginalPath = pdfPath,
            AdditionalFiles = new[] { textPath }
        });
        
        _logger.LogInformation("📝 Registered source: {FileName}", Path.GetFileName(pdfPath));
        return sourceId;
    }

    private async Task<ChunkingResult> ChunkSourceAsync(Guid sourceId)
    {
        using var scope = _services.CreateScope();
        var pipeline = scope.ServiceProvider.GetRequiredService<ProcessingPipelineService>();
        
        _logger.LogInformation("✂️ Chunking source...");
        var result = await pipeline.ProcessSourceAsync(sourceId);
        
        _logger.LogInformation("✅ Created {Count} chunks ({Chars} characters)", 
            result.ChunksCreated, result.CharactersProcessed);
        
        return result;
    }

    private async Task SendNotificationAsync(string fileName, ChunkingResult result, long elapsedMs)
    {
        using var scope = _services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        
        var notification = new ProcessingNotification
        {
            Id = Guid.NewGuid(),
            SourceId = result.SourceId,
            FileName = fileName,
            Status = result.Success ? "Success" : "Failed",
            Message = result.ErrorMessage,
            ChunksCreated = result.ChunksCreated,
            CharactersProcessed = result.CharactersProcessed,
            ProcessingTimeMs = elapsedMs,
            CreatedUtc = DateTime.UtcNow,
            IsRead = false
        };
        
        db.ProcessingNotifications.Add(notification);
        await db.SaveChangesAsync();
        
        _logger.LogInformation("📢 Notification saved for: {FileName}", fileName);
        _logger.LogInformation("📊 Stats: {Chunks} chunks, {Chars} chars, {Ms}ms", 
            result.ChunksCreated, result.CharactersProcessed, elapsedMs);
    }
}
```

**Dependencies**:
- `IServiceProvider` (scoped service resolution)
- `IWebHostEnvironment` (content root path)
- `ILogger<T>` (structured logging)

**Registration** (Program.cs):
```csharp
builder.Services.AddHostedService<BackgroundProcessingService>();
```

---

### **2. ProcessingNotification (Entity)**

**Location**: `src/me_workspace.Web/Data/Entities/ProcessingNotification.cs`

**Purpose**: Track processing history with metrics

```csharp
public class ProcessingNotification
{
    public Guid Id { get; set; }
    public Guid? SourceId { get; set; }
    public string FileName { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty; // "Success" | "Failed" | "Processing"
    public string? Message { get; set; }
    public int? ChunksCreated { get; set; }
    public long? CharactersProcessed { get; set; }
    public long? ProcessingTimeMs { get; set; }
    public DateTime CreatedUtc { get; set; }
    public bool IsRead { get; set; }

    // Navigation
    public Source? Source { get; set; }
}
```

**Database Configuration** (AppDbContext.cs):
```csharp
modelBuilder.Entity<ProcessingNotification>(entity =>
{
    entity.HasKey(x => x.Id);
    entity.Property(x => x.FileName).HasMaxLength(300).IsRequired();
    entity.Property(x => x.Status).HasMaxLength(32).IsRequired();
    entity.Property(x => x.Message).HasMaxLength(2000);
    entity.Property(x => x.CreatedUtc).IsRequired();
    entity.Property(x => x.IsRead).IsRequired();
    
    entity.HasOne(x => x.Source)
        .WithMany()
        .HasForeignKey(x => x.SourceId)
        .OnDelete(DeleteBehavior.Cascade);
    
    entity.HasIndex(x => x.SourceId);
    entity.HasIndex(x => x.CreatedUtc);
    entity.HasIndex(x => x.IsRead);
});
```

---

### **3. ProcessingEndpoints (API)**

**Location**: `src/me_workspace.Web/Features/Processing/ProcessingEndpoints.cs`

**Purpose**: REST API for querying notifications

```csharp
public static class ProcessingEndpoints
{
    public static void MapProcessingEndpoints(this WebApplication app)
    {
        // Get notifications
        app.MapGet("/api/processing/notifications", async (
            AppDbContext db,
            int limit = 10,
            bool unreadOnly = false) =>
        {
            var query = db.ProcessingNotifications
                .Include(n => n.Source)
                .OrderByDescending(n => n.CreatedUtc);
            
            if (unreadOnly)
                query = query.Where(n => !n.IsRead);
            
            var notifications = await query.Take(limit).ToListAsync();
            
            return Results.Ok(new { notifications });
        });

        // Mark notification as read
        app.MapPost("/api/processing/notifications/{id}/mark-read", async (
            Guid id,
            AppDbContext db) =>
        {
            var notification = await db.ProcessingNotifications.FindAsync(id);
            if (notification == null)
                return Results.NotFound();
            
            notification.IsRead = true;
            await db.SaveChangesAsync();
            
            return Results.Ok();
        });

        // Manual processing trigger
        app.MapPost("/api/processing/process/{sourceId}", async (
            Guid sourceId,
            ProcessingPipelineService pipeline) =>
        {
            var result = await pipeline.ProcessSourceAsync(sourceId);
            return Results.Ok(result);
        });
    }
}
```

**Registration** (Program.cs):
```csharp
app.MapProcessingEndpoints();
```

---

### **4. SourceRegistryService**

**Location**: `src/me_workspace.Web/Features/Sources/SourceRegistryService.cs`

**Purpose**: Register sources in database

```csharp
public class SourceRegistryService
{
    private readonly AppDbContext _db;

    public async Task<Guid> RegisterAsync(SourceRegistrationRequest request)
    {
        var sourceKey = GenerateSourceKey();
        var source = new Source
        {
            Id = Guid.NewGuid(),
            SourceKey = sourceKey,
            Title = request.Title,
            SourceType = request.SourceType,
            RightsLabel = request.RightsLabel,
            OriginalRelativePath = request.OriginalPath,
            CurrentStage = "intake",
            Status = "new",
            CreatedUtc = DateTime.UtcNow,
            UpdatedUtc = DateTime.UtcNow
        };

        _db.Sources.Add(source);

        foreach (var filePath in request.AdditionalFiles)
        {
            _db.SourceFiles.Add(new SourceFile
            {
                Id = Guid.NewGuid(),
                SourceId = source.Id,
                RelativePath = filePath,
                FileRole = DetermineFileRole(filePath),
                CreatedUtc = DateTime.UtcNow
            });
        }

        await _db.SaveChangesAsync();
        return source.Id;
    }

    private string GenerateSourceKey()
    {
        var timestamp = DateTime.Now.ToString("yyyyMMdd-HHmmss");
        var random = Guid.NewGuid().ToString()[..5];
        return $"src-{timestamp}-{random}";
    }

    private string DetermineFileRole(string path)
    {
        return Path.GetExtension(path).ToLower() switch
        {
            ".pdf" => "original",
            ".txt" => "extracted-text",
            _ => "unknown"
        };
    }
}
```

---

### **5. ProcessingPipelineService**

**Location**: `src/me_workspace.Web/Features/Processing/ProcessingPipelineService.cs`

**Purpose**: Chunk and save content (dual-write)

```csharp
public class ProcessingPipelineService
{
    private readonly AppDbContext _db;
    private readonly ChunkingService _chunking;
    private readonly WorkspaceLayoutService _workspace;

    public async Task<ChunkingResult> ProcessSourceAsync(Guid sourceId)
    {
        var source = await _db.Sources
            .Include(s => s.Files)
            .FirstOrDefaultAsync(s => s.Id == sourceId);
        
        if (source == null)
            return ChunkingResult.Failed("Source not found");
        
        var textFile = source.Files.FirstOrDefault(f => f.FileRole == "extracted-text");
        if (textFile == null)
            return ChunkingResult.Failed("No text file found");
        
        var text = await File.ReadAllTextAsync(textFile.RelativePath);
        var chunks = _chunking.ChunkText(text);
        
        // Dual write: Database + Filesystem
        var chunkFolder = Path.Combine(_workspace.RuntimeRoot, "03-Chunked", source.SourceKey);
        Directory.CreateDirectory(chunkFolder);
        
        for (int i = 0; i < chunks.Count; i++)
        {
            var chunkPath = Path.Combine(chunkFolder, $"chunk-{i:000}.txt");
            await File.WriteAllTextAsync(chunkPath, chunks[i].Text);
            
            _db.Chunks.Add(new Chunk
            {
                Id = Guid.NewGuid(),
                SourceId = sourceId,
                ChunkIndex = i,
                TextPath = chunkPath,
                CharacterCount = chunks[i].Text.Length,
                TokenCount = EstimateTokens(chunks[i].Text),
                Status = "ready",
                CreatedUtc = DateTime.UtcNow
            });
        }
        
        await _db.SaveChangesAsync();
        
        return new ChunkingResult
        {
            Success = true,
            SourceId = sourceId,
            ChunksCreated = chunks.Count,
            CharactersProcessed = text.Length
        };
    }

    private int EstimateTokens(string text)
    {
        return text.Length / 4; // Rough estimate
    }
}
```

---

### **6. ExtractPdfText Tool**

**Location**: `tools/ExtractPdfText/Program.cs`

**Purpose**: External PDF text extraction

```csharp
using iText.Kernel.Pdf;
using iText.Kernel.Pdf.Canvas.Parser;
using System.Text;

if (args.Length == 0)
{
    Console.Error.WriteLine("Usage: ExtractPdfText <pdf-path>");
    return 1;
}

var pdfPath = args[0];
var outputPath = pdfPath + ".txt";

try
{
    using var reader = new PdfReader(pdfPath);
    using var document = new PdfDocument(reader);
    var text = new StringBuilder();

    for (int i = 1; i <= document.GetNumberOfPages(); i++)
    {
        var pageText = PdfTextExtractor.GetTextFromPage(document.GetPage(i));
        text.AppendLine(pageText);
    }

    await File.WriteAllTextAsync(outputPath, text.ToString());
    return 0;
}
catch (Exception ex)
{
    Console.Error.WriteLine($"Error: {ex.Message}");
    return 1;
}
```

**Build**:
```bash
cd tools/ExtractPdfText
dotnet build
```

---

## Service Registration (Program.cs)

```csharp
// Background service
builder.Services.AddHostedService<BackgroundProcessingService>();

// Scoped services (per-request)
builder.Services.AddScoped<SourceRegistryService>();
builder.Services.AddScoped<ProcessingPipelineService>();
builder.Services.AddScoped<ChunkingService>();

// Singleton services (shared state)
builder.Services.AddSingleton<WorkspaceLayoutService>();

// DbContext
builder.Services.AddDbContext<AppDbContext>(options =>
{
    var dbPath = Path.Combine(dataDirectory, "me_workspace.db");
    options.UseSqlite($"Data Source={dbPath}");
});
```

---

## Testing

**Manual Test**:
1. Start application: `dotnet run --urls "http://127.0.0.1:5078"`
2. Drop PDF into: `C:\me_workspaces_runtime\01-Inbox\`
3. Watch console for emoji logging
4. Query API: `curl http://127.0.0.1:5078/api/processing/notifications`
5. Check chunks: `dir C:\me_workspaces_runtime\03-Chunked\`

**Automated Test** (Future):
```csharp
[Fact]
public async Task ProcessFileAsync_CreatesNotification()
{
    // Arrange
    var testPdf = "test.pdf";
    
    // Act
    await _service.ProcessFileAsync(testPdf);
    
    // Assert
    var notification = await _db.ProcessingNotifications
        .FirstOrDefaultAsync(n => n.FileName == testPdf);
    Assert.NotNull(notification);
    Assert.Equal("Success", notification.Status);
}
```

---

## Deployment

**Requirements**:
- .NET 8.0 Runtime
- ExtractPdfText.exe in `tools/` folder
- SQLite database initialized
- `01-Inbox/` folder created

**Startup**:
```bash
cd src/me_workspace.Web
dotnet run --urls "http://127.0.0.1:5078"
```

**Logs**: `C:\me_workspaces_runtime\logs\me_workspaces-{date}.log`
