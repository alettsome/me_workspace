# Phase Map

## Purpose
Show the whole journey from setup to a working research pipeline.

## Phase Order
### Phase 0: Ethical Guardrails
- Define what the system may do
- Define what the system must not do
- Default to summary-only handling for rights-sensitive material

### Phase 1: Foundations
- Make sure Ollama, Open WebUI, models, scripts, and output folders all work

### Phase 2: Source Intake
- Bring in chats, PDFs, notes, transcripts, and lawful review material
- Tag each source with metadata and rights status

### Phase 3: Summarization
- Run first-pass summaries
- Save only the summary and metadata where required

### Phase 4: Review And Clustering
- Compare sources
- find contradictions
- group ideas into themes

### Phase 5: Structured Outputs
- Build book outlines, briefs, reports, and business-plan material

### Phase 6: App Orchestration
- Turn the repeatable workflow into a `.NET` application
- Connect intake, queueing, Open WebUI, storage, and output building

## Minimum Viable System
The first usable version is:

1. Collect chats and notes
2. Summarize them
3. Review them critically
4. Cluster them by theme
5. Produce one clean research brief

## Technology Summary
- `Ollama`: local model serving
- `Open WebUI`: analysis and research workspace
- `ShareX`: OCR or page capture when needed
- `PowerShell`: automation scripts
- `.NET`: future orchestration app
- `Windows Explorer folders`: visible project structure
