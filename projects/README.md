# me_workspace Projects

This folder contains products built with the me_workspace platform.

## Structure

Each project represents a complete work product that uses the platform's capabilities:
- Local LLM integration via Ollama
- Knowledge base management
- Research workflows
- Document generation

## Current Projects

### Health_Fundamentals
A comprehensive book on health fundamentals built using the me_workspace research and generation pipeline.

**Location:** `Health_Fundamentals/`

**Key Components:**
- `Research/` - Processed summaries and research output
- `Outline/` - Book structure and organization
- `Chapters/` - Generated chapter content
- `Workflow/` - Phase-based research workflow
- `BatchProcessor.ps1` - Automated research summarization
- `GenerateKey.ps1` - Open WebUI API integration setup

## Adding New Projects

Each project should follow this pattern:
```
projects/
└── ProjectName/
    ├── README.md          (project overview)
    ├── Research/          (source material and analysis)
    ├── Output/            (generated content)
    └── Workflow/          (project-specific process)
```

## Integration with me_workspace

Projects leverage me_workspace platform features:
- Local SQLite storage (`App_Data/`)
- Journal-backed context (`Journals/`)
- File-aware operations
- Local LLM orchestration
- Offline-first architecture

The platform provides the infrastructure; projects provide the domain-specific workflows.
