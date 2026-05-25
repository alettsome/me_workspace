# MVP Workspace Folder Tree

## Purpose
Use Explorer folders as the visible operating flow.

## Root
Suggested root:

`C:\me_workspaces_runtime`

This keeps runtime workflow content separate from the source code repository.

## Main Folders
```text
C:\me_workspaces_runtime
|-- 01-Inbox
|-- 02-Normalized
|-- 03-Chunked
|-- 04-Summaries
|-- 05-Reviewed
|-- 06-Outputs
|-- 07-Archive
|-- Logs
`-- Temp
```

## Folder Meaning
### `01-Inbox`
Raw incoming material.

Examples:

- `.txt`
- `.md`
- `.pdf`
- exported chats
- OCR text drops

### `02-Normalized`
Files that have been accepted and converted into clean working text.

### `03-Chunked`
Sources that have already been split into chunks.

### `04-Summaries`
Future home for chunk or source summaries.

### `05-Reviewed`
Future home for approved notes.

### `06-Outputs`
Future home for briefs, outlines, and reports.

### `07-Archive`
Completed or inactive material.

### `Logs`
Human-readable pipeline logs.

### `Temp`
Short-lived working files that should not become durable source storage by accident.

## Suggested Per-Source Layout
For normalized or chunked sources, use a source id folder:

```text
02-Normalized
`-- src-20260522-001
    |-- source.json
    `-- normalized.txt
```

```text
03-Chunked
`-- src-20260522-001
    |-- source.json
    |-- normalized.txt
    `-- chunks
        |-- chunk-001.txt
        |-- chunk-002.txt
        `-- chunk-003.txt
```

## Minimal Rules
- One source gets one source id.
- Folder movement should match the database status.
- `Temp` is allowed for working text, but it should not become an accidental hidden library.
- Rights-sensitive material should default to summary-oriented handling later in the pipeline.

## Done Looks Like
The folder tree tells you where any item is in the process without opening the app.
