# me_workspace Phases

## Phase 1

Build the local chat shell.

- [x] ASP.NET Core web app
- [x] local-only hosting on `127.0.0.1`
- [x] SQLite conversations and messages
- [x] simple browser chat interface
- [x] local context, memory, journal, and voice draft flow connected end to end
- [x] lightweight API smoke test for wiring verification

## Phase 2

Build persistent memory.

- [x] seed pinned memory items into the local flow
- [x] seed journal context into the local flow
- [x] include memory and journal context in assistant replies
- [x] add saved memory create/edit/delete management
- [ ] add saved journal create/edit/delete management
- [ ] improve context assembly beyond the current simple summary

## Phase 3

Build reliable voice input.

- [x] demo voice draft path wired into the composer
- [x] temporary browser dictation fallback wired into the composer for UI testing
- [x] local backend voice session contract added for streaming dictation
- [ ] push-to-talk flow
- [ ] real local speech-to-text worker
- [ ] replace the placeholder local voice session worker with a real offline engine such as `whisper.cpp`
- [ ] long-running dictation that stays active until the user stops it
- [ ] live transcript updates while recording instead of record-first-then-transcribe
- [ ] transcript review before send with a clearer voice workflow

## Phase 4

Build file context.

- [x] approved folders
- [x] show approved folders in a visible folder tree
- [x] expand folders and inspect files from the app
- [x] support drag-and-drop from the folder view into chat context
- [x] reference selected file system content automatically when sending chat
- [x] read-only file ingestion
- [x] include-file-in-chat workflow
- [x] save attached file context alongside the related chat logs
- [ ] define automatic context-gathering rules before chat is sent to the AI tool
- [ ] inspect the folder structure and select relevant context automatically
- [ ] rank likely-relevant files instead of requiring manual file attachment
- [ ] keep manual drag-and-drop as an optional override, not the primary workflow

## Phase 5

Build safe actions.

- [ ] explicit approval before writes
- [ ] action logs
- [ ] rollback-friendly change handling

## Phase 6

Build the working loop around chat.

- [x] create a `ThingsToDo` folder structure for next actions
- [ ] write chat outcomes into task-oriented logs
- [ ] track what should happen next after each loop
- [ ] let the loop continue from saved state instead of starting cold

## Phase 7

Build the long-form journal workflow.

- [x] create a root `Journals/` folder
- [x] create one folder per journal entry
- [x] give each journal entry its own `entry.md`, `summary.md`, and `meta.json`
- [x] create `logs/`, `assets/`, and `resources/` inside each journal entry folder
- [x] allow new chats to link to one selected journal entry
- [x] write linked chat exchanges into that journal entry's `logs/` folder
- [x] create a root `Journals/index.json` summary file
- [x] support journal entries with stable anchors in `entry.md`
- [ ] add journal entry create/edit/delete management beyond basic create
- [ ] let a new chat automatically create or adopt the right journal thread instead of starting as an isolated conversation
- [ ] ensure the full journal folder structure exists as soon as that thread is created
- [ ] treat linked logs as an automatic part of the chat start and continuation flow
- [ ] show recent logs for the selected journal entry in the UI
- [ ] structure anchors so diff-style tools can update sections safely
- [ ] feed anchored journal context into chat when relevant
- [ ] save journal-driven changes back in a predictable format
- [ ] connect journal outcomes into `ThingsToDo/`

## Current Focus

- [x] rename the app to `me_workspace`
- [x] switch the database file to `App_Data/me_workspace.db`
- [x] rename the internal web project to `me_workspace.Web`
- [x] tidy the old `Symphony.Web` leftovers
- [x] build the first folder-based context view
- [x] connect selected files into chat context before send
- [x] create a real folder-based journal structure instead of a stub
- [x] let a chat stay linked to one journal entry
- [x] save linked chat logs into `Journals/<entry>/logs/`
- [x] add a local backend session shape for future offline dictation
- [ ] build the first automatic context selector before AI processing
- [ ] replace the current placeholder dictation worker with a real offline local speech engine
- [ ] define the new-chat rule so a thread automatically adopts or creates its journal and folder structure
- [ ] define how automatic context chooses files, journal summaries, and recent logs together
- [ ] define how chat logs, `ThingsToDo`, and journal anchors work together
- [ ] add a root `README.md` with run and test commands

## Iteration Notes

- [ ] chat should reference the file system for context before the assistant reply is generated
- [ ] the app should gather likely context automatically when chat is engaged
- [ ] manual drag-and-drop should stay available, but only as a fallback or explicit override
- [ ] browser speech recognition should stay only as a temporary fallback, not the long-term offline dictation path
- [ ] the resulting chat and context should be saved into logs
- [ ] new chat should feel like the start of a real working thread, not a blank isolated container
- [ ] if the user starts a new chat without an existing linked journal, the workspace should create or resolve the journal and folders automatically
- [ ] the chat pane should prioritize the thread and composer, while secondary panels such as flow and memory should not crowd the message area
- [ ] next actions should land in a clear folder-based `ThingsToDo` workflow
- [ ] journal entries should be anchor-based so later automated diffs can update them safely

## Journal Shape

- [x] root folder: `Journals/`
- [x] root index: `Journals/index.json`
- [x] per-entry folder: `Journals/<entry-slug>/`
- [x] main entry file: `entry.md`
- [x] metadata file: `meta.json`
- [x] summary file: `summary.md`
- [x] linked chat logs: `logs/chat-<conversation-id>.md`
- [x] optional support folders: `assets/` and `resources/`
- [ ] searchable metadata fields should grow to include stronger tags and keywords
- [ ] selected journal should load summary, entry, and recent logs as chat context

## Next Build Slice

- [x] automatic context ranking before send
- [x] selected journal detail panel with recent logs
- [ ] task capture into `ThingsToDo/`
- [ ] journal edit/delete management
