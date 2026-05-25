# Composed Components Phased Path

## Purpose
This document describes a faster path to a working local research system by combining existing components instead of hand-building every layer.

The principles stay the same:

- local-first
- privacy-first
- continuity
- folder-backed workflow
- ethical summarization
- durable outputs

The method changes:

- use existing components where they save time
- build the workflow glue that makes the system yours
- keep the custom parts focused on organization, chunking, review, and outputs

## Core Idea
The system does not need to come from one tool.

It can be composed from:

- `Windows Explorer` for visible folder flow
- `.NET` for orchestration and state
- `SQLite` for tracking
- `Ollama` for local model access
- `Open WebUI` where it meaningfully speeds up research work
- `ShareX` for OCR or page capture when needed
- `PowerShell` for automation scripts

## What This Path Should Produce
The composed system should be able to:

1. intake sources
2. normalize them
3. chunk them
4. summarize and review them
5. organize them by folder and status
6. build outputs such as briefs, chapter notes, and plans

## Component Map
### Explorer And Folders
Use for:

- visible workflow stages
- manual drop zones
- quick inspection
- simple movement between states

### .NET App
Use for:

- job orchestration
- metadata storage
- queue processing
- chunking services
- status tracking
- review screens

### SQLite
Use for:

- source metadata
- chunk records
- summary records
- job history
- review state

### Ollama
Use for:

- local summarization
- local embeddings later if needed
- model access without cloud dependency

### Open WebUI
Use for:

- fast manual research workflows
- knowledge upload
- comparison and synthesis experiments
- prompt iteration

### ShareX
Use for:

- OCR when direct text is unavailable
- limited page capture for lawful review workflows

### PowerShell
Use for:

- scripts
- helpers
- glue between tools

## Phase 0: Boundaries
### Goal
Make the method explicit before building.

### Components Used
- Markdown docs
- folders
- simple project notes

### Checklist
- [ ] Confirm this is a composed system, not a single monolith
- [ ] Confirm the workflow is local-first
- [ ] Confirm Explorer folders are part of the operating model
- [ ] Confirm summaries and metadata are the durable outputs
- [ ] Confirm rights-sensitive sources default to summary-only handling

### Done Looks Like
- The project has a clear written boundary around what gets composed and what gets custom-built

## Phase 1: Local Foundation
### Goal
Get the local base stack stable.

### Components Used
- `.NET`
- `ASP.NET Core`
- `SQLite`
- `Ollama`
- `PowerShell`

### What This Phase Builds
- local app shell
- local database
- config and logging
- model connectivity

### Checklist
- [x] Create or confirm the `.NET` project shell
- [x] Add SQLite and EF Core
- [x] Add local logging
- [ ] Confirm Ollama is reachable
- [x] Add basic settings for folders and models
- [x] Confirm the app starts on `127.0.0.1`

### Done Looks Like
- The system has a stable local runtime base

## Phase 2: Folder Workflow
### Goal
Use Explorer folders as the visible pipeline.

### Components Used
- `Windows Explorer`
- `.NET`
- `FileSystemWatcher`
- `BackgroundService`

### Suggested Folders
- `01-Inbox`
- `02-Normalized`
- `03-Chunked`
- `04-Summaries`
- `05-Reviewed`
- `06-Outputs`
- `07-Archive`

### What This Phase Builds
- file intake through folders
- state movement through folders
- tracked work items

### Checklist
- [x] Define the workspace root
- [x] Create the main stage folders
- [ ] Add file watchers or scheduled scans
- [x] Track source status in SQLite
- [x] Define state labels such as `new`, `processing`, `reviewed`, `done`

### Done Looks Like
- Files can enter through Explorer and become tracked work

## Phase 3: Source Intake And Metadata
### Goal
Turn incoming material into known source records.

### Components Used
- `.NET`
- `SQLite`
- folder services

### Source Types
- text
- markdown
- PDFs
- exported chats
- transcripts
- OCR capture
- web captures

### What This Phase Builds
- source registry
- metadata tagging
- rights labels
- topic tags

### Checklist
- [x] Create `Source` records
- [ ] Define source-type labels
- [ ] Define rights labels
- [x] Capture title, origin, date, tags, and path
- [x] Build a source list screen or endpoint

### Done Looks Like
- Every source has traceability before processing begins

## Phase 4: Text Extraction And Normalization
### Goal
Convert source files into clean working text.

### Components Used
- `.NET`
- `UglyToad.PdfPig`
- `Markdig`
- `AngleSharp` or `HtmlAgilityPack`
- `ShareX` when needed

### What This Phase Builds
- text extraction from `.txt`, `.md`, `.pdf`, and optionally `.html`
- cleaned line breaks and whitespace
- source references such as pages or sections when available

### Checklist
- [ ] Build text extractor for plain text
- [ ] Build markdown extractor
- [ ] Build PDF extractor
- [ ] Add HTML cleanup if needed
- [ ] Define where ShareX OCR enters the pipeline
- [ ] Save normalized text or approved working text

### Done Looks Like
- Incoming material becomes usable text ready for chunking

## Phase 5: Chunking
### Goal
Split long material into usable units.

### Components Used
- `.NET`
- tokenizer library
- custom chunking service

### Suggested Components
- `SharpToken` or `Microsoft.ML.Tokenizers`

### Preferred Chunking Order
1. headings
2. paragraphs
3. page boundaries when relevant
4. token windows with overlap as fallback

### What This Phase Builds
- chunk generation
- chunk metadata
- chunk state tracking

### Suggested Chunk Fields
- `SourceId`
- `ChunkIndex`
- `SectionTitle`
- `PageReference`
- `Text`
- `TokenCount`
- `Status`

### Checklist
- [ ] Add tokenizer support
- [ ] Build chunking rules
- [ ] Save chunk records in SQLite
- [ ] Test on chats, PDFs, and longer notes
- [ ] Verify chunk boundaries feel useful rather than arbitrary

### Done Looks Like
- Long sources are split into stable, reusable working units

## Phase 6: Summarization And Analysis
### Goal
Turn chunks into structured notes.

### Components Used
- `.NET`
- `HttpClient`
- `Ollama`
- optionally `Open WebUI`
- background job runners

### When To Use Ollama Directly
- automated summarization
- predictable local processing
- fast scriptable batch work

### When To Use Open WebUI
- manual exploration
- prompt iteration
- comparison across sources
- knowledge-based synthesis

### What This Phase Builds
- summarization jobs
- saved summary outputs
- model request logs

### Checklist
- [ ] Create summarization jobs
- [ ] Add direct Ollama calling
- [ ] Decide where Open WebUI adds value versus direct calls
- [ ] Save summary results and errors
- [ ] Test on a small batch

### Done Looks Like
- The system can produce structured notes from chunks reliably

## Phase 7: Review And Organization
### Goal
Separate rough outputs from trusted working material.

### Components Used
- `.NET`
- SQLite queries
- folder movement rules
- optional Open WebUI review passes

### What This Phase Builds
- review states
- tags and themes
- promotion from summaries into reviewed notes

### Checklist
- [ ] Add review statuses
- [ ] Add themes and tags
- [ ] Build a simple review surface
- [ ] Allow notes to move into reviewed and output folders
- [ ] Flag weak or uncertain material

### Done Looks Like
- The system can distinguish processed notes from approved working material

## Phase 8: Outputs
### Goal
Turn organized notes into usable deliverables.

### Components Used
- `.NET`
- `Markdig`
- `QuestPDF`
- local markdown files

### Possible Outputs
- research briefs
- chapter outlines
- synthesis notes
- contradictions reports
- business-plan sections

### Checklist
- [ ] Define one output template
- [ ] Build one output generator
- [ ] Save outputs to the output folder
- [ ] Link outputs back to their source notes

### Done Looks Like
- The system produces real end products, not just internal processing state

## Phase 9: Continuity
### Goal
Reduce restart cost and preserve momentum across sessions.

### Components Used
- `.NET`
- SQLite
- markdown notes
- journals or project logs

### What This Phase Builds
- job history
- source history
- summary history
- project notes and continuity

### Checklist
- [ ] Add job history
- [ ] Add summary history
- [ ] Add journal or project notes
- [ ] Link outputs back to reviewed notes and source records

### Done Looks Like
- Work can resume cleanly without reconstructing context from scratch

## Highest-Value Existing Components
- `ASP.NET Core`
- `EF Core`
- `SQLite`
- `Serilog`
- `FileSystemWatcher`
- `BackgroundService`
- `UglyToad.PdfPig`
- `Markdig`
- `AngleSharp` or `HtmlAgilityPack`
- `SharpToken` or `Microsoft.ML.Tokenizers`
- `HttpClient`
- `OllamaSharp` if desired
- `QuestPDF`

## Parts Worth Owning Yourself
- folder state rules
- metadata model
- chunking rules
- summary templates
- review logic
- output assembly

## MVP Sequence
If speed matters most, do this first:

1. local foundation
2. folder workflow
3. metadata tracking
4. extraction
5. chunking
6. summarization
7. review
8. one output type

## Short Recommendation
The best fast path is a composed local system where:

- Explorer shows the workflow
- `.NET` manages the flow
- SQLite tracks the state
- Ollama does local model work
- Open WebUI stays available where it genuinely accelerates manual research
- outputs become durable files you control
