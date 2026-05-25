# System Hierarchy

**Last Updated:** May 25, 2026  
**Purpose:** Folder structure rules and organization principles

---

## Overview

This document defines how content is organized in the me_workspace file system. The hierarchy follows **encapsulation principles** where each problem/solution is self-contained with all necessary information.

**See also:** [../docs/System-Architecture.md](../docs/System-Architecture.md) for the conceptual vision.

---

## Root Folder Structure

```
C:\me_workspace\
├── projects/              ← All project folders
├── Journals/              ← Journal entries and personal notes
├── ThingsToDo/            ← Strategic tasks and priorities
├── MasterPlan/            ← System-level documentation
├── docs/                  ← Technical documentation
├── src/                   ← Application source code
└── tools/                 ← Utilities and scripts
```

---

## Project Folder Structure

### Standard Project Layout

```
projects/
└── {Project-Name}/
    ├── README.md                    ← Project overview
    ├── vision.md                    ← High-level vision
    ├── executive-summary.md         ← (Conditional: business plans only)
    ├── financial-overview.md        ← (Conditional: business plans only)
    ├── strategic/
    │   ├── timeline.md              ← Project timeline
    │   ├── priorities.md            ← Priority ranking
    │   ├── resources.md             ← Resource allocation
    │   └── dependencies.md          ← Inter-problem dependencies
    │
    └── problems/
        ├── Problem-01/
        │   ├── 00-Feature-Index.md
        │   ├── 01-Problem-Definition.md
        │   ├── 02-Solution-Approach.md
        │   ├── 03-Competitive-Analysis.md
        │   ├── 04-Target-Audience.md
        │   ├── 05-Benefits.md
        │   ├── 06-Financial-Considerations.md
        │   ├── 07-Workflow.md
        │   ├── 08-Functionalities.md
        │   ├── 09-Technology-Choices.md
        │   ├── 10-UI-Design.md
        │   ├── 11-Attributes.md
        │   ├── 12-Configuration-Parameters.md
        │   ├── 13-Modules-Implementation.md
        │   ├── 14-Future-Vision.md
        │   ├── 15-Release-Timeline.md
        │   │
        │   └── features/
        │       ├── feature-a/
        │       │   ├── 00-Feature-Index.md
        │       │   ├── 01-Problem-Definition.md
        │       │   └── ... (same 15 files)
        │       └── feature-b/
        │
        ├── Problem-02/
        │   └── ... (same structure)
        │
        └── Problem-N/
```

### Naming Conventions

**Projects:**
- Format: `{Project-Name}` or `{Project-Name}_{Date}`
- Examples: `Health_Fundamentals`, `Bible_Constitution`, `Marriage_Book_2026`
- Use underscores for spaces, PascalCase for words

**Problems:**
- Format: `Problem-{NN}` (zero-padded)
- Examples: `Problem-01`, `Problem-02`, `Problem-15`
- Use descriptive folder names optionally: `Problem-01-Data-Ingestion`

**Features:**
- Format: `{feature-name}` (lowercase, hyphens)
- Examples: `chunking`, `intake`, `bible-verse-linking`
- Keep short and descriptive

---

## The 15 Decision Files (Universal Template)

Every problem and every feature follows this structure:

| #   | File Name                        | Purpose                                   |
| --- | -------------------------------- | ----------------------------------------- |
| 00  | `00-Feature-Index.md`            | Links upward (project, problem, solution) |
| 01  | `01-Problem-Definition.md`       | What problem exists?                      |
| 02  | `02-Solution-Approach.md`        | How do we solve it?                       |
| 03  | `03-Competitive-Analysis.md`     | Market alternatives, comparison           |
| 04  | `04-Target-Audience.md`          | End users, personas                       |
| 05  | `05-Benefits.md`                 | Value to company, users, system           |
| 06  | `06-Financial-Considerations.md` | Cost, ROI, resources, weights             |
| 07  | `07-Workflow.md`                 | How it works (steps, diagrams)            |
| 08  | `08-Functionalities.md`          | What capabilities provided                |
| 09  | `09-Technology-Choices.md`       | Tech stack, rationale                     |
| 10  | `10-UI-Design.md`                | Screen layout, UX                         |
| 11  | `11-Attributes.md`               | Properties, metadata captured             |
| 12  | `12-Configuration-Parameters.md` | Settings, tuning                          |
| 13  | `13-Modules-Implementation.md`   | Code structure, APIs                      |
| 14  | `14-Future-Vision.md`            | Roadmap, enhancements                     |
| 15  | `15-Release-Timeline.md`         | Now vs later, phasing                     |

### File Numbering Rules

- **Always zero-padded:** `01`, `02`, ..., `09`, `10`, `15`
- **Always sequential:** No gaps in numbering
- **Always start at 00:** Feature index comes first
- **Consistent naming:** Use exact names across all problems/features

### When to Deviate

**Rare cases where you might add files:**
- `16-Testing-Strategy.md` (if testing deserves separate file)
- `17-Security-Considerations.md` (if security is critical)
- `00-CHANGELOG.md` (track changes to the problem/feature)

**Never remove or skip files.** If a section doesn't apply, create the file with "Not Applicable" and explanation.

---

## Conditional Visibility Rules

### Content Types

| Content Type        | Folders/Files That Appear                                |
| ------------------- | -------------------------------------------------------- |
| **Journal**         | problems/ (optional), no executive-summary.md            |
| **Mini Project**    | problems/, strategic/ (minimal), no financial            |
| **Book Project**    | problems/ (chapters), executive-summary.md (preface)     |
| **Business Plan**   | ALL folders, executive-summary.md, financial-overview.md |
| **Report**          | problems/ (findings), strategic/ (timeline)              |

### Visibility Mechanism

**Phase 1 (Current):** Manual - create files as needed

**Phase 2 (Future):** Metadata-driven
```yaml
# project-config.yaml
content_type: business_plan
visible_sections:
  - executive_summary
  - financial_overview
  - competitive_analysis
hidden_sections:
  - none
```

**Phase 3 (Advanced):** UI toggles
- User selects content type in UI
- System shows/hides folders dynamically
- Files still exist (for future promotion)

---

## Journal Folder Structure

```
Journals/
├── index.json                   ← Metadata for all journals
└── {Year}/
    └── {Month}/
        ├── entry-{YYYY-MM-DD}.md
        ├── entry-{YYYY-MM-DD}-{slug}.md
        └── meta/
            ├── {YYYY-MM-DD}.json    ← Entry metadata
            └── themes.json          ← Detected themes
```

### Journal Entry Format

```markdown
---
date: 2026-05-25
time: 14:30
mood: reflective
tags: [ideas, health, business]
evaluated: false
project_promoted: null
---

# Journal Entry: May 25, 2026

[Content here...]
```

### Journal → Project Promotion

When a journal entry is evaluated and promoted:

1. Create project folder: `projects/Project-Name/`
2. Copy relevant content to `problems/Problem-01/01-Problem-Definition.md`
3. Update journal metadata: `project_promoted: "Project-Name"`
4. Link from project back to journal: `00-Feature-Index.md` references journal entry

---

## Features vs. Problems vs. Solutions

### Hierarchy Clarification

```
PROJECT
  └── PROBLEM (high-level challenge)
      └── SOLUTION (approach to solve problem)
          └── FEATURES (capabilities that implement solution)
              └── FUNCTIONALITIES (what the feature does)
                  └── MODULES (code that implements functionalities)
                      └── ATTRIBUTES (properties/data captured)
```

### Example: Health Fundamentals Project

```
projects/Health_Fundamentals/
└── problems/
    └── Problem-01-Information-Overload/
        ├── 01-Problem-Definition.md
        │   "Users overwhelmed by health information from multiple sources"
        │
        ├── 02-Solution-Approach.md
        │   "Structured intake → chunking → summarization → knowledge base"
        │
        └── features/
            ├── chunking/
            │   ├── 01-Problem-Definition.md
            │   │   "Large documents exceed LLM context windows"
            │   ├── 02-Solution-Approach.md
            │   │   "Split into semantic chunks with overlap"
            │   └── ...
            │
            └── summarization/
                └── ...
```

### Key Insight

- **Problem** = User-facing challenge (information overload)
- **Solution** = High-level approach (structured pipeline)
- **Feature** = Technical capability (chunking, summarization)
- **Functionality** = What the feature does (parse conversations, detect speakers)
- **Module** = Code component (ChunkingService.cs)

---

## Multi-Project Coordination

### Strategic Folder

Each project has a `strategic/` folder for cross-cutting concerns:

```
projects/{Project-Name}/strategic/
├── timeline.md              ← Gantt chart, milestones
├── priorities.md            ← Priority matrix (urgent/important)
├── resources.md             ← People, time, budget allocated
├── dependencies.md          ← What blocks what?
└── status.md                ← Weekly/monthly status updates
```

### Cross-Project View

```
ThingsToDo/
├── strategic-dashboard.md   ← All projects overview
├── timeline-consolidated.md ← Combined timeline
└── projects/
    ├── project-a-tasks.json
    └── project-b-tasks.json
```

**Navigation:**
- Each project is self-contained
- `ThingsToDo/` aggregates across projects
- No duplication, links to project files

---

## AI Agent Access Patterns

### Single File Access

```
Agent: "What problem does chunking solve?"
System: Read C:\me_workspace\projects\me_workspace\problems\Problem-01\features\chunking\01-Problem-Definition.md
```

### Multi-File Context

```
Agent: "Summarize the chunking feature"
System: Read:
  - 00-Feature-Index.md (links)
  - 01-Problem-Definition.md
  - 02-Solution-Approach.md
  - 08-Functionalities.md
  - 14-Future-Vision.md
```

### Full Problem Context

```
Agent: "Evaluate Problem-01 feasibility"
System: Read all 15 files in problems/Problem-01/
```

### Why This Matters

- **Context window limits:** Agent loads only needed files
- **Parallel processing:** Multiple agents, different files
- **Incremental updates:** Change one file, agent re-reads only that one

---

## Folder Creation Rules

### When to Create a Problem Folder

**Create when:**
- Distinct user-facing challenge identified
- Solution requires multiple features
- Problem deserves separate strategic planning

**Don't create when:**
- Small enhancement to existing feature
- One-off fix or patch
- Temporary workaround

### When to Create a Feature Folder

**Create when:**
- Reusable capability (used by multiple problems)
- Significant implementation (100+ lines of code)
- Separate documentation needed (15 decision files justified)

**Don't create when:**
- Single function or utility
- Internal-only (no user-facing impact)
- Part of another feature's module

### Naming Rules

**Problems:**
- `Problem-01`, `Problem-02`, etc.
- Optional descriptive suffix: `Problem-01-Data-Ingestion`
- Always hyphenated, zero-padded

**Features:**
- Lowercase, hyphens: `chunking`, `intake`, `bible-verse-linking`
- Short (1-3 words)
- Descriptive of capability

**Files:**
- Zero-padded numbers: `01`, `02`, ..., `15`
- Hyphenated: `Problem-Definition`, `Technology-Choices`
- `.md` extension always

---

## File Size Guidelines

### Expected Sizes

| File                   | Typical Size | Max Recommended          |
| ---------------------- | ------------ | ------------------------ |
| Problem-Definition     | 1-5 pages    | 20 pages                 |
| Solution-Approach      | 2-10 pages   | 30 pages                 |
| Workflow               | 5-20 pages   | 50 pages (with diagrams) |
| Technology-Choices     | 3-10 pages   | 25 pages                 |
| Modules-Implementation | 10-50 pages  | 100 pages                |
| Other files            | 2-15 pages   | 30 pages                 |

### When Files Grow Too Large

**Option 1: Split into sub-sections**
```
09-Technology-Choices/
├── README.md              ← Overview
├── Backend-Stack.md
├── Frontend-Stack.md
└── Infrastructure.md
```

**Option 2: Extract to sub-features**
```
features/
├── chunking/
│   ├── 01-Problem-Definition.md
│   └── sub-features/
│       ├── conversation-parsing/
│       └── size-based-chunking/
```

**Option 3: Archive old content**
```
09-Technology-Choices.md           ← Current
archive/
└── 09-Technology-Choices-2025.md  ← Historical
```

---

## Examples

### Example 1: Simple Mini Project

```
projects/Painting-Living-Room/
├── README.md                     "Paint living room"
├── vision.md                     "Refresh home aesthetics"
└── problems/
    └── Problem-01-Color-Selection/
        ├── 01-Problem-Definition.md   "Which color for living room?"
        ├── 02-Solution-Approach.md    "Test swatches, consult partner"
        ├── 04-Target-Audience.md      "Ourselves, guests"
        ├── 07-Workflow.md             "Buy samples → test → decide → paint"
        ├── 09-Technology-Choices.md   "Sherwin-Williams paint"
        └── 15-Release-Timeline.md     "This Saturday"

        [Files 03,05,06,08,10-14 exist but marked "N/A - Mini project"]
```

### Example 2: Business Plan Project

```
projects/Consulting-Service/
├── README.md
├── vision.md
├── executive-summary.md           ← VISIBLE (business plan)
├── financial-overview.md          ← VISIBLE (business plan)
├── strategic/
│   ├── timeline.md                "Launch Q3 2026"
│   ├── priorities.md              "Client acquisition = P0"
│   └── resources.md               "$50K seed funding needed"
└── problems/
    ├── Problem-01-Client-Acquisition/
    │   ├── [ALL 15 files fully populated]
    │   └── features/
    │       ├── marketing-automation/
    │       └── crm-integration/
    │
    └── Problem-02-Service-Delivery/
        └── [ALL 15 files]
```

### Example 3: Book Project

```
projects/Health-Book/
├── README.md
├── vision.md                      "Empower readers with practical health knowledge"
├── executive-summary.md           ← Acts as preface/introduction
└── problems/
    ├── Problem-01-Foundations/    ← Chapter 1
    │   ├── 01-Problem-Definition.md   "Readers lack foundational understanding"
    │   ├── 02-Solution-Approach.md    "Explain building blocks clearly"
    │   └── ...
    │
    ├── Problem-02-Nutrition/      ← Chapter 2
    └── Problem-03-Exercise/       ← Chapter 3
```

---

## Complexity Scaling & Template System

### Core Principle: Progressive Disclosure

**Start simple. Add structure only when value is clear.**

The system uses **on-demand file creation** from templates rather than pre-creating empty files. This keeps the file system clean and ensures files exist only when they have purpose.

---

### Content Type Matrix

Different content types start with different file sets:

| Content Type | Initial Files | Purpose |
|-------------|--------------|---------|
| **Journal Entry** | 3 files | Daily reflection, notes, thoughts |
| **Evaluation Phase** | 7 files | Assessing whether an idea warrants project status |
| **Mini Project** | 5-7 files | Small focused work (personal projects, quick builds) |
| **Feature** | 15 files | Technical capabilities requiring full decision context |
| **Business Plan** | 15+ files | Strategic projects requiring comprehensive planning |
| **Book Project** | 15+ files | Long-form content with chapter structure |

---

### Template Storage Structure

Templates are stored in `C:\me_workspace\templates\` and copied on-demand:

```
templates/
├── journal-entry/
│   ├── entry.md              ← Main content
│   ├── meta.json             ← Metadata (date, tags, status)
│   └── summary.md            ← Quick context (AI-generated)
│
├── evaluation-phase/
│   ├── 01-Problem-Definition.md
│   ├── 02-Solution-Approach.md
│   ├── 04-Target-Audience.md
│   ├── 05-Benefits.md
│   ├── 06-Financial-Considerations.md
│   ├── 14-Future-Vision.md
│   └── 15-Release-Timeline.md
│
├── mini-project/
│   ├── 01-Problem-Definition.md
│   ├── 02-Solution-Approach.md
│   ├── 07-Workflow.md
│   ├── 14-Future-Vision.md
│   └── 15-Release-Timeline.md
│
├── feature-complete/
│   ├── 00-Feature-Index.md
│   ├── 01-Problem-Definition.md
│   ... (all 15 files)
│   └── 15-Release-Timeline.md
│
└── business-plan/
    ├── executive-summary.md
    ├── financial-overview.md
    └── problems/
        └── problem-template/
            └── (all 15 files)
```

---

### Progressive Disclosure Flow

#### **Stage 1: Journal Entry (3 files)**

User writes a journal entry about an idea, reflection, or discovery.

```
Journals/2026/05/entry-2026-05-25-garlic-benefits/
├── entry.md              ← User writes content here
├── meta.json             ← Auto-generated metadata
└── summary.md            ← AI generates summary
```

**Files created:** 3  
**Purpose:** Capture thoughts, no evaluation yet

---

#### **Stage 2: Evaluation Phase (7 files)**

User or AI recognizes potential value. User approves **"Evaluate for Project"**.

System creates `evaluation/` folder with key decision files:

```
Journals/2026/05/entry-2026-05-25-garlic-benefits/
├── entry.md
├── meta.json
├── summary.md
└── evaluation/           ← NEW FOLDER
    ├── 01-Problem-Definition.md       ← AI extracts from journal
    ├── 02-Solution-Approach.md        ← AI proposes approach
    ├── 04-Target-Audience.md          ← "Who benefits?"
    ├── 05-Benefits.md                 ← "Why pursue this?"
    ├── 06-Financial-Considerations.md ← "What does it cost?"
    ├── 14-Future-Vision.md            ← "Where could this go?"
    └── 15-Release-Timeline.md         ← "When to start?"
```

**Files created:** 7 additional  
**Purpose:** Determine if this warrants project status  
**AI behavior:** Agent reads journal, extracts problem/solution context, pre-fills evaluation files  
**User approval required:** Yes - agent asks permission to go deeper into opportunity analysis

---

#### **Stage 3: Project Status (15 files)**

User reviews evaluation and decides: **"Create Official Project"**.

System creates full project structure:

```
projects/Garlic_Health_Research/
├── README.md                    ← Project overview
├── vision.md                    ← High-level goals
├── strategic/
│   ├── timeline.md
│   ├── priorities.md
│   └── resources.md
│
└── problems/
    └── Problem-01-Health-Benefits/
        ├── 00-Feature-Index.md              ← Links back to journal
        ├── 01-Problem-Definition.md         ← Copied from evaluation
        ├── 02-Solution-Approach.md          ← Copied from evaluation
        ├── 03-Competitive-Analysis.md       ← NEW (template)
        ├── 04-Target-Audience.md            ← Copied from evaluation
        ├── 05-Benefits.md                   ← Copied from evaluation
        ├── 06-Financial-Considerations.md   ← Copied from evaluation
        ├── 07-Workflow.md                   ← NEW (template)
        ├── 08-Functionalities.md            ← NEW (template)
        ├── 09-Technology-Choices.md         ← NEW (template)
        ├── 10-UI-Design.md                  ← NEW (template)
        ├── 11-Attributes.md                 ← NEW (template)
        ├── 12-Configuration-Parameters.md   ← NEW (template)
        ├── 13-Modules-Implementation.md     ← NEW (template)
        ├── 14-Future-Vision.md              ← Copied from evaluation
        └── 15-Release-Timeline.md           ← Copied from evaluation
```

**Files created:** All 15 decision files  
**Purpose:** Full project with comprehensive decision context  
**Evaluation files:** Copied to project (not duplicated, moved)  
**Journal link:** `00-Feature-Index.md` references original journal entry

---

#### **Stage 4: Output Generation (Business Plan, Book, etc.)**

Project structure remains the same. **Outputs are generated FROM the project:**

```
projects/Garlic_Health_Research/
├── ... (all project files)
│
└── outputs/
    ├── business-plan/
    │   ├── Business-Plan-v1.0.pdf
    │   ├── executive-summary.md      ← Generated from project vision
    │   ├── financial-projections.md  ← Generated from 06-Financial-Considerations
    │   └── market-analysis.md        ← Generated from 03-Competitive-Analysis
    │
    ├── book-draft/
    │   ├── Chapter-01-Introduction.md
    │   ├── Chapter-02-Science.md
    │   └── references.md
    │
    └── research-report/
        └── Garlic-Health-Benefits-Report-2026.pdf
```

**Key insight:** Outputs are **generated artifacts** from project data, not separate structures.

---

### When to Use What Structure

#### **Use Journal Entry (3 files) when:**
- Daily reflection, notes, observations
- Capturing ideas without commitment
- Personal thoughts, emotional processing
- Quick notes from conversations or reading

**Example:** "Today I learned garlic has antimicrobial properties"

---

#### **Use Evaluation Phase (7 files) when:**
- An idea shows potential but needs assessment
- Uncertain if pursuit is worthwhile
- Need structured analysis before committing resources
- Want AI to help explore opportunity depth

**Example:** "Garlic research might be valuable - let's evaluate if it warrants a project"

**User approval:** Required before AI digs deeper into full context

---

#### **Use Mini Project (5-7 files) when:**
- Small personal projects (learning a skill, home improvement)
- Quick builds (tools, scripts, small apps)
- Projects with limited scope and clear end date
- No need for competitive analysis or complex workflows

**Example:** "Build a garlic tracking app for personal use"

**Files needed:** Problem, Solution, Workflow, Vision, Timeline

---

#### **Use Feature (15 files) when:**
- Building technical capabilities for the application
- Need comprehensive decision context to avoid blind spots
- Will be maintained and extended over time
- Requires technology choices, configuration, implementation details

**Example:** "Chunking feature for me_workspace"

**Files needed:** All 15 - this is active development requiring full context

---

#### **Use Business Plan (15+ files) when:**
- Seeking funding or strategic partnerships
- Need financial projections and market analysis
- Building for commercial viability
- Stakeholders require comprehensive documentation

**Example:** "Garlic health supplement business"

**Files needed:** All 15 + executive summary + financial overview

---

#### **Use Book Project (15+ files) when:**
- Long-form content creation (book, course, comprehensive guide)
- Each problem = chapter or major section
- Need chapter structure, narrative flow, publication timeline
- Audience research and market positioning required

**Example:** "The Complete Guide to Garlic for Health"

**Files needed:** All 15 per chapter + preface + publication plan

---

### Template Application Logic

#### **Code Example: Promotion Flow**

```csharp
// JournalPromotionService.cs
public async Task<string> PromoteToEvaluationAsync(Guid journalId)
{
    var journal = await _context.Journals.FindAsync(journalId);
    var evaluationPath = Path.Combine(journal.FolderPath, "evaluation");
    
    // Copy evaluation template
    var templatePath = "C:/me_workspace/templates/evaluation-phase";
    await CopyTemplateAsync(templatePath, evaluationPath);
    
    // Pre-fill with AI analysis
    var context = await _aiService.AnalyzeJournalForEvaluationAsync(journal.Content);
    
    await PreFillTemplateAsync(evaluationPath, "01-Problem-Definition.md", context.Problem);
    await PreFillTemplateAsync(evaluationPath, "02-Solution-Approach.md", context.Solution);
    await PreFillTemplateAsync(evaluationPath, "04-Target-Audience.md", context.Audience);
    
    journal.Status = JournalStatus.UnderEvaluation;
    await _context.SaveChangesAsync();
    
    return evaluationPath;
}

public async Task<string> PromoteToProjectAsync(Guid journalId, string projectName)
{
    var journal = await _context.Journals.FindAsync(journalId);
    var evaluationPath = Path.Combine(journal.FolderPath, "evaluation");
    
    // Create project structure
    var projectPath = $"C:/me_workspace/projects/{projectName}";
    var problemPath = Path.Combine(projectPath, "problems", "Problem-01");
    
    Directory.CreateDirectory(problemPath);
    
    // Copy complete template
    var templatePath = "C:/me_workspace/templates/feature-complete";
    await CopyTemplateAsync(templatePath, problemPath);
    
    // Copy evaluation files to project (preserving user edits)
    await CopyFileAsync(
        Path.Combine(evaluationPath, "01-Problem-Definition.md"),
        Path.Combine(problemPath, "01-Problem-Definition.md")
    );
    
    // ... copy other evaluation files
    
    // Create backlink
    var indexContent = $"# Feature Index\n\n**Originated from:** [Journal Entry]({journal.FolderPath}/entry.md)\n";
    await File.WriteAllTextAsync(Path.Combine(problemPath, "00-Feature-Index.md"), indexContent);
    
    journal.Status = JournalStatus.PromotedToProject;
    journal.PromotedProjectName = projectName;
    await _context.SaveChangesAsync();
    
    return projectPath;
}
```

---

### AI Agent Behavior by Stage

#### **Journal Stage:**
```
Agent reads: entry.md (1 file)
Agent looks for: Keywords, themes, potential value signals
Agent suggests: "This looks like it could be evaluated. Would you like me to assess it?"
```

#### **Evaluation Stage:**
```
Agent reads: entry.md + evaluation/*.md (8 files)
Agent analyzes: Problem clarity, solution viability, audience fit, resource requirements
Agent suggests: "This evaluation shows strong potential. Ready to create a project?"
```

#### **Project Stage:**
```
Agent reads: All 15 files when doing comprehensive analysis
Agent reads: Specific files (e.g., 09-Technology-Choices.md) when answering focused questions
Agent suggests: "Based on your current priorities, should we move this to active development?"
```

**Key insight:** AI doesn't load all 15 files unless doing full evaluation. For specific questions, it reads only relevant files.

---

### Visibility & Discoverability

#### **In VS Code File Explorer:**

**Journal (Stage 1):**
```
📁 entry-2026-05-25-garlic-benefits
  📄 entry.md
  📄 meta.json
  📄 summary.md
```
**Clean. Minimal. Focused.**

**Journal (Stage 2 - Evaluation):**
```
📁 entry-2026-05-25-garlic-benefits
  📄 entry.md
  📄 meta.json
  📄 summary.md
  📁 evaluation/
    📄 01-Problem-Definition.md
    📄 02-Solution-Approach.md
    📄 04-Target-Audience.md
    📄 05-Benefits.md
    📄 06-Financial-Considerations.md
    📄 14-Future-Vision.md
    📄 15-Release-Timeline.md
```
**Clear separation: original entry + evaluation context**

**Project (Stage 3):**
```
📁 Garlic_Health_Research
  📄 README.md
  📄 vision.md
  📁 strategic/
  📁 problems/
    📁 Problem-01-Health-Benefits/
      📄 00-Feature-Index.md
      📄 01-Problem-Definition.md
      ... (all 15 files)
      📄 15-Release-Timeline.md
```
**Complete structure for active project management**

---

### File System Cleanliness

**What you WON'T see:**
- ❌ Empty stub files marked "inactive"
- ❌ Hundreds of unused templates cluttering journals
- ❌ Metadata tracking which files are "active"
- ❌ UI logic to hide/show files based on status

**What you WILL see:**
- ✅ Only files that have content or purpose
- ✅ Clear progression: 3 files → 7 files → 15 files
- ✅ Templates copied only when needed
- ✅ Clean git history (only meaningful files tracked)

---

### Summary of Complexity Scaling

| Stage | Files | Purpose | AI Reads | User Effort |
|-------|-------|---------|----------|-------------|
| **Journal** | 3 | Capture thoughts | 1 file | Write entry |
| **Evaluation** | 7 | Assess potential | 8 files | Review AI analysis |
| **Mini Project** | 5-7 | Quick builds | 5-7 files | Fill key decisions |
| **Feature** | 15 | Active development | 15 files | Comprehensive planning |
| **Business Plan** | 15+ | Strategic project | 15+ files | Full business analysis |

**Core principle:** Start simple (journal). Add structure when value is clear (evaluation). Use full structure when actively building (project).

---

## Summary

### Key Principles

1. **Encapsulation** - Everything about a problem in one folder
2. **Consistency** - Same 15 files, every problem, every feature
3. **Unix Philosophy** - Small files, clear purpose, AI-accessible
4. **Progressive Disclosure** - Start simple, add structure on-demand
5. **Template-Driven** - Files copied from templates when needed, not pre-created
6. **Hierarchical** - Project → Problem → Solution → Feature → Module → Attribute

### File Hierarchy Recap

```
15 Decision Files (universal template)
  ↓
Problems (user-facing challenges)
  ↓
Features (technical capabilities)
  ↓
Functionalities (what it does)
  ↓
Modules (code implementation)
  ↓
Attributes (properties/data)
```

### Navigation Rule

**"Enter any folder, understand the whole story."**

An AI agent (or human) should be able to:
1. Enter `problems/Problem-01/`
2. Read the 15 files
3. Understand: problem, solution, tech, design, status, future
4. Without needing context from parent/sibling folders

---

**See also:**
- [../docs/System-Architecture.md](../docs/System-Architecture.md) - Conceptual vision
- [./Features/README.md](./Features/README.md) - Feature catalog
- [./Phases.md](./Phases.md) - Implementation timeline
