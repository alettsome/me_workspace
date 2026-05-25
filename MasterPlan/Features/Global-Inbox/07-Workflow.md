# Workflow - Global Inbox

## User Journey Overview

**Philosophy**: Capture first, organize never (system handles it).

This workflow document describes the complete user experience from initial capture through final output generation.

---

## Scenario 1: Random Thought (No Home Yet)

### The Situation

It's 2:30 AM. You wake up with an insight about gut health and fermented foods. Where does it go?

### The Flow

**Step 1: Capture (5 seconds)**
```
User opens capture interface:
├─→ Desktop app shortcut (Ctrl+Shift+C)
├─→ Voice note on phone (syncs to 00-Global-Inbox/)
├─→ Drag text file into 00-Global-Inbox/ folder
└─→ Type directly in web interface

System:
✓ Saves to 00-Global-Inbox/thoughts/
✓ Creates InboxItem record in database
✓ No categorization required
```

**Step 2: Auto-Triage (Automatic)**
```
System analyzes content:
├─→ Keyword: "gut health" → Already have Health Fundamentals project
├─→ Keyword: "fermented" → Research-oriented content
└─→ Time: 2:30 AM → Low priority, review later

System suggests:
"This looks related to 'Health Fundamentals' project. Add it there?"

User options:
├─→ Yes → Move to projects/health-fundamentals/inbox/
├─→ No → Keep in 00-Global-Inbox/ for later review
└─→ Ignore → System stops suggesting, leaves in inbox
```

**Step 3: Process (Next Morning)**
```
User opens Insights Panel (dashboard)

Shows:
├─→ "Gut health insight" (from 2:30 AM)
├─→ 2 PDFs automatically processed overnight
├─→ 1 journal entry from yesterday (needs review)
└─→ Total unprocessed: 4 items

User clicks "Gut health insight":
Actions available:
├─→ Convert to project → Creates projects/gut-health-research/
├─→ Add to existing project → Moves to projects/health-fundamentals/
├─→ Start journal → Creates journals/gut-health-journey/
├─→ Just save as note → Keeps in 00-Global-Inbox/ for reference
└─→ Archive → Done thinking about it

User selects: "Add to existing project"

System:
✓ Moves to projects/health-fundamentals/notes/2026-05-25-gut-insight.md
✓ Marks InboxItem as processed
✓ Updates project metadata
✓ Removes from Insights Panel
```

**Result**: Captured in 5 seconds at 2:30 AM, processed in 10 seconds next morning, now part of structured project.

---

## Scenario 2: Emotional Issue (Needs Processing)

### The Situation

Unexpected conflict at work. Feeling anxious and need to process it.

### The Flow

**Step 1: Capture (Immediate)**
```
User opens app (stress level high, needs quick capture):

Types: "I'm really upset about the meeting today..."

System detects:
├─→ Keyword: "upset" → Emotion detected
├─→ Keyword: "today" → Current issue
└─→ Context: User has previous journal "Work Stress"

System suggests:
"This sounds emotional. Start journaling?"
[Yes, start new entry] [Add to existing journal] [Just save note]

User clicks: "Add to existing journal"
```

**Step 2: Auto-Generate Entry (Instant)**
```
System:
✓ Opens journals/work-stress/entries/2026-05-25-1445.md
✓ Pre-fills with:
  ---
  Created: 2026-05-25 14:45
  Journal: Work Stress
  Status: Active
  Mood: [System will suggest based on keywords]
  ---
  
  I'm really upset about the meeting today...
  [cursor here - user continues typing]

✓ Auto-saves every 30 seconds
✓ No "save" button needed
```

**Step 3: Pour It Out (Duration: Variable)**
```
User writes for 15 minutes, getting it all out:
├─→ What happened
├─→ How it felt
├─→ Why it triggered them
├─→ What they wish they'd said
└─→ What they might do next

System:
✓ Continuously auto-saves
✓ No formatting required
✓ No structure enforced
✓ Just type and release
```

**Step 4: AI Processing (Optional)**
```
When user finishes typing, system offers:

"Would you like to process this with AI?"
[Yes, let's talk] [No, just save] [Review later]

If user clicks "Yes, let's talk":

System:
✓ Creates journals/work-stress/chat/2026-05-25-processing.md
✓ Loads entry as context
✓ AI responds with:
  - Reflection of what was shared
  - Questions to deepen understanding
  - Possible reframes
  - Actionable suggestions (if appropriate)

User can:
├─→ Continue conversation
├─→ Stop and save (return later)
└─→ Mark as resolved
```

**Step 5: Resolution Options**

**Option A: Writing Resolved It**
```
User marks journal entry as resolved.

System:
✓ Updates metadata: Status = Resolved
✓ Keeps all entries/chat for future reference
✓ Removes from "Active Journals" in Insights Panel
✓ Journal stays accessible (might return to it)
```

**Option B: Need External Help**
```
User decides to share with therapist.

System:
✓ Creates journals/work-stress/curated/therapist-share.md
✓ User selects which entries to include
✓ System generates summary:
  - Timeline of entries
  - Key themes
  - Progress made
  - Current state
✓ Export as PDF for therapist
✓ OR: Grant therapist direct access (practitioner subscription)
```

**Option C: Discover Action Needed**
```
Through journaling, user realizes: "I need to learn conflict resolution skills."

System suggests:
"This sounds like a project. Create one?"

User clicks yes:

System:
✓ Creates projects/conflict-resolution-skills/
✓ Links to journals/work-stress/ (context preserved)
✓ Suggests first steps:
  - Research books on conflict resolution
  - Take online course
  - Practice scenarios
  - Track improvements
```

**Result**: Emotional issue captured immediately, processed through journaling, either resolved through writing or escalated to therapist/project as needed. Full context preserved.

---

## Scenario 3: File Drop (Automatic Processing)

### The Situation

User downloads a PDF about microbiome research.

### The Flow

**Step 1: Drop File (2 seconds)**
```
User drags PDF from Downloads/ to 00-Global-Inbox/files/

System:
✓ Detects file type: PDF
✓ Moves to 01-Inbox/ for automatic processing
✓ Creates Source record in database
✓ Kicks off processing pipeline
```

**Step 2: Automatic Processing (Background)**
```
BackgroundProcessingService detects new file:

Step 1: Extract text with iText7
Step 2: Chunk content (500 tokens per chunk)
Step 3: Save to database (Chunks table)
Step 4: Save to filesystem (03-Chunked/{source-id}/)
Step 5: Create ProcessingNotification

User gets notification:
"✅ Microbiome Research.pdf processed
298 chunks created
Processing time: 4.2 seconds"
```

**Step 3: Review in Insights Panel**
```
User opens Insights Panel:

Shows:
├─→ "Microbiome Research.pdf" (processed overnight)
    ├─→ 298 chunks available
    ├─→ 87,451 characters processed
    └─→ Status: Ready for review

User clicks file:

System shows:
├─→ Chunk preview
├─→ Suggested projects:
    - Health Fundamentals (92% match)
    - Gut Health Research (87% match)
└─→ Actions:
    [Link to project] [Create new project] [Just archive]

User: "Link to Health Fundamentals"

System:
✓ Updates Source.ProjectId = health-fundamentals-id
✓ Chunks now queryable within project context
✓ File moves to 02-Normalized/health-fundamentals/
✓ Notification marked as read
```

**Result**: PDF dropped in 2 seconds, automatically processed overnight, linked to project in 5 seconds. No manual extraction or chunking.

---

## Scenario 4: Project-Specific Capture

### The Situation

User is actively working on "Marriage Research" project, reading 3 books simultaneously.

### The Flow

**Step 1: Set Up Project Context**
```
User creates project:

System:
✓ Creates projects/marriage-research/
✓ Structure:
    ├── inbox/          ← Drop project-specific items here
    ├── sources/        ← Linked books/PDFs
    ├── notes/          ← Synthesized insights
    ├── chat/           ← AI research conversations
    └── outputs/        ← Draft chapters, outlines

User links 3 sources:
✓ Bible (marriage passages)
✓ "Seven Principles for Making Marriage Work"
✓ "Hold Me Tight" (attachment theory)
```

**Step 2: Capture During Research**
```
While reading Book 1, user has insight:

User drops note into projects/marriage-research/inbox/:
"Gottman's 'Four Horsemen' aligns with Biblical wisdom about anger"

System:
✓ Creates InboxItem linked to marriage-research project
✓ Auto-tags with project context
✓ Shows in project-specific Insights Panel
```

**Step 3: Multi-Source Council Workflow**
```
User asks AI: "What do all 3 sources say about conflict resolution?"

System (CouncilWorkflow):
✓ Queries chunks from all 3 sources
✓ Identifies common themes
✓ Generates comparative summary:
  
  Bible: Emphasizes forgiveness, patience, selflessness
  Gottman: Focus on repair attempts, conflict management
  Johnson: Attachment security, emotional accessibility
  
  Common theme: SAFETY in conflict is crucial
  
✓ Saves as projects/marriage-research/notes/conflict-resolution-insights.md
✓ Creates TrendAnalysis record in database
✓ Links to all 3 source chunks for traceability
```

**Step 4: Generate Output**
```
After 3 months of research, user has:
├─→ 47 notes synthesized from 3 sources
├─→ 12 chat conversations exploring themes
├─→ 8 personal journal entries about own marriage
└─→ Database full of cross-referenced insights

User: "Generate book outline from this research"

System:
✓ Analyzes all notes, trends, journal entries
✓ Generates structured outline
✓ Suggests chapters based on themes
✓ Links each chapter to relevant sources
✓ Saves as projects/marriage-research/outputs/book-outline.md

User now has:
├─→ Personal: Improved own marriage through research
└─→ Product: Book outline ready for drafting

DUAL-PURPOSE OUTPUT achieved.
```

**Result**: Project-specific capture keeps research organized, multi-source council finds cross-book insights, outputs generated from synthesized knowledge. Personal growth + marketable product from same work.

---

## Scenario 5: Inbox-Zero Daily Review

### The Situation

User checks Insights Panel every morning to reach inbox-zero.

### The Flow

**Morning Routine (10-15 minutes)**
```
User opens Insights Panel:

Unprocessed items (5):
├─→ [File] "Ethiopian Bible Translation.pdf" (processed overnight)
├─→ [Thought] "Enzyme cofactors need minerals to function"
├─→ [Picture] Screenshot of interesting recipe
├─→ [Emotion] Voice note: "Feeling overwhelmed today"
└─→ [Misc] Web clip about gut microbiome

User processes one by one:
```

**Item 1: File**
```
Action: Link to "Bible Constitution" project
Result: ✓ Processed (10 seconds)
```

**Item 2: Thought**
```
Action: Add to "Health Fundamentals" project notes
Result: ✓ Processed (5 seconds)
```

**Item 3: Picture**
```
Action: Save to projects/health-fundamentals/attachments/recipes/
Result: ✓ Processed (8 seconds)
```

**Item 4: Emotion**
```
Action: "Start journaling"
System: Auto-generates journals/overwhelm-2026-05/
User: Types for 5 minutes, marks as "in progress"
Result: ✓ Captured (journal can continue later)
```

**Item 5: Web clip**
```
Action: "Already have this in Health Fundamentals project"
System: "Delete this duplicate?"
User: Yes
Result: ✓ Archived (2 seconds)
```

**Total time**: 12 minutes  
**Result**: Inbox-zero achieved ✅  
**Everything** captured has a home or is in progress.

---

## Key Workflow Principles

✅ **Capture in <5 seconds** - No friction, no categorization  
✅ **Auto-triage saves time** - System suggests, user decides  
✅ **Inbox-zero is achievable** - Daily review keeps everything processed  
✅ **Context never lost** - Everything linked, full history preserved  
✅ **Flexible duration** - Items can process for minutes or months  
✅ **Dual-purpose by default** - Personal use → Marketable output naturally  
✅ **Local-first control** - Your data, your pace, your privacy
