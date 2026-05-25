# Workflow - Automatic Processing

## 6-Step Processing Pipeline

---

### **Step 1: File Detection**

**Trigger**: FileSystemWatcher detects `Created` or `Changed` event on `*.pdf` in `01-Inbox/`

**Code Location**: `BackgroundProcessingService.OnCreated()`

**Actions**:
- Capture file path
- Log detection with 🔄 emoji
- Queue for processing

**Console Output**:
```
[15:42:13 INF] 🔄 Detected new file: C:\me_workspaces_runtime\01-Inbox\document.pdf
[15:42:13 INF] 🔄 Processing started for: document.pdf
```

---

### **Step 2: File Ready Check**

**Purpose**: Ensure file copy is complete (large files take time)

**Code Location**: `BackgroundProcessingService.WaitForFileReadyAsync()`

**Logic**:
```csharp
for (int i = 0; i < maxRetries; i++)
{
    try
    {
        using var stream = File.Open(path, FileMode.Open, FileAccess.Read, FileShare.None);
        return; // Success
    }
    catch (IOException)
    {
        await Task.Delay(retryDelayMs);
    }
}
```

**Timeout**: 5 seconds (10 retries × 500ms)

**Failure**: Logs error, skips file

---

### **Step 3: PDF Text Extraction**

**Tool**: `ExtractPdfText.exe` (iText7-based external process)

**Code Location**: `BackgroundProcessingService.ExtractPdfTextAsync()`

**Command**:
```powershell
C:\me_workspace\tools\ExtractPdfText\bin\Debug\net8.0\ExtractPdfText.exe "C:\me_workspaces_runtime\01-Inbox\document.pdf"
```

**Output**: `C:\me_workspaces_runtime\01-Inbox\document.pdf.txt`

**Console Output**:
```
[15:42:13 INF] 📄 Extracting text from: document.pdf
[15:42:14 INF] ✓ Extracted text to: document.pdf.txt
```

**Error Handling**: If extraction fails, logs error and returns null (skips remaining steps)

---

### **Step 4: Source Registration**

**Service**: `SourceRegistryService`

**Code Location**: `BackgroundProcessingService.RegisterSourceAsync()`

**Database Writes**:
```sql
INSERT INTO Sources (Id, SourceKey, Title, SourceType, RightsLabel, 
                     OriginalRelativePath, CurrentStage, Status, 
                     CreatedUtc, UpdatedUtc)
VALUES (...)

INSERT INTO SourceFiles (Id, SourceId, RelativePath, FileRole, CreatedUtc)
VALUES (...)  -- Once for PDF
VALUES (...)  -- Once for TXT
```

**SourceKey Generation**: `src-{yyyyMMdd}-{HHmmss}-{random5chars}`
- Example: `src-20260525-144139-49d09a`

**Console Output**:
```
[15:42:14 INF] 📝 Registered source: document.pdf
[15:42:14 INF] ✓ Source ID: 8f3c2d1a-9b4e-4f6a-8c7d-5e9f0a1b2c3d
```

**Returns**: `Guid sourceId` for next step

---

### **Step 5: Chunking**

**Service**: `ProcessingPipelineService`

**Code Location**: `BackgroundProcessingService.ChunkSourceAsync()`

**Process**:
1. Read text file
2. Split into semantic chunks (~4000 characters)
3. **Dual Write**:
   - Database: INSERT into `Chunks` table
   - Filesystem: Write to `03-Chunked/{sourceKey}/chunk-{index}.txt`

**Database Write**:
```sql
INSERT INTO Chunks (Id, SourceId, ChunkIndex, SectionTitle, PageReference,
                   TextPath, CharacterCount, TokenCount, Status, CreatedUtc)
VALUES (...) -- Repeated for each chunk
```

**Filesystem Write**:
```
03-Chunked/
  src-20260525-144139-49d09a/
    chunk-000.txt
    chunk-001.txt
    chunk-002.txt
    ...
```

**Console Output**:
```
[15:42:14 INF] ✂️ Chunking source: document.pdf
[15:42:15 INF] ✅ Created 51 chunks (136749 characters)
[15:42:15 INF] Saved 51 chunks to database and C:\me_workspaces_runtime\03-Chunked\src-20260525-144139-49d09a
```

**Metrics Returned**:
```csharp
{
    Success = true,
    ChunksCreated = 51,
    CharactersProcessed = 136749,
    ElapsedMs = 1200
}
```

---

### **Step 6: Notification Creation**

**Service**: Direct database INSERT via `AppDbContext`

**Code Location**: `BackgroundProcessingService.SendNotificationAsync()`

**Database Write**:
```sql
INSERT INTO ProcessingNotifications 
(Id, SourceId, FileName, Status, Message, ChunksCreated, 
 CharactersProcessed, ProcessingTimeMs, CreatedUtc, IsRead)
VALUES 
(GUID, sourceId, 'document.pdf', 'Success', NULL, 51, 136749, 1200, UTC_NOW, 0)
```

**Console Output**:
```
[15:42:15 INF] 📢 Notification saved for: document.pdf
[15:42:15 INF] Processing complete! ✅
[15:42:15 INF] 📊 Stats: 51 chunks, 136749 chars, 1200ms
```

**API Access**: `GET /api/processing/notifications?limit=10`

**Response Example**:
```json
{
  "notifications": [
    {
      "id": "...",
      "sourceId": "8f3c2d1a-9b4e-4f6a-8c7d-5e9f0a1b2c3d",
      "fileName": "document.pdf",
      "status": "Success",
      "chunksCreated": 51,
      "charactersProcessed": 136749,
      "processingTimeMs": 1200,
      "createdUtc": "2026-05-25T15:42:15Z",
      "isRead": false
    }
  ]
}
```

---

## Complete Console Log Example

```
[15:42:13 INF] 🔄 Detected new file: C:\me_workspaces_runtime\01-Inbox\Ethiopian Bible Translations.pdf
[15:42:13 INF] 🔄 Processing started for: Ethiopian Bible Translations.pdf
[15:42:13 INF] 📄 Extracting text from: Ethiopian Bible Translations.pdf
[15:42:14 INF] ✓ Extracted text to: Ethiopian Bible Translations.pdf.txt
[15:42:14 INF] 📝 Registered source: Ethiopian Bible Translations.pdf
[15:42:14 INF] ✓ Source ID: 8f3c2d1a-9b4e-4f6a-8c7d-5e9f0a1b2c3d
[15:42:14 INF] ✂️ Chunking source: Ethiopian Bible Translations.pdf
[15:42:15 INF] ✅ Created 51 chunks (136749 characters)
[15:42:15 INF] Saved 51 chunks to database and C:\me_workspaces_runtime\03-Chunked\src-20260525-144139-49d09a
[15:42:15 INF] 📢 Notification saved for: Ethiopian Bible Translations.pdf
[15:42:15 INF] Processing complete! ✅
[15:42:15 INF] 📊 Stats: 51 chunks, 136749 chars, 1200ms
```

---

## Error Scenarios

### **Extraction Failure**
```
[15:42:13 INF] 🔄 Detected new file: corrupted.pdf
[15:42:13 INF] 📄 Extracting text from: corrupted.pdf
[15:42:14 ERR] ❌ PDF extraction failed: Invalid PDF structure
[15:42:14 INF] 📢 Notification saved with status: Failed
```

### **File Locked**
```
[15:42:13 INF] 🔄 Detected new file: large-file.pdf
[15:42:13 INF] Waiting for file to be ready...
[15:42:13 INF] Retry 1/10...
[15:42:14 INF] Retry 2/10...
[15:42:14 INF] ✓ File ready
[15:42:14 INF] 📄 Extracting text from: large-file.pdf
```

### **Database Failure**
```
[15:42:15 INF] ✅ Created 51 chunks (136749 characters)
[15:42:15 ERR] ❌ Failed to save notification: SQLite Error 1: 'no such table: ProcessingNotifications'
```

---

## User Workflow

1. **Drop PDF** into `C:\me_workspaces_runtime\01-Inbox\`
2. **Watch console** for emoji-based progress indicators
3. **Query API** at `http://127.0.0.1:5078/api/processing/notifications`
4. **View chunks** in `03-Chunked/{sourceKey}/` folders
5. **Query database** for Sources, Chunks, and Notifications

---

## Performance Characteristics

- **Small PDF** (10 pages): ~2 seconds total
- **Medium PDF** (50 pages): ~5 seconds total
- **Large PDF** (200 pages): ~15 seconds total
- **Multiple files**: Sequential processing, ~3-5 seconds per file

**Bottleneck**: PDF extraction (external process spawn)

**Optimization**: Chunking and database writes are fast (<500ms)
