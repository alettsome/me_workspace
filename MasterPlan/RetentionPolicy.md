# Retention Policy

## Purpose

This document defines what the MVP may keep, what should stay temporary, and what should be logged carefully.

## Core Rule

The system should preserve summaries, metadata, traceability, and user-created outputs.

It should avoid retaining unnecessary copies of source material.

## Retention Categories

### Durable By Default

These can be saved unless a later workflow rule says otherwise:

- source metadata
- source hashes
- source paths
- rights labels
- summaries
- tags
- review decisions
- user-created notes
- journals
- task items
- status reports

### Temporary By Default

These should be treated as working material unless explicitly approved for retention:

- raw extracted text
- normalized full-text copies
- transient OCR output
- temporary chunk files
- model request payload snapshots containing long source text

### Log Carefully

These should be logged in a minimal way:

- file-processing errors
- model errors
- pipeline state transitions
- action approvals and write decisions

Avoid logging large bodies of source text in normal application logs.

## Storage Guidance

### Source Files

- Prefer referencing the original path over copying the full source.
- If a workflow needs a durable retained derivative, store only the minimum needed form.

### Extracted Text

- Prefer transient working storage first.
- If extracted text must be saved for chunking or review, keep it in the smallest practical form and tie it to a rights label.
- Delete or expire temporary extracted text when it is no longer needed for the active workflow.

### Library Research Materials

For books and materials accessed via temporary lending (Internet Archive, library systems, preview windows):

- OCR extracted text is **always transient** - delete immediately after summarization
- Screen captures used for OCR are **transient** - delete after text extraction
- Keep only: summaries, citations, page ranges, ISBN/metadata
- Set automatic deletion triggers tied to summarization completion
- Never retain full extracted text beyond the active processing window
- Exception: brief quotations (< 500 words) with proper citation may be kept as part of research notes

### Summaries

- Summaries are a primary durable output.
- Save them with source links, model name, prompt version, and creation time.

### Chunks

- Chunk metadata is durable.
- Chunk text should follow the same retention rule as extracted text unless there is an approved reason to keep it.

## Rights Labels

Use simple MVP labels:

- `user-owned`
- `user-authored`
- `reference-summary-only`
- `temporary-review-only`
- `restricted`

## Operational Rules

- Do not assume every source may be fully retained.
- Default to summary-first handling where rights are unclear or limited.
- Make retention decisions visible in metadata.
- Keep traceability even when full text is discarded.

## Logging Rules

- Do not write full extracted source text into normal application logs.
- Do log pipeline stage, status, error type, and record identifiers.
- Log approvals for write actions and retention exceptions.

## Follow-Up Need

This policy is an MVP boundary document.

It should be reviewed again once the intake pipeline is live and real source types are moving through it.
