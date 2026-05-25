# Data Model

## Purpose

This document defines the core MVP records so intake, context assembly, journals, tasks, and summaries do not drift apart.

## Core Rule

Prefer a small number of explicit record types with clear links over many ad hoc structures.

## Main Entities

### `Conversation`

Represents one chat thread in the app.

Suggested fields:

- `Id`
- `Title`
- `CreatedUtc`
- `UpdatedUtc`
- `LinkedJournalEntryId` nullable
- `Status`

### `Message`

Represents one user or assistant message inside a conversation.

Suggested fields:

- `Id`
- `ConversationId`
- `Role`
- `Content`
- `CreatedUtc`
- `ContextPackageId` nullable

### `JournalEntry`

Represents one durable working thread on disk and in metadata.

Suggested fields:

- `Id`
- `Slug`
- `Title`
- `SummaryPath`
- `EntryPath`
- `MetaPath`
- `CreatedUtc`
- `UpdatedUtc`
- `Status`

### `TaskItem`

Represents a follow-up action derived from chat or review work.

Suggested fields:

- `Id`
- `Title`
- `Description`
- `SourceConversationId` nullable
- `SourceJournalEntryId` nullable
- `Status`
- `Priority`
- `CreatedUtc`
- `CompletedUtc` nullable

### `Source`

Represents one ingested item entering the intake pipeline.

Suggested fields:

- `Id`
- `Title`
- `SourceType`
- `RightsLabel`
- `RelativePath`
- `Hash`
- `CreatedUtc`
- `UpdatedUtc`
- `Status`

#### Additional fields for Library Research Sources

For sources with `SourceType = library-research`:

- `ISBN` - International Standard Book Number
- `Author` - Book author(s)
- `Publisher` - Publishing house
- `PublicationYear` - Year published
- `BorrowingSource` - Where accessed (e.g., "Internet Archive", "Libby", "Google Books")
- `AccessExpiryUtc` - When temporary access expires
- `PageRangesProcessed` - JSON array of page ranges OCR'd (e.g., `["1-10", "45-52"]`)
- `CitationFormat` - Generated citation string (APA, MLA, etc.)

These fields enable:
- Proper attribution in summaries
- Priority processing before access expires
- Page-level citation tracking
- Ethical transient text handling

### `ExtractedText`

Represents approved working text derived from a source.

Suggested fields:

- `Id`
- `SourceId`
- `TextLocation`
- `StorageMode`
- `CharacterCount`
- `CreatedUtc`
- `Status`

### `Chunk`

Represents a stable segment of extracted text.

Suggested fields:

- `Id`
- `SourceId`
- `ExtractedTextId`
- `ChunkIndex`
- `SectionTitle`
- `PageReference`
- `TextLocation`
- `TokenCount`
- `Status`

### `Summary`

Represents a structured summary of one chunk or one source.

Suggested fields:

- `Id`
- `SourceId`
- `ChunkId` nullable
- `SummaryKind`
- `SummaryPath`
- `ModelName`
- `PromptVersion`
- `CreatedUtc`
- `Status`

### `ReviewDecision`

Represents approval or rejection of extracted or summarized material.

Suggested fields:

- `Id`
- `TargetType`
- `TargetId`
- `Decision`
- `Notes`
- `CreatedUtc`

### `ContextPackage`

Represents the bundle of material assembled for one assistant turn.

Suggested fields:

- `Id`
- `ConversationId`
- `UserMessageId`
- `CreatedUtc`
- `SelectionReason`
- `SnapshotPath` nullable

## Core Relationships

Use these relationships as the MVP backbone:

- `Conversation` has many `Message`
- `Conversation` optionally links to one `JournalEntry`
- `Conversation` can create many `TaskItem`
- `Source` can have one or more `ExtractedText` records
- `ExtractedText` can have many `Chunk` records
- `Chunk` can have one or more `Summary` records
- `Summary` can receive a `ReviewDecision`
- `Message` can link to one `ContextPackage`
- `ContextPackage` can reference files, journal materials, summaries, and recent logs

## Minimal Status Sets

### Source Status

- `new`
- `registered`
- `extracting`
- `extracted`
- `chunked`
- `summarized`
- `reviewed`
- `failed`
- `archived`

### Task Status

- `open`
- `in-progress`
- `blocked`
- `done`

### Review Decision

- `pending`
- `approved`
- `rejected`
- `needs-follow-up`

### Source Type

- `user-authored` - Content created by the user
- `user-owned` - Content owned/purchased by the user
- `public-domain` - Public domain materials
- `library-research` - Temporarily accessed via library lending or previews
- `web-article` - Online articles and reports
- `manual-review` - Unclear rights, needs classification

### Rights Label

- `user-owned` - Full processing and retention allowed
- `user-authored` - Full processing and retention allowed
- `public-domain` - Full processing and retention allowed
- `summary-only` - Extract and summarize, delete full text after
- `manual-review-only` - No automatic processing until classified

## Design Notes

- Add hashes to ingested sources to reduce duplicate processing.
- Prefer storing file paths and metadata first, then durable text only when allowed by the retention policy.
- Track `PromptVersion` and `ModelName` for summaries so outputs remain explainable.
- Keep `ContextPackage` explicit so automatic context selection can be debugged later.
