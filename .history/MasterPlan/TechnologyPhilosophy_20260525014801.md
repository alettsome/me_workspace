# Technology Philosophy & Architecture Principles

## Purpose

This document defines the technology selection criteria and architectural philosophy for the me_workspace project. It serves as a decision-making framework to ensure consistency, reliability, and maintainability.

---

## Core Philosophy: The Bell Curve Principle

**We build with mature, battle-tested technologies.**

```
Innovation Curve:
  Bleeding Edge → Early Adoption → Maturity → Legacy
                                      ↑
                               We build here
```

### What "Mature" Means:

1. **Battle-Tested**
   - Used in mission-critical environments (military, enterprise, finance)
   - Has survived multiple major versions and iterations
   - Known failure modes and recovery patterns documented
   - Production-proven at scale

2. **Stable Labor Pool**
   - Large community of experts
   - Abundant documentation and learning resources
   - Available for hire if technical support needed
   - Not dependent on a single vendor or small team

3. **Modular & Composable**
   - Can be replaced without rewriting the entire system
   - Clear interfaces and contracts
   - Doesn't lock us into proprietary ecosystems
   - Follows established patterns and standards

4. **Reliability Over Hype**
   - Proven track record > marketing claims
   - Boring is good (stability matters more than novelty)
   - We care about uptime, not trends

### Examples:

| Technology | Status | Reason |
|-----------|--------|---------|
| **SQLite** | ✅ USE | Since 2000, billions of deployments, mission-critical |
| **Lucene.NET** | ✅ USE | Port of Apache Lucene (20+ years), enterprise search |
| **.NET 8 / C#** | ✅ USE | Microsoft-backed, enterprise standard, mature ecosystem |
| **PowerShell** | ✅ USE | Windows standard, enterprise automation tool |
| **Ollama** | ⚠️ LIMITED USE | New (2023), use only where necessary, plan migration path |
| **ChromaDB** | ❌ AVOID | Too new, unproven at scale, bleeding edge |
| **Python** | ❌ MINIMIZE | Memory issues, prefer .NET for robustness |

---

## Architecture Preferences

### 1. **Local NLP First, LLM Last**

**Principle:** Use traditional NLP and search tools for heavy lifting. Use LLMs sparingly.

**Rationale:**
- Local NLP (Lucene, FTS) is fast, proven, and doesn't require constant compute
- LLMs are resource-intensive and slow on consumer hardware
- Most search/retrieval problems don't need generative AI

**Decision Framework:**
```
Can this be solved with:
  1. Database queries? → Use SQL
  2. Text search? → Use Lucene/FTS
  3. Semantic ranking? → Use pre-computed embeddings + cosine similarity
  4. None of the above? → Then consider LLM
```

**Example:**
- ❌ Don't: Use LLM to search 100 books for "leaky gut"
- ✅ Do: Use Lucene to find candidates, embeddings to rank, show results
- ✅ Optional: Let user request LLM synthesis of top 10 results

### 2. **.NET First, Everything Else Last**

**Preference Order:**
1. **C# / .NET** (first choice)
2. **PowerShell** (for scripting/automation)
3. **Python** (only for rapid prototyping, must migrate to .NET)
4. **Other languages** (avoid unless no .NET alternative exists)

**Why .NET:**
- Type-safe, catches errors at compile time
- Mature memory management (no leaks like Python)
- Excellent tooling (Visual Studio, Rider)
- Enterprise support and long-term stability
- Large talent pool

**Python Exception:**
- Acceptable for quick prototypes that will be rewritten
- Must document migration path to .NET in code comments
- Use only when .NET alternative doesn't exist (rare)

### 3. **Local-First Architecture**

**Principle:** Everything runs locally by default. Cloud is optional enhancement.

**Requirements:**
- ✅ SQLite (local database)
- ✅ Local file system (no cloud storage dependencies)
- ✅ Offline-capable (no internet required for core features)
- ✅ Single-user optimized (no multi-tenancy complexity)

**Cloud Usage (Acceptable):**
- One-time heavy processing (e.g., summarizing 100 books via API)
- Optional sync/backup (user controls when/if)
- Never required for daily use

### 4. **The Librarian Approach**

**Principle:** The system organizes and retrieves YOUR research. It doesn't generate knowledge.

**What This Means:**
- System is a curator/indexer, not a knowledge creator
- Every answer must cite source + page number
- No hallucinations - only returns what's in your sources
- LLM role limited to:
  1. Summarizing sources (one-time)
  2. Extracting key claims with citations
  3. Optional synthesis of retrieved sources

**Example:**
```
User: "What causes leaky gut?"

❌ Wrong: AI generates answer from its training
✅ Right: System searches YOUR 100 books, returns:
  - Book A (p. 45): "Dysbiosis and gut permeability"
  - Book C (p. 127): "Food sensitivities and zonulin"
  - Book G (p. 89): "NSAIDs damage tight junctions"
```

---

## Technology Stack (Approved)

### Core Infrastructure
- **Language:** C# / .NET 8
- **Database:** SQLite (with FTS5 for full-text search)
- **Scripting:** PowerShell 5.1+
- **Web Framework:** ASP.NET Core

### Search & Retrieval
- **Text Search:** Lucene.NET or SQLite FTS5
- **Semantic Search:** ONNX Runtime + pre-computed embeddings
- **Vector Operations:** Pure C# (cosine similarity, etc.)

### AI/ML (Limited Use)
- **Embeddings:** ONNX Runtime with local models (all-MiniLM-L6-v2)
- **Summarization:** Cloud API (one-time) or local Ollama (when necessary)
- **Generation:** Avoid unless user explicitly requests synthesis

### File Processing
- **PDF:** UglyToad.PdfPig (pure .NET)
- **OCR:** Tesseract (via .NET wrapper)
- **Markdown:** Markdig

---

## Governance: Decision-Making Framework

### When Evaluating New Technology or Approach

**Ask These Questions:**

1. **Maturity Check**
   - [ ] Has this been in production for 3+ years?
   - [ ] Is it used by mission-critical systems (military, finance, enterprise)?
   - [ ] Are there at least 3 major versions released?
   - [ ] Is the community large and active?

2. **Alternatives Comparison**
   - [ ] What mature alternatives exist?
   - [ ] Why can't we use the mature option?
   - [ ] What's the migration path if this new tech fails?

3. **Lock-in Risk**
   - [ ] Can we extract our data if we need to switch?
   - [ ] Is there vendor lock-in?
   - [ ] Are interfaces standard/replaceable?

4. **Support & Expertise**
   - [ ] Can we hire people who know this?
   - [ ] Is documentation comprehensive?
   - [ ] Is the skill transferable?

5. **Performance & Reliability**
   - [ ] Is it proven at our scale?
   - [ ] What are the known failure modes?
   - [ ] Do we have recovery procedures?

### Decision Template

```markdown
## Technology Decision: [Name]

**Problem:** [What are we trying to solve?]

**Options Considered:**
1. **[Mature Option]** - [Why/why not]
2. **[New Option]** - [Why/why not]
3. **[Alternative]** - [Why/why not]

**Decision:** [Chosen option]

**Rationale:**
- Maturity: [Assessment]
- Reliability: [Assessment]
- Support: [Assessment]
- Risk: [Assessment]

**Migration Path:** [If we need to change later]

**Documented:** [Date]
```

---

## Persistent Memory Strategy

### User-Level Memory
- **Location:** `/memories/` (user preferences, patterns)
- **Scope:** Across all projects and conversations
- **Content:** Technology preferences, decision patterns, lessons learned
- **Format:** Markdown files

### Repository Memory
- **Location:** `/memories/repo/` (stored in workspace)
- **Scope:** This project only
- **Content:** Project-specific context, architecture decisions, current work
- **Format:** Markdown files

### Session Memory
- **Location:** `/memories/session/` (temporary)
- **Scope:** Current conversation only
- **Content:** Task-specific notes, in-progress work
- **Cleared:** After conversation ends

---

## Skills System (Explanation)

**What Are Skills?**
Skills are domain-specific knowledge modules that provide detailed instructions for complex tasks.

**Example Skills Available:**
- `project-setup-info-local` - Full project scaffolding
- `agent-customization` - Creating custom agents and instructions
- `get-search-view-results` - Accessing VS Code search results

**How They Work:**
When a task matches a skill's domain, the AI loads detailed instructions from the skill file to ensure best practices are followed.

**Creating Custom Skills:**
You can create workspace-specific skills in `.instructions.md` files to encode your project-specific knowledge and processes.

---

## Innovation Within Boundaries

**We're not anti-innovation. We're pro-reliability.**

### When Innovation is Acceptable:

1. **Proof-of-Concept Phase**
   - Try new tech in isolated prototype
   - Must have migration path to mature tech
   - Document findings and migration plan

2. **Non-Critical Features**
   - Experimental features that can fail safely
   - Clearly marked as "beta" or "experimental"
   - Core functionality unaffected if removed

3. **Clear Advantages**
   - New tech solves a problem mature tech can't
   - Benefits outweigh stability risks
   - Community adoption is accelerating (moving toward maturity)

### Innovation Process:

```
1. Research Phase
   - Document mature alternatives
   - Identify gaps the new tech fills
   - Assess community trajectory

2. Prototype Phase
   - Build isolated proof-of-concept
   - Measure performance, stability
   - Compare to mature baseline

3. Decision Gate
   - Review with governance framework
   - Document decision and rationale
   - Plan migration path (if adopted) or rollback (if rejected)

4. Production (if approved)
   - Monitor closely
   - Maintain mature fallback
   - Re-evaluate quarterly
```

---

## Summary: The Guiding Principles

1. **Mature over trendy** - Bell curve positioning
2. **Local over cloud** - Offline-first architecture
3. **.NET over everything** - Type-safe, robust, enterprise-grade
4. **Retrieval over generation** - Librarian approach
5. **Search over LLM** - Local NLP does heavy lifting
6. **Proven over promised** - Battle-tested wins
7. **Boring over exciting** - Stability matters
8. **Modular over monolithic** - Can replace components
9. **Documented over assumed** - Write it down
10. **Compared over accepted** - Evaluate alternatives

---

## Document History

- **2026-05-25:** Initial creation
- Captures core philosophy after NLP vs LLM discussion
- Establishes decision framework for future technology choices

---

## See Also

- [ArchitecturePrinciples.md](ArchitecturePrinciples.md) - Folder-as-Agent, Unix philosophy
- [AcceptanceCriteria.md](AcceptanceCriteria.md) - What "done" looks like
- [RetentionPolicy.md](RetentionPolicy.md) - Data lifecycle management
- [EthicalSourceHandling.md](EthicalSourceHandling.md) - Source rights and ethics
