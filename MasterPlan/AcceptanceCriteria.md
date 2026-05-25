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

---

## Foundation: Immutable Backup System (Phase 13)

### Priority: 🔴 CRITICAL - Must exist before processing production data

### Goal

Protect against catastrophic data loss from AI agent errors, user mistakes, or system failures.

### The "Railway Incident" Lesson

This requirement exists because of documented cases where AI agents deleted production databases despite explicit instructions not to. If backups are tied to the same volume/credentials as the primary data, a single delete operation destroys everything.

### Done Looks Like

- Backups exist in a separate directory with **different file system permissions** than the working data
- Backup location is **read-only** or **append-only** from the perspective of normal operations
- Backups cannot be deleted by the same user/process that writes to the primary database
- Daily automated backups run successfully
- Backup includes:
  - SQLite database (conversations, sources, summaries, tasks, journals)
  - Critical configuration files (`appsettings.json`, `DataModel.md`, etc.)
  - Journal entry files (but not transient OCR text)
  - Summary files from `Research/summaries/`
- Restore procedure is **documented and tested**
- Last 30 days of backups are retained

### MVP Checks

**Setup:**
- [ ] Create backup directory at a separate path (e.g., `C:\me_workspace_backups\`)
- [ ] Configure directory permissions: main app has READ-ONLY access to backups
- [ ] Implement backup script (PowerShell or C#) that runs daily
- [ ] Add scheduled task to run backup automatically

**Testing (CRITICAL - do not skip):**
- [ ] Run backup manually, verify files are copied
- [ ] Attempt to delete backup from main application → should FAIL
- [ ] Delete the primary database
- [ ] Restore from backup → database works
- [ ] Document restore procedure in `README.md`

**Validation:**
- [ ] Backup has run successfully for 7 consecutive days
- [ ] Backup size is reasonable (<1GB for MVP)
- [ ] Restore procedure takes <5 minutes

### Acceptance Statement

**"The backup system has prevented at least one simulated data loss scenario during testing, and the restore procedure has been successfully executed by someone other than the original developer."**

### Related Documents

- [ArchitecturePrinciples.md - Security Architecture](ArchitecturePrinciples.md#security-architecture)
- [Phases.md - Phase 13: AI Safety Architecture](Phases.md#phase-13-ai-safety-architecture)

---

## Foundation: Database Schema (Phase 6)

### Priority: 🔴 HIGH - Required for batch processing

### Goal

Migrate from JSON-based source tracking to SQLite database for reliability and scalability.

### Done Looks Like

- `Source` table exists in SQLite with all fields from [DataModel.md](DataModel.md)
- Migration script converts `book_targets.json` → SQLite records
- All PowerShell scripts (ProcessBookFolder.ps1, etc.) write to SQLite, not JSON
- Source metadata persists across application restarts
- Can query: "Show all sources with status='processing'"
- Can query: "Show all library-research sources expiring in next 24 hours"

### MVP Checks

**Schema:**
- [ ] Create `Source` table with fields: Id, Title, SourceType, RightsLabel, ISBN, Author, Publisher, BorrowingSource, AccessExpiryUtc, Status, CreatedUtc
- [ ] Create indexes on: Status, SourceType, AccessExpiryUtc
- [ ] Add foreign key from Message.ContextPackageId → Source.Id (for traceability)

**Migration:**
- [ ] Write PowerShell script: `Migrate-BookTargetsToDatabase.ps1`
- [ ] Test migration with 3 sample books
- [ ] Verify no data loss during migration

**Integration:**
- [ ] Update ProcessBookFolder.ps1 to create Source records
- [ ] Update MasterBookProcessor.ps1 to query from database
- [ ] Add database connection string to appsettings.json

### Acceptance Statement

**"All book processing operations read from and write to SQLite. JSON files are reference-only. The database survives application restarts and can be queried for operational insights."**

---

## Foundation: General-Purpose Document Platform (Phase 0)

### Priority: 🟡 MEDIUM - Validates architecture, enables future expansion

### Goal

Prove that the folder-as-agent architecture supports multiple document types, not just books.

### The Platform Test

If the Health Fundamentals book project succeeds using folder structure + markdown instructions, then the same pattern should work for business plans, strategic plans, and task management without custom code.

### Done Looks Like

- The folder structure pattern is **documented** and **templated**
- At least **two different document types** have been created using the same pattern:
  1. Health Fundamentals book (primary validation)
  2. One of: Business plan, strategic plan, or project plan
- Both projects use the same core structure:
  ```
  /ProjectName/
    README.md              # What this project is
    00_Context.md          # Background
    01_Protocol.md         # Instructions for AI
    Research/              # Source materials
    Drafts/                # Work in progress
    Output/                # Finals
    Logs/                  # Audit trail
  ```
- AI behavior adapts based on which folder it's opened in
- No custom code per document type (just folder structure + markdown)

### MVP Checks

**Documentation:**
- [ ] Create `templates/document_project_template/` with standard structure
- [ ] Document in [ArchitecturePrinciples.md](ArchitecturePrinciples.md) how to adapt template
- [ ] Add examples: book project vs. business plan project

**Validation:**
- [ ] Copy template to create a second project (e.g., `projects/Business_Plan/`)
- [ ] Add `00_Context.md` and `01_Protocol.md` specific to business planning
- [ ] Open VS Code in that folder, interact with AI
- [ ] Verify AI understands context without additional configuration

**Reusability Test:**
- [ ] Common components (OCR, summarization, citation) work for both projects
- [ ] PowerShell scripts are reusable (path-agnostic)
- [ ] Database schema supports both project types (generic `Source` table)

### Acceptance Statement

**"A second document project (non-book) has been created using the template folder structure. The AI adapts to the new context without custom code. Core platform components (summarization, file tracking, audit logging) work identically for both projects."**

### Why This Matters

This proves the platform is **general-purpose infrastructure**, not a one-off book tool. When the book is finished, the platform remains valuable for business plans, strategic planning, and ongoing project management.

---

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
