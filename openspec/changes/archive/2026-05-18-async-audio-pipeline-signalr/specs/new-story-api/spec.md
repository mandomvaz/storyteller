## MODIFIED Requirements

### Requirement: Voice story creation endpoint

The system SHALL provide a `POST /api/stories/new` endpoint that accepts audio file uploads, validates the audio, enqueues the pipeline for background processing, and returns immediately with a job identifier.

#### Scenario: Successful audio upload returns job ID immediately
- **WHEN** a POST request is sent to `/api/stories/new` with a multipart form containing an audio file in the `audio` field and a `connectionId` field identifying the SignalR connection
- **THEN** the endpoint SHALL validate the audio and return HTTP 200 with a JSON body containing `{ "jobId": "<guid>" }`
- **AND** the endpoint SHALL NOT wait for transcription, story generation, or audio synthesis to complete before responding

#### Scenario: Missing audio file
- **WHEN** a POST request is sent to `/api/stories/new` without an `audio` field
- **THEN** the endpoint SHALL return HTTP 400 with an appropriate error message

#### Scenario: Empty audio file
- **WHEN** a POST request is sent to `/api/stories/new` with an empty audio file
- **THEN** the endpoint SHALL return HTTP 400 with an appropriate error message

#### Scenario: Unsupported content type
- **WHEN** a POST request is sent to `/api/stories/new` with a non-audio file type
- **THEN** the endpoint SHALL return HTTP 415 with an appropriate error message

#### Scenario: File too large
- **WHEN** a POST request is sent to `/api/stories/new` with an audio file exceeding 10MB
- **THEN** the endpoint SHALL return HTTP 413 with an appropriate error message

#### Scenario: Missing connectionId
- **WHEN** a POST request is sent to `/api/stories/new` without a `connectionId` field
- **THEN** the endpoint SHALL return HTTP 400 with an appropriate error message

### REMOVED Requirements

### Requirement: Full pipeline response in endpoint

**Reason**: Pipeline is now asynchronous. The endpoint enqueues and returns immediately; audio delivery happens via SignalR.

**Migration**: Frontend must connect to SignalR hub and listen for `audioReady` event instead of awaiting the POST response body.
