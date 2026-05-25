# Architecture Principles

## Purpose

This document captures the foundational architectural decisions and design philosophy that guide the `me_workspace` system.

These principles emerged from:
- Security concerns about "black box" AI systems
- Recognition of mature, stable technologies over trendy but fragile ones
- The Unix philosophy: simplicity, composability, text as universal interface
- The need for offline-first, local-first, user-controlled systems

## Core Philosophy: "Slow and Steady"

**Principle:** Reliability over speed. Safety over features. Clarity over complexity.

**Biblical foundation:** *"Desire without knowledge is not good, and whoever makes haste with his feet misses his way."* (Proverbs 19:2)

**Engineering translation:**
- Build deterministic safety layers, not probabilistic trust
- Test on 1, then 5, then 10, then batch
- Require human approval for destructive operations
- Make errors visible and recoverable

**Why this matters:** The AI industry's "move fast" culture is causing production failures. We choose the opposite path: methodical validation, immutable backups, and human-in-the-loop controls.

---

## The Unix Philosophy Applied to AI

### "Everything is a File"

In the 1970s, Unix established that the file system is not just storage—it's the **interface**. Devices, processes, and data all expose themselves as files in a directory tree.

**Our adaptation: "The Folder IS the Agent"**

Instead of building complex Python frameworks with fragile dependencies, we recognize:

1. **A folder structure defines behavior**
   - `/Health_Fundamentals/Research/` tells the AI: "This is research context"
   - `/Business_Plan/Market_Analysis/` tells the AI: "This is business context"
   - The AI doesn't need custom code—it reads the structure

2. **Markdown files are instructions**
   - `01_Research_Protocol.md` = the "code" the agent follows
   - `README.md` = the agent's job description
   - Plain text = auditable, versionable, diffable

3. **One agent, infinite contexts**
   - Open VS Code in `/Health_Fundamentals/` → the AI becomes a research assistant
   - Open VS Code in `/Business_Plan/` → the AI becomes a business analyst
   - Same AI, different context, zero custom code

### Why This Architecture Wins

**Stability:**
- Folders don't crash
- Text files don't have dependencies
- No Python library version conflicts
- No "works on my machine" problems

**Security:**
- Everything is auditable (text files, not compiled code)
- No hidden execution
- Easy to verify: just read the files
- Version control works perfectly (Git tracks text)

**Composability:**
- Combine folders like Unix pipes
- Reuse structures across projects
- Templates are just folder copies

**Longevity:**
- Text files from 1970s still work today
- Python scripts from 2020 often don't work in 2026
- We're building for decades, not sprints

---

## Technology Stack Decisions

### .NET/C# + PowerShell over Python

**Decision:** Primary implementation in C#/.NET with PowerShell for orchestration scripts.

**Rationale:**

1. **Maturity:** .NET is enterprise-grade, used by Fortune 500 companies and militaries
2. **Stability:** Type-safe, compiled, catches errors at build time
3. **Security:** Strong sandboxing, no dynamic `eval()` traps
4. **Performance:** Faster than interpreted Python for production workloads
5. **Tooling:** Visual Studio/VS Code integration is excellent
6. **Cross-platform:** .NET 8+ runs on Windows, Linux, macOS

**Python's weaknesses (for our use case):**
- Dependency hell (virtualenvs, pip conflicts, version matrix)
- Dynamic typing causes runtime errors in production
- Slower execution for large-scale processing
- Security concerns: `pickle` exploits, malicious PyPI packages
- Fragile: code breaks when libraries update

**Where Python is acceptable:**
- Quick one-off research scripts
- Interfacing with ML libraries that have no C# equivalent
- Personal experimentation (not production)

### SQLite over Cloud Databases

**Decision:** SQLite as the primary data store.

**Rationale:**
- Local-first (no network dependency)
- Single file = easy backup
- ACID transactions
- No authentication complexity
- Runs on any platform
- Used by billions of devices (browsers, phones, embedded systems)

**When to upgrade:** Only if we need multi-user concurrent writes or >100GB data. SQLite handles 1TB+ fine for single-user workloads.

### Local LLMs (Ollama) over Cloud APIs

**Decision:** Ollama for local LLM hosting, interfaced via HTTP API.

**Rationale:**
- **Offline-first:** No internet dependency, no API rate limits
- **Privacy:** Data never leaves the machine
- **Cost:** Zero per-token costs
- **Control:** Choose any open-source model (Llama, Mistral, Phi, Qwen)
- **No gatekeeping:** Model won't refuse requests based on corporate policy

**Tradeoff:** Requires decent hardware (GPU preferred, but CPU works). Slower than GPT-4, but acceptable for batch processing.

**Security:** Ollama runs in its own process, no file system write access granted to models.

---

## Security Architecture

### Principle: "Defense in Depth"

Security is not a feature you add. It's the foundation you build on.

### Layer 1: Immutable Backups

**Rule:** Backups must use different credentials than the primary system.

**Implementation:**
- Automated daily backups to separate directory
- Read-only mount for backup target
- Version history (keep last 30 days)
- Test restore procedure monthly

**Why:** If an AI agent (or attacker) deletes the database, the backup survives.

### Layer 2: Deterministic Guardrails

**Rule:** Dangerous operations are blocked by code, not by "asking the AI nicely."

**Implementation:**
- C# validation layer checks every file operation
- Deny list: `delete`, `drop`, `truncate`, `format`, etc.
- Whitelist: only specific directories are writable
- Human approval required for:
  - Deleting >1 file
  - Modifying database schema
  - Running batch operations

**Why:** LLMs are probabilistic. They might ignore instructions. Code is deterministic.

### Layer 3: Human-in-the-Loop

**Rule:** AI suggests, humans approve, system executes.

**Implementation:**
- Dry-run mode: AI generates plan, user reviews, then executes
- Approval dashboard shows pending operations
- Auto-pause after 3 consecutive failures
- Rollback available for last 10 operations

**Why:** Even good AI makes mistakes. Humans catch edge cases.

### Layer 4: Sandboxing

**Rule:** Never run untrusted code or extracted content directly.

**Implementation:**
- OCR output goes to `/temp/` with no execute permissions
- LLM responses are parsed as text, never `eval()`'d
- Downloaded models scanned for malicious patterns before loading

**Why:** Malicious PDFs, compromised models, and supply chain attacks are real threats.

### Layer 5: Audit Logging

**Rule:** Every operation is logged before execution.

**Implementation:**
- SQLite table: `AuditLog` with timestamp, operation, user, result
- Logs are append-only (cannot be deleted)
- Daily review of failed operations
- Alerting on unusual patterns (e.g., 100 delete requests in 1 minute)

**Why:** When something goes wrong, you need to know what happened.

---

## Folder-as-Agent Implementation Pattern

### Template Structure

Every project follows this pattern:

```
/ProjectName/
  README.md                    # What this project is
  00_Context.md                # Background and constraints
  01_Protocol.md               # Step-by-step instructions
  Research/                    # Source materials
    sources.json               # Metadata about sources
    summaries/                 # AI-generated summaries
  Drafts/                      # Work in progress
    v1_draft.md
    v2_draft.md
  Output/                      # Final deliverables
  Logs/                        # Audit trail
    chat_logs/
    operation_logs/
```

### How It Works

1. **User opens VS Code in `/ProjectName/`**
2. **AI reads `README.md` and `01_Protocol.md`** to understand context
3. **User asks: "Summarize the research"**
4. **AI:**
   - Reads `Research/sources.json` to see what exists
   - Opens relevant files in `Research/summaries/`
   - Generates response based on those files
5. **Result saved to `Logs/chat_logs/YYYY-MM-DD.md`**

### Why This Works

- **No custom code needed** — just folder structure + markdown
- **Reusable** — copy folder structure to new project
- **Auditable** — every step is a text file you can inspect
- **Versionable** — Git tracks everything
- **AI-agnostic** — works with any AI that can read files (Claude, GPT, local LLMs)

---

## Primary Use Case: Health Fundamentals Book

### Why This Drives Architecture

The Health Fundamentals book research pipeline validates our entire architecture:

- **100+ books** to process → tests batch processing
- **Library loans expire** → tests time-sensitive workflows
- **OCR from screen captures** → tests security (untrusted input)
- **Citation tracking** → tests metadata management
- **Summary generation** → tests local LLM integration
- **Retention policy** → tests transient vs durable data handling

If this works, everything else is easier.

### Success Criteria

- Process 100 books in 2 weeks
- Zero data loss from expired access windows
- All summaries cite specific pages
- OCR text is deleted after summarization
- Pipeline runs offline (no cloud dependencies)
- Human validates quality at each checkpoint

---

## Anti-Patterns to Avoid

### ❌ "Black Box Authority"

**Problem:** Trusting AI output without verification.

**Solution:** Always validate AI-generated operations before execution.

### ❌ "Move Fast and Break Things"

**Problem:** Shipping untested features to beat competitors.

**Solution:** "Slow and steady" — test on small batches first.

### ❌ "Python for Everything"

**Problem:** Using Python because it's popular, despite fragility.

**Solution:** Use mature, stable languages (.NET/C#) for production systems.

### ❌ "Cloud-First"

**Problem:** Depending on external services that can fail, charge, or gatekeep.

**Solution:** Local-first with optional sync, not cloud-required.

### ❌ "Complexity as Progress"

**Problem:** Adding frameworks, agents, microservices to look sophisticated.

**Solution:** Simplicity wins. Folders + text files + deterministic code.

---

## Decision Framework

When evaluating a new feature or technology, ask:

1. **Is it stable?** Has it been battle-tested for 5+ years?
2. **Is it simple?** Can it be explained in one paragraph?
3. **Is it secure?** Can we audit it? Are there known exploits?
4. **Is it local?** Can it run without internet?
5. **Is it mature?** Do Fortune 500 companies trust it?

If 4/5 are "yes," consider it. If <3, reject it.

---

## Evolution and Refinement

These principles are not fixed dogma. They evolve as we learn.

**When to revisit:**
- After a security incident
- After a stability failure
- When a technology proves inadequate
- When a simpler pattern emerges

**What doesn't change:**
- Local-first
- Security as architecture
- Simplicity over complexity
- Human approval for destructive operations

---

## Summary

**Our architecture is:**
- Unix-inspired (folders as agents, text as interface)
- Security-first (immutable backups, deterministic guardrails)
- Mature tech (C#/.NET, SQLite, PowerShell)
- Offline-capable (local LLMs, no cloud dependencies)
- Simple (folders + markdown > complex frameworks)

**Our philosophy is:**
- Slow and steady (reliability over speed)
- Auditable (text files, logs, version control)
- Human-in-the-loop (AI suggests, humans approve)
- Built for decades (not just this sprint)

**Our validation is:**
- Health Fundamentals book (100+ books, library workflows)
- Real-world pressure testing (time limits, security, batch scale)
- Incremental rollout (test small before batch)

This is not just a system. It's a methodology for building trustworthy, durable, local-first AI tools.
