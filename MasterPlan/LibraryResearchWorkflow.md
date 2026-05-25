# Library Research Workflow

## Quick Reference

This workflow allows ethical, scalable research from 100+ books using temporary library access.

## Legal Access Sources

### Internet Archive
- **Controlled Digital Lending** - 1-hour emergency borrows, 14-day standard loans
- Based on owned physical copies (1 copy = 1 loan at a time)
- URL: https://archive.org/details/books

### Public Libraries
- **Libby/OverDrive** - 2-3 week ebook loans
- **Hoopla** - Instant borrows (some systems)
- Check your local library's digital catalog

### Preview Systems
- **Google Books** - Preview pages for millions of books
- **Amazon Look Inside** - Preview chapters
- **Publisher websites** - Often have chapter previews

### Academic Access
- **JSTOR** - Academic articles and books (free tier available)
- **HathiTrust** - Research library consortium
- **University library access** - If affiliated with an institution

## Workflow Steps

### 1. Discovery (Phase 5)
```
Input: List of 100+ target books (ISBN, title, author)

Actions:
- Check availability across libraries and Internet Archive
- Create intake records with `library-research` source type
- Auto-assign `summary-only` rights label
- Track access windows (borrowed-until timestamp)
```

### 2. Access & Capture (Phase 5, 7)
```
During legal access window:

- Borrow book (1-hour to 14-day window)
- Screen capture key pages (ToC, relevant chapters)
- OR use browser copy-paste from preview systems
- Save captures to 01-Inbox/book-isbn/
- Record page ranges captured
```

### 3. OCR Extraction (Phase 7)
```
Tools: Tesseract OCR

Actions:
- Extract text from screen captures
- Validate OCR quality (confidence thresholds)
- Auto-detect page numbers
- Mark extracted text as TRANSIENT
- Queue for immediate summarization
```

### 4. Summarization (Phase 8)
```
Prompt: "Extract key claims about [topic] from this text. 
         Include page citations for all claims."

Actions:
- Process with local LLM (Phi-3, Llama, etc.)
- Parse page citations from OCR output
- Generate structured summary with citations
- Tag with ISBN for cross-referencing
- Mark summary as DURABLE
```

### 5. Cleanup (Phase 8)
```
Automatic after summarization:

- Delete full OCR text (per retention policy)
- Delete screen captures
- Keep only: summary + citations + metadata
- Update source status to "summarized"
```

### 6. Return/Expire (Phase 5)
```
- Return book (or allow to auto-expire)
- No full text retained beyond access window
- Summary and citations remain durable
```

## Batch Processing Example

### Scale: 100 Books in 2 Weeks

**Week 1:**
- Day 1-2: Build book list, check availability
- Day 3-7: Process first 25 books (5/day)
  - Borrow 5 books
  - Capture during access window
  - OCR + Summarize immediately
  - Return/expire access

**Week 2:**
- Day 8-14: Process remaining 75 books (10-12/day)
  - Same process, batch in groups of 10

**Parallelization:**
- Run 3-5 borrows simultaneously
- OCR and summarization run in parallel
- Can process 50+ books/week with good workflow

## Technical Requirements

### Tools Needed
- **Tesseract OCR** - Text extraction from images
- **Screen capture tool** - Built into Windows (Win+Shift+S) or ShareX
- **Local LLM** - Ollama + Phi-3 3.8B or Llama 3.1
- **Automation scripts** - PowerShell/Python for batch processing

### Storage Estimate
Per book (typical):
- Screen captures: 10-50 MB (temporary)
- OCR text: 200-500 KB (temporary)
- Summary: 5-20 KB (permanent)
- Metadata: 1 KB (permanent)

For 100 books:
- Peak working storage: 5 GB
- Permanent storage: 2-3 MB (summaries + metadata only)

## Ethical Boundaries

### ✅ Allowed
- Borrow from legal sources
- Take research notes (via OCR)
- Summarize with local AI
- Keep summaries + citations
- Process in batches
- Cite sources properly

### ❌ Not Allowed
- Keep full extracted text after summarization
- Bypass DRM
- Use pirated sources
- Redistribute extracted text
- Share full copies
- Keep books beyond lending period

## Citation Format

Every summary must include:

```
Source: [Title] by [Author]
Publisher: [Publisher], [Year]
ISBN: [ISBN]
Accessed: [Date] via [Source]
Pages: [Page ranges]
Summary created: [Timestamp]
```

## Error Handling

### OCR Failures
- Retry with image preprocessing
- Manual review if confidence < 80%
- Mark page ranges as "needs manual review"

### Access Expired
- Re-borrow if needed
- Or skip and mark as "incomplete"
- Summaries from partial access are still valuable

### Summarization Errors
- Retry with different prompt
- Fall back to shorter chunks
- Log error and continue with next book

## Integration with me_workspace

### Folder Structure
```
me_workspace/projects/Health_Fundamentals/Research/
├── library-sources/
│   ├── 9780123456789/         # ISBN-based folders
│   │   ├── metadata.json      # Book info, access window
│   │   ├── summary.md         # Durable summary
│   │   └── citations.txt      # Page-level citations
│   └── batch-2024-05-22/      # Batch tracking
└── summaries-index.json       # All summaries catalog
```

### Database Records
- Source record with `library-research` type
- ISBN, author, publisher in metadata fields
- Access expiry tracked
- Summary linked to source
- Extracted text marked transient (auto-deleted)

## Priority Rules

1. **Expiring access first** - Process books with < 24 hours remaining
2. **Batch by topic** - Group related books for efficient context
3. **High-value sources** - Prioritize primary sources over secondary
4. **Parallel processing** - Run 3-5 books simultaneously

## Success Metrics

- ✅ 100+ books processed in 2-4 weeks
- ✅ All summaries have proper citations
- ✅ No full text retained (retention policy compliant)
- ✅ All access returned/expired properly
- ✅ Cross-referenced by topic/ISBN
- ✅ Ready for book writing phase

## Next Steps

After implementing Phases 5-8 with library research support:

1. Build book list (100+ targets)
2. Set up Internet Archive account
3. Get public library card(s)
4. Install Tesseract OCR
5. Configure batch automation scripts
6. Start with 5-10 book pilot
7. Scale to full 100+ book research

## Questions?

This workflow is **legal, ethical, and scales to 100+ books**. It replicates traditional academic research (borrow → read → take notes → return) with modern tools (OCR + local LLM).
