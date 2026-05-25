# System Architecture

**Last Updated:** May 25, 2026  
**Purpose:** High-level vision and architectural principles for the me_workspace system

---

## Overview

me_workspace is a **holistic project management and content creation platform** that integrates:
- **Strategic Planning** - Multi-project coordination and prioritization
- **Content Generation** - Books, business plans, journals, reports
- **Personal Wellness** - Mental health support, journaling, conversational AI
- **Workflow Automation** - AI agents, evaluation pipelines, recommendation systems

The system follows a **hybrid project/business plan architecture** where every project contains structured decision-making factors that can be evaluated, refined, and transformed into various outputs (business plans, books, reports, etc.).

---

## Core Hierarchy

```
PROJECT (Vision & Strategy)
  │
  ├── PROBLEM 1
  │   ├── Problem Definition
  │   ├── Solution Approach
  │   ├── Competitive Analysis
  │   ├── Target Audience
  │   ├── Benefits
  │   ├── Financial Considerations
  │   ├── Workflow
  │   ├── Functionalities (what it does)
  │   ├── Technology Choices
  │   ├── UI Design
  │   ├── Attributes (properties/metadata)
  │   ├── Configuration Parameters
  │   ├── Modules (code implementation)
  │   ├── Future Vision
  │   └── Release Timeline
  │
  ├── PROBLEM 2
  │   └── [Same 13 decision factors]
  │
  └── PROBLEM N...
```

### Key Principle: Encapsulation

Each problem is **self-contained** with all decision-making information in one place. No jumping between pages 5 and 20 of a document. Everything needed to understand, evaluate, and implement a solution is compartmentalized within that problem's folder.

---

## Content Lifecycle

### Stage 1: Journal Entry (Ideation)
```
User Input (Chat/Voice/Text)
  ↓
System captures thoughts
  ↓
AI evaluates patterns, themes, viability
  ↓
Recommendation: Continue? Create project? Archive?
```

### Stage 2: Unofficial Project (Exploration)
```
Accepted idea becomes "Unofficial Project"
  ↓
Basic structure created (problems, initial solutions)
  ↓
Lightweight evaluation, no full business plan yet
  ↓
Decision: Promote to Official? Continue exploring? Archive?
```

### Stage 3: Official Project (Execution)
```
Promoted to "Official Project"
  ↓
Full 13-factor analysis per problem
  ↓
Strategic management enabled (timelines, priorities, resources)
  ↓
Multi-project coordination
  ↓
Output generation (business plan, book, report)
```

---

## The 13 Decision Factors (Universal Template)

Every problem in every project follows this structure:

1. **Problem Definition** - What pain point exists?
2. **Solution Approach** - How do we solve it?
3. **Competitive Analysis** - What alternatives exist? How do we compare?
4. **Target Audience** - Who benefits? End users, personas
5. **Benefits** - Value to company, users, system
6. **Financial Considerations** - Cost, ROI, resource allocation, priority weights
7. **Workflow** - How it works (steps, diagrams)
8. **Functionalities** - What capabilities does this provide?
9. **Technology Choices** - Tech stack, why chosen, alternatives considered
10. **UI Design** - Screen layout, UX, interaction patterns
11. **Attributes** - Properties, metadata captured by this solution
12. **Configuration Parameters** - Settings, tuning options
13. **Modules** - Code implementation, APIs, architecture
14. **Future Vision** - Roadmap, planned enhancements
15. **Release Timeline** - Now vs later, phasing strategy

**Why 13+ factors?**  
These capture the holistic view needed before building anything:
- **Business factors** (competitive, financial, audience)
- **Design factors** (workflow, UI, functionalities)
- **Technical factors** (technology, modules, configuration)
- **Strategic factors** (future vision, timeline)

---

## Multi-Project Strategic Management

### Problem
How do you manage:
- Weekend painting project
- Book writing project  
- Business plan for new service
- Personal journal entries

...all at the same time, with different priorities, timelines, and outputs?

### Solution
**Things To Do + Project Coordination**

```
Strategic Dashboard
  ├── Active Projects (with priorities)
  ├── Timelines (visualized across projects)
  ├── Resource Allocation
  ├── Dependency Tracking
  └── Output Status (book 60% done, business plan pending review)
```

Each project maintains its own problem/solution structure, but the strategic layer provides:
- Cross-project view
- Priority management
- Timeline coordination
- Resource balancing

---

## Content Type Flexibility

### Core Principle: Conditional Visibility

Every project has the **same underlying structure**, but sections are shown/hidden based on content type:

| Section | Journal | Weekend Project | Book | Business Plan |
|---------|---------|----------------|------|---------------|
| Executive Summary | ❌ | ❌ | ✅ | ✅ |
| Financial Analysis | ❌ | ❌ | ❌ | ✅ |
| Competitive Analysis | ❌ | ❌ | ✅ | ✅ |
| Problem/Solution | ✅ | ✅ | ✅ | ✅ |
| Workflow | ❌ | ✅ | ✅ | ✅ |

**Why?**  
A weekend painting project doesn't need an executive summary *now*, but if it evolves into a painting service business, those sections become visible without restructuring.

### Output Templates

Each content type has a template that:
1. Pulls from the 13-factor structure
2. Formats for the target audience (investors, readers, self)
3. Generates appropriate sections (exec summary, chapters, appendices)

---

## AI Agent Integration

### Agent Access Pattern (Unix Philosophy)

Agents access **individual files** based on need:
- Need tech stack info? → Read `09-Technology-Choices.md`
- Need workflow? → Read `07-Workflow.md`
- Need everything? → Read `00-Feature-Index.md` for links

**Benefits:**
- No context window overflow (agent loads only what it needs)
- Parallel agent operations (multiple agents, different files)
- Incremental updates (change one file without reprocessing 100-page doc)

### Agent Workflows

**Evaluation Agent:**
```
1. Read journal entries
2. Identify patterns, recurring themes
3. Assess viability (problem clarity, solution potential)
4. Generate recommendation report
5. Prompt user: "This idea has potential. Create project?"
```

**Research Agent:**
```
1. Read Problem-Definition.md
2. Search market for alternatives
3. Update Competitive-Analysis.md
4. Flag new technologies in Technology-Choices.md
```

**Implementation Agent:**
```
1. Read Functionalities.md (what to build)
2. Read Modules.md (how it's structured)
3. Generate/update code
4. Update Implementation status
```

---

## Folder Structure Philosophy

### File Explorer View

```
C:\me_workspace\
├── projects/
│   ├── Project-A/
│   │   ├── vision.md
│   │   ├── executive-summary.md (visible if business plan)
│   │   ├── problems/
│   │   │   ├── Problem-01/
│   │   │   │   ├── 01-Problem-Definition.md
│   │   │   │   ├── 02-Solution-Approach.md
│   │   │   │   ├── 03-Competitive-Analysis.md
│   │   │   │   ├── ... (all 13 factors)
│   │   │   │   └── features/
│   │   │   │       ├── chunking/
│   │   │   │       ├── intake/
│   │   │   │       └── ...
│   │   │   ├── Problem-02/
│   │   │   └── Problem-03/
│   │   └── strategic/
│   │       ├── timeline.md
│   │       ├── priorities.md
│   │       └── resources.md
│   │
│   └── Project-B/
│       └── ... (same structure)
│
├── journals/
│   └── 2026/
│       └── 05-May/
│           └── entry-2026-05-25.md
│
└── ThingsToDo/
    ├── strategic-view.md
    └── tasks/
```

### Navigation Principle

**Encapsulation** = AI can enter any problem folder and understand:
- What we're solving
- Why it matters
- How we're solving it
- What's been built
- What's next

Without needing context from other folders.

---

## Mental Health & Wellness Integration

### Core Belief
Development doesn't happen in isolation. The platform supports:

1. **Journaling** - Process thoughts, emotions, ideas
2. **Conversational AI** - Loneliness support, idea exploration
3. **Holistic Planning** - Balance work, life, creativity
4. **Global Attributes** - Mood tracking, energy levels, focus time

### How It Works

**Journal Entry → Project Pipeline:**
```
User: "I've been feeling lonely and thinking about starting a community..."

System:
1. Captures entry with emotional context
2. AI identifies: problem (loneliness), potential solution (community building)
3. Suggests: "Would you like to explore this as a project?"
4. If yes → Creates unofficial project with Problem-01: Loneliness in target demographic
```

**Mental Health Features:**
- Voice journaling (privacy-focused, offline)
- Pattern recognition (recurring themes)
- Supportive recommendations (not just task-focused)
- Flexibility (journal without project pressure)

---

## Design Philosophy

### Unix Philosophy Applied
- **Small focused files** - Each file has one responsibility
- **Composability** - Files link together to form complete picture  
- **Text-based** - Readable by humans and AI
- **Modularity** - Add/remove sections without breaking system

### Business Plan Philosophy Applied
- **Encapsulation** - Everything in one place
- **Decision factors** - Capture WHY before building
- **Holistic view** - Business, technical, design, strategic
- **Audience-aware** - Investors, users, developers all have context

### Hybrid Approach
The system is **both**:
- Technical platform (code, APIs, features)
- Personal wellness tool (journaling, mental health)
- Business generator (plans, books, reports)
- Strategic manager (multi-project coordination)

---

## Key Differentiators

### vs. Traditional Project Management
- **Holistic:** Integrates journals, mental health, creativity
- **AI-Native:** Agents evaluate, recommend, assist at every stage
- **Flexible Output:** One structure → many outputs (book, plan, report)

### vs. Content Creation Tools
- **Structured:** Not just blank page, guided by 13 factors
- **Decision-Focused:** Capture WHY before building
- **Multi-Project:** Manage book + business + journal simultaneously

### vs. Journaling Apps
- **Project Pipeline:** Ideas → evaluation → projects
- **Strategic Value:** Journals aren't dead-end, they seed projects
- **AI Evaluation:** System recognizes patterns, suggests actions

---

## Future Vision

### Phase 1: Foundation (Current)
- Core architecture established
- Chunking, intake, processing pipeline
- Basic folder structure

### Phase 2: Content Types
- Journal entry workflow
- Unofficial → Official project promotion
- Basic strategic dashboard

### Phase 3: AI Agents
- Evaluation agents (journal → recommendation)
- Research agents (competitive analysis updates)
- Implementation agents (code generation)

### Phase 4: Multi-Project
- Timeline visualization across projects
- Resource allocation dashboard
- Priority management UI

### Phase 5: Outputs
- Business plan generator (from 13 factors)
- Book generator (chapters from problems/solutions)
- Report generator (status, progress, insights)

---

## Technical Implementation Notes

### Current State (Phase 6 Complete)
- ✅ Database schema (Sources, Chunks, Summaries, Projects)
- ✅ PDF extraction pipeline
- ✅ Conversation chunking with turn detection
- ✅ Processing orchestration (scan → chunk → summarize)
- ✅ API endpoints for pipeline control

### Next Steps
1. Build Bible Constitution project (KJV + Ethiopian)
2. Add TTS review workflow
3. Native voice capture (security/privacy)
4. Global inbox (unified entry point)
5. Marriage book council workflow

### Technology Constraints
- **Offline-first:** Privacy, security, no cloud dependency
- **Local LLMs:** No API keys, user owns data
- **Cross-platform:** Windows/Linux/Mac support
- **Lightweight:** SQLite, no heavy servers

---

## Summary

me_workspace is a **holistic platform** where:
- Every project follows the **13 decision factors**
- Problems are **encapsulated** (all info in one place)
- Content flows from **journal → evaluation → project → output**
- AI agents access **individual files** (Unix philosophy)
- Output format is **flexible** (book, plan, report)
- Mental health is **integrated** (journaling, loneliness support)
- Multi-project **strategic management** keeps everything coordinated

**Core Principle:** Development is holistic, not isolated. The platform supports the whole person: strategist, creator, builder, human.
