# Chunking - Technical Overview

**Status:** ✅ Implemented  
**Phase:** Phase 6  
**Last Updated:** May 25, 2026

---

## What It Does

Breaks large documents into semantically coherent chunks for processing by LLMs and storage in the database.

## Workflow

```
1. Source Registered
   ↓
2. Load Text Content (from 02-Normalized/)
   ↓
3. Determine Strategy
   ├─→ Conversation? → Parse Speaker Turns
   └─→ Regular Text? → Size-Based Chunking
   ↓
4. Apply Chunking Algorithm
   ↓
5. Save Chunks (files + database)
   ↓
6. Update Source Status → "Chunked"
```

## Input/Output

### Input
- **Source**: Database record with `Id`, `SourceKey`, `Title`
- **Text File**: Extracted text from `02-Normalized/{sourceKey}/content.txt`

### Output
- **Chunk Files**: `03-Chunked/{sourceKey}/chunk-{index}.txt`
- **Database Records**: Chunks table with metadata
- **Status Update**: Source.CurrentStage = "Chunked"

## API Endpoints

### Trigger Processing
```http
POST /api/processing/process/{sourceId}
```

**Response:**
```json
{
  "success": true,
  "message": "Processed {title}",
  "chunksCreated": 53,
  "charactersProcessed": 142627
}
```

## Implementation Files

```
src/me_workspace.Web/Features/
├── Chunking/
│   ├── ChunkingService.cs         - Core chunking logic
│   └── ConversationChunk.cs       - DTO for chunks
└── Processing/
    ├── ProcessingPipelineService.cs  - Orchestration
    ├── ProcessingEndpoints.cs        - HTTP API
    └── ProcessingResultDto.cs        - Response model
```

---

**See also:**
- [technology.md](./technology.md) - Tech stack details
- [metadata.md](./metadata.md) - Data schemas
- [optimization.md](./optimization.md) - Performance tuning
- [modules.md](./modules.md) - Code reference
