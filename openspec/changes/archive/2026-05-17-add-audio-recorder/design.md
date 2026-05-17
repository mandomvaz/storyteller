## Context

The app is a .NET 10 Minimal API serving a single static HTML page from wwwroot. Currently it shows a "Hello World!" page with no user interaction. We need to add voice recording as the primary input mechanism.

## Goals / Non-Goals

**Goals:**
- Users can record audio from the browser microphone on the main page
- Recorded audio is sent to the backend for processing
- Backend accepts the audio at `/api/stories/new` and returns a story ID

**Non-Goals:**
- Transcription or NLP processing of the audio (out of scope for this change)
- Audio playback or review before sending
- Authentication or authorization
- Persistent storage of audio files beyond processing
- Support for older browsers without MediaRecorder API

## Decisions

1. **Minimal API endpoint** over Controller: The endpoint is simple (single POST), so a Minimal API inline in Program.cs keeps things lean. Extract to a service class for the processing logic.

2. **Alpine.js for frontend reactivity**: Aligns with existing project conventions. The recording button state (idle/recording/sending) maps naturally to Alpine.js x-data.

3. **MediaRecorder API** for audio capture: Native browser API, no extra dependencies. Records as webm/opus by default.

4. **IFormFile for audio upload**: Standard ASP.NET Core multipart upload. No custom parsing needed.

5. **CSS Modules for styling**: Use `wwwroot/audio-recorder.module.css` following project conventions.

## Risks / Trade-offs

- MediaRecorder format varies by browser → Mitigation: send as `audio/webm` which is widely supported
- Large audio files could timeout → Mitigation: set reasonable request size limits and client-side timeouts
- Microphone permission UX varies by browser → Mitigation: handle permission denied error gracefully with UI feedback
