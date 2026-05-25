# Quick Start: Export ChatGPT Conversations

**URGENT:** Complete this TODAY before losing access

---

## Method 1: ChatGPT Export Feature (Recommended)

1. **Go to ChatGPT Settings**
   - Click your profile icon (bottom left)
   - Click "Settings"
   - Go to "Data controls"

2. **Request Export**
   - Click "Export data"
   - Confirm email
   - Wait for email with download link (usually within minutes to hours)

3. **Download & Save**
   - Click link in email
   - Download ZIP file
   - Save to: `C:\me_workspace\01-Inbox\ChatGPT-Exports\`

4. **Extract**
   ```powershell
   cd C:\me_workspace\01-Inbox\ChatGPT-Exports
   Expand-Archive -Path "*.zip" -DestinationPath ".\extracted"
   ```

---

## Method 2: Manual Copy (If Export Not Available)

### For Each of Your 3 Primary Chats:

1. **Open Chat**
   - Navigate to the conversation in ChatGPT

2. **Select All**
   - Scroll to top
   - Click in chat area
   - Press `Ctrl+A` (select all)

3. **Copy**
   - Press `Ctrl+C`

4. **Paste to File**
   ```powershell
   # Create file
   notepad "C:\me_workspace\01-Inbox\ChatGPT-Exports\chat-1-marriage-research.txt"
   
   # Paste with Ctrl+V
   # Save with Ctrl+S
   ```

5. **Repeat for other 2 chats**
   - `chat-2-health-research.txt`
   - `chat-3-bible-constitution.txt`

---

## Method 3: Browser Extension (If Methods 1 & 2 Fail)

**Chrome Extension:** "ChatGPT History Export"
1. Install from Chrome Web Store
2. Open ChatGPT
3. Click extension icon
4. Export to JSON or Markdown
5. Save files to: `C:\me_workspace\01-Inbox\ChatGPT-Exports\`

---

## After Export: Process into Database

### Step 1: Create Source Entries

Open PowerShell in `c:\me_workspace`:

```powershell
# Navigate to web project
cd src\me_workspace.Web

# Run application
dotnet run --urls "http://127.0.0.1:5078"
```

### Step 2: Use Existing Pipeline

Once exported files are in `01-Inbox\ChatGPT-Exports\`, the existing intake pipeline can process them:

1. **Register Source** (will need API endpoint or manual database insert)
```sql
-- Manually insert source for now (until API built)
INSERT INTO Sources (
    Id, SourceKey, Title, SourceType, RightsLabel, 
    OriginalRelativePath, CurrentStage, Status, 
    CreatedUtc, UpdatedUtc
) VALUES (
    lower(hex(randomblob(16))),
    'chatgpt-marriage-research',
    'ChatGPT: Marriage Research Conversations',
    'conversation-log',
    'user-created',
    '01-Inbox/ChatGPT-Exports/chat-1-marriage-research.txt',
    'Inbox',
    'new',
    datetime('now'),
    datetime('now')
);
```

2. **Repeat for Other Chats**

3. **Process Through Pipeline**
   - Extract text (already plain text)
   - Chunk into conversation turns
   - Summarize key insights
   - Store in Summary table
   - Tag with topics

---

## Verification

After processing, verify in database:

```sql
-- Check sources created
SELECT Title, SourceType, Status FROM Sources 
WHERE SourceType = 'conversation-log';

-- Check summaries generated
SELECT s.Title, COUNT(su.Id) as SummaryCount
FROM Sources s
LEFT JOIN Summaries su ON s.Id = su.SourceId
WHERE s.SourceType = 'conversation-log'
GROUP BY s.Id;
```

---

## Timeline

**Today (May 25):**
- [ ] Export all 3 ChatGPT conversations
- [ ] Save to `01-Inbox\ChatGPT-Exports\`
- [ ] Verify files are readable

**Tomorrow (May 26):**
- [ ] Process into database (may need API endpoint first)
- [ ] Verify summaries generated
- [ ] Test search across conversation insights

---

## Backup

Once exported and verified:

```powershell
# Add to next backup
cd c:\me_workspace\tools
.\Backup-Workspace.ps1

# Verify backup contains ChatGPT exports
```

---

## Questions?

If you run into issues:
1. Check ChatGPT export documentation
2. Try manual copy/paste method
3. Look for browser extensions
4. **Don't wait** - get these exported ASAP

**Priority:** Get the raw text files saved TODAY, we can process them tomorrow.
