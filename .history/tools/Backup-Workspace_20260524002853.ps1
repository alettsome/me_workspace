#Requires -Version 5.1

<#
.SYNOPSIS
    Creates immutable backup of me_workspace critical data.

.DESCRIPTION
    Backs up SQLite database, configuration files, journals, and critical planning documents
    to a separate directory with read-only permissions to prevent accidental deletion.
    
    Retention: Last 30 days of backups
    
.PARAMETER BackupRoot
    Root directory for backups. Defaults to C:\me_workspace_backups

.PARAMETER WorkspaceRoot
    Workspace root directory. Defaults to parent of script location.

.EXAMPLE
    .\Backup-Workspace.ps1
    Creates timestamped backup with default settings

.EXAMPLE
    .\Backup-Workspace.ps1 -BackupRoot "D:\Backups\me_workspace"
    Creates backup at custom location
#>

[CmdletBinding()]
param(
    [Parameter()]
    [string]$BackupRoot = "C:\me_workspace_backups",
    
    [Parameter()]
    [string]$WorkspaceRoot = (Split-Path -Parent $PSScriptRoot)
)

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'

# Create timestamp for this backup
$timestamp = Get-Date -Format "yyyy-MM-dd_HH-mm-ss"
$backupDir = Join-Path $BackupRoot $timestamp

Write-Host "=== me_workspace Backup ===" -ForegroundColor Cyan
Write-Host "Timestamp: $timestamp"
Write-Host "Workspace: $WorkspaceRoot"
Write-Host "Backup to: $backupDir"
Write-Host ""

# Ensure backup root exists
if (-not (Test-Path $BackupRoot)) {
    Write-Host "Creating backup root directory: $BackupRoot" -ForegroundColor Yellow
    New-Item -ItemType Directory -Path $BackupRoot -Force | Out-Null
}

# Create timestamped backup directory
Write-Host "Creating backup directory..." -ForegroundColor Green
New-Item -ItemType Directory -Path $backupDir -Force | Out-Null

# Define what to backup
$backupItems = @(
    @{
        Name = "Database"
        Source = "App_Data\me_workspace.db"
        Destination = "App_Data"
    },
    @{
        Name = "App Settings"
        Source = "src\me_workspace.Web\appsettings.json"
        Destination = "Config"
    },
    @{
        Name = "Development Settings"
        Source = "src\me_workspace.Web\appsettings.Development.json"
        Destination = "Config"
    },
    @{
        Name = "Master Plan"
        Source = "MasterPlan"
        Destination = "MasterPlan"
    },
    @{
        Name = "Journals"
        Source = "Journals"
        Destination = "Journals"
    },
    @{
        Name = "Documentation"
        Source = "docs"
        Destination = "docs"
    },
    @{
        Name = "ThingsToDo"
        Source = "ThingsToDo"
        Destination = "ThingsToDo"
    }
)

# Perform backup
$totalSize = 0
$filesBackedUp = 0

foreach ($item in $backupItems) {
    $sourcePath = Join-Path $WorkspaceRoot $item.Source
    $destPath = Join-Path $backupDir $item.Destination
    
    if (Test-Path $sourcePath) {
        Write-Host "Backing up $($item.Name)..." -ForegroundColor Green
        
        # Create destination directory
        $destParent = Split-Path -Parent $destPath
        if ($destParent -and -not (Test-Path $destParent)) {
            New-Item -ItemType Directory -Path $destParent -Force | Out-Null
        }
        
        # Copy files
        if (Test-Path $sourcePath -PathType Container) {
            # Directory - copy recursively
            Copy-Item -Path $sourcePath -Destination $destPath -Recurse -Force
            
            # Count files and size
            $files = Get-ChildItem -Path $destPath -Recurse -File
            $filesBackedUp += $files.Count
            $sizeSum = ($files | Measure-Object -Property Length -Sum).Sum
            if ($sizeSum) {
                $totalSize += $sizeSum
            }
        }
        else {
            # Single file
            Copy-Item -Path $sourcePath -Destination $destPath -Force
            $filesBackedUp++
            $totalSize += (Get-Item $sourcePath).Length
        }
        
        Write-Host "  [OK] $($item.Name) backed up" -ForegroundColor Gray
    }
    else {
        Write-Host "  [SKIP] $($item.Name) not found, skipping" -ForegroundColor Yellow
    }
}

# Set backup directory to read-only (helps prevent accidental deletion)
Write-Host ""
Write-Host "Setting read-only permissions..." -ForegroundColor Green
Get-ChildItem -Path $backupDir -Recurse -File | ForEach-Object {
    $_.IsReadOnly = $true
}

# Create backup manifest
$manifest = @{
    Timestamp = $timestamp
    WorkspaceRoot = $WorkspaceRoot
    BackupLocation = $backupDir
    FilesBackedUp = $filesBackedUp
    TotalSizeBytes = $totalSize
    TotalSizeMB = [math]::Round($totalSize / 1MB, 2)
    BackupItems = $backupItems | ForEach-Object { $_.Name }
}

$manifestPath = Join-Path $backupDir "backup-manifest.json"
$manifest | ConvertTo-Json -Depth 3 | Set-Content $manifestPath
(Get-Item $manifestPath).IsReadOnly = $true

# Clean up old backups (keep last 30 days)
Write-Host ""
Write-Host "Cleaning up old backups (keeping last 30 days)..." -ForegroundColor Green
$cutoffDate = (Get-Date).AddDays(-30)
$oldBackups = Get-ChildItem -Path $BackupRoot -Directory | Where-Object {
    $_.Name -match '^\d{4}-\d{2}-\d{2}_\d{2}-\d{2}-\d{2}$' -and
    $_.CreationTime -lt $cutoffDate
}

if ($oldBackups) {
    foreach ($oldBackup in $oldBackups) {
        Write-Host "  Removing old backup: $($oldBackup.Name)" -ForegroundColor Gray
        
        # Remove read-only attribute before deleting
        Get-ChildItem -Path $oldBackup.FullName -Recurse -File | ForEach-Object {
            $_.IsReadOnly = $false
        }
        
        Remove-Item -Path $oldBackup.FullName -Recurse -Force
    }
}
else {
    Write-Host "  No old backups to remove" -ForegroundColor Gray
}

# Summary
Write-Host ""
Write-Host "=== Backup Complete ===" -ForegroundColor Cyan
Write-Host "Location: $backupDir"
Write-Host "Files backed up: $filesBackedUp"
Write-Host "Total size: $($manifest.TotalSizeMB) MB"
Write-Host ""
Write-Host "[SUCCESS] Backup successful" -ForegroundColor Green
Write-Host ""

# Return manifest for potential automation
return $manifest
