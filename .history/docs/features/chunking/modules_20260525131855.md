# Chunking - Implementation Modules

## Code Organization

```
src/me_workspace.Web/Features/
├── Chunking/
│   ├── ChunkingService.cs         ✅ Core chunking logic
│   └── ConversationChunk.cs       ✅ DTO model
│
├── Processing/
│   ├── ProcessingPipelineService.cs  ✅ Orchestration
│   ├── ProcessingEndpoints.cs        ✅ HTTP API
│   └── ProcessingResultDto.cs        ✅ Response model
│
└── Sources/
    ├── SourceRegistryService.cs      ✅ Source registration
    └── SourceListItemDto.cs          ✅ Source DTO
```

---

## Module: ChunkingService

### Purpose
Core chunking algorithms and strategies.

### Location
`src/me_workspace.Web/Features/Chunking/ChunkingService.cs`

### Public Methods

#### ChunkByConversationTurns
```csharp
public List<ConversationChunk> ChunkByConversationTurns(
    string text,
    string sourceKey,
    int targetTokenCount = 700
)
```

**Purpose:** Parse conversation text into chunks based on speaker turns

**Algorithm:**
1. Parse text line-by-line
2. Detect speaker patterns (User:, Assistant:, etc.)
3. Group consecutive turns by same speaker
4. Split when target token count reached
5. Maintain speaker context across chunks

**Parameters:**
- `text` - Full conversation text
- `sourceKey` - Unique identifier for file naming
- `targetTokenCount` - Target size (default: 700)

**Returns:** List of `ConversationChunk` with:
- `Index` - Position (0-based)
- `Content` - Chunk text
- `TurnCount` - Number of conversation turns
- `EstimatedTokens` - Calculated token count

**Example Usage:**
```csharp
var chunks = chunkingService.ChunkByConversationTurns(
    text: chatGptExport,
    sourceKey: "src-20260525-120545",
    targetTokenCount: 700
);

Console.WriteLine($"Created {chunks.Count} chunks");
// Output: Created 53 chunks
```

#### ChunkBySize
```csharp
public List<ConversationChunk> ChunkBySize(
    string text,
    string sourceKey,
    int targetTokenCount = 700,
    int overlapTokens = 100
)
```

**Purpose:** Fixed-size chunking with overlap (fallback for non-conversational text)

**Algorithm:**
1. Estimate target character count (tokens × 4)
2. Split text at paragraph boundaries when possible
3. Add overlap from previous chunk
4. Continue until text exhausted

**Parameters:**
- `text` - Document text
- `sourceKey` - Unique identifier
- `targetTokenCount` - Target size (default: 700)
- `overlapTokens` - Overlap between chunks (default: 100)

**Returns:** List of `ConversationChunk`

**Example Usage:**
```csharp
var chunks = chunkingService.ChunkBySize(
    text: bookContent,
    sourceKey: "book-health-fundamentals",
    targetTokenCount: 800,
    overlapTokens: 150
);
```

### Private Methods

#### ParseConversationTurns
```csharp
private List<(string Speaker, List<string> Lines)> ParseConversationTurns(string text)
```

**Purpose:** Parse text into structured conversation turns

**Returns:** List of tuples:
- `Speaker` - "User", "Assistant", "System", etc.
- `Lines` - List of text lines for this turn

#### DetectSpeaker
```csharp
private string? DetectSpeaker(string line)
```

**Purpose:** Identify speaker from line patterns

**Patterns Matched:**
- `^User:`
- `^Assistant:`
- `^System:`
- `^ChatGPT:`

**Returns:** Speaker name or `null` if no match

#### EstimateTokenCount
```csharp
private int EstimateTokenCount(string text)
```

**Purpose:** Rough token estimation

**Formula:** `text.Length / 4.0`

**Returns:** Estimated token count (integer)

---

## Module: ProcessingPipelineService

### Purpose
Orchestrate the complete processing pipeline: load → chunk → save.

### Location
`src/me_workspace.Web/Features/Processing/ProcessingPipelineService.cs`

### Dependencies
```csharp
public ProcessingPipelineService(
    AppDbContext db,                          // Database access
    ChunkingService chunkingService,          // Chunking logic
    WorkspaceLayoutService workspaceLayout,   // File paths
    ILogger<ProcessingPipelineService> logger // Logging
)
```

### Public Methods

#### ProcessSourceAsync
```csharp
public async Task<ProcessingResultDto> ProcessSourceAsync(
    Guid sourceId,
    CancellationToken cancellationToken = default
)
```

**Purpose:** Main entry point - process a source from start to finish

**Workflow:**
1. Query source from database (with `.Include(s => s.Files)`)
2. Load text content from file
3. Determine chunking strategy
4. Execute chunking
5. Save chunks to files + database
6. Update source status

**Parameters:**
- `sourceId` - Unique source identifier
- `cancellationToken` - For cancellation support

**Returns:** `ProcessingResultDto` with:
- `Success` - true/false
- `Message` - Status message
- `ChunksCreated` - Number of chunks
- `CharactersProcessed` - Total character count

**Example Usage:**
```csharp
var result = await processingPipeline.ProcessSourceAsync(
    sourceId: Guid.Parse("dd8819ff-2eb3-447c-b9b3-25be66b5af06")
);

if (result.Success) {
    Console.WriteLine($"{result.ChunksCreated} chunks created");
}
```

**Error Handling:**
- Source not found → `ProcessingResultDto.CreateFailure()`
- File not found → Exception logged, failure returned
- Chunking error → Exception logged, failure returned

### Private Methods

#### LoadTextContentAsync
```csharp
private async Task<string?> LoadTextContentAsync(
    Source source,
    CancellationToken cancellationToken
)
```

**Purpose:** Load extracted text from file system

**Logic:**
1. Check for `.txt` file in `02-Normalized/{sourceKey}/`
2. If PDF source, look for `{sourceKey}.txt`
3. Return content or null if not found

#### ChunkContentAsync
```csharp
private Task<List<ConversationChunk>> ChunkContentAsync(
    string text,
    Source source
)
```

**Purpose:** Determine chunking strategy and execute

**Logic:**
```csharp
if (source.SourceType == "conversation-log") {
    return ChunkByConversationTurns(text, source.SourceKey);
} else {
    return ChunkBySize(text, source.SourceKey);
}
```

#### SaveChunksAsync
```csharp
private async Task SaveChunksAsync(
    List<ConversationChunk> chunks,
    Source source,
    CancellationToken cancellationToken
)
```

**Purpose:** Persist chunks to files and database

**Logic:**
1. Create output folder: `03-Chunked/{sourceKey}/`
2. For each chunk:
   - Write to file: `chunk-{index:0000}.txt`
   - Create database record in `Chunks` table
3. Batch save to database (future optimization)

---

## Module: ProcessingEndpoints

### Purpose
HTTP API for triggering processing.

### Location
`src/me_workspace.Web/Features/Processing/ProcessingEndpoints.cs`

### Endpoints

#### POST /api/processing/process/{sourceId:guid}
```csharp
app.MapPost("/api/processing/process/{sourceId:guid}", 
    async (Guid sourceId, ProcessingPipelineService pipeline, CancellationToken ct) => 
{
    var result = await pipeline.ProcessSourceAsync(sourceId, ct);
    return result.Success 
        ? Results.Ok(result) 
        : Results.BadRequest(result);
});
```

**Request:**
```http
POST /api/processing/process/dd8819ff-2eb3-447c-b9b3-25be66b5af06
```

**Response (200 OK):**
```json
{
  "success": true,
  "message": "Processed Manhood Device Development",
  "chunksCreated": 53,
  "charactersProcessed": 142627
}
```

**Response (400 Bad Request):**
```json
{
  "success": false,
  "message": "Source {id} not found",
  "chunksCreated": 0,
  "charactersProcessed": 0
}
```

---

## Data Transfer Objects (DTOs)

### ConversationChunk
```csharp
public sealed class ConversationChunk
{
    public int Index { get; init; }
    public string Content { get; init; } = string.Empty;
    public int TurnCount { get; init; }
    public int EstimatedTokens { get; init; }
}
```

**Purpose:** Temporary model for chunking operations

**Lifecycle:** Created during chunking → Saved to database as `Chunk` entity

### ProcessingResultDto
```csharp
public sealed record ProcessingResultDto(
    bool Success,
    string Message,
    int ChunksCreated,
    int CharactersProcessed
)
{
    public static ProcessingResultDto CreateSuccess(
        string message, 
        int chunksCreated, 
        int charactersProcessed
    );
    
    public static ProcessingResultDto CreateFailure(string message);
}
```

**Purpose:** API response model

**Factory Methods:**
- `CreateSuccess()` - For successful processing
- `CreateFailure()` - For error responses

---

## Service Registration

### Program.cs
```csharp
// Register services
builder.Services.AddScoped<ChunkingService>();
builder.Services.AddScoped<ProcessingPipelineService>();

// Register endpoints
app.MapProcessingEndpoints();
```

### Dependency Injection Scope
- **Scoped**: Services created once per HTTP request
- **Why**: Database context (AppDbContext) is scoped
- **Lifetime**: Created → Used → Disposed automatically

---

## Testing Patterns

### Unit Testing: ChunkingService
```csharp
[Fact]
public void ChunkByConversationTurns_SplitsOnSpeakers()
{
    // Arrange
    var service = new ChunkingService();
    var text = "User: Hello\nAssistant: Hi\nUser: How are you?";
    
    // Act
    var chunks = service.ChunkByConversationTurns(text, "test", 700);
    
    // Assert
    Assert.Single(chunks);  // Should be 1 chunk (under token limit)
    Assert.Equal(3, chunks[0].TurnCount);  // 3 speaker turns
}
```

### Integration Testing: ProcessingPipeline
```csharp
[Fact]
public async Task ProcessSourceAsync_CreatesChunks()
{
    // Arrange
    var db = CreateTestDbContext();
    var source = CreateTestSource();
    await db.Sources.AddAsync(source);
    await db.SaveChangesAsync();
    
    var pipeline = new ProcessingPipelineService(db, ...);
    
    // Act
    var result = await pipeline.ProcessSourceAsync(source.Id);
    
    // Assert
    Assert.True(result.Success);
    Assert.True(result.ChunksCreated > 0);
    
    var chunks = await db.Chunks.Where(c => c.SourceId == source.Id).ToListAsync();
    Assert.Equal(result.ChunksCreated, chunks.Count);
}
```

---

## Extension Points

### Custom Chunking Strategies
```csharp
// Add new strategy
public interface IChunkingStrategy
{
    List<ConversationChunk> Execute(string text, string sourceKey);
}

// Example: Semantic chunking
public class SemanticChunkingStrategy : IChunkingStrategy
{
    public List<ConversationChunk> Execute(string text, string sourceKey)
    {
        var embeddings = GenerateEmbeddings(text);
        var boundaries = DetectTopicShifts(embeddings);
        return SplitAtBoundaries(text, boundaries, sourceKey);
    }
}

// Register in ChunkingService
public List<ConversationChunk> ChunkWithStrategy(
    string text, 
    string sourceKey,
    IChunkingStrategy strategy
) 
{
    return strategy.Execute(text, sourceKey);
}
```

### Custom Post-Processing
```csharp
// Add hook after chunking
public interface IChunkProcessor
{
    Task ProcessAsync(Chunk chunk, CancellationToken ct);
}

// Example: Keyword extraction
public class KeywordExtractor : IChunkProcessor
{
    public async Task ProcessAsync(Chunk chunk, CancellationToken ct)
    {
        var keywords = await ExtractKeywordsAsync(chunk.Content);
        // Save to ChunkTag table
    }
}
```

---

**See also:**
- [README.md](./README.md) - Feature overview
- [technology.md](./technology.md) - Tech stack
- [metadata.md](./metadata.md) - Data schemas
- [optimization.md](./optimization.md) - Performance tuning
