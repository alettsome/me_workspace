# me_workspace Master Strategy

## Purpose

This is the cleaned strategy document for the project.

It merges:

- the earlier exploratory planning
- the existing `me_workspace` phase progress
- the newer ingestion and summarization ideas
- the `.NET` component-based MVP approach

The aim is to keep the project grounded in what already exists while making space for the missing workflow pieces that were discovered later.

## How To Use This Plan

Use `MasterPlan/Phases.md` as the single active implementation tracker.

Use this document for:

- scope decisions
- architecture direction
- MVP boundaries
- ordering and prioritization

Use the supporting planning docs for the details that can otherwise cause drift:

- `DataModel.md`
- `RetentionPolicy.md`
- `EthicalSourceHandling.md`
- `AcceptanceCriteria.md`
- `RuntimeConstraints.md`

## Core Direction

Build forward from the existing `me_workspace` app.

Do not start a separate greenfield product unless the current app clearly blocks progress. A meaningful foundation already exists:

- local ASP.NET Core app
- SQLite-backed chat data
- journal-linked workflow
- approved folder context
- offline dictation with `whisper.cpp`
- local-first design

That means the best path is not "replace everything."

It is:

1. stabilize what already works
2. add the missing ingestion pipeline
3. improve context assembly and continuity
4. add reporting and safe action handling

## Boundaries

These boundaries should stay explicit.

- The app is local-first.
- The app is single-user first.
- The app is built with `.NET` components and small custom services.
- The existing web app remains the main shell.
- Open Web UI is out of scope for the main implementation path.
- The system should summarize and organize information, not accumulate unnecessary copies of protected material.
- Rights-sensitive workflows should default to summaries, notes, metadata, and traceability rather than bulk source retention.
- Ethical source handling must support public-domain, user-owned, and summary-only workflows without treating every source the same.

## Current Assessment

The project has already solved important foundation work:

- chat shell
- local persistence
- memory flow
- journal structure
- linked logs
- file context browsing
- real offline dictation worker

The missing pieces are mostly workflow and orchestration issues:

- intake from mixed sources is not yet formalized
- extraction and chunking are not yet first-class pipeline stages
- automatic context selection still needs to become the default
- task capture and continuation are still weak
- status reporting does not yet tell you where the project stands
- safe write actions are still not formalized

## MVP Objective

The MVP should not try to be a full autonomous operating system.

The MVP should be a reliable local research and work assistant inside `me_workspace` that can:

1. take in different source types
2. register and track them
3. extract or normalize working text
4. chunk long material
5. summarize it into durable notes
6. connect those notes to chat, journals, and tasks
7. show what has been done and what needs to happen next

The MVP should support ethical source handling by design:

- public-domain sources can follow a fuller pipeline
- user-owned or user-authored sources can follow the normal local workflow
- restricted or unclear-rights sources should default to summary-first handling with minimal retention

## Recommended Planning Folder Structure

For planning docs:

```text
MasterPlan/
  README.md
  Plan.md
  Phases.md
```

For the runtime research pipeline inside the app workspace, use a visible staged structure like:

```text
Workspace/
  01-Inbox/
  02-Normalized/
  03-Chunked/
  04-Summaries/
  05-Reviewed/
  06-Outputs/
  07-Archive/
  Journals/
  ThingsToDo/
```

This does not require creating everything immediately, but it gives the ingestion side a clean operating model instead of an ad hoc folder mess.

## .NET-First MVP Architecture

Use the existing app plus a small set of supporting services.

### Main App Shell

- `ASP.NET Core`
- local-only hosting on `127.0.0.1`
- existing UI as the front door

### Durable Storage

- `SQLite`
- `EF Core` where it helps

### Background Processing

- `BackgroundService`
- `Channel<T>`
- `FileSystemWatcher`

### Extraction And Normalization

- plain text reader
- markdown reader with `Markdig`
- PDF extraction with `UglyToad.PdfPig`
- optional HTML cleanup later with `AngleSharp`

### Chunking

- `SharpToken` or `Microsoft.ML.Tokenizers`
- custom chunking rules owned by the project

### Summarization

- local model calls from `.NET`
- `HttpClient`
- optional `OllamaSharp` only if it simplifies local model access

### Existing Voice Path

- keep the current `whisper.cpp` path
- replace browser microphone capture later with native local capture

## Service Direction

These are the main services worth designing around:

- `WorkspaceOptions`
- `SourceRegistryService`
- `InboxWatcherService`
- `ExtractionService`
- `ChunkingService`
- `SummarizationService`
- `PipelineQueue`
- `SourceFileMover`
- `ContextAssemblyService`
- `TaskCaptureService`
- `StatusReportService`

## Strategy By Track

To keep the work understandable, think in tracks rather than one giant blur.

### Track 1: Finish The Existing Workspace Loop

This is the most important immediate track.

Needed:

- automatic context selection before send
- journal adopt-or-create rules for new chat
- task capture into `ThingsToDo`
- stronger context assembly from files, summaries, and logs

Why:

This turns the current app into a working loop instead of a good collection of disconnected features.

### Track 2: Add Formal Intake And Summarization

This is the newer area that needs to be folded in as MVP work.

Needed:

- intake folders
- source registration
- metadata and rights labels
- text extraction
- chunking
- summarization jobs
- batch-safe processing rules
- reviewed notes and outputs

Why:

This is the missing ingest path you discovered. Without it, useful information keeps arriving from different places without a clean way to enter the system.

### Track 3: Reporting And Safety

Needed:

- status report generation
- next-step recommendations
- explicit approval before writes
- action logging
- rollback-friendly handling

Why:

This is what keeps the app understandable and trustworthy as it grows.

## Priority Order

This is the recommended build order now.

1. clean planning structure and checkbox tracker
2. lock the data model, retention policy, acceptance criteria, and runtime constraints
3. automatic context selection before send
4. source intake registry and visible staged folders
5. extraction for plain text, markdown, and PDF
6. chunking service
7. summarization pipeline
8. task capture into `ThingsToDo`
9. journal continuation rules
10. native microphone capture and clearer voice review flow
11. status reports and safe actions
12. optional semantic retrieval later if needed

## Recommended NuGet Direction

Highest-value pieces for this path:

- `Microsoft.EntityFrameworkCore`
- `Microsoft.EntityFrameworkCore.Sqlite`
- `Microsoft.EntityFrameworkCore.Design`
- `Serilog.AspNetCore`
- `Serilog.Sinks.File`
- `UglyToad.PdfPig`
- `Markdig`
- `SharpToken`

Later, only if needed:

- `AngleSharp`
- `Microsoft.ML.Tokenizers`
- `OllamaSharp`
- `QuestPDF`

## What Should Stay Custom

These parts define the value of the project and should not be over-outsourced:

- folder state rules
- metadata model
- chunking rules
- summary templates
- context assembly logic
- journal continuation rules
- task capture rules
- report generation logic
- rights-aware source handling rules
- batch processing rules

## Definition Of Success

The project is on the right path if `me_workspace` becomes a local-first workspace that can:

- take in information from different places without chaos
- summarize it into durable notes
- connect those notes to chats and journals
- capture follow-up work
- continue across sessions
- report clearly on where things stand
- stay safe around write actions

## Immediate Next Slice

The next implementation slice should combine these items:

- lock the core data model and retention rules for MVP
- lock the ethical source-handling rules for public-domain, owned, and summary-only material
- define acceptance checks for automatic context and summarization
- finish the first automatic context selector before AI processing
- define the staged intake folders and source statuses
- add a source registry for incoming material
- define extraction support for `.txt`, `.md`, and `.pdf`
- write chat outcomes into `ThingsToDo`

That slice respects the current app, brings in the new ingestion thinking, and creates a much stronger MVP foundation.
