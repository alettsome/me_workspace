# MVP Slice 01: Intake And Chunking

## Goal
Build the first end-to-end slice that turns an inbox file into chunk records.

## Slice Outcome
Input:

- a file dropped into `01-Inbox`

Output:

- a `Source` record
- a normalized text file
- chunk files
- chunk database records
- source moved to `03-Chunked`

## Supported File Types For This Slice
- `.txt`
- `.md`
- `.pdf`

## Sequential Implementation
### Step 1: Workspace Options
Create options that define:

- runtime root
- folder names
- chunk size target
- overlap target

### Step 2: Tables
Create:

- `Sources`
- `SourceFiles`
- `ProcessingJobs`
- `Chunks`

### Step 3: Inbox Detection
Build either:

- a `FileSystemWatcher`

or

- a polling background worker

Polling is often simpler and more reliable for MVP 1.

### Step 4: Source Registration
When a new file is found:

1. create a source key
2. infer `SourceType`
3. create a `Source` row
4. create an `intake` job row

### Step 5: Extraction
Choose extractor by extension:

- `.txt` -> raw text reader
- `.md` -> markdown-to-text or raw markdown reader
- `.pdf` -> PdfPig extractor

Then:

1. normalize whitespace
2. save `normalized.txt`
3. move source stage to `Normalized`

### Step 6: Chunking
Chunk rules:

1. split by headings if available
2. otherwise split by paragraph groups
3. otherwise use token windows with overlap

Suggested default:

- target: `700` tokens
- overlap: `100` tokens

Then:

1. save chunk text files
2. save chunk records
3. create a `chunk` job row
4. move source stage to `Chunked`

### Step 7: Logging
For every source, write readable logs to:

`Logs\pipeline-YYYYMMDD.log`

Record:

- source detected
- extraction success or failure
- chunk count
- final stage

## Suggested Service Flow
```text
InboxScanner
  -> SourceRegistryService
  -> ExtractionService
  -> ChunkingService
  -> SourceFileMover
  -> Logger
```

## Suggested First API Endpoints
- `GET /api/health`
- `GET /api/sources`
- `GET /api/sources/{id}`
- `POST /api/pipeline/scan`

## First Success Test
1. Drop one `.txt` file into `01-Inbox`
2. Trigger scan
3. Confirm `Source` row exists
4. Confirm normalized text was written
5. Confirm chunk files were written
6. Confirm chunk rows exist
7. Confirm source moved to `03-Chunked`

## What Not To Add Yet
- summarization
- Open WebUI integration
- embeddings
- review UI
- output generation

## Done Looks Like
The first useful pipeline slice is real and stable before any bigger features are added.
