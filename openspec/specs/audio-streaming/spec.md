## ADDED Requirements

### Requirement: Real-time pipeline status via SignalR

The system SHALL broadcast pipeline progress events to the frontend via a SignalR hub at `/storyHub`. Audio SHALL be transferred using the MessagePack protocol.

#### Scenario: Frontend connects to hub on page load
- **WHEN** the page loads
- **THEN** the frontend SHALL establish a SignalR connection to `/storyHub` and obtain a `connectionId`

#### Scenario: Pipeline progress events are received
- **WHEN** the transcription step starts
- **THEN** the server SHALL send a `transcribing` event to the client's connection
- **WHEN** the story generation step starts
- **THEN** the server SHALL send a `writing` event to the client's connection
- **WHEN** the audio generation step starts
- **THEN** the server SHALL send a `generatingAudio` event to the client's connection

#### Scenario: Audio delivery via MessagePack binary
- **WHEN** the full pipeline completes
- **THEN** the server SHALL send an `audioReady` event to the client's connection containing the WAV audio data as a binary MessagePack payload, not as base64-encoded JSON

#### Scenario: Step-level error reporting
- **WHEN** the transcription step fails
- **THEN** the server SHALL send an `error` event with `{ step: "transcription", message: "<error details>" }` and stop the pipeline
- **WHEN** the story generation step fails
- **THEN** the server SHALL send an `error` event with `{ step: "story", message: "<error details>" }` and stop the pipeline
- **WHEN** the audio generation step fails
- **THEN** the server SHALL send an `error` event with `{ step: "audio", message: "<error details>" }`

#### Scenario: Worker runs to completion regardless of client state
- **WHEN** a job is enqueued
- **THEN** the background worker SHALL process it to completion even if the client disconnects before receiving the result

### Requirement: In-memory job queue via Channel<T>

The system SHALL use `System.Threading.Channels.Channel<T>` as an in-memory, unbounded FIFO queue for pipeline jobs. A `BackgroundService` SHALL consume from the channel and execute the pipeline.

#### Scenario: Job is enqueued after audio validation
- **WHEN** the POST endpoint validates an audio file
- **THEN** a job containing the `connectionId` and audio data SHALL be written to the channel

#### Scenario: Background worker consumes and processes
- **WHEN** a job is available in the channel
- **THEN** the `BackgroundService` SHALL read it and execute the pipeline: transcribe → generate story → generate audio
- **WHEN** no jobs are available
- **THEN** the worker SHALL await asynchronously without polling
