# Health Fundamentals Research Phases

## Purpose
Build a local, privacy-first research system that can gather, summarize, compare, and organize information from books, PDFs, chats, transcripts, web sources, and voice notes into usable documents for books, research projects, and business plans.

## Core Principle
Use the cleanest ethical workflow first:

- Read or capture only material you have a lawful right to access.
- Prefer public domain, licensed, owned, user-created, or openly available materials.
- Prefer summaries, notes, metadata, and extracted insights over copying full copyrighted text.
- Keep source tracking so every summary can be traced back to its origin.

## Ethical Working Rule
The intended workflow is:

1. View a source you can lawfully access.
2. Read or capture a limited page, section, clip, or transcript.
3. Summarize it into your own research notes.
4. Store the summary with source details.
5. Move to the next section.

This is the operating model to stay aligned with ordinary human research behavior rather than bulk reproduction.

## Summary-Only Mode
For books and other rights-sensitive material, the system should default to `summary-only mode`.

This means:

- The source may be viewed for review and summarization.
- The system may generate notes in your own words.
- The system may save summary text, page references, source title, and tags.
- The system should avoid saving full page text by default.
- The system should avoid building a hidden reconstruction of the source.

Safe saved fields:

- Source title
- Author or publisher when known
- Page number or section reference
- Rights label
- Topic tags
- Your summary
- Questions, caveats, and follow-up checks

Avoid saving by default:

- Full page text
- Large copied passages
- Screenshot archives kept as a substitute for the original work
- Sequential extracts detailed enough to recreate the source

## Phase 0: Ethical Guardrails
Goal: Make the boundaries explicit before scaling any workflow.

Checklist:

- [ ] Only use sources you can lawfully access
- [ ] Prefer public-domain, owned, licensed, user-created, or open materials
- [ ] Use summary-only mode for books and other rights-sensitive sources
- [ ] Save summaries and metadata instead of full passages
- [ ] Keep page numbers or section references for traceability
- [ ] Do not design the workflow to reconstruct a full book or document
- [ ] Review any new source type before automating it at scale

Done looks like:

- The workflow is set up to produce research notes, not copies of source material

## End State
At the end of this process, you should have:

- A structured local knowledge base
- Source-linked summaries
- Topic clusters
- Contradiction and gap reviews
- Draft-ready outlines for a book, reports, or business plans

## Phase 1: Stabilize The System
Goal: Make sure the local toolchain is working reliably.

Checklist:

- [ ] Ollama is running locally
- [ ] Open WebUI is running at `http://localhost:3000`
- [ ] Models are visible in Open WebUI
- [ ] Default low-resource model is selected for first-pass work
- [ ] Output folders exist for summaries and source notes
- [ ] A naming convention exists for saved outputs

Done looks like:

- You can upload or paste content and get a usable summary back

## Phase 2: Define Source Types
Goal: Separate source material by type so the capture method is appropriate.

Source categories:

- Books
- PDFs and papers
- Web articles
- Chat logs
- Audio or voice notes
- Videos and transcripts
- Business notes and planning documents

Checklist:

- [ ] Create a simple label for each source type
- [ ] Decide capture method for each type
- [ ] Add a rights-status field for each source

Recommended rights labels:

- `public-domain`
- `owned-copy`
- `licensed-access`
- `user-created`
- `open-web`
- `restricted-review-only`

Done looks like:

- Every source has a type and rights label before processing

## Phase 3: Capture And Normalize
Goal: Convert raw material into clean text plus metadata.

Preferred order:

1. Direct text or PDF import
2. Exported transcript or copied text
3. OCR with ShareX only when needed

Checklist:

- [ ] Save title
- [ ] Save author or source name
- [ ] Save URL or origin
- [ ] Save date captured
- [ ] Save rights label
- [ ] Save topic tags
- [ ] Save summary notes or approved working text only

For books:

- [ ] Capture page-by-page only when direct text is unavailable
- [ ] Summarize page-by-page or section-by-section in your own words
- [ ] Save the summary, not the page text
- [ ] Keep page references in notes
- [ ] Clear temporary working text after summarization when appropriate

For chats:

- [ ] Export or paste the chat into text files
- [ ] Break long chats into manageable chunks
- [ ] Label by topic, date, and source

Done looks like:

- Every item becomes a traceable research note with metadata

## Phase 4: First-Pass Summarization
Goal: Turn raw text into concise research notes.

Default model use:

- `phi3:3.8b` for first-pass summaries and extraction

Checklist:

- [ ] Summarize each item into key points
- [ ] Extract claims, recommendations, and concepts
- [ ] Remove fluff and duplication
- [ ] Save summaries into organized folders

Recommended output shape:

- Source summary
- Key health claims
- Key nutrition claims
- Protein-related points
- Warnings or caveats
- Questions to verify later

Done looks like:

- Each source has a compact note that can be reviewed quickly

## Phase 5: Critical Review
Goal: Challenge the material rather than just collecting it.

Checklist:

- [ ] Identify contradictions
- [ ] Identify weak or unsupported claims
- [ ] Flag outdated advice
- [ ] Note missing context
- [ ] Mark items needing stronger evidence

Prompt role:

- `Critical Reviewer`

Done looks like:

- You are no longer just collecting notes; you are filtering for quality

## Phase 6: Theme Clustering
Goal: Merge many summaries into major topic groups.

Possible clusters:

- Protein
- Recovery
- Sleep
- Energy balance
- Metabolism
- Training
- Habit formation
- Research quality
- Business strategy

Checklist:

- [ ] Group summaries by theme
- [ ] Merge overlapping ideas
- [ ] Separate consensus from disagreement
- [ ] Create a short brief for each theme

Done looks like:

- You can see patterns across dozens of sources without rereading everything

## Phase 7: Structured Outputs
Goal: Turn clustered research into usable documents.

Possible outputs:

- Book outline
- Chapter notes
- Research brief
- Contradictions report
- Business plan draft
- Decision memo

Checklist:

- [ ] Create outline from themes
- [ ] Attach source support to each section
- [ ] Mark sections needing more evidence
- [ ] Draft in plain language first

Done looks like:

- Research becomes a real document instead of a pile of notes

## Phase 8: Ongoing Maintenance
Goal: Keep the system usable as more sources are added.

Checklist:

- [ ] Weekly ingest backlog review
- [ ] Remove duplicates
- [ ] Re-run critical review on major themes
- [ ] Promote strong summaries into master notes
- [ ] Archive low-value material

Done looks like:

- The system stays organized instead of turning into a dump

## Practical Workflow Order
If you want the simplest starting sequence, use this:

1. Process existing chats first
2. Process owned or public PDFs next
3. Process books after that
4. Add transcripts, audio, and videos afterward
5. Run clustering and critical review only after enough material exists

## Page-By-Page Ethical Workflow
Use this when reviewing books or other sensitive long-form sources.

1. Open one page or one small section you can lawfully access.
2. Read it or OCR it for temporary working use when needed.
3. Generate a summary in your own words.
4. Save only the summary, page number, source title, and tags.
5. Discard the temporary working text if you do not need to retain it.
6. Move to the next page or section.

Done looks like:

- You retain the research value without storing the source itself

## First 10 Tasks
- [ ] Confirm Open WebUI and Ollama are stable
- [ ] Create one root folder for research inputs
- [ ] Create one root folder for processed summaries
- [ ] Create one root folder for final outputs
- [ ] Export or collect your existing chats
- [ ] Sort chats by topic
- [ ] Run first-pass summaries on chats
- [ ] Upload the first batch of PDFs or notes to Open WebUI
- [ ] Choose one topic for the first knowledge collection
- [ ] Create one critical review prompt for that collection

## Simple Decision Rule
When unsure what to do with a source, ask:

1. Do I have a lawful and ethical right to use this source?
2. Can I import text directly instead of OCR?
3. Can I summarize it instead of copying it?
4. Did I keep the source details?
5. Did I save the output in the right folder?

## What To Do Next
Immediate next move:

- Start with chats and notes you already own

Then:

- Build one small, successful pipeline end-to-end before scaling to 100 books

That means:

1. One topic
2. Five to ten sources
3. Summaries
4. Critical review
5. One clean output document
