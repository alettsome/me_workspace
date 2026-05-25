# Phase 1: Foundations

## Goal
Make sure the local stack is reliable before you scale research intake.

## Technology Used
- `Ollama`
- `Open WebUI`
- `Docker`
- `PowerShell`
- `Windows folders`

## What Happens In This Phase
- Ollama runs locally
- Open WebUI connects to Ollama
- A default model is chosen
- The summary scripts work
- Output folders are in place

## Checklist
- [ ] Confirm Ollama is running
- [ ] Confirm Open WebUI opens at `http://localhost:3000`
- [ ] Confirm models are visible in Open WebUI
- [ ] Confirm `phi3:3.8b` is available for first-pass work
- [ ] Confirm `GenerateKey.ps1` works
- [ ] Confirm `BatchProcessor.ps1` works
- [ ] Confirm `C:\Users\alett\Desktop\Health_Summaries` exists
- [ ] Confirm a simple naming convention exists for saved summaries

## Done Looks Like
- You can input text and reliably get a saved summary back

## Notes
Do not scale source intake until this phase feels boring and dependable.
