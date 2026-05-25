# Immediate Action Plan

**Date:** May 25, 2026  
**Status:** Database migration complete, ready for high-priority workflows

---

## Priority 1: 🔴 URGENT - Export ChatGPT Conversations

**Why Urgent:** May lose access to ChatGPT, contains critical information from 3 primary chats

### Steps to Complete TODAY:

1. **Export from ChatGPT**
   - Log into ChatGPT
   - Go to Settings → Data Controls → Export Data
   - Or manually copy conversations (if export not available)
   - Save as: `ChatGPT-Export-[date].zip` or individual HTML/JSON files

2. **Organize Exports**
   ```powershell
   # Create intake directory
   New-Item -ItemType Directory -Path "C:\me_workspace\01-Inbox\ChatGPT-Exports" -Force
   
   # Move exported files there
   Move-Item "Downloads\ChatGPT-Export-*.zip" "C:\me_workspace\01-Inbox\ChatGPT-Exports\"
   ```

3. **Extract if Zipped**
   ```powershell
   cd C:\me_workspace\01-Inbox\ChatGPT-Exports
   Expand-Archive -Path "ChatGPT-Export-*.zip" -DestinationPath ".\extracted"
   ```

4. **Create Sources in Database**
   - Each chat becomes a Source
   - SourceType: "conversation-log"
   - RightsLabel: "user-created"
   - Process through existing pipeline (extract → chunk → summarize)

5. **Verify Backup**
   - Once processed, verify summaries in database
   - Keep original exports in safe location
   - Add to next immutable backup

**Acceptance:** All 3 primary ChatGPT conversations are extracted, processed, and searchable in database.

---

## Priority 2: 🔴 Build Bible Constitution Project

**Purpose:** AI's "Constitution" - source of truth for moral/spiritual guidance and AI personality

### What This Project Contains:

1. **King James Version** (primary reference)
2. **Ethiopian Bible** (88 books - includes apocrypha/missing books)
3. **Apocrypha/Missing Books** (books removed from canonical Bible)
4. **AI Personality Rules** (stored as manifest)

### Project Structure:

```
projects/
  Bible_Constitution/
    manifest.json              # AI personality rules
    sources/
      KJV/                     # King James Version
      Ethiopian/               # Ethiopian Bible (88 books)
      Apocrypha/              # Missing books
    database/
      unified-bible.db        # Normalized verse structure
    anchors/
      old-testament.md        # DocumentAnchor for OT
      new-testament.md        # DocumentAnchor for NT
      apocrypha.md           # DocumentAnchor for apocrypha
```

### Implementation Steps:

**Step 1: Source the Texts**
- [ ] Find digital King James Version (public domain)
  - Options: Project Gutenberg, Bible Gateway API, local Bible software
- [ ] Find Ethiopian Bible (88 books)
  - Search: "Ethiopian Orthodox Tewahedo Church Bible" or "Ge'ez Bible"
- [ ] Identify apocrypha books (Enoch, Jubilees, Tobit, etc.)

**Step 2: Create Project**
```powershell
# Create project structure
New-Item -ItemType Directory -Path "C:\me_workspace\projects\Bible_Constitution" -Force
New-Item -ItemType Directory -Path "C:\me_workspace\projects\Bible_Constitution\sources\KJV" -Force
New-Item -ItemType Directory -Path "C:\me_workspace\projects\Bible_Constitution\sources\Ethiopian" -Force
New-Item -ItemType Directory -Path "C:\me_workspace\projects\Bible_Constitution\sources\Apocrypha" -Force
```

**Step 3: Create AI Personality Manifest**
```json
{
  "projectName": "Bible_Constitution",
  "contentType": "SourceOfTruth",
  "purpose": "AI Constitution - moral/spiritual guidance and personality rules",
  "aiPersonality": {
    "sourceOfTruth": {
      "primary": "King James Version",
      "supplementary": ["Ethiopian Bible", "Apocrypha"],
      "referenceStyle": "Always cite book, chapter, verse"
    },
    "responseGuidelines": {
      "healthTopics": {
        "priority": "Natural remedies FIRST, medication only after natural options exhausted",
        "reasoning": "User values natural healing, whole foods approach"
      },
      "tone": "Conversational, willing to challenge and question user's assumptions",
      "tokenEfficiency": "Know user's values upfront to avoid wasting tokens on irrelevant approaches"
    },
    "boundaries": {
      "moralGuidance": "Reference Bible Constitution for spiritual/moral questions",
      "healthGuidance": "Prefer natural, affordable, accessible solutions",
      "debateStyle": "Socratic - ask questions, challenge assumptions, encourage critical thinking"
    }
  },
  "sources": [
    {"type": "bible", "version": "KJV", "books": 66, "status": "canonical"},
    {"type": "bible", "version": "Ethiopian", "books": 88, "status": "expanded-canon"},
    {"type": "bible", "version": "Apocrypha", "books": "variable", "status": "non-canonical"}
  ]
}
```

**Step 4: Ingest Bible Texts**
- [ ] Add KJV to database (book/chapter/verse normalized)
- [ ] Add Ethiopian Bible (identify additional 22 books)
- [ ] Tag verses with topics (healing, marriage, wisdom, etc.)
- [ ] Create cross-references between versions

**Step 5: Create Database Project Entry**
```sql
INSERT INTO Projects (Id, Name, ContentType, Status, Description, CreatedUtc, UpdatedUtc)
VALUES (
  lower(hex(randomblob(16))),
  'Bible_Constitution',
  'SourceOfTruth',
  'active',
  'AI Constitution: KJV + Ethiopian Bible + Apocrypha. Source of truth for moral/spiritual guidance and AI personality rules.',
  datetime('now'),
  datetime('now')
);
```

**Acceptance:** Bible Constitution project exists with unified database, searchable by book/chapter/verse, AI references this as source of truth.

---

## Priority 3: 🟡 Add Text-to-Speech Review Workflow

**Purpose:** Listen to book chapters with eyes closed, provide feedback, iterate on content

### User's Current Setup:
- Already has offline TTS tool
- Wants to avoid copying content to external tools
- Needs integrated workflow: generate → TTS → listen → feedback → revise

### Workflow Design:

```
1. AI generates chapter draft
   ↓
2. Save to temp file: projects/Health_Fundamentals/drafts/chapter1-draft.md
   ↓
3. Convert to audio: chapter1-draft.wav (using offline TTS)
   ↓
4. User plays audio, listens with eyes closed
   ↓
5. User provides feedback (voice notes or text annotations)
   ↓
6. AI processes feedback → generates revision
   ↓
7. Show diff: original vs. revised
   ↓
8. User approves → apply patch to master document
```

### Implementation:

**Step 1: Integrate Offline TTS**
- [ ] Identify user's TTS tool (likely Balabolka or similar)
- [ ] Create PowerShell wrapper script: `Invoke-TTS.ps1`
- [ ] Input: markdown file
- [ ] Output: WAV/MP3 audio file

**Step 2: Add TTS Review Command**
```powershell
# Generate TTS for review
.\Invoke-TTS.ps1 -InputFile "projects\Health_Fundamentals\drafts\chapter1.md" -OutputFile "chapter1.wav"

# Play audio (Windows Media Player or VLC)
Start-Process "chapter1.wav"
```

**Step 3: Capture Feedback**
- [ ] Voice notes (user records feedback using whisper.cpp voice capture)
- [ ] Text annotations (user types notes in feedback.md file)
- [ ] Timestamped feedback (link feedback to specific sections)

**Step 4: Process Feedback**
- [ ] Parse feedback → extract revision requests
- [ ] Feed to LLM: "Original text: [chapter]. Feedback: [notes]. Generate revised version."
- [ ] Create diff showing changes
- [ ] User reviews diff → approves/rejects

**Acceptance:** User can generate audio of chapter, listen, provide feedback, and see revised version based on feedback.

---

## Priority 4: 🟡 Native Voice Capture (Security Fix)

**Problem:** Browser-based microphone capture allows extensions to hijack audio

**Solution:** Native Windows microphone capture using NAudio or Windows Core Audio APIs

### Implementation:

**Option 1: NAudio Library (Recommended)**
```csharp
// Install: dotnet add package NAudio
using NAudio.Wave;

public class NativeMicrophoneCapture
{
    private WaveInEvent waveIn;
    private WaveFileWriter writer;
    
    public void StartRecording(string outputPath)
    {
        waveIn = new WaveInEvent();
        waveIn.WaveFormat = new WaveFormat(16000, 1); // 16kHz mono for whisper.cpp
        
        writer = new WaveFileWriter(outputPath, waveIn.WaveFormat);
        
        waveIn.DataAvailable += (sender, args) => {
            writer.Write(args.Buffer, 0, args.BytesRecorded);
        };
        
        waveIn.StartRecording();
    }
    
    public void StopRecording()
    {
        waveIn?.StopRecording();
        writer?.Dispose();
        waveIn?.Dispose();
    }
}
```

**Option 2: Windows Core Audio APIs**
- Direct COM interop (more complex)
- Lower latency, more control
- Requires P/Invoke declarations

### Steps:

1. **Add NAudio package**
   ```powershell
   cd c:\me_workspace\src\me_workspace.Web
   dotnet add package NAudio
   ```

2. **Create VoiceCapture service**
   - Replace browser microphone code with native capture
   - Use same whisper.cpp backend
   - Keep existing voice session UI

3. **Test isolation**
   - Verify browser extensions cannot access audio
   - Confirm audio quality matches or exceeds browser capture

**Acceptance:** Voice capture works without browser involvement, extensions cannot hijack audio stream.

---

## Priority 5: 🟡 Global Inbox

**Purpose:** Capture ideas/thoughts that don't belong to specific projects

### Implementation:

```
ThingsToDo/
  global-inbox.md         # Quick capture file
  ideas/
    idea-001.md          # Detailed idea notes
    idea-002.md
```

**Quick Capture Format:**
```markdown
# Global Inbox

## Unsorted Ideas
- [ ] Investigate Ethiopian Bible differences
- [ ] Research monogamy vs polygamy historical context
- [ ] Find natural remedy for inflammation
- [ ] Create template for business plan projects

## Needs Categorization
- Health topic: vitamin cofactors
- Marriage topic: biblical marriage structure
```

**Acceptance:** User can quickly jot down ideas without creating full projects, later sort into projects when ready.

---

## Priority 6: 🟡 Multi-Source Council Workflow

**Use Case:** Marriage book project - Bible + supporting books on monogamy

### Project Structure:

```sql
-- Create Marriage project
INSERT INTO Projects (Id, Name, ContentType, Status, Description, CreatedUtc, UpdatedUtc)
VALUES (
  lower(hex(randomblob(16))),
  'Marriage_Biblical_Perspective',
  'Book',
  'assessment',
  'Research biblical marriage (monogamy/polygamy) using Bible + historical/academic sources',
  datetime('now'),
  datetime('now')
);

-- Link sources to project
-- Bible (already in Bible_Constitution project, can cross-reference)
-- Book 1: [Marriage book title]
-- Book 2: [Marriage book title]
```

### Council Workflow:

1. **Assessment Phase**
   - Query: "Biblical marriage monogamy polygamy"
   - Search Bible Constitution + linked sources
   - AgentLog: "researcher" found 15 Bible verses, 3 books

2. **Acquisition Phase**
   - Add sources to project
   - Generate summaries with page/verse citations
   - Store in Summary table with ProjectId link

3. **Analysis Phase**
   - TrendAnalysis: "Council analyzed 4 sources (1 Bible, 3 books)"
   - Common themes: [list themes]
   - Conflicting views: [list conflicts]
   - Biblical perspective: [synthesis]

4. **Synthesis Phase**
   - Generate outline based on trends
   - Create DocumentAnchors for book chapters
   - Populate with content referencing summaries

**Acceptance:** User can create project, link multiple sources, run council analysis, see cross-source insights.

---

## Next Steps After These Priorities

1. **Document Assembly with Patch/Diff** (Phase 14 full implementation)
2. **Semantic Search** (Phase 12 - embeddings for better retrieval)
3. **Automated Acquisition** (Phase 5 - web scraping, library APIs)

---

## Resources Needed

**For ChatGPT Export:**
- ChatGPT account access
- Export data feature or manual copy/paste

**For Bible Constitution:**
- King James Version (public domain, easy to find)
- Ethiopian Bible (harder to source, may need research)
- Apocrypha texts (various sources online)

**For TTS:**
- User already has offline TTS tool
- Need tool name/command-line interface

**For Native Voice:**
- NAudio NuGet package (free, MIT license)
- No additional resources needed

---

## Questions for User

1. **ChatGPT Export**: Do you have access to export feature, or will you need to manually copy conversations?

2. **Bible Sources**: Do you have digital copies of KJV and Ethiopian Bible, or do you need help sourcing them?

3. **TTS Tool**: What offline TTS tool do you currently use? (Balabolka, eSpeak, Microsoft Speech Platform, other?)

4. **Timeline**: Do you want to tackle these in order (ChatGPT → Bible → TTS → Voice), or work on multiple in parallel?

---

## Summary

**Immediate Actions:**
1. 🔴 Export ChatGPT conversations TODAY (3 chats)
2. 🔴 Set up Bible_Constitution project structure
3. 🟡 Integrate TTS review workflow
4. 🟡 Add native voice capture (NAudio)
5. 🟡 Create global inbox
6. 🟡 Build marriage book project (multi-source council)

**Foundation is Complete:**
- ✅ Database schema migrated
- ✅ Application tested and working
- ✅ Backup system operational

**Ready to Build Features.**
