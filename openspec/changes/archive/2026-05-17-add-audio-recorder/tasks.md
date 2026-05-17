## 1. Backend: Voice Story Endpoint

- [x] 1.1 Add POST `/api/stories/new` Minimal API endpoint in Program.cs
- [x] 1.2 Validate audio file presence, size (max 10MB), and content type
- [x] 1.3 Return HTTP 200 with `{ "storyId": "<guid>" }` on success
- [x] 1.4 Return appropriate error responses (400, 413, 415) for invalid input
- [x] 1.5 Create a `VoiceStoryService` class with a method to process the audio and return a story ID

## 2. Frontend: Audio Recorder Component

- [x] 2.1 Create `wwwroot/audio-recorder.module.css` with idle, recording, loading, success, and error styles
- [x] 2.2 Add Alpine.js CDN to `wwwroot/index.html`
- [x] 2.3 Rewrite `wwwroot/index.html` with the audio recorder Alpine.js component
- [x] 2.4 Implement MediaRecorder logic: start/stop recording on button click
- [x] 2.5 Implement upload: send recorded blob as multipart form to `/api/stories/new`
- [x] 2.6 Handle all UI states: idle, recording, uploading, success, error, permission denied
