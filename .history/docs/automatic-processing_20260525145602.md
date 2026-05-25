# Automatic Background Processing

## Overview

The system automatically processes PDF files dropped into the inbox folder. No manual intervention required - just drop the file and the system handles the rest.

## How It Works

```
1. Drop PDF → 01-Inbox/
2. File Watcher Detects → BackgroundProcessingService
3. Extract Text → ExtractPdfText.exe
4. Register Source → Database
5. Chunk Content → 03-Chunked/
6. Notify Complete → ProcessingNotifications table + Console log
```

## Monitored Location

```
C:\me_workspaces_runtime\01-Inbox\
```

The system watches this folder for `.pdf` files. When a new file is detected:
- Waits for file to finish copying (up to 5 seconds)
- Automatically extracts text using ExtractPdfText tool
- Registers source in database
- Chunks content (conversation-based or size-based)
- Saves chunks to database and file system
- Creates notification record

## Notification System

### Query Notifications

**Get recent notifications:**
```
GET /api/processing/notifications?limit=10
```

**Get unread notifications only:**
```
GET /api/processing/notifications?unreadOnly=true
```

**Mark notification as read:**
```
POST /api/processing/notifications/{id}/mark-read
```

### Example Response

```json
[
  {
    "id": "a1b2c3d4-...",
    "fileName": "Computer Build Summary.pdf",
    "status": "Success",
    "message": "Processed Computer Build Summary",
    "chunksCreated": 73,
    "charactersProcessed": 196115,
    "processingTimeMs": 1234,
    "createdUtc": "2026-05-25T14:30:00Z",
    "isRead": false
  }
]
```

## Console Output

The background service logs all processing steps to the console with emojis for easy monitoring:

```
🔄 Starting automatic processing: Computer Build Summary.pdf
📄 Step 1: Extracting text from PDF...
✓ Text extracted: Computer Build Summary.txt
📝 Step 2: Registering source in database...
✓ Source registered: 29b8fc46-868f-446c-ade8-f4594cc8c6dd
✂️ Step 3: Chunking content...
✅ Processing complete for Computer Build Summary.pdf!
   📊 Stats: 73 chunks, 196115 characters, 1234ms
📢 NOTIFICATION: Computer Build Summary.pdf processed successfully - 73 chunks created
```

## Status Tracking

Each notification has a status:
- **Processing** - Currently being processed (not yet implemented)
- **Success** - Completed successfully
- **Failed** - Error occurred during processing

## Error Handling

If processing fails at any step:
1. Error is logged with details
2. File is removed from processing queue
3. Can be retried by moving file out and back into inbox

## Performance Notes

- **Extraction:** ~1-2 seconds per page
- **Registration:** < 100ms
- **Chunking:** ~1-2 seconds for 200KB of text
- **Total:** Expect 5-10 seconds for a 100-page PDF

## Future Enhancements

Planned features:
- [ ] Real-time UI notifications (SignalR/WebSocket)
- [ ] Progress tracking (show percentage while processing)
- [ ] Batch processing (process multiple files in parallel)
- [ ] Email notifications for completed processing
- [ ] Webhook support for external integrations
- [ ] Retry mechanism for failed processing
- [ ] File type detection and routing (not just PDFs)

## Troubleshooting

**File not processing:**
1. Check console logs for errors
2. Verify ExtractPdfText.exe exists at `tools/ExtractPdfText/bin/Debug/net8.0/`
3. Ensure file is fully copied (not still transferring)
4. Check file permissions

**Duplicate processing:**
The system prevents duplicate processing while a file is being handled. If you see duplicate warnings, it means the file system event fired multiple times (normal behavior).

## Development Notes

**Key Components:**
- `BackgroundProcessingService.cs` - File watcher and orchestration
- `ProcessingNotification.cs` - Database entity for tracking
- `ProcessingEndpoints.cs` - API endpoints for notifications
- `ProcessingPipelineService.cs` - Core processing logic

**Testing:**
```powershell
# Drop a test PDF
Copy-Item "test.pdf" "C:\me_workspaces_runtime\01-Inbox\"

# Watch console for processing logs
# Query notifications
Invoke-WebRequest "http://127.0.0.1:5078/api/processing/notifications?limit=5"
```
