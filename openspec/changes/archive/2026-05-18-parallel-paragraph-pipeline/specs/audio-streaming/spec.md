## MODIFIED Requirements

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

#### Scenario: Audio delivered as sequential chunks
- **WHEN** each paragraph audio is ready
- **THEN** the server SHALL send an `"audioChunk"` event to the client's connection containing the WAV audio data as a binary MessagePack payload
- **WHEN** all paragraphs have been processed and delivered
- **THEN** the server SHALL send a `"storyComplete"` event to the client's connection

#### Scenario: Step-level error reporting
- **WHEN** any step in the pipeline fails
- **THEN** the server SHALL send an `"error"` event with `{ jobId, connectionId, step, message }` and stop processing the current job

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
- **THEN** the `BackgroundService` SHALL read it, create a DI scope, and delegate to the pipeline runner

## REMOVED Requirements

### Requirement: Pipeline progress events for audio generation step

**Reason**: Audio generation is now continuous and per-paragraph. The `generatingAudio` event is replaced by the arrival of `audioChunk` events.

**Migration**: Remove `generatingAudio` state from frontend state machine. Replace with logic that receives `audioChunk` events and plays them sequentially until `storyComplete`.

### Requirement: Audio delivery as a single event

**Reason**: Replaced by sequential `audioChunk` events + terminal `storyComplete` event.

**Migration**: Replace `"audioReady"` handler with `"audioChunk"` + `"storyComplete"` listeners.
