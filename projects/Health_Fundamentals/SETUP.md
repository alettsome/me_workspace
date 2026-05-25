# Internet Archive Automation Setup

## Prerequisites

### 1. Install Python (if not already installed)
Download from: https://www.python.org/downloads/

Verify installation:
```powershell
python --version
```

### 2. Install Playwright
```powershell
pip install playwright
playwright install chromium
```

### 3. Install Tesseract OCR
Download from: https://github.com/UB-Mannheim/tesseract/wiki

**Important:** During installation, note the installation path (e.g., `C:\Program Files\Tesseract-OCR`)

Add Tesseract to your PATH:
```powershell
$env:PATH += ";C:\Program Files\Tesseract-OCR"
```

Or add permanently via System Environment Variables.

Verify installation:
```powershell
tesseract --version
```

### 4. Ensure Ollama is Running
```powershell
ollama list
```

If Ollama is not installed, download from: https://ollama.com/download

Pull the model:
```powershell
ollama pull phi3:3.8b
```

### 5. Create Required Folders
```powershell
New-Item -ItemType Directory -Path "Research\01-Inbox" -Force
New-Item -ItemType Directory -Path "Research\summaries" -Force
```

## Configuration

### Edit `book_targets.json`
Add your target books with Internet Archive identifiers:

```json
[
  {
    "isbn": "archive_identifier_here",
    "title": "Book Title",
    "author": "Author Name",
    "page_ranges": [[1, 50], [100, 150]]
  }
]
```

**Finding Archive Identifiers:**
1. Go to https://archive.org
2. Search for your book
3. The identifier is in the URL: `archive.org/details/{identifier}`

### Adjust `config.json` (optional)
- `screenshot_delay_ms`: Time between page captures (default: 1500ms)
- `headless`: Set to `true` to hide browser (default: false for debugging)
- `model`: Ollama model to use (default: "phi3:3.8b")

## Usage

### Test with Single Book
```powershell
python internet_archive_automator.py --isbn "vitaminDsolution00holick" --title "The Vitamin D Solution" --author "Michael Holick" --pages "10-50,80-120"
```

### Process Batch
```powershell
.\MasterBookProcessor.ps1
```

### Dry Run (see what will be processed)
```powershell
.\MasterBookProcessor.ps1 -DryRun
```

### Process Single Book from Captured Images
If you already have screenshots:
```powershell
.\ProcessBookFolder.ps1 -ISBN "vitaminDsolution00holick"
```

## Workflow

```
1. MasterBookProcessor.ps1 (orchestrates everything)
   ↓
2. internet_archive_automator.py (browser automation)
   → Borrows book
   → Captures pages
   → Returns book
   → Saves to Research/01-Inbox/{ISBN}/
   ↓
3. ProcessBookFolder.ps1 (OCR + summarization)
   → OCR images with Tesseract
   → Summarize with Ollama
   → Delete transient data
   → Save summary to Research/summaries/{ISBN}.md
```

## Output Structure

```
Research/
├── 01-Inbox/
│   └── {ISBN}/
│       ├── metadata.json
│       └── PROCESSED.txt
├── summaries/
│   ├── vitaminDsolution00holick.md
│   └── magnesiummiracl00dean.md
└── processing_log.jsonl
```

## Troubleshooting

### "Tesseract not found"
- Verify Tesseract is in PATH: `tesseract --version`
- Manually add to PATH or specify full path in script

### "Ollama not reachable"
- Start Ollama: `ollama serve` (in separate terminal)
- Check http://localhost:11434 in browser

### "Failed to borrow book"
- Book may not be available for borrowing
- May require Internet Archive account (create free account at archive.org)
- Check if book is waitlisted

### Browser automation fails
- Ensure Playwright browsers installed: `playwright install chromium`
- Try with `headless: false` in config.json to watch what happens
- Internet Archive may change their interface; script may need updates

### OCR quality poor
- Adjust `screenshot_delay_ms` in config.json (increase for slower connections)
- Manually review images in 01-Inbox before OCR
- Consider using higher quality captures

## Performance

**Single book (50 pages):**
- Browser capture: 2-3 minutes
- OCR: 1-2 minutes  
- Summarization: 2-5 minutes (depends on model/hardware)
- Total: ~5-10 minutes per book

**Batch (100 books):**
- With automation: 10-15 hours (overnight run)
- Manual: 25-30 hours

## Ethical Compliance

✅ This system follows your retention policy:
- Borrows books legally from Internet Archive
- Captures pages during authorized access window
- Returns books immediately after capture
- OCR text marked as TRANSIENT
- Full OCR text automatically deleted after summarization
- Only summaries + citations retained (DURABLE)
- All sources properly attributed

## Next Steps

1. Install all prerequisites above
2. Test with 1-2 books first
3. Review summaries for quality
4. Adjust prompts/config if needed
5. Scale to full batch

## Support

For issues with:
- Internet Archive access: archive.org/about/contact.php
- Tesseract: github.com/tesseract-ocr/tesseract/issues
- Playwright: playwright.dev/python/docs/intro
- This automation: check processing_log.jsonl for errors
