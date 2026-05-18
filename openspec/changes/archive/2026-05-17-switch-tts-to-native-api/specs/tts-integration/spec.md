## MODIFIED Requirements

### Requirement: XTTS connection failure handling

The system SHALL report a meaningful error when the XTTS server is unreachable or the synthesis request returns an error.

#### Scenario: XTTS server is not running
- **WHEN** the `HttpClient` attempts to call `POST /tts_to_audio/` but the connection is refused
- **THEN** the system SHALL return an HTTP 500 error with a message indicating the XTTS service is unavailable

#### Scenario: Synthesis request returns an error
- **WHEN** the `POST /tts_to_audio/` endpoint returns a non-200 HTTP status or the audio download fails
- **THEN** the system SHALL return an HTTP 500 error with details from the exception
