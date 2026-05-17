## MODIFIED Requirements

### Requirement: Voice story creation endpoint

The system SHALL provide a `POST /api/stories/new` endpoint that accepts audio file uploads, transcribes the audio, and returns a story identifier with the transcript.

#### Scenario: Successful audio upload with transcription
- **WHEN** a POST request is sent to `/api/stories/new` with a multipart form containing an audio file in the `audio` field and transcription succeeds
- **THEN** the endpoint SHALL return HTTP 200 with a JSON body containing `{ "storyId": "<guid>", "transcript": "<transcribed text>" }`

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

#### Scenario: Transcription failure returns error
- **WHEN** a POST request is sent with a valid audio file but transcription fails
- **THEN** the endpoint SHALL return HTTP 500 with an appropriate error message
