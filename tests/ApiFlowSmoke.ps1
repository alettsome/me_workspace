param(
    [string]$BaseUrl = "http://127.0.0.1:5090"
)

$ErrorActionPreference = "Stop"

function Get-DotNetPath {
    $command = Get-Command dotnet -ErrorAction SilentlyContinue
    if ($command) {
        return $command.Source
    }

    $fallback = "C:\Program Files\dotnet\dotnet.exe"
    if (Test-Path $fallback) {
        return $fallback
    }

    throw "Could not find dotnet."
}

function Wait-ForHealth {
    param(
        [string]$Url,
        [int]$Attempts = 30
    )

    for ($attempt = 0; $attempt -lt $Attempts; $attempt++) {
        try {
            $response = Invoke-RestMethod "$Url/api/health" -TimeoutSec 2
            if ($response.status -eq "ok") {
                return
            }
        }
        catch {
            Start-Sleep -Milliseconds 500
        }
    }

    throw "The app did not become healthy at $Url."
}

function Assert-True {
    param(
        [bool]$Condition,
        [string]$Message
    )

    if (-not $Condition) {
        throw $Message
    }
}

$root = Split-Path -Parent $PSScriptRoot
$projectPath = Join-Path $root "src\me_workspace.Web\me_workspace.Web.csproj"
$contentRoot = Join-Path $root "src\me_workspace.Web"
$buildOutput = Join-Path $root "artifacts\tests\ApiFlowSmoke\"
$appDll = Join-Path $buildOutput "me_workspace.Web.dll"
$dotNet = Get-DotNetPath
$process = $null

try {
    & $dotNet build $projectPath "-p:OutDir=$buildOutput" "-p:UseAppHost=false" | Out-Null

    if (-not (Test-Path $appDll)) {
        throw "Expected compiled app at $appDll."
    }

    $process = Start-Process `
        -FilePath $dotNet `
        -ArgumentList @($appDll, "--urls", $BaseUrl, "--contentRoot", $contentRoot) `
        -WorkingDirectory $root `
        -WindowStyle Hidden `
        -PassThru

    Wait-ForHealth -Url $BaseUrl

    $health = Invoke-RestMethod "$BaseUrl/api/health"
    Assert-True ($health.mode -eq "offline-local") "Expected offline-local mode."
    Assert-True ($health.speech -eq "local-draft") "Expected local-draft speech status."

    $flow = Invoke-RestMethod "$BaseUrl/api/system/flow"
    Assert-True ($flow.connections.Count -ge 5) "Expected at least five connected feature entries."
    Assert-True (($flow.connections.name -contains "voice")) "Expected voice connection status."
    Assert-True (($flow.connections.name -contains "files")) "Expected file connection status."

    $fileTree = Invoke-RestMethod "$BaseUrl/api/files/tree"
    Assert-True ($fileTree.Count -ge 1) "Expected at least one approved file root."

    $filePreview = Invoke-RestMethod "$BaseUrl/api/files/preview?path=docs/phases.md"
    Assert-True ($filePreview.relativePath -eq "docs/phases.md") "Expected phases preview path."
    Assert-True ($filePreview.contentPreview -match "me_workspace Phases") "Expected phases preview content."

    $memoryItems = Invoke-RestMethod "$BaseUrl/api/memory/items"
    Assert-True ($memoryItems.Count -ge 3) "Expected seeded memory items."

    $journalEntries = Invoke-RestMethod "$BaseUrl/api/journal/entries"
    Assert-True ($journalEntries.Count -ge 1) "Expected at least one journal entry."
    $journalEntry = $journalEntries[0]

    $createdMemory = Invoke-RestMethod "$BaseUrl/api/memory/items" `
        -Method Post `
        -ContentType "application/json" `
        -Body '{"key":"smoke-test","content":"Created during the smoke test.","pinned":true}'

    Assert-True ($createdMemory.key -eq "smoke-test") "Expected created memory item."

    $updatedMemory = Invoke-RestMethod "$BaseUrl/api/memory/items/$($createdMemory.id)" `
        -Method Put `
        -ContentType "application/json" `
        -Body '{"key":"smoke-test","content":"Updated during the smoke test.","pinned":false}'

    Assert-True ($updatedMemory.content -eq "Updated during the smoke test.") "Expected updated memory content."
    Assert-True ($updatedMemory.pinned -eq $false) "Expected updated pinned flag."

    Invoke-RestMethod "$BaseUrl/api/memory/items/$($createdMemory.id)" -Method Delete | Out-Null

    $voice = Invoke-RestMethod "$BaseUrl/api/voice/demo-transcript" -Method Post
    Assert-True (-not [string]::IsNullOrWhiteSpace($voice.text)) "Expected a demo voice transcript."

    $conversation = Invoke-RestMethod "$BaseUrl/api/chat/conversations" `
        -Method Post `
        -ContentType "application/json" `
        -Body "{""title"":""New Chat"",""journalEntryId"":""$($journalEntry.id)""}"

    $message = Invoke-RestMethod "$BaseUrl/api/chat/conversations/$($conversation.id)/messages" `
        -Method Post `
        -ContentType "application/json" `
        -Body "{""content"":""Please confirm the smoke test flow."",""filePaths"":[""docs/phases.md""],""journalEntryId"":""$($journalEntry.id)""}"

    Assert-True ($message.messages.Count -eq 2) "Expected a user and assistant message."
    Assert-True ($message.messages[0].role -eq "user") "Expected the first message to be from the user."
    Assert-True ($message.messages[1].role -eq "assistant") "Expected the second message to be from the assistant."
    Assert-True ($message.journalEntryId -eq $journalEntry.id) "Expected the conversation to stay linked to the selected journal entry."
    Assert-True ($message.journalTitle -eq $journalEntry.title) "Expected the journal title on the conversation."
    Assert-True ($message.messages[0].fileContexts.Count -eq 1) "Expected the user message to log one attached file."
    Assert-True ($message.messages[0].fileContexts[0].relativePath -eq "docs/phases.md") "Expected the logged file path on the user message."
    Assert-True ($message.messages[1].content -match "Local assistant pipeline is connected") "Expected the assistant pipeline confirmation."
    Assert-True ($message.messages[1].content -match "files:1") "Expected attached file context to be included."
    Assert-True ($message.messages[1].content -match "Journal focus:") "Expected journal context to be included."

    $generalConversation = Invoke-RestMethod "$BaseUrl/api/chat/conversations" `
        -Method Post `
        -ContentType "application/json" `
        -Body '{"title":"General Chat"}'

    $generalMessage = Invoke-RestMethod "$BaseUrl/api/chat/conversations/$($generalConversation.id)/messages" `
        -Method Post `
        -ContentType "application/json" `
        -Body "{""content"":""Please adopt journal context before send."",""filePaths"":[],""journalEntryId"":""$($journalEntry.id)""}"

    Assert-True ($generalMessage.journalEntryId -eq $journalEntry.id) "Expected an unlinked chat to adopt the journal on send."
    Assert-True ($generalMessage.journalTitle -eq $journalEntry.title) "Expected the adopted journal title on the conversation."
    Assert-True ($generalMessage.messages.Count -eq 2) "Expected the general chat to save a user and assistant message."

    $journalDetail = Invoke-RestMethod "$BaseUrl/api/journal/entries/$($journalEntry.id)"
    Assert-True ($journalDetail.logs.Count -ge 2) "Expected at least two saved journal logs after both linked sends."
    Assert-True ($journalDetail.logs[0].relativePath -match "^Journals/") "Expected journal logs to live under the Journals folder."

    Write-Host "API smoke test passed."
    Write-Host "Health: $($health | ConvertTo-Json -Compress)"
    Write-Host "Conversation: $($message | ConvertTo-Json -Depth 6 -Compress)"
}
finally {
    if ($process -and -not $process.HasExited) {
        Stop-Process -Id $process.Id
    }
}
