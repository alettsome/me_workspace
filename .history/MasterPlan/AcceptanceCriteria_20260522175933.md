# Acceptance Criteria

## Purpose

This document defines what "good enough" means for the riskiest MVP areas.

## Core Understanding

**The platform comes first. The book validates it.**

- The Health Fundamentals book is the **forcing function** that validates the platform, not the platform's sole purpose
- The same infrastructure must serve multiple document types:
  - Books (long-form research)
  - Business plans (strategic documents)
  - Strategic plans (project roadmaps)
  - ThingsToDo (task/project management)
- **Platform stability is the prerequisite** for any document project to succeed
- The book project pressure-tests the platform at scale (100+ sources, batch processing, time constraints)

### The Hierarchy

1. **Foundation:** Platform infrastructure (backups, security, database, file structure)
2. **Validation:** Health Fundamentals book (proves the platform works)
3. **Expansion:** Business plans, strategic plans, other document types (reuses proven platform)

**Implication:** If the platform isn't solid, the book project will fail. Therefore, foundational infrastructure (especially immutable backups) takes priority over book-specific features.

## Phase 4: Automatic Context Selection

### Goal

The app should assemble likely useful context automatically before the assistant responds.

### Done Looks Like

- For a small benchmark set of representative prompts, the selected context is relevant most of the time.
- Manual drag-and-drop still works as an override.
- The assembled context can be inspected after the fact.

### MVP Checks

- Create at least 10 representative test prompts from real project use.
- For each prompt, record the expected likely context sources.
- The automatic selector should place clearly relevant material in the top set often enough to be useful in practice.
- The system should record why an item was chosen, even if the reason is simple.

## Phase 7: Extraction And Normalization

### Goal

The system should extract usable text from the main MVP source types without chaos.

### Done Looks Like

- `.txt` files extract cleanly
- `.md` files extract cleanly
- `.pdf` files extract well enough for summarization on a small test set
- failures are visible and do not silently disappear

### MVP Checks

- Test at least one file of each supported type.
- Confirm whitespace cleanup does not destroy structure.
- Confirm errors create a visible failure state.

## Phase 8: Chunking And Summarization

### Goal

The system should turn extracted text into stable chunks and useful summaries.

### Done Looks Like

- chunk boundaries are understandable rather than arbitrary
- summaries are traceable to a source or chunk
- failed model calls do not lose the work item

### MVP Checks

- Test one short source and one longer source.
- Confirm chunk counts and token sizes are logged or inspectable.
- Confirm summary records keep `ModelName` and `PromptVersion`.

## Phase 9: Tasks And Continuity

### Goal

Useful follow-up work should survive beyond the current chat.

### Done Looks Like

- a chat can create task items
- task items can link back to a conversation or journal
- the next session can see what was left open

## Phase 11: Status Reports And Safe Actions

### Goal

The app should explain where the workspace stands and protect higher-risk actions.

### Done Looks Like

- a status report shows current focus and major open gaps
- the report includes pending tasks and unresolved review items
- write actions require explicit approval
- write decisions are logged

## Benchmark Rule

For the MVP, simple manual benchmark sets are enough.

Do not wait for a large automated evaluation system before testing the workflow.
