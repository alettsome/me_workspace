# MVP Sequence

## Purpose
This is the concrete build order for the first useful version of `me_workspaces`.

Read and execute these in order:

1. `mvp-workspace-folder-tree.md`
2. `mvp-nuget-and-components.md`
3. `mvp-data-model.md`
4. `mvp-slice-01-intake-and-chunking.md`

## First Outcome
The first useful version should do this:

1. detect a new source dropped into the inbox
2. register it in SQLite
3. extract usable text
4. split it into chunks
5. save the chunk records
6. move the source into the next visible folder state

## Scope For MVP 1
Included:

- local web app shell
- folder-driven intake
- metadata tracking
- text extraction for `.txt`, `.md`, and `.pdf`
- chunking
- chunk persistence
- basic status tracking

Not included yet:

- polished review UI
- advanced embeddings
- Open WebUI integration
- final output generation
- multi-user support
- complex automation rules

## Sequential Build Order
### Step 1
Set up the workspace folders and config.
Status: `Done`

### Step 2
Add the project packages and service registrations.
Status: `Done`

### Step 3
Create the database tables and status model.
Status: `Done`

### Step 4
Build intake registration.
Status: `Done`

### Step 5
Build text extraction.
Status: `Next`

### Step 6
Build chunking.

### Step 7
Store chunk records and move the file state.

## Done Looks Like
At the end of MVP 1, you can drop a source into `01-Inbox` and the system turns it into tracked chunked material without manual bookkeeping.

## Progress Update
- [x] Step 1 completed
- [x] Step 2 completed
- [x] Step 3 completed
- [x] Step 4 completed
- [ ] Step 5 not started
- [ ] Step 6 not started
- [ ] Step 7 not started
