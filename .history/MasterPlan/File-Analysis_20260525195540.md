# MasterPlan File Analysis

**Purpose**: Review each file in MasterPlan/ to confirm it belongs where it is or needs to move.

**Review Criteria**:
- Does it belong where it is?
- Is it obsolete/duplicate?
- Should it move to Projects/, docs/, or elsewhere?
- Is naming consistent?

---

## Files to Review

### ✅ Reviewed Files

#### 1. Phases.md
- **Location**: `C:\me_workspace\MasterPlan\Phases.md`
- **What it is**: Main phase tracker - temporal roadmap showing what's built when
- **Decision**: ✅ **STAY** - Central planning document, belongs at MasterPlan root
- **Status**: CORRECT LOCATION

#### 2. AcceptanceCriteria.md
- **Location**: `C:\me_workspace\MasterPlan\AcceptanceCriteria.md`
- **What it is**: Defines acceptance criteria for high-risk MVP phases (immutable backups, database schema, etc.)
- **Decision**: ✅ **STAY** - Core planning document, referenced by Phases.md
- **Status**: CORRECT LOCATION

#### 3. ArchitecturePrinciples.md
- **Location**: `C:\me_workspace\MasterPlan\ArchitecturePrinciples.md`
- **What it is**: Core architectural philosophy - "Folder-as-Agent", Unix principles, "slow and steady"
- **Decision**: ✅ **STAY** - Foundational design document, referenced by Phases.md
- **Status**: CORRECT LOCATION

#### 4. DataModel.md
- **Location**: `C:\me_workspace\MasterPlan\DataModel.md`
- **What it is**: Canonical database schema definition (Conversation, Message, Source, Summary, Project, etc.)
- **Decision**: ✅ **STAY** - Core data architecture document, prevents schema drift
- **Status**: CORRECT LOCATION

#### 5. EthicalSourceHandling.md
- **Location**: `C:\me_workspace\MasterPlan\EthicalSourceHandling.md`
- **What it is**: Ethical guidelines for source handling (public domain, library research, copyright, summary-only rules)
- **Decision**: ✅ **STAY** - Core policy document, governs intake and retention behavior
- **Status**: CORRECT LOCATION

#### 6. Features/
- **Location**: `C:\me_workspace\MasterPlan\Features\` (contains README.md only)
- **What it is**: Redirect folder - points to `Projects/` for feature documentation
- **Decision**: ✅ **STAY** - Acts as quick reference/index, prevents broken links
- **Status**: CORRECT LOCATION (already reorganized)

---

### ⏳ Pending Review

#### 7. HealthFundamentals_ImplementationStatus.md
- **Location**: ~~`C:\me_workspace\MasterPlan\HealthFundamentals_ImplementationStatus.md`~~ → **MOVED**
- **New Location**: `C:\me_workspace\projects\Health_Fundamentals\ImplementationStatus.md`
- **What it is**: Maps Health Fundamentals project scripts to platform phase workflow (shows what's already built)
- **Decision**: ✅ **MOVED** (Option A) - Project-specific documentation belongs with project
- **Rationale**: Health_Fundamentals is a real project (validation use case), should have proper project structure
- **Status**: ✅ **COMPLETE**

#### 8. LibraryResearchWorkflow.md
- **Location**: `C:\me_workspace\MasterPlan\LibraryResearchWorkflow.md`
- **What it is**: Workflow for ethically researching 100+ books using temporary library access (Internet Archive, Libby, Google Books previews, screen captures during legal access windows)
- **Decision**: ✅ **STAY** - Platform-level workflow document
- **Rationale**: General workflow that any book project uses (not project-specific), complements EthicalSourceHandling.md policy, referenced from Phases.md Phase 5
- **Status**: ✅ **CORRECT LOCATION**

#### 9. Plan.md
- **Location**: `C:\me_workspace\MasterPlan\Plan.md`
- **What it is**: Master strategy document - explains how to use Phases.md and other planning docs, defines MVP boundaries, architecture direction, prioritization
- **Decision**: ✅ **STAY** - High-level platform strategy document
- **Rationale**: Explains how all planning docs relate to each other, defines "build from existing app" philosophy, referenced by Phases.md
- **Status**: ✅ **CORRECT LOCATION**

#### 10. Projects/
- **Location**: `C:\me_workspace\MasterPlan\Projects\`
- **What it is**: Feature documentation (mirrors runtime structure)
- **Status**: ✅ **ALREADY REORGANIZED** (earlier in session)

#### 11. README.md
- **Location**: `C:\me_workspace\MasterPlan\README.md`
- **What it is**: Document map / table of contents for MasterPlan folder - lists all planning docs and their purposes
- **Decision**: ✅ **STAY** - Navigation document for planning folder
- **Rationale**: Helps users/AI understand what each planning document is for, acts as index to MasterPlan contents
- **Status**: ✅ **CORRECT LOCATION**

#### 12. RetentionPolicy.md
- **Location**: `C:\me_workspace\MasterPlan\RetentionPolicy.md`
- **What it is**: Defines what data to keep vs. delete (durable vs. temporary) - governs all data handling (summaries keep, OCR text delete, etc.)
- **Decision**: ✅ **STAY** - Core platform policy document
- **Rationale**: Platform-level retention rules that all projects follow, referenced by Phases.md and EthicalSourceHandling.md
- **Status**: ✅ **CORRECT LOCATION**

#### 13. RuntimeConstraints.md
- **Location**: `C:\me_workspace\MasterPlan\RuntimeConstraints.md`
- **What it is**: Platform runtime assumptions and limits (local-only, SQLite, bounded queues, file size limits, model constraints, logging rules, safety)
- **Decision**: ✅ **STAY** - Core platform configuration policy
- **Rationale**: Platform-level constraints that all implementations follow (single-user, local-first, safety-first), referenced by Phases.md
- **Status**: ✅ **CORRECT LOCATION**

#### 14. System-Hierarchy.md
- **Location**: `C:\me_workspace\MasterPlan\System-Hierarchy.md`
- **What it is**: Folder structure rules and organization principles - defines how projects are organized, 15-file template structure, naming conventions
- **Decision**: ✅ **STAY** - Core platform architecture document
- **Rationale**: Platform-level folder structure rules that all projects follow, defines the "encapsulation" principle, referenced by multiple planning docs
- **Status**: ✅ **CORRECT LOCATION**

#### 15. TechnologyPhilosophy.md
- **Location**: `C:\me_workspace\MasterPlan\TechnologyPhilosophy.md`
- **What it is**: Technology selection criteria and "Bell Curve Principle" - choose mature, battle-tested tech over bleeding-edge or legacy
- **Decision**: ✅ **STAY** - Core platform technology policy
- **Rationale**: Platform-level technology selection framework (SQLite, Lucene.NET, .NET/C#), guides all architecture decisions, referenced by Plan.md
- **Status**: ✅ **CORRECT LOCATION**

#### 16. UserWorkflow.md
- **Location**: `C:\me_workspace\MasterPlan\UserWorkflow.md`
- **What it is**: Envisioned user workflow and interaction patterns - Problem → Material → Decision, Intake → Review → Test → Decision phases
- **Decision**: ✅ **STAY** - Platform-level UX design document
- **Rationale**: Describes how users interact with entire platform (not project-specific), complements ArchitecturePrinciples.md, referenced by Plan.md
- **Status**: ✅ **CORRECT LOCATION**

---

## Summary

**✅ All 16 files reviewed!**

### Files Moved:
- **1 file moved**: HealthFundamentals_ImplementationStatus.md → `projects/Health_Fundamentals/ImplementationStatus.md`

### Files Staying in MasterPlan/:
- **15 files staying**: All other files are platform-level planning/policy documents that belong in MasterPlan/

### Final Structure:
```
MasterPlan/
├── Phases.md                         ← Active implementation tracker
├── Plan.md                           ← Master strategy
├── README.md                         ← Document map
├── AcceptanceCriteria.md             ← Success criteria for high-risk phases
├── ArchitecturePrinciples.md         ← Folder-as-Agent, Unix philosophy
├── DataModel.md                      ← Database schema
├── EthicalSourceHandling.md          ← Source handling policy
├── LibraryResearchWorkflow.md        ← Library research process
├── RetentionPolicy.md                ← Data retention rules
├── RuntimeConstraints.md             ← Platform runtime limits
├── System-Hierarchy.md               ← Folder structure rules
├── TechnologyPhilosophy.md           ← Tech selection (Bell Curve)
├── UserWorkflow.md                   ← UX interaction patterns
├── Features/                         ← Redirect to Projects/
│   └── README.md
└── Projects/                         ← Feature documentation
    ├── README.md
    ├── 00-Global-Inbox-System/
    ├── Automatic-Processing/
    └── Chunking/
```

**Conclusion**: MasterPlan folder structure is now clean and well-organized. Only platform-level documents remain, project-specific docs moved to appropriate locations.

---

## Review Process

**How to use this file**:
1. User picks one file from "Pending Review" section
2. Agent reads file, analyzes purpose
3. Agent updates analysis with decision
4. Agent moves file if needed (with user approval)
5. Agent updates status to ✅ Reviewed
6. Repeat for next file

**Benefits of this approach**:
- Not overwhelming (one file at a time)
- Clear tracking (what's done, what's pending)
- Documented decisions (why file stayed or moved)
- Easy to resume (pick up where we left off)
