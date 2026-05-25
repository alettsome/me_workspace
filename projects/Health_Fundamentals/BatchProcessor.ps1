$OllamaUrl = "http://localhost:11434"
$ModelName = "phi3:3.8b"
$OutputFolder = "C:\me_workspace\projects\Health_Fundamentals\Research"

if (!(Test-Path -LiteralPath $OutputFolder)) {
    New-Item -ItemType Directory -Force -Path $OutputFolder | Out-Null
}

Write-Host "Batch processor is ready." -ForegroundColor Green
Write-Host "Copy text to the clipboard to summarize it." -ForegroundColor Cyan
Write-Host "Press Ctrl+C to stop." -ForegroundColor Cyan

$counter = 0
$lastClipText = ""

while ($true) {
    $clipText = Get-Clipboard -ErrorAction SilentlyContinue

    if ([string]::IsNullOrWhiteSpace($clipText)) {
        Start-Sleep -Seconds 2
        continue
    }

    if ($clipText -eq $lastClipText) {
        Start-Sleep -Seconds 2
        continue
    }

    $lastClipText = $clipText
    $counter++
    $fileName = "Summary_Page_$counter.txt"
    $filePath = Join-Path $OutputFolder $fileName

    Write-Host ""
    Write-Host "Processing clipboard item $counter..." -ForegroundColor Yellow

    $prompt = @"
Summarize the following text.
Focus on key health, nutrition, and protein facts.
Output only the cleaned summary.

TEXT:
${clipText}
"@

    $body = @{
        model = $ModelName
        prompt = $prompt
        stream = $false
    } | ConvertTo-Json -Depth 5

    try {
        $response = Invoke-RestMethod -Uri "$OllamaUrl/api/generate" -Method Post -Body $body -ContentType "application/json"
        $summary = $response.response

        if ([string]::IsNullOrWhiteSpace($summary)) {
            throw "The model returned an empty summary."
        }

        $summary | Set-Content -LiteralPath $filePath -Encoding UTF8
        Write-Host "Saved: $filePath" -ForegroundColor Green
        Set-Clipboard -Value "Saved: $fileName"
    } catch {
        Write-Host "Processing failed." -ForegroundColor Red
        Write-Host $_ -ForegroundColor Red
    }
}
