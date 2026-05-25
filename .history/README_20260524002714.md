# me_workspace

A local-first, single-user document platform for managing books, business plans, strategic plans, and project management.

## Quick Start

### Running the Application

```powershell
# Navigate to the web project
cd src\me_workspace.Web

# Run the application
dotnet run
```

The application will be available at `https://localhost:5001` (or the port shown in the console).

### Running Tests

```powershell
# API smoke test
.\tests\ApiFlowSmoke.ps1
```

## Project Structure

```
me_workspace/
├── src/                    # Source code (.NET application)
├── docs/                   # Architecture and planning documents
├── MasterPlan/            # Project phases and acceptance criteria
├── Journals/              # Development journals and logs
├── ThingsToDo/            # Task and project management
├── projects/              # Document projects (e.g., Health_Fundamentals)
├── tests/                 # Test scripts
└── tools/                 # Utility scripts (backup, restore, etc.)
```

## Backup and Restore

### Overview

The workspace uses an **immutable backup system** to protect against data loss from accidental deletion, system failures, or AI agent errors.

- **Backup Location**: `C:\me_workspace_backups\` (separate from workspace)
- **Retention**: Last 30 days
- **Schedule**: Daily at 2:00 AM (automated)
- **Protection**: Backups are read-only to prevent accidental deletion

### Initial Setup

**First time only** - Run as Administrator:

```powershell
cd tools
.\Setup-BackupAutomation.ps1
```

This will:
1. Create the backup directory
2. Run a test backup
3. Validate restore functionality
4. Create a scheduled task for daily backups

### Manual Backup

To create a backup manually:

```powershell
cd tools
.\Backup-Workspace.ps1
```

### Restore from Backup

**Step 1: List available backups**

```powershell
cd tools
.\Restore-Workspace.ps1
```

**Step 2: Restore from specific backup**

```powershell
.\Restore-Workspace.ps1 -BackupTimestamp "2026-05-24_14-30-00"
```

**WARNING**: Restore will overwrite current workspace data. You will be prompted to confirm.

### What Gets Backed Up

- SQLite database (`App_Data\me_workspace.db`)
- Configuration files (`appsettings.json`)
- Master planning documents (`MasterPlan/`)
- Journals (`Journals/`)
- Documentation (`docs/`)
- ThingsToDo tasks (`ThingsToDo/`)

### What Does NOT Get Backed Up

- Transient OCR text (deleted after summarization per retention policy)
- Temporary files
- Build artifacts (`artifacts/`, `bin/`, `obj/`)
- External tools (`tools/whispercpp/`)

### Testing the Backup System

**Critical**: Before relying on backups in production, test the restore procedure:

```powershell
# 1. Create a test backup
.\Backup-Workspace.ps1

# 2. Note a distinctive piece of data in your database or journals

# 3. Delete or modify that data

# 4. Restore from backup
.\Restore-Workspace.ps1 -BackupTimestamp "yyyy-MM-dd_HH-mm-ss"

# 5. Verify the data was restored correctly
```

**Acceptance Criteria**: The restore procedure should take less than 5 minutes and successfully recover all critical data.

## Architecture Principles

See [MasterPlan/ArchitecturePrinciples.md](MasterPlan/ArchitecturePrinciples.md) for detailed philosophy:

- **Local-first**: All data stored locally, no cloud dependencies
- **Single-user**: Optimized for individual use
- **Folder-as-Agent**: Unix-style directory structure as interface
- **Security-first**: Immutable backups, deterministic safety layers
- **Slow and steady**: Reliability over speed

## Development Phases

Current status: **Foundation phase** (Phase 0-1)

See [MasterPlan/Phases.md](MasterPlan/Phases.md) for complete roadmap.

### Critical Foundation Items

Before processing production data, these must be complete:

- [x] Immutable backup system
- [ ] Database schema migration (JSON → SQLite)
- [ ] Human-in-the-loop approval gates

### Active Use Cases

- **Health Fundamentals Book**: Primary validation project (100+ sources)
- **General Document Platform**: Supports multiple document types

## Data Model

See [MasterPlan/DataModel.md](MasterPlan/DataModel.md) for complete schema.

Key entities:
- **Conversation**: Chat threads with context
- **Message**: Individual chat messages with role (user/assistant)
- **Source**: Documents, books, files being researched
- **Summary**: AI-generated summaries of source material
- **Journal**: Development journals linked to conversations

## Retention Policy

See [MasterPlan/RetentionPolicy.md](MasterPlan/RetentionPolicy.md) for detailed rules.

**Key principle**: Transient data (e.g., full OCR text) is automatically deleted after summarization. Only summaries and metadata are retained long-term.

## Security and Ethics

- **Ethical Source Handling**: See [MasterPlan/EthicalSourceHandling.md](MasterPlan/EthicalSourceHandling.md)
- **AI Safety**: Human approval required for destructive operations
- **Audit Logging**: All operations logged for review

## Technology Stack

- **Backend**: ASP.NET Core 8.0, C#
- **Database**: SQLite (local)
- **Scripting**: PowerShell 5.1+
- **AI Models**: Local LLMs via Ollama
- **Speech Recognition**: whisper.cpp (offline)
- **OCR**: Tesseract (for library research)

## Contributing

This is a personal project, but documentation and architecture are designed for clarity and maintainability.

## License

Private project - not licensed for redistribution.
