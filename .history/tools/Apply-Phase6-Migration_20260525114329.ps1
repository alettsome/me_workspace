#Requires -Version 5.1

<#
.SYNOPSIS
    Apply Phase 6 database migration using SQL script

.DESCRIPTION
    This script applies the Phase 6 database schema changes using raw SQL.
    Safer than EF migrations when database already exists with data.

.EXAMPLE
    .\Apply-Phase6-Migration.ps1
#>

[CmdletBinding()]
param()

Set-StrictMode -Off
$ErrorActionPreference = 'Stop'

$dbPath = Join-Path $PSScriptRoot "..\App_Data\me_workspace.db"
$sqlScript = Join-Path $PSScriptRoot "Phase6-Migration.sql"
$backupPath = Join-Path $PSScriptRoot "..\App_Data\me_workspace.db.backup-phase6-manual-$(Get-Date -Format 'yyyyMMdd-HHmmss')"

Write-Host "=== Phase 6 Database Migration (SQL) ===" -ForegroundColor Cyan
Write-Host ""

# Check if database exists
if (-not (Test-Path $dbPath)) {
    Write-Host "[ERROR] Database not found at: $dbPath" -ForegroundColor Red
    Write-Host "The database will be created on first application run." -ForegroundColor Yellow
    exit 1
}

# Check if SQL script exists
if (-not (Test-Path $sqlScript)) {
    Write-Host "[ERROR] SQL script not found at: $sqlScript" -ForegroundColor Red
    exit 1
}

# Backup database first
Write-Host "[Step 1] Backing up database..." -ForegroundColor Cyan
try {
    Copy-Item $dbPath $backupPath -Force
    Write-Host "[OK] Database backed up to:" -ForegroundColor Green
    Write-Host "  $backupPath" -ForegroundColor Gray
}
catch {
    Write-Host "[ERROR] Backup failed: $($_.Exception.Message)" -ForegroundColor Red
    exit 1
}

Write-Host ""
Write-Host "[Step 2] Applying SQL migration..." -ForegroundColor Cyan

# Load System.Data.SQLite if available, otherwise use basic file check
$sqlContent = Get-Content $sqlScript -Raw

# Try using .NET SQLite provider
try {
    Add-Type -Path "System.Data.SQLite.dll" -ErrorAction Stop
    $connectionString = "Data Source=$dbPath;Version=3;"
    $connection = New-Object System.Data.SQLite.SQLiteConnection($connectionString)
    $connection.Open()
    
    $command = $connection.CreateCommand()
    $command.CommandText = $sqlContent
    $command.ExecuteNonQuery() | Out-Null
    
    $connection.Close()
    
    Write-Host "[OK] Migration applied successfully" -ForegroundColor Green
}
catch {
    # Fallback: Write instructions for manual application
    Write-Host "[WARNING] Could not apply migration automatically" -ForegroundColor Yellow
    Write-Host ""
    Write-Host "Please apply the migration manually:" -ForegroundColor Yellow
    Write-Host "1. Install SQLite tools: https://www.sqlite.org/download.html" -ForegroundColor Cyan
    Write-Host "2. Run this command:" -ForegroundColor Cyan
    Write-Host "   sqlite3 `"$dbPath`" < `"$sqlScript`"" -ForegroundColor White
    Write-Host ""
    Write-Host "Or use a SQLite GUI tool like DB Browser for SQLite:" -ForegroundColor Yellow
    Write-Host "  https://sqlitebrowser.org/" -ForegroundColor Cyan
    Write-Host ""
    Write-Host "SQL script location: $sqlScript" -ForegroundColor Gray
    exit 1
}

Write-Host ""
Write-Host "[Step 3] Verifying new tables..." -ForegroundColor Cyan

# Verify tables were created
try {
    $connection = New-Object System.Data.SQLite.SQLiteConnection($connectionString)
    $connection.Open()
    
    $tablesToCheck = @("Projects", "Summaries", "DocumentAnchors", "ProjectTasks", "AgentLogs", "TrendAnalyses")
    $allExist = $true
    
    foreach ($table in $tablesToCheck) {
        $command = $connection.CreateCommand()
        $command.CommandText = "SELECT name FROM sqlite_master WHERE type='table' AND name='$table';"
        $result = $command.ExecuteScalar()
        
        if ($result) {
            Write-Host "  [OK] $table" -ForegroundColor Green
        }
        else {
            Write-Host "  [ERROR] $table not found" -ForegroundColor Red
            $allExist = $false
        }
    }
    
    $connection.Close()
    
    if ($allExist) {
        Write-Host ""
        Write-Host "[SUCCESS] Phase 6 migration complete!" -ForegroundColor Green
        Write-Host ""
        Write-Host "New tables added:" -ForegroundColor Gray
        Write-Host "  - Projects" -ForegroundColor White
        Write-Host "  - Summaries" -ForegroundColor White
        Write-Host "  - DocumentAnchors" -ForegroundColor White
        Write-Host "  - ProjectTasks" -ForegroundColor White
        Write-Host "  - AgentLogs" -ForegroundColor White
        Write-Host "  - TrendAnalyses" -ForegroundColor White
        Write-Host ""
        Write-Host "Sources table enhanced with:" -ForegroundColor Gray
        Write-Host "  - ProjectId, Author, ISBN, URL, Publisher" -ForegroundColor White
        Write-Host "  - PublicationYear, BorrowingSource, AccessExpiryUtc" -ForegroundColor White
    }
    else {
        Write-Host ""
        Write-Host "[ERROR] Some tables were not created" -ForegroundColor Red
        exit 1
    }
}
catch {
    Write-Host "[WARNING] Could not verify tables: $($_.Exception.Message)" -ForegroundColor Yellow
    Write-Host "Migration may have succeeded. Check database manually." -ForegroundColor Yellow
}

Write-Host ""
Write-Host "=== Migration Complete ===" -ForegroundColor Cyan
