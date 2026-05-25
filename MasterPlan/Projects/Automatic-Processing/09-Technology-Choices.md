# Technology Choices - Automatic Processing

## Technology Stack

---

### **1. FileSystemWatcher (.NET 8)**

**Purpose**: Real-time file detection

**Why Chosen**:
- Native to .NET, no external dependencies
- Event-driven (zero CPU when idle)
- Reliable on Windows
- Supports filters (`*.pdf`)
- Handles `Created`, `Changed`, `Deleted`, `Renamed` events

**Alternatives Considered**:
- ❌ **Polling**: Wastes CPU, delayed detection
- ❌ **Manual trigger API**: Requires user action (defeats automation)
- ❌ **Third-party file watcher**: Unnecessary complexity

**Configuration**:
```csharp
var watcher = new FileSystemWatcher(inboxPath)
{
    Filter = "*.pdf",
    NotifyFilter = NotifyFilters.FileName | NotifyFilters.LastWrite,
    EnableRaisingEvents = true
};
watcher.Created += OnCreated;
watcher.Changed += OnCreated; // Handle copy-in-progress
```

**Trade-offs**:
- Windows-only (acceptable for local-first design)
- Events can fire twice for same file (handled with duplicate check)

---

### **2. IHostedService (.NET 8)**

**Purpose**: Background service lifecycle management

**Why Chosen**:
- Integrated with ASP.NET Core hosting
- Automatic start/stop with application
- Dependency injection support
- Graceful shutdown handling

**Alternatives Considered**:
- ❌ **Windows Service**: Over-engineered for local app
- ❌ **Separate process**: More complex deployment
- ❌ **Timer-based**: Polling approach, less responsive

**Implementation**:
```csharp
public class BackgroundProcessingService : IHostedService
{
    public Task StartAsync(CancellationToken cancellationToken)
    {
        // Start FileSystemWatcher
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        // Cleanup watcher
    }
}
```

**Registration**:
```csharp
builder.Services.AddHostedService<BackgroundProcessingService>();
```

---

### **3. iText7 (PDF Extraction)**

**Purpose**: Extract text from PDF files

**Why Chosen**:
- Industry-standard PDF library
- Reliable text extraction
- Handles complex PDFs (multi-column, images, tables)
- Well-maintained, active development

**Alternatives Considered**:
- ❌ **PdfPig**: Less reliable for complex layouts
- ❌ **PDFBox (.NET port)**: Less mature .NET support
- ❌ **Ghostscript**: Requires external binary

**Deployment**:
- Compiled as separate `ExtractPdfText.exe`
- Spawned as external process (isolation)
- Located: `tools/ExtractPdfText/bin/Debug/net8.0/`

**Why External Process**:
- PDF libraries can leak memory
- Crashes don't affect main app
- Easier to update independently

**Code** (ExtractPdfText.exe):
```csharp
using iText.Kernel.Pdf;
using iText.Kernel.Pdf.Canvas.Parser;

var reader = new PdfReader(pdfPath);
var document = new PdfDocument(reader);
var text = new StringBuilder();

for (int i = 1; i <= document.GetNumberOfPages(); i++)
{
    text.Append(PdfTextExtractor.GetTextFromPage(document.GetPage(i)));
}

File.WriteAllText(outputPath, text.ToString());
```

---

### **4. Entity Framework Core with SQLite**

**Purpose**: Data persistence

**Why Chosen**:
- Local file-based database (no server required)
- Type-safe queries with LINQ
- Migration support
- Cross-platform

**Alternatives Considered**:
- ❌ **JSON files**: No querying, no relationships
- ❌ **SQL Server**: Over-engineered for local app
- ❌ **PostgreSQL**: Requires server process

**Tables Used**:
- `Sources` - Source metadata
- `SourceFiles` - Associated files (PDF, TXT)
- `Chunks` - Chunked content
- `ProcessingNotifications` - Processing history

**Connection**:
```csharp
options.UseSqlite($"Data Source={databasePath}");
```

**Location**: `C:\me_workspace\App_Data\me_workspace.db`

---

### **5. Custom Chunking Algorithm (C#)**

**Purpose**: Split text into processable chunks

**Why Chosen**:
- Simple, predictable behavior
- No external dependencies
- Configurable chunk size
- Semantic boundary detection (paragraphs)

**Alternatives Considered**:
- ❌ **LangChain**: Overkill for MVP, Python dependency
- ❌ **Token-based splitting**: Requires tokenizer library
- ❌ **Fixed-size**: Ignores semantic boundaries

**Algorithm**:
```csharp
- Target: ~4000 characters per chunk
- Strategy: Split on paragraph boundaries (\n\n)
- Merge small paragraphs until near target
- Preserve section titles if detected
```

**Service**: `ChunkingService`

---

### **6. Serilog (Logging)**

**Purpose**: Structured logging with emoji indicators

**Why Chosen**:
- Structured logging (JSON support)
- Multiple sinks (Console + File)
- Performance optimized
- Easy to filter and search

**Configuration**:
```csharp
.WriteTo.Console()
.WriteTo.File(
    Path.Combine(logDirectory, "me_workspaces-.log"),
    rollingInterval: RollingInterval.Day
)
```

**Emoji Convention**:
- 🔄 Starting/Processing
- 📄 Extracting
- ✓ Success
- ✂️ Chunking
- ✅ Complete
- 📊 Statistics
- 📢 Notification
- ❌ Error

---

### **7. System.Diagnostics.Process (.NET)**

**Purpose**: Spawn external ExtractPdfText.exe

**Why Chosen**:
- Native .NET process management
- Capture stdout/stderr
- Set timeout
- Isolation from main app

**Implementation**:
```csharp
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
await process.WaitForExitAsync(cancellationToken);
```

---

## Dependency Management

**NuGet Packages**:
```xml
<PackageReference Include="Microsoft.EntityFrameworkCore.Sqlite" Version="8.0.0" />
<PackageReference Include="Serilog.AspNetCore" Version="8.0.0" />
<PackageReference Include="Serilog.Sinks.File" Version="5.0.0" />
```

**External Tools**:
- `ExtractPdfText.exe` (iText7-based, built separately)

---

## Performance Considerations

**FileSystemWatcher**:
- Minimal CPU usage when idle
- Event queue prevents missed files
- Duplicate event handling (same file triggers twice)

**External Process**:
- Spawning overhead: ~100ms per file
- Memory isolation: Prevents leaks in main app
- Parallel potential: Could spawn multiple processes (not implemented)

**Database**:
- Batch inserts for chunks (single transaction)
- Indexes on SourceId, CreatedUtc, IsRead
- SQLite Write-Ahead Logging (WAL) for concurrency

**Chunking**:
- In-memory processing (fast for <10MB files)
- Stream-based for larger files (if needed)

---

## Security Considerations

**Trusted Input**:
- Files in `01-Inbox/` are implicitly trusted
- No validation of PDF content
- User responsible for source safety

**Isolation**:
- PDF extraction runs in separate process
- Cannot affect main application stability

**File System**:
- Monitored folder is user-configured
- No automatic deletion of source files
- All writes go to designated output folders

---

## Platform Requirements

- **.NET 8.0 SDK**
- **Windows 10/11** (FileSystemWatcher optimized for Windows)
- **SQLite** (embedded, no installation)
- **Disk Space**: ~10MB per 100-page PDF (text + chunks)

---

## Future Technology Considerations

**Potential Upgrades** (Not MVP):
- **Parallel Processing**: Queue library (e.g., Hangfire)
- **OCR Support**: Tesseract.NET for image-based PDFs
- **Cloud Storage**: Azure Blob / S3 for large files
- **Real-time UI**: SignalR for live progress updates
- **Docker**: Containerized deployment
