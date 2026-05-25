# Phase 6: App Orchestration

## Goal
Build the `.NET` layer that manages the pipeline end to end.

## Technology Used
- `.NET`
- File system storage
- Open WebUI integration
- Ollama integration
- PowerShell or background job runners

## Main Idea
Open WebUI is the research brain.
The `.NET` application becomes the workflow manager.

## Core Modules
- `Source Intake`
- `Metadata And Rights Tagging`
- `Processing Queue`
- `Open WebUI Connector`
- `Summary Store`
- `Review Workspace`
- `Output Builder`

## What Happens In This Phase
- A user adds a source
- The app tags and queues it
- The app sends it for summarization and review
- The app stores the results in folders
- The app promotes approved notes into project outputs

## Suggested MVP
Start with:

1. Add a source manually
2. Save metadata
3. Launch a summary job
4. Save the result to the right folder
5. Show job status in one simple screen

## Checklist
- [ ] Define the folder schema the app will manage
- [ ] Define the job states
- [ ] Define how summaries are stored
- [ ] Define how source metadata is stored
- [ ] Define when Open WebUI is used versus direct Ollama calls
- [ ] Build the smallest possible MVP first

## Done Looks Like
- The workflow is repeatable without relying on memory, scattered chats, or manual guesswork
