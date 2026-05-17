## Why

The app currently has no way for users to input stories. Adding voice recording lets users quickly capture story ideas by speaking, which is faster and more natural than typing. This is the first input mechanism for the app.

## What Changes

- Add a microphone button to the main page that starts/stops voice recording
- Record audio from the user's microphone using the MediaRecorder API
- Send recorded audio to a new `/api/stories/new` endpoint
- Create the backend `/api/stories/new` endpoint that receives and processes the audio
- Replace the "Hello World!" placeholder page with a proper story input interface

## Capabilities

### New Capabilities
- `audio-recorder`: Frontend component for recording voice from the browser microphone and uploading the audio blob
- `new-story-api`: Backend API endpoint accepting audio uploads and returning a story identifier

### Modified Capabilities

<!-- No existing capabilities are modified -->

## Impact

- **Frontend**: `wwwroot/index.html` rewritten with recording UI; new `wwwroot/audio-recorder.module.css` for styles; Alpine.js CDN for reactive behavior
- **Backend**: New `Controllers/StoriesController.cs` or Minimal API endpoint in `Program.cs` for `/api/stories/new`; model for the request/response
- **Dependencies**: No new NuGet packages required (ASP.NET Core IFormFile handles multipart uploads); Alpine.js CDN added to index.html
