# Runtime Constraints

## Purpose

This document makes the MVP's local runtime assumptions explicit.

## Core Rule

Choose defaults that are dependable on one local machine before optimizing for scale.

## MVP Assumptions

- single-user local environment
- local-only hosting on `127.0.0.1`
- SQLite as the main durable store
- one main app process plus background workers
- local model access from `.NET`

## Queue And Processing Constraints

- Prefer bounded background queues over unbounded ingestion.
- Process one item type at a time until the workflow is stable.
- Make every stage resumable where practical.
- Track failures explicitly instead of retrying forever.

## File Constraints

- Set a practical max file size for MVP processing.
- Flag oversized files for manual review rather than trying to process everything automatically.
- Support `.txt`, `.md`, and `.pdf` first.
- Treat HTML and OCR-heavy inputs as later additions unless they become necessary sooner.

## Model Constraints

- Choose one canonical local summarization path for MVP.
- Record model name and prompt version on each summary.
- Avoid switching model families casually during early evaluation, because it makes output quality harder to compare.

## Logging Constraints

- Prefer structured logs with identifiers and statuses.
- Avoid logging long source bodies.
- Keep enough state to resume failed work.

## Safety Constraints

- No autonomous write actions without explicit approval.
- No silent destructive state changes.
- Keep action logs for write paths and review decisions.

## Performance Guidance

- Start with predictable throughput rather than maximum speed.
- Add concurrency only after the single-item pipeline behaves well.
- Prefer measurable limits and observability over clever background behavior.

## Review Trigger

Revisit this document when:

- the intake pipeline is live
- actual model/runtime bottlenecks appear
- additional source types are introduced
