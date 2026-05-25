# MVP Data Model

## Purpose
Track the pipeline state in SQLite while Explorer shows the visible flow.

## Tables
### `Sources`
One record per source entering the system.

Fields:

- `Id`
- `SourceKey`
- `Title`
- `SourceType`
- `RightsLabel`
- `OriginalRelativePath`
- `CurrentStage`
- `Status`
- `CreatedUtc`
- `UpdatedUtc`

### `SourceFiles`
Physical files linked to one source.

Fields:

- `Id`
- `SourceId`
- `RelativePath`
- `FileRole`
- `CreatedUtc`

Examples of `FileRole`:

- `original`
- `normalized`
- `chunk`

### `SourceTags`
Simple tag mapping.

Fields:

- `Id`
- `SourceId`
- `Tag`

### `ProcessingJobs`
Tracks pipeline work.

Fields:

- `Id`
- `SourceId`
- `JobType`
- `Status`
- `StartedUtc`
- `CompletedUtc`
- `ErrorMessage`

Examples of `JobType`:

- `intake`
- `extract`
- `chunk`

### `Chunks`
One record per chunk.

Fields:

- `Id`
- `SourceId`
- `ChunkIndex`
- `SectionTitle`
- `PageReference`
- `TextPath`
- `CharacterCount`
- `TokenCount`
- `Status`
- `CreatedUtc`

## Suggested Enums
### `SourceType`
- `Text`
- `Markdown`
- `Pdf`
- `ChatExport`
- `Transcript`
- `OcrText`

### `CurrentStage`
- `Inbox`
- `Normalized`
- `Chunked`
- `Summaries`
- `Reviewed`
- `Outputs`
- `Archive`

### `Status`
- `New`
- `Queued`
- `Processing`
- `Completed`
- `Failed`

## Minimum Rule
The folder stage and the database stage should always agree.

## Done Looks Like
You can answer:

- what entered the system
- where it is now
- what jobs ran
- what chunks were created
