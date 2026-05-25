$WebUiUrl = "http://localhost:3000"
$OllamaUrl = "http://localhost:11434"
$Email = "alettsome@hotmail.com"
$Password = "EqRC3nbP9XBPpwb"
$OutputFolder = "C:\me_workspace\projects\Health_Fundamentals\Research"
$BatchFolder = "C:\me_workspace\projects\Health_Fundamentals"
$BatchScriptPath = Join-Path $BatchFolder "BatchProcessor.ps1"
$ApiKeyPath = Join-Path $BatchFolder "OpenWebUI_API_Key.txt"
$ModelName = "phi3:3.8b"

if (!(Test-Path -LiteralPath $OutputFolder)) {
    New-Item -ItemType Directory -Force -Path $OutputFolder | Out-Null
}

if (!(Test-Path -LiteralPath $BatchFolder)) {
    New-Item -ItemType Directory -Force -Path $BatchFolder | Out-Null
}

Write-Host "Checking Open WebUI..." -ForegroundColor Cyan
try {
    $null = Invoke-WebRequest -Uri $WebUiUrl -UseBasicParsing
    Write-Host "Open WebUI is reachable at $WebUiUrl" -ForegroundColor Green
} catch {
    Write-Host "Open WebUI is not responding at $WebUiUrl" -ForegroundColor Red
    Write-Host $_.Exception.Message -ForegroundColor Red
    exit 1
}

try {
    Start-Process $WebUiUrl | Out-Null
    Write-Host "Opened Open WebUI in your browser." -ForegroundColor Green
} catch {
    Write-Host "Could not open the browser automatically. Use $WebUiUrl" -ForegroundColor Yellow
}

Write-Host "Signing in..." -ForegroundColor Cyan
try {
    $loginBody = @{
        email = $Email
        password = $Password
    } | ConvertTo-Json

    $loginResponse = Invoke-RestMethod -Uri "$WebUiUrl/api/v1/auths/signin" -Method Post -Body $loginBody -ContentType "application/json"

    if (-not $loginResponse.token) {
        throw "No token returned from Open WebUI."
    }

    $token = $loginResponse.token
    Write-Host "Login successful." -ForegroundColor Green
} catch {
    Write-Host "Login failed." -ForegroundColor Red
    Write-Host $_.Exception.Message -ForegroundColor Red
    exit 1
}

Write-Host "Creating API key..." -ForegroundColor Cyan
try {
    $headers = @{
        Authorization = "Bearer $token"
        "Content-Type" = "application/json"
    }

    $keyResponse = Invoke-RestMethod -Uri "$WebUiUrl/api/v1/auths/api_key" -Method Post -Headers $headers
    $apiKey = $keyResponse.api_key

    if ([string]::IsNullOrWhiteSpace($apiKey)) {
        throw "No API key returned from Open WebUI."
    }

    $apiKey | Set-Content -LiteralPath $ApiKeyPath -Encoding UTF8
    Write-Host "API key created." -ForegroundColor Green
    Write-Host "Saved to: $ApiKeyPath" -ForegroundColor Green
    Write-Host $apiKey -ForegroundColor Yellow
} catch {
    Write-Host "API key creation failed." -ForegroundColor Red
    Write-Host $_.Exception.Message -ForegroundColor Red
    exit 1
}

$batchScript = @'
$OllamaUrl = "__OLLAMA_URL__"
$ModelName = "__MODEL_NAME__"
$OutputFolder = "__OUTPUT_FOLDER__"

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
'@

$batchScript = $batchScript.Replace("__OLLAMA_URL__", $OllamaUrl)
$batchScript = $batchScript.Replace("__MODEL_NAME__", $ModelName)
$batchScript = $batchScript.Replace("__OUTPUT_FOLDER__", $OutputFolder)

$batchScript | Set-Content -LiteralPath $BatchScriptPath -Encoding UTF8

Write-Host ""
Write-Host "BatchProcessor.ps1 has been refreshed." -ForegroundColor Green
Write-Host "Run it with:" -ForegroundColor Cyan
Write-Host "powershell -ExecutionPolicy Bypass -File `"$BatchScriptPath`"" -ForegroundColor White
