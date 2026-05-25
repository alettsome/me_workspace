# Chunking - Metadata Schema

## Database Schema

### Source Table
```csharp
public sealed class Source
{
    public Guid Id { get; set; }
    public Guid? ProjectId { get; set; }
    public string SourceKey { get; set; }         // e.g., "src-20260525-120545-ff014e"
    public string Title { get; set; }             // e.g., "Manhood Device Development"
    public string SourceType { get; set; }        // "Pdf", "Text", "conversation-log"
    public string? Author { get; set; }
    public string? ISBN { get; set; }
    public string? URL { get; set; }
    public string? Publisher { get; set; }
    public int? PublicationYear { get; set; }
    public string? BorrowingSource { get; set; }  // "Internet Archive", "Libby"
    public DateTime? AccessExpiryUtc { get; set; }
    public string RightsLabel { get; set; }       // "public-domain", "owned", "borrowed"
    public string OriginalRelativePath { get; set; }
    public string CurrentStage { get; set; }      // "Inbox" → "Extracted" → "Chunked"
    public string Status { get; set; }            // "Queued", "Processing", "Complete"
    public DateTime CreatedUtc { get; set; }
    public DateTime UpdatedUtc { get; set; }
    
    public ICollection<SourceFile> Files { get; set; }
    public ICollection<Chunk> Chunks { get; set; }
}
```

### Chunk Table
```csharp
public sealed class Chunk
{
    public Guid Id { get; set; }
    public Guid SourceId { get; set; }
    public int ChunkIndex { get; set; }           // 0-based position
    public string? SectionTitle { get; set; }     // Extracted heading
    public string? PageReference { get; set; }    // e.g., "1-3"
    public string TextPath { get; set; }          // "03-Chunked/{key}/chunk-0001.txt"
    public int CharacterCount { get; set; }
    public int TokenCount { get; set; }           // Estimated (chars / 4)
    public string Status { get; set; }            // "New", "Processing", "Summarized"
    public DateTime CreatedUtc { get; set; }
    
    public Source? Source { get; set; }
}
```

## What's Captured

### ✅ Currently Captured
- **Source Identity**: ID, Key, Title, Type
- **Chunk Position**: Index, page references
- **Size Metrics**: Character count, estimated tokens
- **File Locations**: Relative paths from runtime root
- **Processing State**: Status, timestamps
- **Provenance**: Source relationship, original file path

### ❌ NOT Captured (Gaps)

#### Missing Keywords/Tags
**Problem:** Cannot filter chunks by topic
**Example:** "Find all chunks about 'database design'"
**Solution:** Add `ChunkTag` table with many-to-many relationship
```csharp
public class ChunkTag
{
    public Guid Id { get; set; }
    public Guid ChunkId { get; set; }
    public string Tag { get; set; }       // "database", "c-sharp", "architecture"
    public string? TagType { get; set; }  // "keyword", "category", "topic"
    public float Confidence { get; set; } // 0.0-1.0
}
```

#### Missing Quality Metrics
**Problem:** Don't know if chunk boundaries are good
**Example:** Chunk split mid-sentence vs. paragraph boundary
**Solution:** Add quality fields to `Chunk`
```csharp
public float? BoundaryQuality { get; set; }  // 0.0-1.0 (1.0 = perfect boundary)
public string? BoundaryType { get; set; }    // "paragraph", "section", "mid-sentence"
public int? ConversationTurnCount { get; set; } // For dialogue chunks
```

#### Missing Speaker Metadata (Conversations)
**Problem:** Know it's a conversation but not who the speakers are
**Example:** "Show all User messages" or "Find Assistant responses about X"
**Solution:** Add `ConversationTurn` table
```csharp
public class ConversationTurn
{
    public Guid Id { get; set; }
    public Guid ChunkId { get; set; }
    public int TurnIndex { get; set; }
    public string Speaker { get; set; }    // "User", "Assistant", "System"
    public string Content { get; set; }
    public DateTime? Timestamp { get; set; }
}
```

#### Missing Chunk Relationships
**Problem:** Don't know how chunks relate to each other
**Example:** "Show chunks before/after this one" or "Find parent section"
**Solution:** Add relationship fields
```csharp
public Guid? PreviousChunkId { get; set; }
public Guid? NextChunkId { get; set; }
public Guid? ParentChunkId { get; set; }    // For hierarchical documents
public int? SectionLevel { get; set; }      // Heading level (1-6)
```

#### Missing Optimization Metadata
**Problem:** Don't track why chunking decisions were made
**Example:** "Show chunks that exceeded target size" or "Find chunks with high overlap"
**Solution:** Add optimization tracking
```csharp
public int OverlapCharacters { get; set; }     // How much overlap with previous
public string ChunkingStrategy { get; set; }   // "conversation", "size-based", "semantic"
public string? RejectionReason { get; set; }   // If chunk quality was poor
public float? ProcessingTimeMs { get; set; }   // Performance tracking
```

## Optimization Parameters

### Current Configuration
```csharp
// ChunkingService.cs
private const int TARGET_TOKEN_COUNT = 700;
private const int TOKEN_OVERLAP = 100;
private const double CHARS_PER_TOKEN = 4.0;

// Speaker detection patterns
private static readonly string[] SpeakerPatterns = {
    @"^User:",
    @"^Assistant:",
    @"^System:",
    @"^ChatGPT:"
};
```

### Where Configuration Should Live
**Problem:** Hardcoded in service class
**Better:** Configuration file or database

**Proposed `appsettings.json`:**
```json
{
  "Chunking": {
    "TargetTokenCount": 700,
    "TokenOverlap": 100,
    "CharsPerToken": 4.0,
    "ConversationPatterns": [
      "^User:",
      "^Assistant:",
      "^System:",
      "^ChatGPT:"
    ]
  }
}
```

**Proposed `ChunkingConfiguration` table:**
```csharp
public class ChunkingConfiguration
{
    public Guid Id { get; set; }
    public string SourceType { get; set; }      // "conversation", "book", "article"
    public int TargetTokenCount { get; set; }
    public int TokenOverlap { get; set; }
    public string? CustomPatterns { get; set; } // JSON array of patterns
}
```

## File System Metadata

### Chunk Files
**Location:** `C:\me_workspaces_runtime\03-Chunked\{sourceKey}\chunk-{index}.txt`

**File Naming Convention:**
- `chunk-0000.txt` - Always 4 digits, zero-padded
- Sequential: 0000, 0001, 0002...
- Max: 9999 chunks per source

**File Content:**
- Plain text
- No headers or footers
- Preserved whitespace from original
- UTF-8 encoding

### Missing File Metadata
**Not Captured:**
- File size (bytes)
- File hash (MD5/SHA256) for deduplication
- Compression applied
- Encoding detected

---

**See also:**
- [README.md](./README.md) - Feature overview
- [technology.md](./technology.md) - Tech stack
- [optimization.md](./optimization.md) - Performance tuning
