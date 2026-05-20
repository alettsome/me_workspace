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

1. a native local microphone component captures audio outside the browser
2. audio is streamed to the local ASP.NET Core backend or directly into the local speech worker
3. the backend coordinates the session and transcript state
4. the speech worker returns interim and final transcript updates
5. the frontend shows transcript updates without owning raw microphone capture

This keeps the UI simple while moving microphone trust out of the browser.

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
4. add a configurable external local speech-engine adapter in `.NET`
5. replace the placeholder worker behavior with a real offline engine such as `whisper.cpp`
6. replace browser microphone capture with a native local microphone path
7. add clearer listening and transcript state in the composer

## Current Scaffold

The repo now includes:

- a local backend voice session contract
- a `.NET` external-process speech engine runner
- configuration fields for a local executable and arguments

This means the next integration step is no longer "invent the backend shape." It is "point the existing backend shape at a real offline engine and refine the transcript workflow."

## Product Rules

- dictation should remain local by default
- microphone audio should not be sent to remote services
- microphone capture should move out of the browser for the long-term privacy-first build
- the user should control when dictation starts and stops
- transcript updates should be visible while recording
- transcript text should remain editable before send
