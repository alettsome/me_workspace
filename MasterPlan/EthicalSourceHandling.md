# Ethical Source Handling

## Purpose

This document explains how the project can work with books and other long-form sources in an ethical way.

It is meant to answer:

- how the workflow can be useful
- how some automation can still be used
- how batch processing can be done responsibly

## Core Rule

Do not treat every source the same.

The ethical path depends on what kind of source it is and what rights or permissions are attached to it.

## Source Categories

### Public Domain

Examples:

- books from public-domain collections
- public-domain items from sources such as Open Library where the underlying work is actually public domain

Ethical handling:

- full local intake is acceptable
- extraction, chunking, summarization, and durable retention are acceptable
- batch processing is acceptable
- keep metadata showing where the source came from

### User-Owned Or User-Authored

Examples:

- your own writing
- your own notes
- material you created
- material you have a clear right to process locally

Ethical handling:

- normal local workflow is acceptable
- extraction, chunking, summarization, and durable retention are acceptable
- batch processing is acceptable

### Reference Summary Only

Examples:

- books or long-form texts you may read and study
- materials where you want notes and summaries but should avoid building a retained full-text copy

Ethical handling:

- summary-first workflow
- minimal retained excerpts only when truly needed
- preserve metadata, notes, tags, and summaries
- avoid durable full-text storage unless you have a clear right to keep it
- batch processing may be used only if retention rules stay summary-first

### Library Research Sources

Examples:

- books borrowed from Internet Archive controlled digital lending (1-hour borrows, 14-day loans)
- public library ebooks accessed via Libby, OverDrive, or similar platforms
- Google Books preview pages
- academic database articles with time-limited access
- legally accessed materials during temporary viewing windows

Ethical handling:

- borrow or access legally during the authorized window
- extract text via OCR or copy-paste for research notes only
- summarize immediately with local LLM during access period
- keep only summaries, key excerpts, and citations (per retention policy)
- delete full OCR text after summarization completes
- return or allow access to expire as required by the lending terms
- never redistribute extracted full text
- always maintain source attribution with ISBN, author, title, publisher, year
- track page ranges for citations
- treat extracted text as transient working material, not durable storage

Rights label: `summary-only`

Handling mode: `library-research` (automatically expires working text after summarization)

Retention: summaries and citations are durable; full extracted text is transient

### Manual Review Only

Examples:

- unclear-rights material
- sources with uncertain origin
- anything where the right workflow is not obvious

Ethical handling:

- do not run automatic batch processing yet
- require manual classification first
- allow metadata capture and review notes

## Ethical Operating Model

For the MVP, the safest useful model is:

1. classify the source first
2. assign a rights label and handling mode
3. only then allow extraction and summarization
4. retain summaries, notes, metadata, and traceability
5. avoid retaining unnecessary full text for restricted or unclear sources

## What Can Be Automated Ethically

These are good automation targets:

- source registration
- metadata capture
- rights-label selection prompts
- extraction for approved source categories
- chunking
- summarization
- tagging
- task generation
- batch queueing for approved items

## What Should Stay Controlled

These should not run blindly:

- processing of unclear-rights sources
- durable storage of extracted full text from restricted sources
- large batch runs across mixed-rights material without classification

## Recommended Handling Modes

Each source should get one handling mode:

- `full-processing`
- `summary-only`
- `manual-review-only`

### `full-processing`

Use for:

- public-domain material
- user-owned material

Allows:

- extraction
- chunking
- summarization
- durable storage according to the normal workflow

### `summary-only`

Use for:

- sources you can consult and study, but do not want to retain as full text

Allows:

- temporary working extraction if needed
- summarization
- durable notes and summaries
- durable metadata and traceability

Avoid:

- long-term retained full-text copies

### `manual-review-only`

Use for:

- unclear or mixed-rights material

Allows:

- metadata only
- review notes

Blocks:

- automatic batch summarization
- durable full-text processing until reviewed

## Batch Level

Yes, "batch level" is a useful way to think about it.

For this project, batch level means processing multiple approved sources through the same pipeline under one set of queue and retention rules.

### Ethical Batch Rules

- only batch items that already have a rights label
- only batch items whose handling mode allows the next step
- keep public-domain and restricted workflows separate
- keep logs and summaries, not uncontrolled raw text dumps
- stop the batch when an item enters `manual-review-only` or `failed`

### Suggested Batch Levels

#### Batch Level 1: Approved Summaries

Use for:

- public-domain sources
- user-owned sources
- summary-only sources that have already been classified

Behavior:

- register source
- extract according to handling mode
- chunk if allowed
- summarize
- save summaries, metadata, and task suggestions

#### Batch Level 2: Review Queue

Use for:

- unclear sources
- failed sources
- mixed-quality OCR or extraction

Behavior:

- capture metadata
- flag for manual review
- do not continue blindly into full summarization

## Open Library And Similar Sources

Ethically, the key question is not only where the source came from, but what category it belongs to.

Use this rule:

- if the item is public domain, it can go through `full-processing`
- if it is not clearly public domain, use `summary-only` or `manual-review-only`
- keep source attribution and rights labeling in metadata

## Durable Outputs

The safest durable outputs for broad use are:

- summaries
- your own notes
- tags
- outlines
- synthesis documents
- task items
- review decisions

## Practical MVP Rule

If you want a clear ethical path without freezing progress, use this default:

- public domain -> `full-processing`
- user-owned -> `full-processing`
- uncertain or restricted -> `summary-only` or `manual-review-only`

That gives you a usable system with automation while still respecting source boundaries.
