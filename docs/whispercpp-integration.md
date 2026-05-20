# whisper.cpp Integration Notes

## Goal

Use `whisper.cpp` as the first real offline speech engine behind the local dictation session contract.

## Current State

The repo now has:

- a backend voice session contract
- a `.NET` external speech-engine runner
- a `WhisperCpp` provider mode in the speech configuration

This means the app can now be pointed at a local `whisper.cpp` executable and model path without changing the overall architecture.

## Configuration Shape

The `Speech` section in `appsettings.json` now supports:

- `Provider`
- `ExecutablePath`
- `ModelPath`
- `Language`
- `TranslateToEnglish`
- `NoTimestamps`
- `Threads`
- `InputFileExtension`

For `whisper.cpp`, the intended values are roughly:

- `Provider`: `WhisperCpp`
- `ExecutablePath`: path to the local `main` or equivalent `whisper.cpp` executable
- `ModelPath`: path to the local model file
- `Language`: language code such as `en`

## Expected Command Shape

The `.NET` runner now builds a `whisper.cpp` style command using:

- `-m <model>`
- `-f <input>`
- `-of <output-base>`
- `-otxt`
- `-l <language>`
- optional `-nt`
- optional `-tr`
- optional `-t <threads>`

If `whisper.cpp` writes a `.txt` output file, the runner reads that file back into the app.

## Important Limitation

The current browser capture path still records audio chunks in a web-media format for the local session flow.

That means the next real integration step is still needed:

- either capture audio in a format the local engine accepts directly
- or add a local conversion step before calling `whisper.cpp`

So this is a real engine scaffold, but not yet a finished transcription pipeline.

## Recommended Next Step

The cleanest next move is to make the audio handoff compatible with `whisper.cpp`, then test with a real local executable and model file.
