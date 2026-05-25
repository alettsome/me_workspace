# User Workflow & Interaction Model

## Purpose

This document captures the envisioned user workflow and interaction patterns for the me_workspace system. It's an early-stage working document to organize thoughts about how users will interact with the system.

**Status:** 🚧 Work in Progress - Early conceptual phase

---

## Core Workflow: Problem → Material → Decision

### The Fundamental Pattern

```
User Problem/Question
    ↓
[1] INTAKE PHASE: Retrieve relevant material
    ↓
[2] REVIEW PHASE: Present sources for evaluation
    ↓
[3] TEST PHASE: User validates/refines understanding
    ↓
[4] DECISION PHASE: User makes informed choice
```

**Key Principle:** The system cannot advise until it has **your material** first.

---

## Phase 1: Intake - Retrieving Summarization Information

### Two Types of Intake:

#### A. Initial Intake (One-Time Batch Processing)
**Purpose:** Add new books to the system

**Workflow:**
```
1. User adds book to intake folder
2. System extracts text (PDF/OCR)
3. System summarizes content (LLM)
4. System generates embeddings
5. Book is now searchable
```

**When:** Adding new sources to the library

#### B. Query-Time Retrieval (Daily Use)
**Purpose:** Find relevant material for current question

**Workflow:**
```
1. User asks question or defines problem
2. System searches existing summaries (Lucene + embeddings)
3. System ranks results by relevance
4. System presents top N sources
```

**When:** Using existing library to answer questions

---

## Phase 2: Review & Testing

### Initial Feedback Loop

**Before making decisions, user needs to:**

1. **Review material**
   - See which books address the topic
   - Read relevant summaries with page citations
   - Assess quality and relevance

2. **Test the idea**
   - Compare different sources
   - Identify agreements/contradictions
   - Find gaps in current knowledge

3. **Refine understanding**
   - Narrow or broaden search
   - Add filters (date, author, source type)
   - Request deeper analysis

---

## Interface Models (TBD)

### Option A: Console/Terminal Interface (Unix-Inspired)

**Characteristics:**
- Text-based commands
- Pipe-able output
- Composable operations
- Scriptable workflows

**Example Commands:**
```bash
# Search for topic
> search "leaky gut"

# Show specific result
> show result 3

# Compare multiple sources
> compare 1 3 7

# Export to journal
> export results > journal/current-focus/sources.md

# Pipe operations
> search "inflammation" | filter author:"Gundry" | synthesize
```

**Pros:**
- Fast for power users
- Scriptable/automatable
- Fits Unix philosophy
- Keyboard-driven

**Cons:**
- Steeper learning curve
- May need GUI for some visualizations

### Option B: Web-Based Chat Interface

**Characteristics:**
- Natural language queries
- Visual presentation of results
- Click-to-expand summaries
- Drag-and-drop file upload

**Already partially implemented** in current me_workspace.Web

### Option C: Hybrid Approach

**Best of both:**
- Web UI for initial exploration and visualization
- Console commands for power users and automation
- Both interfaces access same backend

---

## The "Personalities" Concept

### Working Hypothesis: Different Agents for Different Tasks

**Potential Agent Types:**

#### 1. **Researcher Agent**
- **Role:** Find and organize sources
- **Input:** User query or problem statement
- **Output:** Ranked list of relevant sources with summaries
- **Personality:** Thorough, methodical, citation-focused

#### 2. **Analyst Agent**
- **Role:** Compare and contrast findings
- **Input:** Multiple sources selected by user
- **Output:** Comparison table, agreements/contradictions
- **Personality:** Critical, balanced, highlights tensions

#### 3. **Synthesizer Agent**
- **Role:** Create unified understanding across sources
- **Input:** Selected sources + user question
- **Output:** Integrated answer with citations
- **Personality:** Integrative, clear, pedagogical

#### 4. **Gap-Finder Agent**
- **Role:** Identify what's missing from current knowledge
- **Input:** Current library + user's research goals
- **Output:** Suggestions for additional sources needed
- **Personality:** Strategic, forward-looking

### Questions to Answer:

- **Are these separate "modes" the user switches between?**
- **Or different commands/functions within one interface?**
- **Do they have distinct voices/approaches?**
- **How does the user invoke them?**

---

## Example User Journey (Draft)

### Scenario: Researching "What causes leaky gut?"

**Step 1: Initial Search**
```
User: "search leaky gut causes"

System (Researcher mode):
  Searching 127 books...
  Found 12 relevant sources:
  
  1. [Book A] "The Gut Health Protocol" p.45-67
     Summary: Discusses dysbiosis, zonulin, and tight junction breakdown
     Relevance: 92%
     
  2. [Book C] "The Autoimmune Solution" p.127-145
     Summary: NSAIDs, gluten, and intestinal permeability
     Relevance: 88%
  
  ... (10 more)
```

**Step 2: Review & Test**
```
User: "show 1 2 3"

System: [Displays detailed summaries with page citations]

User: "compare 1 2"

System (Analyst mode):
  Comparing sources 1 and 2:
  
  Agreements:
  - Both cite dysbiosis as primary cause
  - Both mention zonulin's role
  
  Differences:
  - Book A emphasizes diet (80% focus)
  - Book C emphasizes medication damage (60% focus)
  
  Unique in Book A: Specific protocol steps
  Unique in Book C: Autoimmune connection
```

**Step 3: Synthesis (Optional)**
```
User: "synthesize 1-5"

System (Synthesizer mode):
  Based on 5 sources from your library:
  
  Primary causes of increased intestinal permeability:
  
  1. Dysbiosis (mentioned in 5/5 sources)
     - Book A, p.45: "Imbalance of gut bacteria..."
     - Book C, p.130: "Pathogenic overgrowth leads to..."
  
  2. Dietary factors (4/5 sources)
     - ...
```

**Step 4: Identify Gaps**
```
User: "what am I missing?"

System (Gap-Finder mode):
  Your library covers:
  ✓ Diet and dysbiosis (strong coverage, 12 sources)
  ✓ Medication effects (moderate, 4 sources)
  
  Potential gaps:
  ⚠ Stress and cortisol's effect (only 1 source mentions)
  ⚠ Heavy metal exposure (not covered)
  ⚠ Recent research (most sources 2018-2020, missing 2022+ data)
  
  Suggested additions:
  - Sources on stress-gut axis
  - Toxicology perspective
```

---

## Decision Points (Need Input)

### 1. Interface Priority
- [ ] Start with console/terminal commands?
- [ ] Enhance existing web chat interface?
- [ ] Build both in parallel?

### 2. Agent Implementation
- [ ] Distinct "personalities" with different prompts?
- [ ] Single system with different functions?
- [ ] Folder-based agents (different folders = different modes)?

### 3. Workflow Emphasis
- [ ] Focus on retrieval (finding sources)?
- [ ] Focus on synthesis (creating answers)?
- [ ] Focus on comparison (analyzing differences)?
- [ ] All three equally?

### 4. User Control Level
- [ ] High automation (system suggests next steps)?
- [ ] Manual control (user explicitly requests each action)?
- [ ] Hybrid with "autopilot" mode?

---

## Next Steps (To Be Determined)

1. **Clarify the intake workflow**
   - Define the exact steps for batch processing books
   - Define the exact steps for query-time retrieval

2. **Design the console interface** (if chosen)
   - List of commands
   - Input/output formats
   - Scripting capabilities

3. **Prototype the agent system**
   - Test different "personalities"
   - Measure if distinct voices add value
   - Decide on implementation approach

4. **Validate with first use case**
   - Pick one research question
   - Walk through entire workflow
   - Identify friction points

---

## Related Documents

- [TechnologyPhilosophy.md](TechnologyPhilosophy.md) - Principles guiding tech choices
- [ArchitecturePrinciples.md](ArchitecturePrinciples.md) - Folder-as-Agent pattern
- [Phases.md](Phases.md) - Implementation roadmap
- [LibraryResearchWorkflow.md](LibraryResearchWorkflow.md) - Time-limited source handling

---

## Document Status

**Created:** 2026-05-25
**Status:** Early conceptual phase - capturing initial thoughts
**Next Review:** After Phase 6 (database schema) is complete

This document will evolve as we prototype and test different interaction patterns.
