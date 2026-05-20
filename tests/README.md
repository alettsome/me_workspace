# Tests

## API Smoke Test

Run the local end-to-end API flow check with:

```powershell
powershell -ExecutionPolicy Bypass -File .\tests\ApiFlowSmoke.ps1
```

What it covers:

- app health endpoint
- system flow endpoint
- demo voice transcript endpoint
- create conversation
- send message through the local chat pipeline

This is a lightweight smoke test meant to confirm the pieces are talking to each other before deeper component work begins.
