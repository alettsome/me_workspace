#Requires -RunAsAdministrator
#Requires -Version 5.1

<#
.SYNOPSIS
    Sets up automated daily backups for me_workspace.

.DESCRIPTION
    Creates a Windows scheduled task to run backups daily at 2:00 AM.
    Tests backup/restore functionality before enabling automation.
    
.PARAMETER BackupRoot
    Root directory for backups. Defaults to C:\me_workspace_backups

.PARAMETER BackupTime
    Time to run daily backup (24-hour format). Defaults to "02:00"

.EXAMPLE
    .\Setup-BackupAutomation.ps1
    Sets up daily backup at 2:00 AM

.EXAMPLE
    .\Setup-BackupAutomation.ps1 -BackupTime "14:30"
    Sets up daily backup at 2:30 PM
#>

[CmdletBinding()]
param(
    [Parameter()]
    [string]$BackupRoot = "C:\me_workspace_backups",
    
    [Parameter()]
    [string]$BackupTime = "02:00"
)

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'

$WorkspaceRoot = Split-Path -Parent $PSScriptRoot

Write-Host "=== me_workspace Backup Automation Setup ===" -ForegroundColor Cyan
Write-Host ""
Write-Host "This script will:" -ForegroundColor Green
Write-Host "  1. Create backup root directory"
Write-Host "  2. Test backup functionality"
Write-Host "  3. Test restore functionality"
Write-Host "  4. Create scheduled task for daily backups"
Write-Host ""

# Step 1: Create backup root
Write-Host "[Step 1] Creating backup root directory..." -ForegroundColor Cyan
if (-not (Test-Path $BackupRoot)) {
    New-Item -ItemType Directory -Path $BackupRoot -Force | Out-Null
    Write-Host "✓ Created: $BackupRoot" -ForegroundColor Green
}
else {
    Write-Host "✓ Already exists: $BackupRoot" -ForegroundColor Green
}
Write-Host ""

# Step 2: Test backup
Write-Host "[Step 2] Testing backup functionality..." -ForegroundColor Cyan
$backupScript = Join-Path $PSScriptRoot "Backup-Workspace.ps1"
if (-not (Test-Path $backupScript)) {
    Write-Host "✗ ERROR: Backup script not found: $backupScript" -ForegroundColor Red
    exit 1
}

try {
    $testBackup = & $backupScript -BackupRoot $BackupRoot -WorkspaceRoot $WorkspaceRoot
    Write-Host "✓ Backup test successful" -ForegroundColor Green
    Write-Host "  Files backed up: $($testBackup.FilesBackedUp)" -ForegroundColor Gray
    Write-Host "  Size: $($testBackup.TotalSizeMB) MB" -ForegroundColor Gray
    Write-Host "  Location: $($testBackup.BackupLocation)" -ForegroundColor Gray
}
catch {
    Write-Host "✗ ERROR: Backup test failed: $($_.Exception.Message)" -ForegroundColor Red
    exit 1
}
Write-Host ""

# Step 3: Test restore (dry-run style - just verify script works)
Write-Host "[Step 3] Testing restore functionality..." -ForegroundColor Cyan
$restoreScript = Join-Path $PSScriptRoot "Restore-Workspace.ps1"
if (-not (Test-Path $restoreScript)) {
    Write-Host "✗ ERROR: Restore script not found: $restoreScript" -ForegroundColor Red
    exit 1
}

try {
    # Just list backups to verify restore script works
    $null = & $restoreScript -BackupRoot $BackupRoot -ErrorAction Stop
    Write-Host "✓ Restore script validated" -ForegroundColor Green
    Write-Host "  NOTE: Full restore test requires manual verification" -ForegroundColor Yellow
}
catch {
    Write-Host "✗ ERROR: Restore script validation failed: $($_.Exception.Message)" -ForegroundColor Red
    exit 1
}
Write-Host ""

# Step 4: Create scheduled task
Write-Host "[Step 4] Creating scheduled task..." -ForegroundColor Cyan

$taskName = "me_workspace_DailyBackup"
$taskDescription = "Automated daily backup of me_workspace critical data"

# Remove existing task if present
$existingTask = Get-ScheduledTask -TaskName $taskName -ErrorAction SilentlyContinue
if ($existingTask) {
    Write-Host "  Removing existing task..." -ForegroundColor Yellow
    Unregister-ScheduledTask -TaskName $taskName -Confirm:$false
}

# Parse backup time
$timeParts = $BackupTime -split ':'
if ($timeParts.Count -ne 2) {
    Write-Host "✗ ERROR: Invalid time format. Use HH:mm (e.g., '02:00')" -ForegroundColor Red
    exit 1
}

# Create scheduled task action
$action = New-ScheduledTaskAction `
    -Execute "PowerShell.exe" `
    -Argument "-NoProfile -ExecutionPolicy Bypass -File `"$backupScript`" -BackupRoot `"$BackupRoot`" -WorkspaceRoot `"$WorkspaceRoot`"" `
    -WorkingDirectory $PSScriptRoot

# Create trigger (daily at specified time)
$trigger = New-ScheduledTaskTrigger -Daily -At $BackupTime

# Create settings
$settings = New-ScheduledTaskSettings `
    -AllowStartIfOnBatteries `
    -DontStopIfGoingOnBatteries `
    -StartWhenAvailable `
    -RunOnlyIfNetworkAvailable:$false

# Create principal (run as current user)
$principal = New-ScheduledTaskPrincipal `
    -UserId $env:USERNAME `
    -LogonType S4U `
    -RunLevel Highest

# Register task
try {
    Register-ScheduledTask `
        -TaskName $taskName `
        -Description $taskDescription `
        -Action $action `
        -Trigger $trigger `
        -Settings $settings `
        -Principal $principal `
        -Force | Out-Null
    
    Write-Host "✓ Scheduled task created: $taskName" -ForegroundColor Green
    Write-Host "  Runs daily at: $BackupTime" -ForegroundColor Gray
    Write-Host "  Backup location: $BackupRoot" -ForegroundColor Gray
}
catch {
    Write-Host "✗ ERROR: Failed to create scheduled task: $($_.Exception.Message)" -ForegroundColor Red
    exit 1
}
Write-Host ""

# Summary
Write-Host "=== Setup Complete ===" -ForegroundColor Cyan
Write-Host ""
Write-Host "✓ Backup system is ready!" -ForegroundColor Green
Write-Host ""
Write-Host "Next steps:" -ForegroundColor Yellow
Write-Host "  1. Review backup in: $BackupRoot"
Write-Host "  2. Test restore manually with: .\Restore-Workspace.ps1"
Write-Host "  3. Verify scheduled task in Task Scheduler"
Write-Host ""
Write-Host "Manual backup command:" -ForegroundColor Yellow
Write-Host "  .\Backup-Workspace.ps1"
Write-Host ""
Write-Host "Manual restore command:" -ForegroundColor Yellow
Write-Host "  .\Restore-Workspace.ps1 -BackupTimestamp ""yyyy-MM-dd_HH-mm-ss"""
Write-Host ""
Write-Host "View scheduled task:" -ForegroundColor Yellow
Write-Host "  Get-ScheduledTask -TaskName '$taskName'"
Write-Host ""
