# me_workspace Architecture

## Goal

me_workspace starts as a local chat and journal shell that runs fully offline after setup. The first release keeps the shape simple:

- local web UI
- local ASP.NET Core backend
- local SQLite storage
- no cloud APIs
- no remote model calls

## Runtime Rules

- Bind the app to `127.0.0.1` only.
- Store all persistent state in `App_Data/me_workspace.db`.
- Keep speech-to-text and LLM integration behind local adapters.
- Treat any future online mode as optional and disabled by default.

## Phase 1 Request Flow

1. The user types into the chat box.
2. The frontend posts the message to the local backend.
3. The backend loads the active conversation from SQLite.
4. The backend stores the new user message.
5. The backend stores a placeholder assistant message.
6. The frontend reloads the updated conversation.

Phase 1 does not use advanced memory retrieval yet. Context is the active conversation only.

## Future Context Sources

Phase 2 adds:

- pinned memory
- journal context
- recent conversation summary

Phase 4 adds:

- approved file and folder context

## Workspace Continuity Loop

The target workflow is not just "send a message and get a reply." It is a folder-backed loop that keeps chats, journals, logs, and later tasks moving together.

Desired behavior:

1. A new chat should not stay isolated by default.
2. When a new chat starts, the app should resolve which journal owns that work.
3. If no suitable journal is already selected or linked, the app should be able to create a new journal entry for that thread.
4. Once a journal exists, its folder structure should exist immediately under `Journals/<entry-slug>/`.
5. That folder should include the expected files and support folders such as `entry.md`, `summary.md`, `meta.json`, `logs/`, `assets/`, and `resources/`.
6. As chat continues, linked log files should be written into that journal's `logs/` folder in a predictable way.
7. Later phases can extend the same loop into `ThingsToDo/`, anchored document updates, and approved local actions.

This means the folder structure is not a side effect. It is part of the core workflow contract.

## Folder Intent

- `Features/` holds user-facing functionality.
- `Data/` holds database access and entities.
- `Infrastructure/` holds local integrations like LLM and speech.
- `wwwroot/` holds the local web interface.
- `docs/` holds planning and system notes.
