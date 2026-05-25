#Requires -Version 5.1

<#
.SYNOPSIS
    Phase 6 Database Migration - Adds project management and multi-source support

.DESCRIPTION
    This script creates and applies EF Core migrations for the Phase 6 database schema.
    
    New tables:
    - Project: Organizes sources into books, business plans, etc.
    - Summary: Stores summarized content with optional embeddings
    - DocumentAnchor: Master document template anchor points
    - ProjectTask: Tasks tied to anchors and projects
    - AgentLog: Tracks agent activities
    - TrendAnalysis: Stores trend analysis results
    
    Enhanced tables:
    - Source: Adds multi-source fields (books, journals, web, etc.)

.PARAMETER Apply
    If specified, applies the migration to the database immediately.
    If not specified, only creates the migration files.

.EXAMPLE
    .\Migrate-Phase6-Database.ps1
    Creates migration files without applying

.EXAMPLE
    .\Migrate-Phase6-Database.ps1 -Apply
    Creates and applies migration to database
#>

[CmdletBinding()]
param(
    [Parameter()]
    [switch]$Apply
)

Set-StrictMode -Off
$ErrorActionPreference = 'Stop'

$projectPath = Join-Path $PSScriptRoot "..\src\me_workspace.Web"
$migrationName = "Phase6_ProjectManagement"

Write-Host "=== Phase 6 Database Migration ===" -ForegroundColor Cyan
Write-Host ""
Write-Host "Project: $projectPath" -ForegroundColor Gray
Write-Host "Migration: $migrationName" -ForegroundColor Gray
Write-Host ""

# Check if dotnet CLI is available
try {
    $dotnetVersion = dotnet --version
    Write-Host "[OK] .NET SDK version: $dotnetVersion" -ForegroundColor Green
}
catch {
    Write-Host "[ERROR] .NET SDK not found. Please install .NET 8 SDK." -ForegroundColor Red
    exit 1
}

# Check if EF Core tools are installed
try {
    $efVersion = dotnet ef --version
    Write-Host "[OK] EF Core tools version: $efVersion" -ForegroundColor Green
}
catch {
    Write-Host "[WARNING] EF Core tools not installed. Installing..." -ForegroundColor Yellow
    dotnet tool install --global dotnet-ef
    Write-Host "[OK] EF Core tools installed" -ForegroundColor Green
}

Write-Host ""

# Navigate to project directory
Push-Location $projectPath

try {
    # Create migration
    Write-Host "[Step 1] Creating migration..." -ForegroundColor Cyan
    dotnet ef migrations add $migrationName --output-dir Data/Migrations
    
    if ($LASTEXITCODE -ne 0) {
        throw "Migration creation failed"
    }
    
    Write-Host "[OK] Migration created successfully" -ForegroundColor Green
    Write-Host ""
    
    # Show migration details
    Write-Host "Migration files created in: Data/Migrations/" -ForegroundColor Gray
    Write-Host ""
    
    if ($Apply) {
        Write-Host "[Step 2] Applying migration to database..." -ForegroundColor Cyan
        
        # Backup database first
        $dbPath = "..\..\App_Data\me_workspace.db"
        if (Test-Path $dbPath) {
            $backupPath = "..\..\App_Data\me_workspace.db.backup-phase6-$(Get-Date -Format 'yyyyMMdd-HHmmss')"
            Copy-Item $dbPath $backupPath
            Write-Host "[OK] Database backed up to: $(Split-Path -Leaf $backupPath)" -ForegroundColor Green
        }
        
        # Apply migration
        dotnet ef database update
        
        if ($LASTEXITCODE -ne 0) {
            throw "Migration application failed"
        }
        
        Write-Host "[OK] Migration applied successfully" -ForegroundColor Green
        Write-Host ""
        
        # Show updated schema
        Write-Host "New tables added:" -ForegroundColor Green
        Write-Host "  - Project" -ForegroundColor Gray
        Write-Host "  - Summary" -ForegroundColor Gray
        Write-Host "  - DocumentAnchor" -ForegroundColor Gray
        Write-Host "  - ProjectTask" -ForegroundColor Gray
        Write-Host "  - AgentLog" -ForegroundColor Gray
        Write-Host "  - TrendAnalysis" -ForegroundColor Gray
        Write-Host ""
        Write-Host "Enhanced tables:" -ForegroundColor Green
        Write-Host "  - Source (added multi-source fields)" -ForegroundColor Gray
    }
    else {
        Write-Host "[INFO] Migration created but NOT applied" -ForegroundColor Yellow
        Write-Host ""
        Write-Host "To apply the migration, run:" -ForegroundColor Yellow
        Write-Host "  .\Migrate-Phase6-Database.ps1 -Apply" -ForegroundColor Cyan
        Write-Host ""
        Write-Host "Or manually from project directory:" -ForegroundColor Yellow
        Write-Host "  cd $projectPath" -ForegroundColor Cyan
        Write-Host "  dotnet ef database update" -ForegroundColor Cyan
    }
    
    Write-Host ""
    Write-Host "=== Migration Complete ===" -ForegroundColor Cyan
}
catch {
    Write-Host ""
    Write-Host "[ERROR] $($_.Exception.Message)" -ForegroundColor Red
    exit 1
}
finally {
    Pop-Location
}
