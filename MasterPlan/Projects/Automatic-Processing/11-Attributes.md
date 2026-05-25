# Attributes - Automatic Processing

## Data Model

### **ProcessingNotification Entity**

```csharp
public class ProcessingNotification
{
    public Guid Id { get; set; }                    // Primary key
    public Guid? SourceId { get; set; }             // Foreign key to Sources
    public string FileName { get; set; }            // Original PDF name
    public string Status { get; set; }              // "Success" | "Failed" | "Processing"
    public string? Message { get; set; }            // Error message (if failed)
    public int? ChunksCreated { get; set; }         // Number of chunks generated
    public long? CharactersProcessed { get; set; }   // Total character count
    public long? ProcessingTimeMs { get; set; }      // Processing duration
    public DateTime CreatedUtc { get; set; }        // When notification created
    public bool IsRead { get; set; }                // User has acknowledged
    
    // Navigation property
    public Source? Source { get; set; }
}
```

### **Attribute Specifications**

| Attribute           | Type     | Required | Max Length | Default   | Indexed        |
| ------------------- | -------- | -------- | ---------- | --------- | -------------- |
| Id                  | Guid     | ✅        | -          | NewGuid() | Primary Key    |
| SourceId            | Guid?    | ❌        | -          | null      | ✅ FK Index     |
| FileName            | string   | ✅        | 300        | -         | ❌              |
| Status              | string   | ✅        | 32         | -         | ❌              |
| Message             | string?  | ❌        | 2000       | null      | ❌              |
| ChunksCreated       | int?     | ❌        | -          | null      | ❌              |
| CharactersProcessed | long?    | ❌        | -          | null      | ❌              |
| ProcessingTimeMs    | long?    | ❌        | -          | null      | ❌              |
| CreatedUtc          | DateTime | ✅        | -          | UtcNow    | ✅ Range Query  |
| IsRead              | bool     | ✅        | -          | false     | ✅ Filter Index |

---

## Attribute Details

### **Id** (Primary Key)

**Purpose**: Unique identifier for notification

**Type**: `Guid`

**Generation**: `Guid.NewGuid()` on creation

**Format**: `xxxxxxxx-xxxx-xxxx-xxxx-xxxxxxxxxxxx`

**Example**: `8f3c2d1a-9b4e-4f6a-8c7d-5e9f0a1b2c3d`

**Constraints**:
- Must be unique
- Cannot be null
- Immutable after creation

---

### **SourceId** (Foreign Key)

**Purpose**: Links notification to processed source

**Type**: `Guid?` (nullable)

**Relationship**: Many-to-One with `Sources` table

**Cascade**: `ON DELETE CASCADE` (delete notifications when source deleted)

**Nullable**: Yes (if source registration fails)

**Example**: `7e2d3c1b-8a5f-4e7a-9c6d-4f8e0b1a2c3d`

**Constraints**:
- Must reference existing `Sources.Id` (if not null)
- Indexed for performance (frequent joins)

---

### **FileName**

**Purpose**: Display name of processed file

**Type**: `string`

**Max Length**: 300 characters

**Required**: Yes

**Example**: `"Ethiopian Bible Translations.pdf"`

**Constraints**:
- Cannot be null or empty
- Includes file extension
- Extracted from original file path

**Rationale**: User needs to know which file was processed without querying `Sources` table

---

### **Status**

**Purpose**: Processing outcome

**Type**: `string` (enum-like)

**Max Length**: 32 characters

**Required**: Yes

**Valid Values**:
- `"Success"` - Processing completed successfully
- `"Failed"` - Processing encountered error
- `"Processing"` - Currently in progress (future use)

**Example**: `"Success"`

**Constraints**:
- Cannot be null
- Should use defined constants (not free-form)
- Case-sensitive

**Future Enhancement**: Convert to enum type for type safety

---

### **Message**

**Purpose**: Error details or additional context

**Type**: `string?` (nullable)

**Max Length**: 2000 characters

**Required**: No

**Example**: `"PDF extraction failed: Invalid PDF structure at page 42"`

**Nullable**: Yes (only populated on errors)

**Constraints**:
- Null for successful processing
- Contains error message for failed processing
- Truncated if exceeds 2000 characters

---

### **ChunksCreated**

**Purpose**: Number of chunks generated

**Type**: `int?` (nullable)

**Required**: No

**Example**: `51`

**Nullable**: Yes (null if processing failed before chunking)

**Range**: 0 - 10000 (realistic upper bound)

**Constraints**:
- Non-negative
- Null if chunking didn't occur

**Use Cases**:
- Quality metrics (average chunks per source)
- Debugging (unexpectedly high/low chunk count)

---

### **CharactersProcessed**

**Purpose**: Total character count of extracted text

**Type**: `long?` (nullable)

**Required**: No

**Example**: `136749`

**Nullable**: Yes (null if extraction failed)

**Range**: 0 - 100,000,000 (100MB text file)

**Constraints**:
- Non-negative
- Null if extraction didn't occur

**Use Cases**:
- Size estimates
- Processing time correlation
- Storage calculations

---

### **ProcessingTimeMs**

**Purpose**: Total processing duration in milliseconds

**Type**: `long?` (nullable)

**Required**: No

**Example**: `1200` (1.2 seconds)

**Nullable**: Yes (null if processing didn't complete)

**Range**: 0 - 3,600,000 (1 hour max)

**Constraints**:
- Non-negative
- Measures full pipeline (extraction → chunking → notification)

**Use Cases**:
- Performance monitoring
- Bottleneck identification
- SLA tracking

---

### **CreatedUtc**

**Purpose**: Timestamp of notification creation

**Type**: `DateTime`

**Required**: Yes

**Format**: UTC (Coordinated Universal Time)

**Example**: `2026-05-25T15:42:22.1234567Z`

**Constraints**:
- Cannot be null
- Always UTC (never local time)
- Indexed for sorting/filtering

**Use Cases**:
- Recent notifications query
- Historical analysis
- Audit trail

**Index**: Clustered or non-clustered for `ORDER BY CreatedUtc DESC`

---

### **IsRead**

**Purpose**: User acknowledgment status

**Type**: `bool`

**Required**: Yes

**Default**: `false`

**Example**: `true`

**Constraints**:
- Cannot be null
- Defaults to false on creation

**Use Cases**:
- Unread notification count
- Filter unread only
- Notification badge in UI

**Index**: Supports `WHERE IsRead = false` queries efficiently

---

## Computed Attributes (Not Stored)

### **RelativeTime** (UI Display)

**Purpose**: Human-readable time ago

**Computation**: `CreatedUtc` compared to `DateTime.UtcNow`

**Examples**:
- "2 minutes ago"
- "1 hour ago"
- "yesterday"
- "May 25, 2026"

**Format**:
- < 1 min: "just now"
- < 60 min: "X minutes ago"
- < 24 hours: "X hours ago"
- < 7 days: "X days ago"
- \>= 7 days: Full date

---

### **StatusColor** (UI Display)

**Purpose**: Visual indicator color

**Mapping**:
- `"Success"` → Green (#28A745)
- `"Failed"` → Red (#DC3545)
- `"Processing"` → Yellow (#FFC107)

---

### **StatusIcon** (UI Display)

**Purpose**: Visual status symbol

**Mapping**:
- `"Success"` → ✅
- `"Failed"` → ❌
- `"Processing"` → 🔄

---

## Attribute Relationships

### **Foreign Key: SourceId → Sources.Id**

**Relationship**: Many ProcessingNotifications → One Source

**Cardinality**: `0..*` (zero or more notifications per source)

**Delete Behavior**: `CASCADE` (delete notifications when source deleted)

**Query Pattern**:
```csharp
var notifications = await db.ProcessingNotifications
    .Include(n => n.Source)
    .Where(n => n.SourceId == sourceId)
    .ToListAsync();
```

---

## Validation Rules

### **Business Rules**

1. **Status Consistency**:
   - If `Status == "Success"`, `ChunksCreated` should be > 0
   - If `Status == "Failed"`, `Message` should be populated
   - If `Status == "Processing"`, `ChunksCreated` should be null

2. **Metric Validity**:
   - `ChunksCreated` ≥ 0
   - `CharactersProcessed` ≥ 0
   - `ProcessingTimeMs` > 0 (if not null)

3. **Timestamp Validity**:
   - `CreatedUtc` ≤ `DateTime.UtcNow` (can't be in future)

---

## Database Schema (SQL)

```sql
CREATE TABLE ProcessingNotifications (
    Id TEXT NOT NULL PRIMARY KEY,
    SourceId TEXT NULL,
    FileName TEXT NOT NULL,
    Status TEXT NOT NULL,
    Message TEXT NULL,
    ChunksCreated INTEGER NULL,
    CharactersProcessed INTEGER NULL,
    ProcessingTimeMs INTEGER NULL,
    CreatedUtc TEXT NOT NULL,
    IsRead INTEGER NOT NULL DEFAULT 0,
    
    FOREIGN KEY (SourceId) 
        REFERENCES Sources(Id) 
        ON DELETE CASCADE
);

CREATE INDEX IX_ProcessingNotifications_SourceId 
    ON ProcessingNotifications(SourceId);

CREATE INDEX IX_ProcessingNotifications_CreatedUtc 
    ON ProcessingNotifications(CreatedUtc);

CREATE INDEX IX_ProcessingNotifications_IsRead 
    ON ProcessingNotifications(IsRead);
```

---

## Attribute Evolution

### **Phase 1** (MVP - Current)

✅ Core attributes implemented  
✅ Basic indexing  
✅ Foreign key relationship

### **Phase 2** (Future)

⏳ Add `ProcessingStage` (extraction, chunking, summarization)  
⏳ Add `ErrorCode` (enum for error types)  
⏳ Add `RetryCount` (track auto-retry attempts)  
⏳ Add `Priority` (queue management)

### **Phase 3** (Future)

⏳ Add `UserId` (multi-user support)  
⏳ Add `Tags` (categorization)  
⏳ Add `Attachments` (screenshots, previews)

---

## Performance Considerations

### **Index Strategy**

**Primary Index**: `Id` (clustered)

**Foreign Key Index**: `SourceId` (non-clustered)
- Supports JOIN queries
- Enables cascading deletes efficiently

**Range Query Index**: `CreatedUtc` (non-clustered)
- Supports `ORDER BY CreatedUtc DESC`
- Enables date range filters

**Filter Index**: `IsRead` (non-clustered)
- Supports `WHERE IsRead = false`
- Small cardinality (only 2 values), still useful for frequent query

### **Query Optimization**

**Most Common Query**:
```sql
SELECT * FROM ProcessingNotifications
WHERE IsRead = 0
ORDER BY CreatedUtc DESC
LIMIT 10;
```

**Index Usage**: `IX_ProcessingNotifications_IsRead` + `IX_ProcessingNotifications_CreatedUtc`

**Estimated Rows**: 10-100 (typical unread count)

**Performance**: <10ms

---

## Security Considerations

### **Sensitive Attributes**

**FileName**: May reveal private information (e.g., "Medical Records.pdf")
- Mitigation: Local-only database, no cloud exposure

**Message**: May contain file paths with username
- Mitigation: Sanitize paths before storing

### **Non-Sensitive Attributes**

- Id, SourceId, Status, Metrics, Timestamps
- Safe to log or display in UI

---

## Serialization

### **JSON Output**

```json
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
```

**Naming Convention**: camelCase for JSON (C# uses PascalCase internally)

**Null Handling**: Omit null fields in JSON (not `"message": null`)

---

## Summary

**Total Attributes**: 10

**Required**: 5 (Id, FileName, Status, CreatedUtc, IsRead)

**Optional**: 5 (SourceId, Message, ChunksCreated, CharactersProcessed, ProcessingTimeMs)

**Indexed**: 4 (Id, SourceId, CreatedUtc, IsRead)

**Foreign Keys**: 1 (SourceId → Sources.Id)

**Purpose**: Track automatic processing history with queryable metrics
