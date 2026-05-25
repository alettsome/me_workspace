# Configuration Parameters - Automatic Processing

## System Configuration

### **WorkspaceOptions** (appsettings.json)

```json
{
  "WorkspaceOptions": {
    "RuntimeRoot": "C:\\me_workspaces_runtime",
    "InboxFolderName": "01-Inbox",
    "ChunkedFolderName": "03-Chunked",
    "LogsFolderName": "logs"
  }
}
```

#### **RuntimeRoot**

**Purpose**: Base directory for all runtime data

**Type**: `string` (absolute path)

**Default**: `C:\me_workspaces_runtime`

**Required**: Yes

**Example**: `C:\me_workspaces_runtime`

**Validation**:
- Must be absolute path
- Directory must exist (created if missing)
- Must have write permissions

**Impact**: All intake, chunked, and log folders created under this root

---

#### **InboxFolderName**

**Purpose**: Name of monitored intake folder

**Type**: `string` (relative folder name)

**Default**: `"01-Inbox"`

**Required**: Yes

**Example**: `"01-Inbox"`

**Full Path**: `{RuntimeRoot}\{InboxFolderName}`  
→ `C:\me_workspaces_runtime\01-Inbox`

**Constraints**:
- Valid folder name (no special characters: `<>:"/\|?*`)
- Cannot be `.` or `..`

**Impact**: FileSystemWatcher monitors this folder

---

#### **ChunkedFolderName**

**Purpose**: Name of chunked output folder

**Type**: `string` (relative folder name)

**Default**: `"03-Chunked"`

**Required**: Yes

**Example**: `"03-Chunked"`

**Full Path**: `{RuntimeRoot}\{ChunkedFolderName}`  
→ `C:\me_workspaces_runtime\03-Chunked`

**Impact**: Chunk files written to `{ChunkedFolderName}\{SourceKey}\`

---

#### **LogsFolderName**

**Purpose**: Name of log output folder

**Type**: `string` (relative folder name)

**Default**: `"logs"`

**Required**: Yes

**Example**: `"logs"`

**Full Path**: `{RuntimeRoot}\{LogsFolderName}`  
→ `C:\me_workspaces_runtime\logs`

**Impact**: Serilog writes daily log files here

---

## Service Configuration

### **BackgroundProcessingService** (Hard-Coded)

#### **FileFilter**

**Purpose**: Which file types to monitor

**Type**: `string`

**Current Value**: `"*.pdf"`

**Modifiable**: Code change required

**Location**: `BackgroundProcessingService.StartAsync()`

```csharp
_watcher.Filter = "*.pdf";
```

**Future Enhancement**: Move to appsettings.json

```json
{
  "ProcessingOptions": {
    "FileFilter": "*.pdf"
  }
}
```

---

#### **FileReadyTimeout**

**Purpose**: How long to wait for file copy to complete

**Type**: `int` (milliseconds)

**Current Value**: `5000` (5 seconds)

**Modifiable**: Code change required

**Location**: `BackgroundProcessingService.WaitForFileReadyAsync()`

```csharp
private const int MaxRetries = 10;
private const int RetryDelayMs = 500;
// Total timeout = 10 × 500 = 5000ms
```

**Calculation**: `MaxRetries × RetryDelayMs`

**Rationale**: 5 seconds enough for typical network copies

**Future Enhancement**: Configuration parameter

```json
{
  "ProcessingOptions": {
    "FileReadyTimeoutMs": 5000,
    "FileReadyRetries": 10
  }
}
```

---

#### **NotifyFilters**

**Purpose**: Which filesystem events to watch

**Type**: `NotifyFilters` enum (flags)

**Current Value**: `FileName | LastWrite`

**Modifiable**: Code change required

**Location**: `BackgroundProcessingService.StartAsync()`

```csharp
_watcher.NotifyFilter = NotifyFilters.FileName | NotifyFilters.LastWrite;
```

**Options**:
- `FileName` - File created/renamed
- `LastWrite` - File modified
- `Size` - File size changed
- `CreationTime` - File creation time changed

**Current Choice**: `FileName` (detect new files) + `LastWrite` (detect copy completion)

---

## Extraction Configuration

### **ExtractPdfText Tool Path**

**Purpose**: Location of PDF extraction executable

**Type**: `string` (relative path)

**Current Value**: Calculated dynamically

```csharp
var toolPath = Path.GetFullPath(Path.Combine(
    _environment.ContentRootPath, "..", "..", 
    "tools", "ExtractPdfText", "bin", "Debug", "net8.0", "ExtractPdfText.exe"
));
```

**Resolved**: `C:\me_workspace\tools\ExtractPdfText\bin\Debug\net8.0\ExtractPdfText.exe`

**Modifiable**: Hard-coded path calculation

**Future Enhancement**: Configuration parameter

```json
{
  "ProcessingOptions": {
    "ExtractPdfToolPath": "C:\\me_workspace\\tools\\ExtractPdfText\\bin\\Debug\\net8.0\\ExtractPdfText.exe"
  }
}
```

---

#### **ExtractPdfTimeout**

**Purpose**: Maximum extraction time before kill

**Type**: `int` (milliseconds)

**Current Value**: `None` (waits indefinitely)

**Modifiable**: Code change required

**Location**: `BackgroundProcessingService.ExtractPdfTextAsync()`

```csharp
await process.WaitForExitAsync(cancellationToken);
```

**Risk**: Large PDFs could hang forever

**Future Enhancement**: Add timeout

```csharp
var timeout = TimeSpan.FromMinutes(5);
if (!await process.WaitForExitAsync(timeout))
{
    process.Kill();
    throw new TimeoutException("PDF extraction timed out");
}
```

---

## Chunking Configuration

### **ChunkingService** (Hard-Coded)

#### **TargetChunkSize**

**Purpose**: Approximate characters per chunk

**Type**: `int`

**Current Value**: `4000` characters

**Modifiable**: Code change required

**Location**: `ChunkingService.ChunkText()`

**Rationale**: 
- ~1000 tokens (assuming 4 chars per token)
- Fits within most LLM context windows
- Large enough for semantic coherence

**Future Enhancement**: Configuration parameter

```json
{
  "ChunkingOptions": {
    "TargetChunkSize": 4000,
    "MinChunkSize": 1000,
    "MaxChunkSize": 8000
  }
}
```

---

#### **SplitBoundary**

**Purpose**: Where to split text

**Type**: `string` (regex pattern)

**Current Value**: `"\n\n"` (double newline / paragraph break)

**Modifiable**: Code change required

**Location**: `ChunkingService.ChunkText()`

**Alternatives**:
- Sentence boundary: `"\\. "`
- Fixed size: No boundary (char count only)
- Smart: ML-based semantic segmentation

**Current Choice**: Paragraph boundary (simple, effective)

---

## Database Configuration

### **ConnectionString** (Program.cs)

```csharp
var dataDirectory = ResolveDataDirectory(builder.Environment.ContentRootPath);
var databasePath = Path.Combine(dataDirectory, "me_workspace.db");
options.UseSqlite($"Data Source={databasePath}");
```

**Current Path**: `C:\me_workspace\App_Data\me_workspace.db`

**Modifiable**: Code change required

**Future Enhancement**: Move to appsettings.json

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Data Source=C:\\me_workspace\\App_Data\\me_workspace.db"
  }
}
```

---

## Logging Configuration

### **Serilog** (Program.cs)

```csharp
builder.Host.UseSerilog((_, _, loggerConfiguration) => loggerConfiguration
    .WriteTo.Console()
    .WriteTo.Debug()
    .WriteTo.File(
        Path.Combine(runtimeLogDirectory, "me_workspaces-.log"),
        rollingInterval: RollingInterval.Day,
        shared: true
    )
);
```

#### **Console Logging**

**Enabled**: Yes

**Output**: Standard output (terminal)

**Format**: `[HH:mm:ss INF] message`

**Modifiable**: Code change required

---

#### **File Logging**

**Enabled**: Yes

**Output Directory**: `{RuntimeRoot}\logs\`

**File Pattern**: `me_workspaces-YYYYMMDD.log`

**Rolling Interval**: Daily

**Shared**: Yes (multiple processes can write)

**Modifiable**: Code change required

**Future Enhancement**: appsettings.json

```json
{
  "Serilog": {
    "MinimumLevel": "Information",
    "WriteTo": [
      { "Name": "Console" },
      { 
        "Name": "File",
        "Args": {
          "path": "logs/me_workspaces-.log",
          "rollingInterval": "Day"
        }
      }
    ]
  }
}
```

---

## API Configuration

### **URLs** (Command Line)

**Current**: `--urls "http://127.0.0.1:5078"`

**Modifiable**: Command line argument or appsettings.json

**Format**: `{scheme}://{host}:{port}`

**Example**: `http://127.0.0.1:5078`

**Constraints**:
- Must use `127.0.0.1` (local-only)
- Port must be available
- HTTPS not required (local network)

---

## Environment-Specific Configuration

### **Development**

```json
{
  "WorkspaceOptions": {
    "RuntimeRoot": "C:\\me_workspaces_runtime"
  }
}
```

### **Production**

```json
{
  "WorkspaceOptions": {
    "RuntimeRoot": "D:\\me_workspaces_runtime"
  }
}
```

**Override**: `appsettings.Production.json` overrides `appsettings.json`

---

## Tuning Recommendations

### **For Large PDFs** (>100 pages)

```json
{
  "ProcessingOptions": {
    "FileReadyTimeoutMs": 10000,
    "ExtractPdfTimeoutMs": 300000
  }
}
```

### **For Slow Network Drives**

```json
{
  "ProcessingOptions": {
    "FileReadyTimeoutMs": 30000,
    "FileReadyRetries": 30
  }
}
```

### **For Small Chunks** (Dense text)

```json
{
  "ChunkingOptions": {
    "TargetChunkSize": 2000
  }
}
```

### **For Large Chunks** (Sparse text)

```json
{
  "ChunkingOptions": {
    "TargetChunkSize": 8000
  }
}
```

---

## Configuration Hierarchy

1. **Code Defaults** (Hard-coded values)
2. **appsettings.json** (Base configuration)
3. **appsettings.{Environment}.json** (Environment override)
4. **Environment Variables** (OS-level override)
5. **Command Line Arguments** (Runtime override)

**Precedence**: Lower number = lower priority (overridden by higher)

---

## Future Configuration Needs

### **Phase 2**

⏳ Move file filter to configuration  
⏳ Move timeouts to configuration  
⏳ Move tool paths to configuration  
⏳ Move chunking parameters to configuration

### **Phase 3**

⏳ Add configuration UI (web interface)  
⏳ Add validation on configuration load  
⏳ Add hot-reload (change config without restart)

---

## Configuration Schema (Future)

```json
{
  "WorkspaceOptions": {
    "RuntimeRoot": "C:\\me_workspaces_runtime",
    "InboxFolderName": "01-Inbox",
    "ChunkedFolderName": "03-Chunked",
    "LogsFolderName": "logs"
  },
  "ProcessingOptions": {
    "FileFilter": "*.pdf",
    "FileReadyTimeoutMs": 5000,
    "FileReadyRetries": 10,
    "ExtractPdfToolPath": "tools/ExtractPdfText/bin/Debug/net8.0/ExtractPdfText.exe",
    "ExtractPdfTimeoutMs": 300000
  },
  "ChunkingOptions": {
    "TargetChunkSize": 4000,
    "MinChunkSize": 1000,
    "MaxChunkSize": 8000,
    "SplitPattern": "\\n\\n"
  },
  "NotificationOptions": {
    "EnableNotifications": true,
    "MaxNotificationsStored": 1000
  }
}
```

---

## Configuration Validation

### **Startup Checks** (Future)

```csharp
public void ValidateConfiguration(WorkspaceOptions options)
{
    if (string.IsNullOrWhiteSpace(options.RuntimeRoot))
        throw new InvalidOperationException("RuntimeRoot is required");
    
    if (!Directory.Exists(options.RuntimeRoot))
        Directory.CreateDirectory(options.RuntimeRoot);
    
    // Validate writable
    var testFile = Path.Combine(options.RuntimeRoot, ".test");
    File.WriteAllText(testFile, "test");
    File.Delete(testFile);
}
```

---

## Summary

**Configured Parameters**: 4 (in appsettings.json)

**Hard-Coded Parameters**: 8 (requires code changes)

**Future Configuration Needs**: 12 (move to appsettings.json)

**Configuration Files**: 1 (appsettings.json)

**Environment Overrides**: Supported (appsettings.{Environment}.json)
