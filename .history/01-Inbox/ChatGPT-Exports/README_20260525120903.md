# ChatGPT Exports Intake Folder

**Purpose:** Drop your ChatGPT conversation exports here for processing into the database.

## Instructions

1. **Export from ChatGPT:**
   - Settings → Data controls → Export data
   - OR manually copy/paste conversations

2. **Save files here:**
   - ZIP files (will be extracted)
   - Text files (`.txt`)
   - HTML files (`.html`)
   - JSON files (`.json`)

3. **Naming suggestion:**
   - `chat-1-marriage-research.txt`
   - `chat-2-health-fundamentals.txt`
   - `chat-3-bible-constitution.txt`

## After You Drop Files Here

Run the processing pipeline (or let AI know files are ready):
- Files will be registered as Sources (type: "conversation-log")
- Text will be chunked by conversation turns
- Key insights will be summarized
- Everything searchable in database

## Status

- [ ] Chat 1 exported
- [ ] Chat 2 exported
- [ ] Chat 3 exported
- [ ] All processed into database
