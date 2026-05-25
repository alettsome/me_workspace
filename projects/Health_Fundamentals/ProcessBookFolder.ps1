# ProcessBookFolder.ps1
# OCR + Summarization pipeline for captured book pages

param(
    [Parameter(Mandatory=$true)]
    [string]$ISBN,
    
    [string]$InboxPath = "Research\01-Inbox",
    [string]$SummariesPath = "Research\summaries",
    [string]$OllamaUrl = "http://localhost:11434",
    [string]$Model = "phi3:3.8b"
)

$ErrorActionPreference = "Stop"

$bookFolder = Join-Path $InboxPath $ISBN

if (-not (Test-Path $bookFolder)) {
    Write-Host "❌ Folder not found: $bookFolder" -ForegroundColor Red
    exit 1
}

Write-Host "`n$('='*60)" -ForegroundColor Cyan
Write-Host "Processing Book: $ISBN" -ForegroundColor Cyan
Write-Host "$('='*60)`n" -ForegroundColor Cyan

# Load metadata
$metadataPath = Join-Path $bookFolder "metadata.json"
if (Test-Path $metadataPath) {
    $metadata = Get-Content $metadataPath | ConvertFrom-Json
    $title = $metadata.title
    $author = $metadata.author
    Write-Host "📚 Title: $title" -ForegroundColor Green
    Write-Host "✍️  Author: $author" -ForegroundColor Green
} else {
    $title = "Unknown"
    $author = "Unknown"
    Write-Host "⚠️  No metadata found" -ForegroundColor Yellow
}

# Check for Tesseract
$tesseractPath = "tesseract"
try {
    $null = & $tesseractPath --version 2>&1
} catch {
    Write-Host "❌ Tesseract OCR not found. Install from: https://github.com/UB-Mannheim/tesseract/wiki" -ForegroundColor Red
    exit 1
}

# Find all images
$images = Get-ChildItem -Path $bookFolder -Filter "*.png" | Sort-Object Name

if ($images.Count -eq 0) {
    Write-Host "❌ No PNG images found in $bookFolder" -ForegroundColor Red
    exit 1
}

Write-Host "`n📸 Found $($images.Count) images" -ForegroundColor Cyan
Write-Host "🔍 Starting OCR...`n" -ForegroundColor Cyan

# OCR all images
$fullText = ""
$pageCount = 0

foreach ($img in $images) {
    $pageCount++
    
    # Extract page number from filename (e.g., page-0001.png)
    if ($img.Name -match "page-(\d+)") {
        $pageNum = [int]$matches[1]
    } else {
        $pageNum = $pageCount
    }
    
    Write-Host "  Processing page $pageNum... " -NoNewline
    
    try {
        # Run Tesseract OCR
        $ocrOutput = & $tesseractPath $img.FullName stdout --psm 6 2>$null
        
        if ($ocrOutput) {
            $fullText += "`n`n=== Page $pageNum ===`n"
            $fullText += $ocrOutput
            Write-Host "✓" -ForegroundColor Green
        } else {
            Write-Host "✗ (empty)" -ForegroundColor Yellow
        }
    } catch {
        Write-Host "✗ (error)" -ForegroundColor Red
    }
}

Write-Host "`n✅ OCR complete: $($fullText.Length) characters extracted`n" -ForegroundColor Green

# Save transient OCR text (for debugging/review before deletion)
$ocrPath = Join-Path $bookFolder "full_text_TRANSIENT.txt"
$fullText | Out-File $ocrPath -Encoding UTF8
Write-Host "💾 Transient OCR saved: $ocrPath" -ForegroundColor Yellow

# Summarize with Ollama
Write-Host "`n🤖 Summarizing with $Model...`n" -ForegroundColor Cyan

$prompt = @"
Extract key health-related claims from this book: "$title" by $author

Your task:
1. Identify major health claims, protocols, vitamin/mineral recommendations
2. Include specific page citations for EVERY claim (format: p. X)
3. Note any contradictions or nuanced positions
4. Focus on actionable information

Book text:
$fullText
"@

$body = @{
    model = $Model
    prompt = $prompt
    stream = $false
    options = @{
        temperature = 0.3
        num_ctx = 8192
    }
} | ConvertTo-Json -Depth 10

try {
    $response = Invoke-RestMethod -Uri "$OllamaUrl/api/generate" -Method Post -Body $body -ContentType "application/json" -TimeoutSec 300
    $summary = $response.response
    
    Write-Host "✅ Summary generated`n" -ForegroundColor Green
    
} catch {
    Write-Host "❌ Summarization failed: $_" -ForegroundColor Red
    Write-Host "OCR text preserved in: $ocrPath" -ForegroundColor Yellow
    exit 1
}

# Save durable summary
if (-not (Test-Path $SummariesPath)) {
    New-Item -ItemType Directory -Path $SummariesPath -Force | Out-Null
}

$summaryPath = Join-Path $SummariesPath "$ISBN.md"

$summaryContent = @"
---
ISBN: $ISBN
Title: $title
Author: $author
Processed: $(Get-Date -Format "yyyy-MM-dd HH:mm:ss")
Source: Internet Archive
Rights: summary-only
Pages Processed: $($images.Count)
---

# Summary: $title

**Author:** $author  
**Source:** Internet Archive  
**Processing Date:** $(Get-Date -Format "yyyy-MM-dd")

## Key Claims and Findings

$summary

---

*Note: This summary was generated from page captures during temporary library access. Full text was not retained per ethical source handling policy. All page citations reference the original book.*
"@

$summaryContent | Out-File $summaryPath -Encoding UTF8

Write-Host "💾 Summary saved: $summaryPath`n" -ForegroundColor Green

# Cleanup transient data (CRITICAL for retention policy compliance)
Write-Host "🧹 Cleaning up transient data..." -ForegroundColor Cyan

try {
    # Delete images
    Remove-Item (Join-Path $bookFolder "*.png") -Force
    Write-Host "  ✓ Deleted PNG images" -ForegroundColor Green
    
    # Delete OCR text
    Remove-Item $ocrPath -Force
    Write-Host "  ✓ Deleted OCR text" -ForegroundColor Green
    
    # Keep only metadata and summary reference
    $keepFile = Join-Path $bookFolder "PROCESSED.txt"
    "Summary: $summaryPath`nProcessed: $(Get-Date)" | Out-File $keepFile -Encoding UTF8
    
    Write-Host "`n✅ Transient data deleted (retention policy compliant)" -ForegroundColor Green
    
} catch {
    Write-Host "⚠️  Warning: Cleanup failed: $_" -ForegroundColor Yellow
    Write-Host "   Please manually delete: $bookFolder\*.png and $ocrPath" -ForegroundColor Yellow
}

# Log completion
$logEntry = @{
    isbn = $ISBN
    title = $title
    author = $author
    processed_utc = (Get-Date).ToUniversalTime().ToString("o")
    pages_processed = $images.Count
    summary_path = $summaryPath
    ocr_chars = $fullText.Length
} | ConvertTo-Json

Add-Content "processing_log.jsonl" $logEntry

Write-Host "`n$('='*60)" -ForegroundColor Green
Write-Host "✅ COMPLETE: $title" -ForegroundColor Green
Write-Host "$('='*60)`n" -ForegroundColor Green
