# Health Fundamentals Book Project

A comprehensive book on health fundamentals built using the me_workspace platform with local AI processing.

## Goal

Create an accessible, evidence-based book on health fundamentals using fully offline AI tools to ensure privacy and bypass content gatekeeping.

## Architecture

### Current Workflow
- **Ollama** - Local model execution (runs at `http://localhost:11434`)
- **Open WebUI** - Research interface and RAG engine (runs at `http://localhost:3000`)
- **Phi-3 3.8B** - Primary analysis model
- **me_workspace** - Platform infrastructure and file organization

### Folder Structure

```
Health_Fundamentals/
├── Research/              Generated summaries and research output
├── Outline/               Book structure and organization
├── Chapters/              Final chapter content
├── Workflow/              Phase-based research process
│   ├── 00-Start-Here/
│   ├── 01-Foundations/
│   ├── 02-Source-Intake/
│   ├── 03-Summarization/
│   ├── 04-Review-And-Clustering/
│   ├── 05-Structured-Outputs/
│   ├── 06-App-Orchestration/
│   └── 07-Next-Steps/
├── BatchProcessor.ps1     Automated summarization script
├── GenerateKey.ps1        Open WebUI API setup
└── OpenWebUI_API_Key.txt  API credentials
```

## Quick Start

### 1. Start Services

```powershell
# Start Docker Desktop, then:
docker start open-webui

# Verify Ollama is running:
ollama list
```

### 2. Generate API Key (First Time Only)

```powershell
cd "C:\me_workspace\projects\Health_Fundamentals"
.\GenerateKey.ps1
```

### 3. Run Batch Processor

```powershell
.\BatchProcessor.ps1
```

Copy text to clipboard to automatically summarize it. Output goes to `Research/` folder.

## Open WebUI Workflow

### Create Knowledge Base
1. Open `http://localhost:3000`
2. Go to **Workspace → Knowledge → + New Knowledge**
3. Name it `Health Fundamentals`
4. Upload PDFs, documents, or paste text

### Chat with Knowledge Base
1. Start **New Chat**
2. Click **+ icon** → **Knowledge** → select your KB
3. Set model to **phi3:3.8b**
4. Ask questions - model answers from your documents

### Critical Reviewer Approach
Set system prompt:
```
You are a critical health research reviewer. Cite sources. 
Identify contradictions, gaps, and unsupported claims. 
Be specific and concise.
```

## Integration with me_workspace

This project uses me_workspace platform features:
- Journal-backed workflow tracking
- Local file context
- Offline LLM orchestration
- Privacy-first architecture

Future phases will tighten Open WebUI integration with the me_workspace backend.

## Current Status

- ✅ Docker and Ollama running
- ✅ Open WebUI connected to models
- ✅ Scripts migrated to me_workspace structure
- ✅ Research output folder configured
- ⏳ Knowledge base population in progress
- ⏳ Chapter outline development
- ⏳ Content generation pipeline

## Next Steps

1. Upload research sources to Open WebUI Knowledge Base
2. Run critical review analysis
3. Build chapter outline in `Outline/`
4. Begin chapter generation in `Chapters/`
5. Integrate with me_workspace journals for progress tracking
