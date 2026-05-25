# Phase 6 Database Schema - Documentation

## Overview

This document describes the Phase 6 database schema enhancements that enable project management, multi-source research, and the complete workflow from idea to master document.

**Migration Date:** May 25, 2026  
**Migration Script:** `tools/Migrate-Phase6-Database.ps1`

---

## New Tables

### 1. **Project**
Organizes sources, summaries, and generated content into cohesive projects.

**Purpose:** Top-level container for books, business plans, strategic plans, etc.

**Key Fields:**
- `Name` - Project name (e.g., "Health_Fundamentals")
- `ContentType` - "Book", "BusinessPlan", "StrategicPlan", "Journal"
- `Status` - "idea", "assessment", "acquisition", "active", "complete"
- `Timeline` - User's target timeline
- `FolderPath` - Relative path to project folder

**Relationships:**
- Has many `Source`, `Summary`, `DocumentAnchor`, `ProjectTask`, `AgentLog`, `TrendAnalysis`

---

### 2. **Summary**
Stores summarized content from sources with optional semantic embeddings.

**Purpose:** Efficient storage of key information for fast retrieval and semantic search.

**Key Fields:**
- `SummaryText` - The summarized content
- `PageRange` - Source page reference (e.g., "p. 45-67")
- `Keywords` - Metadata keywords for search
- `EmbeddingVector` - BLOB for semantic search (populated in Phase 12)
- `EmbeddingModel` - Model used to generate embedding
- `RelevanceScore` - Quality/relevance metric

**Relationships:**
- Belongs to `Source` (many summaries per source)
- Belongs to `Project` (can be shared across projects)

**Usage:**
```csharp
// Create summary
var summary = new Summary 
{
    SourceId = book.Id,
    ProjectId = project.Id,
    SummaryText = "Key findings...",
    PageRange = "p. 45-67",
    Keywords = "vitamins, cofactors, whole foods"
};
await db.Summaries.AddAsync(summary);
```

---

### 3. **DocumentAnchor**
Defines structure points in master documents where content is assembled.

**Purpose:** Template-based document generation with traceability.

**Key Fields:**
- `AnchorName` - Display name (e.g., "Chapter 1: Vitamin Basics")
- `AnchorKey` - Unique key for markdown reference
- `Position` - Order in document
- `Status` - "pending", "in-progress", "complete"
- `CompletionPercent` - Progress indicator

**Relationships:**
- Belongs to `Project`
- Has many `ProjectTask` (tasks tied to this anchor)

**Usage:**
```csharp
// Create document structure
var anchors = new[] 
{
    new DocumentAnchor 
    { 
        ProjectId = project.Id,
        AnchorName = "Chapter 1: What Are Vitamins?",
        AnchorKey = "chapter-1-vitamins",
        Position = 1
    },
    new DocumentAnchor 
    { 
        ProjectId = project.Id,
        AnchorName = "Chapter 2: The Cofactor Connection",
        AnchorKey = "chapter-2-cofactors",
        Position = 2
    }
};
```

---

### 4. **ProjectTask**
Tasks and action items tied to projects and document anchors.

**Purpose:** Track work items with traceability to document structure and conversations.

**Key Fields:**
- `Description` - Task description
- `Status` - "pending", "in-progress", "complete", "blocked"
- `Priority` - "low", "medium", "high", "critical"
- `AnchorId` - Optional link to document anchor
- `ConversationId` - Optional link to originating conversation

**Relationships:**
- Belongs to `Project`
- Optionally belongs to `DocumentAnchor`
- Optionally links to `Conversation`

**Usage:**
```csharp
// Create task tied to anchor
var task = new ProjectTask 
{
    ProjectId = project.Id,
    AnchorId = anchor.Id,
    Description = "Find 5 sources on vitamin A cofactors",
    Status = "pending",
    Priority = "high"
};
```

---

### 5. **AgentLog**
Tracks AI agent activities during project phases.

**Purpose:** Audit trail and debugging for agent operations.

**Key Fields:**
- `AgentType` - "researcher", "analyst", "council", "synthesizer", "gap-finder"
- `Action` - "search", "summarize", "analyze", "assess", "recommend"
- `Input` - What was provided to agent
- `Result` - Agent's output
- `Status` - "started", "completed", "failed"
- `DurationMs` - Execution time

**Relationships:**
- Belongs to `Project`

**Usage:**
```csharp
// Log agent activity
var log = new AgentLog 
{
    ProjectId = project.Id,
    AgentType = "researcher",
    Action = "search",
    Input = "whole foods vitamins cofactors",
    Status = "started"
};
await db.AgentLogs.AddAsync(log);

// ... agent does work ...

log.Result = "Found 127 books, 45 journals";
log.Status = "completed";
log.CompletedUtc = DateTime.UtcNow;
log.DurationMs = 5420;
await db.SaveChangesAsync();
```

---

### 6. **TrendAnalysis**
Stores trend analysis results across multiple sources.

**Purpose:** Capture insights from analyst agents during council phase.

**Key Fields:**
- `Topic` - Trend being analyzed (e.g., "synthetic vitamins lack cofactors")
- `TrendSummary` - Summary of findings
- `SourceCount` - How many sources discuss this
- `PrevalencePercent` - Percentage of total sources
- `SourceIds` - JSON array of contributing source IDs
- `Evidence` - Key quotes (JSON format)
- `Recommendations` - Suggested actions

**Relationships:**
- Belongs to `Project`

**Usage:**
```csharp
// Store trend analysis
var trend = new TrendAnalysis 
{
    ProjectId = project.Id,
    Topic = "Synthetic vitamins lack cofactors",
    TrendSummary = "89 of 127 sources emphasize that isolated vitamins...",
    SourceCount = 89,
    PrevalencePercent = 70.1,
    Recommendations = "Emphasize this in Part 1 of book"
};
```

---

## Enhanced Tables

### Source (Enhanced)
Added fields to support multi-source types (books, journals, web sources).

**New Fields:**
- `ProjectId` - Optional link to parent project
- `Author` - Author name(s)
- `ISBN` - ISBN for books
- `URL` - URL for web sources
- `Publisher` - Publisher name
- `PublicationYear` - Year published
- `BorrowingSource` - For library research (e.g., "Internet Archive")
- `AccessExpiryUtc` - When borrowed access expires

**Supported Source Types:**
- `"book"` - Physical or digital books
- `"medical-journal"` - Medical/scientific journals
- `"web"` - Web articles and pages
- `"article"` - General articles
- `"text"` - Plain text files
- `"pdf"` - PDF documents

**Usage:**
```csharp
// Create book source
var book = new Source 
{
    ProjectId = project.Id,
    SourceType = "book",
    Title = "The Vitamin Solution",
    Author = "Dr. Jane Smith",
    ISBN = "978-1234567890",
    Publisher = "Health Press",
    PublicationYear = 2020,
    Status = "new"
};

// Create library-borrowed source
var borrowed = new Source 
{
    ProjectId = project.Id,
    SourceType = "book",
    Title = "Nutritional Biochemistry",
    BorrowingSource = "Internet Archive",
    AccessExpiryUtc = DateTime.UtcNow.AddHours(1),
    RightsLabel = "summary-only"
};
```

---

## Workflow Examples

### Complete Project Workflow

```csharp
// 1. Create project
var project = new Project 
{
    Name = "Health_Fundamentals",
    ContentType = "Book",
    Status = "idea",
    Timeline = "6 months",
    Description = "Book on whole foods and affordability"
};
await db.Projects.AddAsync(project);
await db.SaveChangesAsync();

// 2. Assessment phase (agent logs)
var assessment = new AgentLog 
{
    ProjectId = project.Id,
    AgentType = "researcher",
    Action = "assess",
    Input = "whole foods, vitamins, affordability",
    Result = "Found 127 books, 45 journals. Viable project."
};
await db.AgentLogs.AddAsync(assessment);

// 3. Add sources
var sources = new[] 
{
    new Source { ProjectId = project.Id, Title = "Book 1", SourceType = "book" },
    new Source { ProjectId = project.Id, Title = "Journal 1", SourceType = "medical-journal" }
};
await db.Sources.AddRangeAsync(sources);

// 4. Generate summaries
foreach (var source in sources) 
{
    var summary = new Summary 
    {
        SourceId = source.Id,
        ProjectId = project.Id,
        SummaryText = "..." // Generated by LLM
    };
    await db.Summaries.AddAsync(summary);
}

// 5. Analyze trends
var trend = new TrendAnalysis 
{
    ProjectId = project.Id,
    Topic = "Vitamin cofactors",
    TrendSummary = "Strong consensus across 67 sources",
    SourceCount = 67
};
await db.TrendAnalyses.AddAsync(trend);

// 6. Create document structure
var anchors = new[] 
{
    new DocumentAnchor 
    {
        ProjectId = project.Id,
        AnchorName = "Chapter 1",
        AnchorKey = "ch1",
        Position = 1
    }
};
await db.DocumentAnchors.AddRangeAsync(anchors);

// 7. Create tasks
var task = new ProjectTask 
{
    ProjectId = project.Id,
    AnchorId = anchors[0].Id,
    Description = "Find vitamin A sources",
    Status = "pending"
};
await db.ProjectTasks.AddAsync(task);

// 8. Update project status
project.Status = "active";
await db.SaveChangesAsync();
```

---

## Queries

### Find all sources for a project
```csharp
var sources = await db.Sources
    .Where(s => s.ProjectId == projectId)
    .Include(s => s.Summaries)
    .ToListAsync();
```

### Find summaries related to a topic
```csharp
var summaries = await db.Summaries
    .Where(s => s.ProjectId == projectId 
        && s.Keywords.Contains("vitamins"))
    .Include(s => s.Source)
    .OrderByDescending(s => s.RelevanceScore)
    .ToListAsync();
```

### Get document structure with tasks
```csharp
var structure = await db.DocumentAnchors
    .Where(a => a.ProjectId == projectId)
    .Include(a => a.Tasks)
    .OrderBy(a => a.Position)
    .ToListAsync();
```

### Recent agent activity
```csharp
var recentLogs = await db.AgentLogs
    .Where(l => l.ProjectId == projectId)
    .OrderByDescending(l => l.CreatedUtc)
    .Take(20)
    .ToListAsync();
```

### Find sources expiring soon (library research)
```csharp
var expiringSoon = await db.Sources
    .Where(s => s.AccessExpiryUtc != null 
        && s.AccessExpiryUtc <= DateTime.UtcNow.AddHours(24))
    .ToListAsync();
```

---

## Migration Steps

### 1. Create Migration
```powershell
cd tools
.\Migrate-Phase6-Database.ps1
```

### 2. Review Migration Files
Check `src/me_workspace.Web/Data/Migrations/` for generated files.

### 3. Apply Migration
```powershell
.\Migrate-Phase6-Database.ps1 -Apply
```

Or manually:
```powershell
cd src/me_workspace.Web
dotnet ef database update
```

### 4. Verify Schema
```powershell
sqlite3 App_Data/me_workspace.db ".schema Project"
sqlite3 App_Data/me_workspace.db ".schema Summary"
# etc.
```

---

## Backup & Recovery

**Automatic Backup:**
The migration script automatically backs up the database before applying changes:
```
App_Data/me_workspace.db.backup-phase6-YYYYMMDD-HHMMSS
```

**Manual Restore:**
```powershell
cd App_Data
Copy-Item me_workspace.db.backup-phase6-YYYYMMDD-HHMMSS me_workspace.db -Force
```

---

## Next Steps

After Phase 6 database migration:

1. **Test with existing UI** - Verify chat, journals, files still work
2. **Add project creation flow** - Enhance UI to create projects
3. **Build assessment agent** - Prototype quick viability check
4. **Enhance acquisition** - Use new schema in existing pipeline

See [Phases.md](../MasterPlan/Phases.md) for complete roadmap.

---

## Related Documents

- [TechnologyPhilosophy.md](../MasterPlan/TechnologyPhilosophy.md) - Architecture principles
- [DataModel.md](../MasterPlan/DataModel.md) - Conceptual data model
- [UserWorkflow.md](../MasterPlan/UserWorkflow.md) - User interaction patterns
- [Phases.md](../MasterPlan/Phases.md) - Implementation phases
