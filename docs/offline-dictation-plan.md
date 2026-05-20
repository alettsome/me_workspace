# Offline Dictation Plan

## Goal

Dictation should become a real local-first, long-running input mode.

The target behavior is:

1. the user clicks `Dictation` once to start
2. microphone capture stays active until the user stops it
3. transcript text appears while the user is speaking
4. processing stays local
5. the resulting transcript can be edited and sent through the existing chat flow

This is different from a one-shot "record first, transcribe later" flow. The intended experience is live dictation with continuous transcript updates.

## Why The Browser Path Is Temporary

The current browser speech-recognition path is acceptable as a short-term fallback for testing the UI, but it is not the long-term product direction.

Reasons:

- browser speech recognition is not consistently available across browsers
- it is not reliable enough for long-running dictation
- the behavior is outside the app's local backend control
- it does not match the product's offline-first promise

## Recommended Architecture

The preferred direction is:

1. the browser captures microphone audio
2. audio is streamed to the local ASP.NET Core backend
3. the backend forwards audio chunks to a local speech worker
4. the speech worker returns interim and final transcript updates
5. the frontend updates the composer until the session is stopped

This keeps the UI simple while preserving local control in the backend.

## Technology Direction

The preferred stack stays aligned with the current project choices:

- `.NET` remains the main app and orchestration layer
- a local offline speech engine handles transcription
- Python is not required

Strong options for the local engine are:

- `whisper.cpp`
- a C# binding around Whisper-style models
- another local offline engine with a stable streaming API

The best fit for this repo is likely `whisper.cpp` or a `.NET`-friendly wrapper around it, because that keeps the product local and avoids adding a separate Python stack just for voice.

## Implementation Shape

The backend should support a voice session model:

- start session
- append audio chunk
- read session state
- stop session

Each session should expose:

- session id
- status such as `recording` or `stopped`
- current transcript text
- whether the latest transcript is final
- chunk count
- timestamps

## Near-Term Build Steps

1. keep the current browser dictation only as a temporary fallback
2. add backend session contracts for local streaming speech
3. add a local placeholder worker so the session flow can be tested end to end
4. replace the placeholder worker with a real offline engine
5. add clearer listening and transcript state in the composer

## Product Rules

- dictation should remain local by default
- microphone audio should not be sent to remote services
- the user should control when dictation starts and stops
- transcript updates should be visible while recording
- transcript text should remain editable before send
