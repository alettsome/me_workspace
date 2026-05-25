# Master Plan

This folder is the planning home for the project.

It exists to keep the strategy clean, reduce drift between notes, and make the next steps easy to follow.

## Document Map

- `Plan.md`
  Clean strategy document.
  Use this when deciding direction, scope, architecture, and MVP priorities.

- `Phases.md`
  Checkbox-based implementation tracker.
  Use this as the working checklist while building.

- `TechnologyPhilosophy.md`
  Technology selection criteria and decision framework.
  Use this when evaluating new technologies or architectural approaches.
  Captures the "Bell Curve Principle" and mature-tech-first philosophy.

- `UserWorkflow.md`
  Envisioned user workflow and interaction patterns.
  Use this when designing UI and agent behaviors.
  Describes the complete system vision: VS Code-style interface, agent types, project-based workflow.

- `ArchitecturePrinciples.md`
  Core architectural patterns (Folder-as-Agent, Unix philosophy).
  Use this for structural decisions and system design.

- `DataModel.md`
  Core records and relationships.
  Use this before or during any work that touches intake, journals, tasks, or summaries.

- `RetentionPolicy.md`
  Handling rules for source text, extracted text, summaries, logs, and metadata.
  Use this to keep the workflow aligned with the project's ethical boundaries.

- `EthicalSourceHandling.md`
  Practical rules for working with books and other sources in an ethical way.
  Use this when deciding what may be summarized, what may be retained, and how batch processing should behave.

- `AcceptanceCriteria.md`
  Success checks for the highest-risk MVP phases.
  Use this to avoid vague "it feels done" decisions.

- `RuntimeConstraints.md`
  Local runtime assumptions and guardrails for the MVP.
  Use this when choosing models, queue behavior, and file-size limits.

## Planning Rules

- The main product shell is the existing `.NET` `me_workspace` app.
- The project stays local-first and single-user first.
- The workflow should be visible in folders and durable in `SQLite`.
- The app should summarize and organize information without storing unnecessary source copies.
- Open Web UI is not part of the primary implementation plan.

## What Changed

The earlier planning was useful, but too mixed together:

- exploratory thoughts
- architecture ideas
- implementation notes
- phase tracking

This folder separates those concerns so the plan is easier to act on.

## How To Use This Folder

To avoid drift:

1. Use `Phases.md` as the single active tracker.
2. Use `Plan.md` for direction and priority, not as a checklist.
3. Use the supporting docs when implementing cross-cutting areas such as retention, data flow, and acceptance rules.
4. Treat the older external phase notes as reference only unless they are intentionally merged back in here.
