# Solution Approach - Global Inbox

## Overview

Build a **three-level inbox system** that extends GTD inbox-zero principles to your entire life.

**Core Philosophy**: Capture anything → Auto-triage → Process → Resolve → Output

## The Three Levels

### Level 1: Universal RAW Capture (00-Global-Inbox/)

**Purpose**: Friction-free capture of anything that doesn't have a home yet.

**Accepts**:
- Random thoughts
- Emotions and feelings
- Pictures and screenshots
- Files (PDFs, documents)
- Voice notes
- Web clips
- Anything you don't want to categorize

**Auto-Triage Logic**:
```
Item arrives in 00-Global-Inbox/
    ↓
System detects type:
├─→ File (PDF, image)? → Auto-move to 01-Inbox/ → Automatic processing
├─→ Text note? → Create InboxItem in database → Show in Insights panel
├─→ Emotion keyword detected? → Auto-generate journal/{id}/ → Start typing
└─→ Project keyword detected? → Suggest creating project/ → User confirms
```

### Level 2: Project-Specific Capture (projects/{id}/inbox/)

**Purpose**: When you know where something belongs.

**Use Cases**:
- Research for specific project
- Ideas related to ongoing work
- Files specific to deliverable
- Notes for particular goal

**Flow**:
```
User drops item in projects/{id}/inbox/
    ↓
System:
├─→ Files → Process with project context
├─→ Notes → Link to project timeline
└─→ Ideas → Add to project notes.md
```

### Level 3: Journal-Specific Capture (journals/{id}/inbox/)

**Purpose**: Emotional processing and therapeutic journaling.

**Triggers**:
- User explicitly creates journal
- System detects emotion keywords in 00-Global-Inbox/
- Scheduled check-in prompts
- Voice note with emotional content

**Auto-Generated Structure**:
```
journals/{id}/
├── inbox/              ← Raw captures
├── entries/
│   ├── 2026-05-25-1430.md   ← Auto-generated, start typing
│   └── 2026-05-26-0900.md
├── chat/               ← AI processing conversations
├── curated/            ← Refined for sharing/publishing
└── metadata.json       ← Status, duration, resolution state
```

## The Processing Flow

### Step 1: Capture (No Friction)

User drops anything into any inbox (global/project/journal).

**Key Feature**: No categorization required. System doesn't force choices.

### Step 2: Auto-Triage (System Intelligence)

```
Analyze item:
├─→ File type? (PDF, image, doc)
├─→ Content keywords? (emotion, project, topic)
├─→ Time sensitivity? (urgent, routine, someday)
└─→ Relationship? (standalone, relates to existing item)

Route accordingly:
├─→ Automatic processing (files)
├─→ Insights panel (review queue)
├─→ Journal generation (emotions)
└─→ Project suggestion (actionable ideas)
```

### Step 3: Process (Inbox-Zero)

**Insights Panel** (Central Review Queue):
```
Show all unprocessed items:
├─→ Random thoughts → Convert to note, project, or journal
├─→ Pictures → Tag and connect to context
├─→ Voice notes → Transcribe and route
└─→ Web clips → Extract and categorize

User actions:
├─→ Archive (done, no action needed)
├─→ Convert to project (needs work)
├─→ Convert to journal (needs processing)
├─→ Defer (not now, someday)
└─→ Delete (not useful)
```

### Step 4: Resolve (Effective Resolution)

**Journal Path**:
```
Emotion captured → Journal auto-generated
    ↓
User pours out thoughts (3 min to 3 years)
    ↓
Resolution options:
├─→ Writing alone resolves ✓
├─→ AI conversation provides insight ✓
├─→ Share curated logs with practitioner 💰
└─→ Discover action needed → Create project
```

**Project Path**:
```
Idea captured → Project created
    ↓
Gather information (files, research, notes)
    ↓
Process with AI assistance
    ↓
Generate outputs:
├─→ Strategy document
├─→ Action plan
├─→ Book draft
└─→ Course outline
```

### Step 5: Output (Dual-Purpose)

**Personal Outputs**:
- Resolved emotional issues
- Completed projects
- Organized knowledge
- Clear action plans

**Marketable Outputs**:
- Books (healing journey → published work)
- Courses (research → educational content)
- Practitioner insights (curated logs → subscription service)
- Consulting materials (strategy docs → client deliverables)

## Technical Architecture

### Database + Filesystem Hybrid

**Database** (SQLite):
- `InboxItems` - Universal capture queue
- `Projects` - Already exists ✅
- `Journals`, `JournalEntries` - Emotional processing
- `Outputs` - Publishable artifacts
- `PractitionerShares` - Revenue feature (future)

**Filesystem** (Folder-as-Agent):
```
me_workspaces_runtime/
├── 00-Global-Inbox/        ← Universal capture
│   ├── files/
│   ├── thoughts/
│   ├── pictures/
│   └── misc/
├── projects/{id}/          ← Project-specific
│   ├── inbox/
│   ├── notes/
│   ├── chat/
│   └── outputs/
├── journals/{id}/          ← Journal-specific
│   ├── inbox/
│   ├── entries/
│   ├── chat/
│   └── curated/
└── 01-Inbox/               ← Automatic file processing ✅
```

### Services Layer

**InboxService.cs**:
- Watch 00-Global-Inbox/ for new items
- Analyze content and detect type
- Auto-route to appropriate location
- Create database records

**JournalService.cs**:
- Auto-generate journal folders
- Create time-stamped entries
- Manage journal lifecycle
- Track resolution state

**InsightsPanelService.cs**:
- Query unprocessed InboxItems
- Present in priority order
- Handle user actions (convert, archive, defer, delete)
- Track inbox-zero metrics

**OutputService.cs**:
- Curate raw logs for publishing
- Generate book drafts from journals
- Create practitioner-shareable summaries
- Manage revenue-generating artifacts

### API Endpoints

**InboxEndpoints.cs**:
- `POST /api/inbox/capture` - Universal capture endpoint
- `GET /api/inbox/items?unprocessed=true` - Insights panel data
- `POST /api/inbox/{id}/convert` - Convert to project/journal
- `POST /api/inbox/{id}/archive` - Mark as done

**JournalEndpoints.cs**:
- `POST /api/journals/create` - Auto-generate journal
- `GET /api/journals/{id}/entries` - List entries
- `POST /api/journals/{id}/curate` - Create shareable version
- `GET /api/journals/metrics` - Track resolution progress

## Key Differentiators

✅ **No Forced Categorization** - Drop first, organize later  
✅ **Three-Level Flexibility** - Global/Project/Journal as needed  
✅ **Auto-Triage Intelligence** - System routes items  
✅ **Context Preservation** - Never lose conversation history  
✅ **Dual-Purpose Design** - Healing + Products from same journey  
✅ **Local-First Ownership** - Your data, your computer  
✅ **Inbox-Zero Everywhere** - Not just email  
✅ **Revenue-Enabled** - Built for practitioner subscriptions + content products
