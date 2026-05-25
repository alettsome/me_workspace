# User Vision & Requirements Summary

**Date:** May 25, 2026  
**Purpose:** Quick reference for user's complete system vision

---

## Core Philosophy

**"The Bible is my Constitution"**
- AI's source of truth for moral/spiritual guidance
- King James Version (primary) + Ethiopian Bible (88 books) + Apocrypha
- Every AI response should reference this foundation
- Natural remedies first, medication last resort
- Conversational AI that challenges and questions (not just agreeable)

---

## System Architecture

**Platform Type:** Local-first ASP.NET Core web app (localhost only)
- ✅ Correct approach validated (VS Code pattern)
- ✅ No change needed to web vs desktop architecture
- Database: SQLite (local, offline-capable)
- File system = interface (Folder-as-Agent pattern)
- Web UI = view into file structure

---

## Project Types

### 1. Bible Constitution (Foundation)
- **Purpose**: AI's "Constitution" - source of truth for all projects
- **Contents**: KJV + Ethiopian Bible (88 books) + Apocrypha
- **Status**: Needs to be built NEXT (after ChatGPT export)
- **Why**: Council agents reference this, AI personality rules stored here

### 2. Health_Fundamentals (In Progress)
- **Type**: Book
- **Status**: Partially complete, batch processing working
- **Sources**: 100+ books on whole foods, vitamins, cofactors
- **Focus**: Affordable, accessible, natural health

### 3. Marriage_Biblical_Perspective (Planned)
- **Type**: Book
- **Status**: Concept phase
- **Sources**: Bible + books on monogamy/polygamy historical context
- **Purpose**: Research controversial topics, contrast with Bible

### 4. Global Inbox (Needed)
- **Type**: Idea capture
- **Status**: Not built yet
- **Purpose**: Quick thoughts not tied to specific projects
- **Format**: Simple markdown file with line items

---

## AI Personality / Constitution Rules

**Stored in:** `projects/Bible_Constitution/manifest.json`

### Response Guidelines:
1. **Health Topics**: Natural remedies FIRST, medication only after natural options exhausted
2. **Tone**: Conversational, willing to "talk back" and challenge assumptions
3. **Token Efficiency**: Know user's values upfront, don't waste tokens on irrelevant approaches
4. **Moral Guidance**: Reference Bible Constitution for spiritual/moral questions
5. **Debate Style**: Socratic - ask questions, encourage critical thinking

### User Profile:
- Values natural healing over medication
- Prefers whole foods, affordable/accessible solutions
- Interested in controversial topics (marriage structures, apocrypha)
- Appreciates being challenged intellectually
- Reads less, listens more (TTS workflow important)

---

## Critical Workflows

### 1. Multi-Source Council (HIGH PRIORITY)
**Purpose:** Analyze topic across multiple sources, find common themes/conflicts

**Example - Marriage Research:**
```
Bible (Constitution) ──┐
Marriage Book 1 ───────┼──→ Project: Marriage_Biblical_Perspective
Marriage Book 2 ───────┘
         ↓
   TrendAnalysis: "3 sources analyzed"
   - Common themes: [list]
   - Conflicts: [list]
   - Biblical perspective: [synthesis]
```

### 2. TTS Review Workflow (HIGH PRIORITY)
**Purpose:** Listen to book chapters, provide feedback, iterate

**Flow:**
```
AI generates chapter → Save to file → TTS generates audio → 
User listens (eyes closed) → User provides feedback → 
AI processes feedback → Show diff → User approves → 
Apply patch to master document
```

**Why Important:** User prefers listening over reading, catches things better this way

### 3. Document Assembly with Patch/Diff (VALIDATED)
**Purpose:** Use mature tools (not AI) for precise document updates

**Pattern:** Linux kernel development approach
- AI generates content + coordinates
- GNU patch/git apply does the application
- Works on any size document (10 pages or 500 pages)
- No ambiguity, no guessing

### 4. Native Voice Capture (SECURITY CRITICAL)
**Problem:** Browser extensions can hijack microphone
**Solution:** NAudio or Windows Core Audio APIs (native capture)
**Why:** Security - eliminate browser from audio pipeline entirely

---

## Urgent Priorities

### 🔴 IMMEDIATE (TODAY/THIS WEEK):
1. **Export ChatGPT conversations** (3 primary chats) - may lose access
2. **Build Bible Constitution project** (KJV + Ethiopian Bible)
3. **Set up TTS review workflow**

### 🟡 HIGH (NEXT 2 WEEKS):
4. **Native voice capture** (security fix)
5. **Global inbox** (idea capture)
6. **Marriage book project** (multi-source council test case)

### 🟢 MEDIUM (AFTER HIGH PRIORITIES):
7. **Document assembly workflow** (full patch/diff implementation)
8. **Semantic search** (embeddings for better retrieval)
9. **Automated acquisition** (web scraping, library APIs)

---

## Technology Stack (Validated)

**Backend:**
- .NET 8 / C# (mature, preferred over Python)
- ASP.NET Core (localhost:5078)
- SQLite (local, file-based)
- Entity Framework Core

**Frontend:**
- Web UI (already working)
- VS Code-style interface (sidebar, document pane, chat pane)

**Search:**
- Lucene.NET or SQLite FTS5 (mature text search)
- Pre-computed embeddings (optional, Phase 12)
- ONNX Runtime (local embeddings, no LLM for search)

**Voice:**
- whisper.cpp (offline speech-to-text)
- NAudio (native microphone capture)
- Offline TTS tool (user already has one)

**Tools:**
- PowerShell (scripting, automation)
- Git/GNU Patch (document assembly)

---

## Key Insights from User

1. **"The file is the system"** - Unix philosophy, Folder-as-Agent pattern
2. **"Skills vs Agents"** - Folders = skills (instructions), AI = agent (executor)
3. **"AI for intelligence, not execution"** - Use mature tools (patch, diff) for deterministic operations
4. **"Constitution, not just source"** - Bible isn't just a book, it's the AI's governing document
5. **"I listen more than I read"** - TTS workflow is critical, not optional
6. **"Talk back to me"** - User wants conversational AI that challenges, not just agrees

---

## Questions Still Open

1. **Bible Sources**: Where to get Ethiopian Bible (88 books)? Digital copy available?
2. **TTS Tool**: Which offline TTS does user currently use? (Balabolka, eSpeak, other?)
3. **ChatGPT**: Does user have export feature, or manual copy needed?
4. **Apocrypha**: Which specific books to include? (Enoch, Jubilees, Tobit, etc.)
5. **Council Agents**: Specific personalities needed? (pessimistic, optimistic, skeptical, etc.)

---

## Related Documents

- [ImmediateActionPlan.md](./ImmediateActionPlan.md) - Step-by-step action items
- [Phase6-Database-Schema.md](./Phase6-Database-Schema.md) - Database structure
- [../MasterPlan/Phases.md](../MasterPlan/Phases.md) - Complete phase tracker
- [../MasterPlan/TechnologyPhilosophy.md](../MasterPlan/TechnologyPhilosophy.md) - Tech decision framework
- [../MasterPlan/UserWorkflow.md](../MasterPlan/UserWorkflow.md) - User interaction patterns

---

## Summary

**You have built:**
- ✅ Mature, battle-tested architecture (.NET + SQLite + local-first)
- ✅ Complete database schema (projects, sources, summaries, agents, trends)
- ✅ Immutable backup system
- ✅ Working application (tested and verified)
- ✅ Clear vision and priorities

**Next step:** Execute on immediate priorities (ChatGPT export, Bible Constitution, TTS workflow)

**You're not far off. You've done more than you think.**
