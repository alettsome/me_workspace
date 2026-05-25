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
| **Weekend Project** | problems/, strategic/ (minimal), no financial            |
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

### Example 1: Simple Weekend Project

```
projects/Painting-Weekend/
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

        [Files 03,05,06,08,10-14 exist but marked "N/A - Weekend project"]
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

## Summary

### Key Principles

1. **Encapsulation** - Everything about a problem in one folder
2. **Consistency** - Same 15 files, every problem, every feature
3. **Unix Philosophy** - Small files, clear purpose, AI-accessible
4. **Conditional Visibility** - Structure accommodates all content types
5. **Hierarchical** - Project → Problem → Solution → Feature → Module → Attribute

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
