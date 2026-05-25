# Target Audience - Automatic Processing

## Primary User

**Solo Knowledge Worker Building Personal Knowledge Base**

**Profile**:
- Individual managing 100+ sources
- Privacy-conscious (local-first preference)
- Time-sensitive access (library borrows, web archives)
- Building multi-source research projects
- Wants AI-assisted summarization/search

**Pain Points**:
- Manual PDF processing takes too long
- Sources expire before they're processed
- Friction prevents adding valuable sources
- No way to track what's been processed

**Use Cases**:
1. **ChatGPT Export** - Preserve conversations before access lost
2. **Book Research** - Process 50+ books for Health Fundamentals project
3. **Bible Assembly** - Integrate KJV + Ethiopian Bible + apocrypha
4. **Business Opportunities** - Quick intake for assessment documents

---

## Secondary Audience

### **Academic Researchers**

**Profile**:
- Managing papers, books, theses
- Need to cite sources accurately
- Working with time-limited library access
- Privacy-sensitive data (unpublished research)

**Why This Feature**:
- Automatic page tracking for citations
- Local storage (no cloud exposure)
- Batch processing for multiple papers
- Notification history for verification

### **Writers / Authors**

**Profile**:
- Researching for books/articles
- Multiple sources per project
- Need to find quotes/references quickly
- Want to avoid losing source material

**Why This Feature**:
- Fast intake (don't lose momentum)
- Searchable chunks
- Source tracking ("Where did I read that?")
- Local backup of ephemeral web content

### **Business Analysts**

**Profile**:
- Evaluating opportunities from documents
- Need quick turnaround on assessments
- Multiple competing proposals
- Tracking due diligence sources

**Why This Feature**:
- Instant intake (fast decision-making)
- Notification tracking (what's processed)
- Database queries (compare across sources)
- Audit trail (source of insights)

---

## User Personas

### **Persona 1: The Bootstrapper**

**Name**: Alex
**Goal**: Build Health Fundamentals book from 100+ sources
**Challenge**: Library books expire, need to extract before return
**Workflow**: 
1. Borrow book from Internet Archive (1-hour access)
2. Screen capture 200 pages to PDF
3. Drop PDF in intake folder
4. System extracts + chunks automatically
5. Return book (no penalty)

**Why Automatic Processing**:
- Can't waste time on manual extraction
- Access window too short
- Need to process while still have access

---

### **Persona 2: The Refugee**

**Name**: Jordan
**Goal**: Export ChatGPT conversations before access revoked
**Challenge**: ChatGPT rate-limiting manual exports
**Workflow**:
1. Print ChatGPT conversation to PDF
2. Get rate-limited ("too many requests")
3. Wait 10 minutes
4. Repeat for each conversation
5. Drop all PDFs in intake
6. System processes overnight

**Why Automatic Processing**:
- Can't manually trigger each one
- Need batch processing
- Want to verify all processed (notifications)

---

### **Persona 3: The Synthesizer**

**Name**: Sam
**Goal**: Cross-reference Bible versions for constitution project
**Challenge**: 88-book Ethiopian Bible + KJV + apocrypha
**Workflow**:
1. Acquire digital Bible texts (PDFs)
2. Drop all versions in intake
3. System chunks each version
4. Use council workflow to compare themes
5. Generate unified constitution

**Why Automatic Processing**:
- Too many files to process manually
- Need consistent chunking across versions
- Want database for cross-referencing

---

## User Needs Hierarchy

**Must Have** (P0):
1. Zero-friction intake (drop file, done)
2. Reliable processing (no silent failures)
3. Notification of completion
4. Local storage (privacy)

**Should Have** (P1):
1. Error reporting (failed extractions)
2. Processing history (what succeeded)
3. Unread notification tracking
4. API access (programmatic queries)

**Nice to Have** (P2):
1. Progress updates during long processing
2. Preview before processing
3. Duplicate detection
4. Batch status overview

---

## Anti-Personas (Not Target Audience)

### **Enterprise Teams**
- Need: Multi-user collaboration
- Why Not: Single-user local-first design

### **Cloud-First Users**
- Need: Cross-device sync
- Why Not: Local-first architecture

### **Non-Technical Users**
- Need: GUI-only workflow
- Why Not: Requires understanding of folder structure

### **Real-Time Collaboration**
- Need: Live shared editing
- Why Not: Offline-first, batch processing model

---

## User Success Metrics

1. **Time to Intake**: <5 seconds from drop to processing start
2. **Success Rate**: >95% of PDFs extract successfully
3. **Notification Rate**: 100% of files generate notification
4. **User Satisfaction**: "I don't think about it, it just works"
