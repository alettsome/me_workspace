# MasterBookProcessor.ps1
# Orchestrates entire workflow: browser automation → OCR → summarization → cleanup

param(
    [string]$BookListPath = "book_targets.json",
    [switch]$DryRun
)

$ErrorActionPreference = "Continue"

Write-Host "`n$('='*60)" -ForegroundColor Cyan
Write-Host "MASTER BOOK PROCESSOR" -ForegroundColor Cyan
Write-Host "$('='*60)`n" -ForegroundColor Cyan

# Check prerequisites
Write-Host "🔍 Checking prerequisites...`n" -ForegroundColor Cyan

# Check Python
try {
    $pythonVersion = & python --version 2>&1
    Write-Host "  ✓ Python: $pythonVersion" -ForegroundColor Green
} catch {
    Write-Host "  ✗ Python not found" -ForegroundColor Red
    exit 1
}

# Check Playwright
try {
    $null = & python -c "import playwright" 2>&1
    Write-Host "  ✓ Playwright installed" -ForegroundColor Green
} catch {
    Write-Host "  ✗ Playwright not installed" -ForegroundColor Red
    Write-Host "    Run: pip install playwright" -ForegroundColor Yellow
    Write-Host "    Then: playwright install chromium" -ForegroundColor Yellow
    exit 1
}

# Check Tesseract
try {
    $null = & tesseract --version 2>&1
    Write-Host "  ✓ Tesseract OCR installed" -ForegroundColor Green
} catch {
    Write-Host "  ✗ Tesseract not found" -ForegroundColor Red
    Write-Host "    Download: https://github.com/UB-Mannheim/tesseract/wiki" -ForegroundColor Yellow
    exit 1
}

# Check Ollama
try {
    $response = Invoke-WebRequest -Uri "http://localhost:11434/api/tags" -UseBasicParsing -TimeoutSec 2
    Write-Host "  ✓ Ollama is running" -ForegroundColor Green
} catch {
    Write-Host "  ✗ Ollama not reachable at http://localhost:11434" -ForegroundColor Red
    exit 1
}

Write-Host ""

# Load book list
if (-not (Test-Path $BookListPath)) {
    Write-Host "❌ Book list not found: $BookListPath" -ForegroundColor Red
    Write-Host "   Create book_targets.json with your book list" -ForegroundColor Yellow
    exit 1
}

$books = Get-Content $BookListPath | ConvertFrom-Json
Write-Host "📚 Loaded $($books.Count) books from $BookListPath`n" -ForegroundColor Green

if ($DryRun) {
    Write-Host "🔍 DRY RUN MODE - No processing will occur`n" -ForegroundColor Yellow
    foreach ($book in $books) {
        Write-Host "  - $($book.title) by $($book.author)" -ForegroundColor Cyan
        Write-Host "    ISBN: $($book.isbn)" -ForegroundColor Gray
        Write-Host "    Pages: $($book.page_ranges -join ', ')" -ForegroundColor Gray
    }
    exit 0
}

# Process books
$processed = 0
$failed = 0
$skipped = 0

foreach ($book in $books) {
    $isbn = $book.isbn
    $title = $book.title
    $author = $book.author
    
    Write-Host "`n$('='*60)" -ForegroundColor Cyan
    Write-Host "[$($processed + $failed + $skipped + 1)/$($books.Count)] $title" -ForegroundColor Cyan
    Write-Host "$('='*60)`n" -ForegroundColor Cyan
    
    # Check if already processed
    $summaryPath = "Research\summaries\$isbn.md"
    if (Test-Path $summaryPath) {
        Write-Host "⏭️  Already processed, skipping..." -ForegroundColor Yellow
        $skipped++
        continue
    }
    
    # Step 1: Browser automation (Python)
    Write-Host "🌐 Step 1: Browser automation (borrowing + capturing)..." -ForegroundColor Cyan
    
    $pageRangesStr = ($book.page_ranges | ForEach-Object { "$($_[0])-$($_[1])" }) -join ","
    
    try {
        & python internet_archive_automator.py `
            --isbn $isbn `
            --title $title `
            --author $author `
            --pages $pageRangesStr
        
        if ($LASTEXITCODE -ne 0) {
            Write-Host "❌ Browser automation failed" -ForegroundColor Red
            $failed++
            continue
        }
    } catch {
        Write-Host "❌ Browser automation error: $_" -ForegroundColor Red
        $failed++
        continue
    }
    
    # Wait for screenshots to appear
    $inboxPath = "Research\01-Inbox\$isbn"
    $maxWait = 30
    $waited = 0
    
    while (-not (Test-Path $inboxPath) -and $waited -lt $maxWait) {
        Start-Sleep -Seconds 1
        $waited++
    }
    
    if (-not (Test-Path $inboxPath)) {
        Write-Host "❌ Screenshots not found after $maxWait seconds" -ForegroundColor Red
        $failed++
        continue
    }
    
    # Step 2: OCR + Summarization (PowerShell)
    Write-Host "`n🔍 Step 2: OCR + Summarization..." -ForegroundColor Cyan
    
    try {
        & .\ProcessBookFolder.ps1 -ISBN $isbn
        
        if ($LASTEXITCODE -ne 0) {
            Write-Host "❌ Processing failed" -ForegroundColor Red
            $failed++
            continue
        }
        
        $processed++
        Write-Host "`n✅ Successfully processed: $title`n" -ForegroundColor Green
        
    } catch {
        Write-Host "❌ Processing error: $_" -ForegroundColor Red
        $failed++
        continue
    }
    
    # Rate limiting between books
    if (($processed + $failed) -lt $books.Count) {
        Write-Host "⏳ Waiting 90 seconds before next book..." -ForegroundColor Yellow
        Start-Sleep -Seconds 90
    }
}

# Final summary
Write-Host "`n$('='*60)" -ForegroundColor Green
Write-Host "BATCH COMPLETE" -ForegroundColor Green
Write-Host "$('='*60)`n" -ForegroundColor Green

Write-Host "✅ Processed: $processed" -ForegroundColor Green
Write-Host "❌ Failed: $failed" -ForegroundColor Red
Write-Host "⏭️  Skipped: $skipped" -ForegroundColor Yellow
Write-Host "📊 Total: $($books.Count)`n" -ForegroundColor Cyan

if ($processed -gt 0) {
    Write-Host "📁 Summaries saved in: Research\summaries\" -ForegroundColor Cyan
    Write-Host "📋 Processing log: processing_log.jsonl`n" -ForegroundColor Cyan
}
