# UI Design - Automatic Processing

## Design Philosophy

**"Invisible by design, visible when needed"**

- **Primary UI**: None (automatic processing)
- **Secondary UI**: Console logs (for developers/power users)
- **Tertiary UI**: API endpoints (for programmatic access)
- **Future UI**: Notification panel (web interface)

---

## Current UI: Console Output

### **Layout**

```
┌─────────────────────────────────────────────────────────────────┐
│  Terminal Window (PowerShell / CMD)                             │
├─────────────────────────────────────────────────────────────────┤
│ [15:42:13 INF] Workspace runtime folders ready                  │
│ [15:42:13 INF] Starting background file watcher                 │
│ [15:42:13 INF] Background file watcher started. Monitoring...   │
│ [15:42:13 INF] Now listening on: http://127.0.0.1:5078          │
│                                                                  │
│ [15:42:20 INF] 🔄 Detected new file: document.pdf               │
│ [15:42:20 INF] 🔄 Processing started for: document.pdf          │
│ [15:42:20 INF] 📄 Extracting text from: document.pdf            │
│ [15:42:21 INF] ✓ Extracted text to: document.pdf.txt            │
│ [15:42:21 INF] 📝 Registered source: document.pdf               │
│ [15:42:21 INF] ✂️ Chunking source...                            │
│ [15:42:22 INF] ✅ Created 51 chunks (136749 characters)         │
│ [15:42:22 INF] Saved 51 chunks to database and filesystem      │
│ [15:42:22 INF] 📢 Notification saved for: document.pdf          │
│ [15:42:22 INF] Processing complete! ✅                           │
│ [15:42:22 INF] 📊 Stats: 51 chunks, 136749 chars, 1200ms       │
└─────────────────────────────────────────────────────────────────┘
```

### **Visual Elements**

**Emoji Convention**:
- 🔄 Blue (Starting) - Information
- 📄 Yellow (Extracting) - In-progress
- ✓ Green (Success) - Completion
- ✂️ Blue (Chunking) - Processing
- ✅ Green (Complete) - Success
- 📊 Blue (Stats) - Summary
- 📢 Purple (Notification) - Record
- ❌ Red (Error) - Failure

**Timestamp**: `[HH:mm:ss INF]` format (24-hour clock)

**Level**: `INF` (Info), `WRN` (Warning), `ERR` (Error)

---

## Future UI: Web Notification Panel

### **Mockup (Planned)**

```
┌─────────────────────────────────────────────────────────────────┐
│  Processing Notifications                           [Mark All Read]│
├─────────────────────────────────────────────────────────────────┤
│  ● document.pdf                                     2 min ago    │
│    Success - 51 chunks created (136,749 characters)             │
│    [View Chunks]  [View Source]  [Mark Read]                    │
├─────────────────────────────────────────────────────────────────┤
│  ● Ethiopian Bible Translations.pdf                 15 min ago   │
│    Success - 51 chunks created (136,749 characters)             │
│    [View Chunks]  [View Source]  [Mark Read]                    │
├─────────────────────────────────────────────────────────────────┤
│  ● Rolex Watch Manufacturing Cost.pdf               1 hour ago   │
│    Success - 31 chunks created (81,231 characters)              │
│    [View Chunks]  [View Source]  [Mark Read]                    │
├─────────────────────────────────────────────────────────────────┤
│  ● Observation 3.pdf                                 2 hours ago  │
│    Success - 298 chunks created (808,552 characters)            │
│    [View Chunks]  [View Source]  [Mark Read]                    │
├─────────────────────────────────────────────────────────────────┤
│  [Load More]                                        Page 1 of 5  │
└─────────────────────────────────────────────────────────────────┘
```

### **Component Breakdown**

**Notification Card**:
- **Status Indicator**: ● (green = success, red = failed, yellow = processing)
- **File Name**: Bold, clickable
- **Status Message**: "Success - X chunks created (Y characters)"
- **Timestamp**: Relative time ("2 min ago")
- **Actions**: View Chunks, View Source, Mark Read

**Filter Bar** (Future):
- All / Unread Only / Success Only / Failed Only
- Date Range Picker
- Search by filename

---

## User Workflows

### **Workflow 1: Drop and Forget** (Primary)

```
User Action                 System Response             User Sees
───────────────────────────────────────────────────────────────────
Drop PDF in 01-Inbox/       Detect file immediately     Nothing (automatic)
                            Extract text                Nothing (background)
                            Chunk content               Nothing (background)
                            Save to database            Nothing (background)
                            Create notification         Nothing (background)

(Optional) Check console    View emoji logging          ✅ Success
(Optional) Query API        View notification JSON      200 OK
```

**User Effort**: 1 second (drop file)  
**System Effort**: 3-5 seconds (processing)  
**User Visibility**: Optional (can ignore)

---

### **Workflow 2: Check Status** (Secondary)

```
User Action                 System Response             User Sees
───────────────────────────────────────────────────────────────────
Open browser                                            
Navigate to:                GET /api/processing/        JSON array:
/api/processing/            notifications               [
notifications                                             { "fileName": "doc.pdf",
                                                           "status": "Success",
                                                           "chunksCreated": 51 }
                                                        ]
```

**User Effort**: 10 seconds (manual query)  
**Frequency**: As needed (not required)

---

### **Workflow 3: Investigate Failure** (Error Handling)

```
User Action                 System Response             User Sees
───────────────────────────────────────────────────────────────────
Drop corrupted PDF          Detect file                 🔄 Detected
                            Extraction fails            ❌ Extraction failed
                            Create Failed notification  📢 Notification (Failed)

Check console               View error log              [ERR] PDF extraction failed
                                                        Invalid PDF structure

Query API                   GET /api/processing/        { "status": "Failed",
                            notifications?              "message": "Invalid PDF" }
                            unreadOnly=true
```

**User Effort**: 20 seconds (investigate error)  
**Frequency**: Rare (<5% of files)

---

## API Interface (Current UI)

### **Endpoint: GET /api/processing/notifications**

**Request**:
```http
GET /api/processing/notifications?limit=10&unreadOnly=false
Host: 127.0.0.1:5078
```

**Response**:
```json
{
  "notifications": [
    {
      "id": "8f3c2d1a-9b4e-4f6a-8c7d-5e9f0a1b2c3d",
      "sourceId": "7e2d3c1b-8a5f-4e7a-9c6d-4f8e0b1a2c3d",
      "fileName": "document.pdf",
      "status": "Success",
      "message": null,
      "chunksCreated": 51,
      "charactersProcessed": 136749,
      "processingTimeMs": 1200,
      "createdUtc": "2026-05-25T15:42:22Z",
      "isRead": false
    }
  ]
}
```

### **Endpoint: POST /api/processing/notifications/{id}/mark-read**

**Request**:
```http
POST /api/processing/notifications/8f3c2d1a-9b4e-4f6a-8c7d-5e9f0a1b2c3d/mark-read
Host: 127.0.0.1:5078
```

**Response**:
```http
200 OK
```

---

## Design Principles

### **1. Zero Attention Required**

**Principle**: Default behavior should require no user attention

**Implementation**:
- No pop-ups
- No required acknowledgments
- Console logging is opt-in (can ignore)

**Rationale**: User dropped file to process it, not to interact with processing

---

### **2. Information on Demand**

**Principle**: Status available when user wants it

**Implementation**:
- Console: Real-time if watching
- API: Historical if querying
- Future UI: Dashboard if exploring

**Rationale**: Power users want details, casual users don't

---

### **3. Emoji > Text**

**Principle**: Visual scanning faster than text parsing

**Implementation**:
- 🔄 = Currently processing
- ✅ = Success (scan for this)
- ❌ = Error (scan for this)

**Rationale**: Human eye detects symbols faster than words

---

### **4. Progressive Disclosure**

**Principle**: Show summary first, details on request

**Implementation**:
- Console: Single line per step
- Notification: High-level status
- API: Full details available

**Rationale**: Most users need "did it work?" not "how did it work?"

---

## Accessibility Considerations

### **Current (Console)**

**Limitations**:
- Emoji may not render in all terminals
- Color-blind users can't rely on emoji color
- Screen readers may not read emoji

**Mitigations**:
- Emoji + text (redundant information)
- Status words ("Success", "Failed") always present
- Log levels (INF, WRN, ERR) machine-readable

---

### **Future (Web UI)**

**Requirements**:
- ARIA labels on all interactive elements
- Keyboard navigation (no mouse required)
- High contrast mode support
- Screen reader announcements for new notifications

---

## Performance Considerations

### **Console Logging**

- **Overhead**: <1ms per log statement
- **Impact**: Negligible (processing takes seconds)
- **Optimization**: Structured logging (Serilog) is async

### **API Queries**

- **Response Time**: <50ms for 100 notifications
- **Index Usage**: `CreatedUtc` and `IsRead` indexed
- **Pagination**: `LIMIT` clause prevents large result sets

---

## Visual Design Language

### **Current**

**Colors** (Console):
- Information: Default (white/gray)
- Success: Green emoji (✅, ✓)
- Processing: Blue emoji (🔄, ✂️)
- Error: Red emoji (❌)

**Typography** (Console):
- Font: Monospaced (Consolas, Courier New)
- Size: Terminal default
- Weight: Regular

---

### **Future Web UI**

**Colors**:
- Primary: #007BFF (Blue - Information)
- Success: #28A745 (Green - Completed)
- Warning: #FFC107 (Yellow - Processing)
- Danger: #DC3545 (Red - Failed)
- Background: #F8F9FA (Light Gray)

**Typography**:
- Heading: Inter, 18px, Bold
- Body: Inter, 14px, Regular
- Code: Fira Code, 12px, Monospaced

**Spacing**:
- Card Padding: 16px
- Card Margin: 12px
- Button Padding: 8px 16px

---

## Responsive Design (Future)

### **Desktop** (>1024px)
- Two-column layout (notifications + details)
- Show 10 notifications per page
- Full metadata visible

### **Tablet** (768px - 1024px)
- Single column
- Show 8 notifications per page
- Abbreviated metadata

### **Mobile** (< 768px)
- Single column
- Show 5 notifications per page
- Minimal metadata (filename + status only)

---

## Interaction Patterns

### **Current (API-Only)**

- REST endpoints (GET, POST)
- JSON responses
- HTTP status codes (200, 404, 500)

### **Future (Web UI)**

- Click notification → View details
- Mark as read → Update status
- View chunks → Navigate to source
- Filter by status → Client-side or API query

---

## Design Constraints

### **Technical**

- Must work without JavaScript (API access)
- Must work in terminal (console logging)
- Must be machine-readable (JSON)

### **UX**

- No pop-ups (don't interrupt)
- No required actions (fully automatic)
- No learning curve (drop file = done)

---

## Success Metrics

**UI Quality**:
- User can determine status in <2 seconds
- Error investigation takes <30 seconds
- Zero UI-related support questions

**Usability**:
- No training required
- No documentation needed (self-explanatory)
- Works first time every time
