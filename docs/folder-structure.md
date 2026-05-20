# Folder Structure

## Root

- `src/` contains the app source.
- `tests/` contains lightweight verification scripts for the local API flow.
- `artifacts/` contains generated verification output such as isolated smoke-test builds.
- `App_Data/` contains the local SQLite database file.
- `docs/` contains planning, architecture, and phase notes.
- `Journals/` contains folder-backed journal entries, summaries, and linked chat logs.
- `ThingsToDo/` is the placeholder root for next-action files that the loop can create later.

## Web App

- `Features/Chat/` is the current Phase 1 feature area.
- `Features/Memory/` currently provides pinned memory items to the local flow.
- `Features/Journal/` currently provides journal focus to the local flow.
- `Features/Journal/` now backs journal entries with real folders under `Journals/`.
- `Features/Context/` is where context assembly logic lives.
- `Features/Files/` provides the approved folder tree and read-only file preview flow.
- `Features/Voice/` currently exposes the local draft transcript path.
- `Features/System/` reports which major parts of the local flow are connected.

## Support Areas

- `Data/` contains the database context and entities.
- `Infrastructure/Llm/` contains local model integration points and the current reply adapter.
- `Infrastructure/Speech/` contains local speech integration points and the current demo transcript adapter.
- `Infrastructure/Security/` contains local encryption and protection helpers.
- `Infrastructure/Time/` contains time abstractions if needed later.

## Frontend

- `wwwroot/index.html` is the chat shell.
- `wwwroot/css/` contains app styling.
- `wwwroot/js/` contains browser logic.

## Tests

- `tests/ApiFlowSmoke.ps1` launches the app on a test URL and checks the core API flow.
- `tests/README.md` explains how to run the smoke test.

## Journal Entry Shape

- `Journals/index.json` holds a lightweight summary of known journal entries.
- `Journals/<entry-slug>/meta.json` stores the journal id, title, slug, tags, and timestamps.
- `Journals/<entry-slug>/summary.md` stores the short summary used for quick context.
- `Journals/<entry-slug>/entry.md` stores the main journal note with simple anchors.
- `Journals/<entry-slug>/logs/` stores linked chat logs such as `chat-<conversation-id>.md`.
- `Journals/<entry-slug>/assets/` is reserved for entry-specific files.
- `Journals/<entry-slug>/resources/` is reserved for supporting material.
