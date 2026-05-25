# Feature: Chunking

**Status:** ✅ Implemented (Phase 6)  
**Last Updated:** May 25, 2026

---

## Problem Definition

### The Pain Point
Large documents (PDFs, text files, conversation logs) cannot be processed by LLMs in one go due to token limits. Raw text lacks structure for retrieval and analysis.

### What Must Be Solved
1. **Size**: Break documents into manageable pieces
2. **Context**: Preserve semantic boundaries (conversation turns, paragraphs, sections)
3. **Retrieval**: Create addressable chunks for later reference
4. **Overlap**: Maintain continuity between chunks

### Why It Matters
Without chunking:
- Cannot process documents larger than context window
- Lose conversational context (who said what)
- Cannot reference specific parts of documents
- Cannot parallelize processing

---

## Solution Approach

### Strategy
**Context-Aware Chunking** - Two strategies based on content type:

1. **Conversation Turn Parsing** (ChatGPT exports, dialogue)
   - Detect speakers (User, Assistant, System)
   - Group by conversation turns
   - Preserve who-said-what context

2. **Size-Based Chunking** (books, articles, general text)
   - Target ~700 tokens per chunk
   - 100 token overlap between chunks
   - Break on paragraph boundaries when possible

### Architecture Decision
**Service-based** with clear separation:
- `ChunkingService.cs` - Chunking logic
- `ProcessingPipelineService.cs` - Orchestration
- Database + File storage - Dual persistence

---

## Functionality

### Workflow Order
1. **Load Source** - Query database for source metadata
2. **Load Content** - Read extracted text file
3. **Determine Strategy** - Conversation vs. size-based
4. **Execute Chunking** - Apply appropriate algorithm
5. **Persist Chunks** - Save to files + database
6. **Update Status** - Mark source as processed

### What It Accomplishes
- **Input**: 142KB ChatGPT conversation (105 pages)
- **Output**: 53 contextual chunks with metadata
- **Preserves**: Speaker identity, conversation flow, page references
- **Creates**: Addressable chunks for summarization pipeline

---

## Technology Used

### Core Technology
- **Language**: C# (.NET 8)
- **Framework**: ASP.NET Core
- **Database**: SQLite + Entity Framework Core

### Key Libraries
- **Docnet.Core 2.6.0** - PDF text extraction
- Built-in: Regex, LINQ, File I/O

### Why These Choices
- **C#/.NET 8**: Strong typing, performance, async/await
- **SQLite**: Lightweight, file-based, no server needed
- **Docnet.Core**: Fast PDF rendering, MIT license, actively maintained
- **EF Core**: Type-safe queries, migrations, LINQ integration

### Future Upgrades
- Consider **SentenceTransformers** for semantic boundary detection
- Add **ML.NET** for adaptive chunking
- Implement **Lucene.NET** for full-text search within chunks

---

## Metadata Captured

### Source Metadata
```csharp
public class Source
{
    Guid Id                      // Unique identifier
    string SourceKey             // Human-readable key
    string Title                 // Document title
    string SourceType            // "Pdf", "Text", "conversation-log"
    string CurrentStage          // "Inbox", "Extracted", "Chunked"
    string Status                // "Queued", "Processing", "Complete"
}
```

### Chunk Metadata
```csharp
public class Chunk
{
    Guid Id                      // Unique identifier
    Guid SourceId                // Links to source
    int ChunkIndex               // Position in sequence (0-based)
    string? SectionTitle         // Extracted section heading
    string? PageReference        // Page numbers (e.g., "1-3")
    string TextPath              // File location
    int CharacterCount           // Length in characters
    int TokenCount               // Estimated tokens (~chars/4)
    string Status                // "New", "Processing", "Summarized"
    DateTime CreatedUtc          // Timestamp
}
```

### What's NOT Captured (Yet)
- **Keywords/Tags** - For filtering and search
- **Chunk Quality Score** - Boundary detection confidence
- **Speaker Metadata** - Structured conversation participants
- **Chunk Relationships** - Parent/child, follows/precedes

---

## Optimization Parameters

### Current Configuration
```csharp
// Target chunk size
private const int TARGET_TOKEN_COUNT = 700;
private const int TOKEN_OVERLAP = 100;

// Token estimation
private const double CHARS_PER_TOKEN = 4.0;

// Conversation detection patterns
private static readonly string[] SpeakerPatterns = {
    @"^User:",
    @"^Assistant:",
    @"^System:",
    @"^ChatGPT:"
};
```

### Performance Targets
- **Chunking Speed**: <1 second per 100KB
- **Memory Usage**: <50MB per document
- **Chunk Size**: 500-900 tokens (target 700)
- **Overlap**: 100 tokens between chunks

### Measured Results
- **142KB text** → **53 chunks** → **<2 seconds**
- **Average chunk**: 2,691 characters (~673 tokens)
- **Memory footprint**: <20MB

### Future Optimizations
- Stream processing for huge files (>10MB)
- Parallel chunking for multi-document batches
- Caching for repeated source processing

---

## Implementation Modules

### Core Services
1. **ChunkingService.cs**
   - `ChunkByConversationTurns()` - Parse dialogue
   - `ChunkBySize()` - Fixed-size chunking
   - `DetectSpeaker()` - Identify speaker patterns
   - `EstimateTokenCount()` - Token calculation

2. **ProcessingPipelineService.cs**
   - `ProcessSourceAsync()` - Main orchestrator
   - `LoadTextContentAsync()` - File loading
   - `ChunkContentAsync()` - Strategy selection
   - `SaveChunksAsync()` - Persistence

3. **ProcessingEndpoints.cs**
   - `POST /api/processing/process/{sourceId}` - Trigger processing
   - Returns: `ProcessingResultDto` with stats

### Data Models
- `Source.cs` - Source entity
- `Chunk.cs` - Chunk entity
- `ConversationChunk.cs` - DTO with turn counts

### File Locations
```
C:\me_workspace\
├── src\me_workspace.Web\
│   └── Features\
│       ├── Chunking\
│       │   ├── ChunkingService.cs
│       │   └── ConversationChunk.cs
│       └── Processing\
│           ├── ProcessingPipelineService.cs
│           ├── ProcessingEndpoints.cs
│           └── ProcessingResultDto.cs
```

---

## API Usage

### Process a Source
```powershell
# Scan inbox to register sources
Invoke-RestMethod -Uri "http://127.0.0.1:5078/api/pipeline/scan" -Method Post

# Process source to create chunks
Invoke-RestMethod -Uri "http://127.0.0.1:5078/api/processing/process/{sourceId}" -Method Post
```

### Response
```json
{
  "success": true,
  "message": "Processed Manhood Device Development",
  "chunksCreated": 53,
  "charactersProcessed": 142627
}
```

---

## Testing & Validation

### Test Case: ChatGPT Export
- **Input**: "Manhood Device Development.txt" (142KB, 105 pages)
- **Expected**: 50-60 contextual chunks
- **Result**: ✅ 53 chunks created
- **Quality**: ✅ Speaker context preserved
- **Performance**: ✅ <2 seconds

### Test Files
```
C:\me_workspaces_runtime\
├── 01-Inbox\
│   └── Manhood Device Development.txt (input)
└── 03-Chunked\
    └── src-20260525-120545-ff014e\
        ├── chunk-0000.txt
        ├── chunk-0001.txt
        └── ... (53 total)
```

---

## Related Documentation

### MasterPlan
- [../Intake/](../Intake/) - How sources enter the system
- [../Summarization/](../Summarization/) - Next step after chunking
- [../../Phases.md](../../Phases.md) - Phase 6 implementation

### Technical Docs
- [../../../docs/features/chunking/](../../../docs/features/chunking/) - Implementation details
- [../../../docs/Phase6-Database-Schema.md](../../../docs/Phase6-Database-Schema.md) - Database design

### Code
- [../../../src/me_workspace.Web/Features/Chunking/](../../../src/me_workspace.Web/Features/Chunking/)
- [../../../src/me_workspace.Web/Features/Processing/](../../../src/me_workspace.Web/Features/Processing/)
