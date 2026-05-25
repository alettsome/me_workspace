#Requires -Version 5.1

<#
.SYNOPSIS
    Restores me_workspace from an immutable backup.

.DESCRIPTION
    Restores SQLite database, configuration files, journals, and planning documents
    from a timestamped backup directory.
    
    WARNING: This will overwrite current workspace data. Make sure you want to do this.
    
.PARAMETER BackupTimestamp
    Timestamp of backup to restore (format: yyyy-MM-dd_HH-mm-ss)
    If not specified, lists available backups.

.PARAMETER BackupRoot
    Root directory for backups. Defaults to C:\me_workspace_backups

.PARAMETER WorkspaceRoot
    Workspace root directory. Defaults to parent of script location.

.PARAMETER Force
    Skip confirmation prompt (use with caution)

.EXAMPLE
    .\Restore-Workspace.ps1
    Lists available backups

.EXAMPLE
    .\Restore-Workspace.ps1 -BackupTimestamp "2026-05-24_14-30-00"
    Restores from specific backup with confirmation

.EXAMPLE
    .\Restore-Workspace.ps1 -BackupTimestamp "2026-05-24_14-30-00" -Force
    Restores without confirmation (dangerous!)
#>

[CmdletBinding()]
param(
    [Parameter()]
    [string]$BackupTimestamp,
    
    [Parameter()]
    [string]$BackupRoot = "C:\me_workspace_backups",
    
    [Parameter()]
    [string]$WorkspaceRoot = (Split-Path -Parent $PSScriptRoot),
    
    [Parameter()]
    [switch]$Force
)

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'

Write-Host "=== me_workspace Restore ===" -ForegroundColor Cyan
Write-Host ""

# Check if backup root exists
if (-not (Test-Path $BackupRoot)) {
    Write-Host "ERROR: Backup root not found: $BackupRoot" -ForegroundColor Red
    exit 1
}

# Get available backups
$availableBackups = Get-ChildItem -Path $BackupRoot -Directory | 
    Where-Object { $_.Name -match '^\d{4}-\d{2}-\d{2}_\d{2}-\d{2}-\d{2}$' } |
    Sort-Object Name -Descending

if ($availableBackups.Count -eq 0) {
    Write-Host "No backups found in: $BackupRoot" -ForegroundColor Yellow
    exit 0
}

# If no timestamp specified, list available backups
if (-not $BackupTimestamp) {
    Write-Host "Available backups:" -ForegroundColor Green
    Write-Host ""
    
    foreach ($backup in $availableBackups) {
        $manifestPath = Join-Path $backup.FullName "backup-manifest.json"
        
        if (Test-Path $manifestPath) {
            $manifest = Get-Content $manifestPath | ConvertFrom-Json
            Write-Host "  $($backup.Name)" -ForegroundColor Cyan
            Write-Host "    Files: $($manifest.FilesBackedUp)"
            Write-Host "    Size: $($manifest.TotalSizeMB) MB"
            Write-Host "    Date: $($backup.CreationTime)"
        }
        else {
            Write-Host "  $($backup.Name)" -ForegroundColor Yellow
            Write-Host "    (No manifest found)"
        }
        Write-Host ""
    }
    
    Write-Host "To restore a backup, run:" -ForegroundColor Green
    Write-Host "  .\Restore-Workspace.ps1 -BackupTimestamp ""yyyy-MM-dd_HH-mm-ss"""
    Write-Host ""
    exit 0
}

# Validate backup exists
$backupDir = Join-Path $BackupRoot $BackupTimestamp
if (-not (Test-Path $backupDir)) {
    Write-Host "[ERROR] Backup not found: $backupDir" -ForegroundColor Red
    Write-Host ""
    Write-Host "Run without -BackupTimestamp to see available backups" -ForegroundColor Yellow
    exit 1
}

# Load manifest
$manifestPath = Join-Path $backupDir "backup-manifest.json"
if (-not (Test-Path $manifestPath)) {
    Write-Host "WARNING: Backup manifest not found. Proceeding anyway..." -ForegroundColor Yellow
    $manifest = $null
}
else {
    $manifest = Get-Content $manifestPath | ConvertFrom-Json
}

# Show what will be restored
Write-Host "Restore Details:" -ForegroundColor Green
Write-Host "  From: $backupDir"
Write-Host "  To: $WorkspaceRoot"
if ($manifest) {
    Write-Host "  Files: $($manifest.FilesBackedUp)"
    Write-Host "  Size: $($manifest.TotalSizeMB) MB"
}
Write-Host ""

# Confirmation
if (-not $Force) {
    Write-Host "WARNING: This will OVERWRITE current workspace data!" -ForegroundColor Yellow
    Write-Host ""
    $response = Read-Host "Type 'YES' to confirm restore"
    
    if ($response -ne 'YES') {
        Write-Host "Restore cancelled" -ForegroundColor Yellow
        exit 0
    }
    Write-Host ""
}

# Perform restore
Write-Host "Starting restore..." -ForegroundColor Green
Write-Host ""

$restoredFiles = 0
$failedItems = @()

# Define restore mappings (inverse of backup)
$restoreItems = @(
    @{
        Name = "Database"
        Source = "App_Data"
        Destination = "App_Data\me_workspace.db"
    },
    @{
        Name = "App Settings"
        Source = "Config\appsettings.json"
        Destination = "src\me_workspace.Web\appsettings.json"
    },
    @{
        Name = "Development Settings"
        Source = "Config\appsettings.Development.json"
        Destination = "src\me_workspace.Web\appsettings.Development.json"
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

foreach ($item in $restoreItems) {
    $sourcePath = Join-Path $backupDir $item.Source
    $destPath = Join-Path $WorkspaceRoot $item.Destination
    
    if (Test-Path $sourcePath) {
        Write-Host "Restoring $($item.Name)..." -ForegroundColor Green
        
        try {
            # Create destination directory if needed
            $destParent = Split-Path -Parent $destPath
            if ($destParent -and -not (Test-Path $destParent)) {
                New-Item -ItemType Directory -Path $destParent -Force | Out-Null
            }
            
            # Remove existing destination to avoid conflicts
            if (Test-Path $destPath) {
                if (Test-Path $destPath -PathType Container) {
                    Remove-Item -Path $destPath -Recurse -Force
                }
                else {
                    Remove-Item -Path $destPath -Force
                }
            }
            
            # Copy from backup (remove read-only temporarily if needed)
            if (Test-Path $sourcePath -PathType Container) {
                Copy-Item -Path $sourcePath -Destination $destPath -Recurse -Force
                
                # Remove read-only attribute from restored files
                Get-ChildItem -Path $destPath -Recurse -File | ForEach-Object {
                    $_.IsReadOnly = $false
                }
                
                $fileCount = (Get-ChildItem -Path $destPath -Recurse -File).Count
                $restoredFiles += $fileCount
            }
            else {
                # Single file - handle read-only
                $sourceItem = Get-Item $sourcePath
                if ($sourceItem.IsReadOnly) {
                    # Copy to temp location, remove read-only, then move
                    $tempPath = "$destPath.temp"
                    Copy-Item -Path $sourcePath -Destination $tempPath -Force
                    (Get-Item $tempPath).IsReadOnly = $false
                    Move-Item -Path $tempPath -Destination $destPath -Force
                }
                else {
                    Copy-Item -Path $sourcePath -Destination $destPath -Force
                }
                $restoredFiles++
            }
            
            Write-Host "  [OK] $($item.Name) restored" -ForegroundColor Gray
        }
        catch {
            Write-Host "  [ERROR] $($item.Name) failed: $($_.Exception.Message)" -ForegroundColor Red
            $failedItems += $item.Name
        }
    }
    else {
        Write-Host "  [SKIP] $($item.Name) not found in backup, skipping" -ForegroundColor Yellow
    }
}

# Summary
Write-Host ""
Write-Host "=== Restore Complete ===" -ForegroundColor Cyan
Write-Host "Files restored: $restoredFiles"

if ($failedItems.Count -gt 0) {
    Write-Host ""
    Write-Host "Failed items:" -ForegroundColor Red
    $failedItems | ForEach-Object { Write-Host "  - $_" -ForegroundColor Red }
    Write-Host ""
    Write-Host "[WARNING] Restore completed with errors" -ForegroundColor Yellow
}
else {
    Write-Host ""
    Write-Host "[SUCCESS] Restore successful" -ForegroundColor Green
}

Write-Host ""
