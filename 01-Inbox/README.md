# 01-Inbox: Universal Intake Folder

**Purpose:** Central location for all incoming content before processing into the system.

## Current Intake Folders

### ChatGPT-Exports/
URGENT: Export your 3 primary ChatGPT conversations here
- Drop ZIP, text, HTML, or JSON files
- Will be processed as "conversation-log" sources
- See `ChatGPT-Exports/README.md` for details

## Future Intake Folders (Create as Needed)

### Books/
- PDF books from library
- EPUB files
- Scanned book pages
- Text transcriptions

### Web-Research/
- Downloaded web articles
- Saved HTML pages
- Web scraping results

### Voice-Notes/
- Recorded audio files
- Whisper.cpp transcriptions
- Voice memos

### Documents/
- Business plans
- Strategic plans
- Personal writings

## Workflow

1. **Drop files in appropriate subfolder**
2. **System scans intake folders**
3. **Registers as Sources in database**
4. **Processes based on type:**
   - Conversation logs → chunk by turns → summarize
   - Books → OCR if needed → chunk by pages → summarize
   - Voice → transcribe → chunk by topic → summarize
5. **Files move from Inbox → processed locations**
6. **Searchable in database**

## Processing Pipeline

```
01-Inbox/[type]/file.ext
    ↓
Source created (database)
    ↓
Extract text / OCR / transcribe
    ↓
Chunk into manageable pieces
    ↓
Generate summaries
    ↓
Store in database (Sources + Summaries)
    ↓
File archived: artifacts/verify-files/
```

## Current Priority

🔴 **URGENT: ChatGPT-Exports/** - Export your 3 conversations TODAY
