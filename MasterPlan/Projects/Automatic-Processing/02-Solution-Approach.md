# Solution Approach - Automatic Processing

## Core Strategy

**FileSystemWatcher + Background Service + Dual-Write Pattern**

Monitor the `01-Inbox/` folder continuously. When a PDF appears, automatically execute the full processing pipeline without user intervention.

---

## Key Design Decisions

### 1. FileSystemWatcher (Not Polling)
- **Why**: Real-time detection, zero CPU when idle
- **Alternative Rejected**: Scheduled polling (wastes resources, delays processing)

### 2. Background Hosted Service (Not On-Demand)
- **Why**: Runs continuously, instant response to file drops
- **Alternative Rejected**: API-triggered processing (requires manual action)

### 3. Dual-Write Pattern (Database + Filesystem)
- **Why**: Database for querying, filesystem for raw content preservation
- **Alternative Rejected**: Database-only (loses raw chunks, harder to debug)

### 4. External Tool for Extraction (iText7 via ExtractPdfText.exe)
- **Why**: Isolated process, can't crash main app
- **Alternative Rejected**: In-process extraction (memory leaks, crashes)

### 5. Notifications Table (Not Just Logs)
- **Why**: Queryable history, unread tracking, API access
- **Alternative Rejected**: File logs only (hard to query, no UI integration)

---

## Architecture

```
┌─────────────────────────────────────────────────────────────┐
│                   User Drops PDF                            │
│                  01-Inbox/document.pdf                      │
└─────────────────────┬───────────────────────────────────────┘
                      │
                      ▼
┌─────────────────────────────────────────────────────────────┐
│            FileSystemWatcher Detects File                   │
│       (BackgroundProcessingService.OnCreated)               │
└─────────────────────┬───────────────────────────────────────┘
                      │
                      ▼
┌─────────────────────────────────────────────────────────────┐
│              Wait For File Ready (5s timeout)               │
│         (Handle copy-in-progress, locked files)             │
└─────────────────────┬───────────────────────────────────────┘
                      │
                      ▼
┌─────────────────────────────────────────────────────────────┐
│         Extract PDF → Text (ExtractPdfText.exe)             │
│         Output: 01-Inbox/document.pdf.txt                   │
└─────────────────────┬───────────────────────────────────────┘
                      │
                      ▼
┌─────────────────────────────────────────────────────────────┐
│            Register Source (SourceRegistryService)          │
│  INSERT Sources, SourceFiles → Returns SourceId (Guid)     │
└─────────────────────┬───────────────────────────────────────┘
                      │
                      ▼
┌─────────────────────────────────────────────────────────────┐
│      Chunk Source (ProcessingPipelineService)               │
│  • Read text file                                           │
│  • Split into semantic chunks (~4000 chars)                 │
│  • Save to database (Chunks table)                          │
│  • Write to filesystem (03-Chunked/{sourceKey}/)            │
└─────────────────────┬───────────────────────────────────────┘
                      │
                      ▼
┌─────────────────────────────────────────────────────────────┐
│      Create Notification (ProcessingNotification)           │
│  • Status: Success/Failed                                   │
│  • Metrics: ChunksCreated, CharactersProcessed, TimeMs      │
│  • Save to ProcessingNotifications table                    │
└─────────────────────┬───────────────────────────────────────┘
                      │
                      ▼
┌─────────────────────────────────────────────────────────────┐
│                 Processing Complete                         │
│          User can query /api/processing/notifications       │
└─────────────────────────────────────────────────────────────┘
```

---

## Error Handling Strategy

1. **File Lock Retry**: Wait up to 5 seconds for file copy to complete
2. **Extraction Failure**: Log error, create Failed notification, continue service
3. **Chunking Failure**: Log error, update notification status, continue service
4. **Database Failure**: Log error, stop processing (data integrity critical)
5. **Duplicate Detection**: Compare SourceKey (hash), skip if exists

---

## Scalability Approach

- **Single file**: Processes immediately
- **Multiple files**: Queued naturally by filesystem events
- **Large file**: Extraction happens in external process (won't block service)
- **Concurrent drops**: FileSystemWatcher queues events sequentially

---

## Why This Works

1. **Zero friction** - User action: drop file. System action: everything else.
2. **Reliable** - Background service restarts with app, no lost files
3. **Observable** - Notifications table provides full history
4. **Debuggable** - Dual-write means raw chunks always available
5. **Extensible** - Add more file types by extending extraction logic

---

## Trade-offs Accepted

- **Security**: Files in monitored folder are implicitly trusted
- **Performance**: Sequential processing (not parallel) for simplicity
- **Storage**: Dual-write uses more disk space for reliability
- **Complexity**: External extraction tool requires separate build/deploy

---

## Future Enhancements (Not MVP)

- Parallel processing queue
- Progress updates during long extractions
- Preview before processing
- Configurable chunking strategies
- Automatic duplicate merging
