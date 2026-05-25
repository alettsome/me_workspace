# Phase 3: Summarization

## Goal
Turn raw source material into concise, reusable research notes.

## Technology Used
- `Ollama`
- `phi3:3.8b`
- `Open WebUI`
- `BatchProcessor.ps1`
- `ShareX` when OCR is needed

## What Happens In This Phase
- One source or one page enters the summary workflow
- The system produces a summary in your own words
- The summary gets saved with source metadata

## Preferred Processing Order
1. Direct text import
2. PDF import
3. Transcript import
4. OCR only when needed

## Standard Output Shape
- Summary
- Main points
- Key claims
- Caveats
- Questions to verify
- Tags
- Source reference

## Book Workflow
1. View one page or one small section lawfully
2. Generate a summary
3. Save only the summary plus page reference and tags
4. Move to the next page or section

## Checklist
- [ ] Create a standard summary template
- [ ] Confirm first-pass prompt wording
- [ ] Test the workflow on chats
- [ ] Test the workflow on one PDF
- [ ] Test the workflow on one book section
- [ ] Save summaries in an organized folder
- [ ] Confirm temporary working text is not retained when it should not be

## Done Looks Like
- Every processed item becomes a clean research note instead of a messy text dump
