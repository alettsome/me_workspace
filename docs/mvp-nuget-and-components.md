# MVP NuGet And Components

## Purpose
These are the components that speed up MVP 1 without overbuilding it.

## Core App
- `Microsoft.EntityFrameworkCore`
- `Microsoft.EntityFrameworkCore.Sqlite`
- `Microsoft.EntityFrameworkCore.Design`
- `Serilog.AspNetCore`
- `Serilog.Sinks.File`

## Extraction
- `UglyToad.PdfPig`
- `Markdig`

Optional later:

- `AngleSharp`

## Chunking
- `SharpToken`

Alternative:

- `Microsoft.ML.Tokenizers`

## Local Processing
No extra package required for:

- `BackgroundService`
- `Channel<T>`
- `FileSystemWatcher`
- `HttpClient`

## Future, Not Needed For MVP 1
- `OllamaSharp`
- `QuestPDF`
- `HtmlAgilityPack`

## Service Map
### `WorkspaceOptions`
Holds:

- runtime root
- folder names
- model settings

### `SourceRegistryService`
Registers new sources in SQLite.

### `InboxWatcherService`
Detects new files in `01-Inbox`.

### `ExtractionService`
Chooses the right extractor by file type.

### `ChunkingService`
Splits normalized text into chunks.

### `PipelineQueue`
Handles background jobs in order.

### `SourceFileMover`
Moves files between folder states.

## Suggested Registration Order
1. options
2. db context
3. registry service
4. extraction service
5. chunking service
6. file mover
7. background queue
8. inbox watcher

## Done Looks Like
You are leaning on stable components for infrastructure while keeping the workflow logic in your own services.
