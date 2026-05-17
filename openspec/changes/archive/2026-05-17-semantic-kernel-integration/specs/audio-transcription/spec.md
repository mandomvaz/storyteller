## ADDED Requirements

### Requirement: Audio transcription via Faster Whisper (OpenAI-compatible API)

The system SHALL transcribe uploaded audio files to text by sending them to the Faster Whisper server's `/v1/audio/transcriptions` endpoint using the OpenAI-compatible API format.

#### Scenario: Successful transcription
- **WHEN** an audio file is uploaded and sent to the Whisper endpoint
- **THEN** the system SHALL return the `text` field from the OpenAI-compatible JSON response

#### Scenario: Audio sent as multipart form data
- **WHEN** an audio stream is received for transcription
- **THEN** the system SHALL send it as `multipart/form-data` with a `file` field (containing the audio stream, filename, and content type) and a `model` field (containing the configured model name)

#### Scenario: Transcription returns empty result
- **WHEN** the Whisper API returns an empty or null `text` field
- **THEN** the system SHALL return an empty string as the transcript

### Requirement: Transcription error handling

The system SHALL handle transcription failures gracefully and report the error.

#### Scenario: Whisper returns an error response during transcription
- **WHEN** the Whisper API returns a non-success HTTP status during transcription
- **THEN** the system SHALL bubble the error up to the API endpoint

### Requirement: ITranscriptionService interface

The system SHALL define an `ITranscriptionService` interface with a `TranscribeAsync(Stream audioStream, string contentType)` method for testability and SRP.

#### Scenario: Interface injectable via DI
- **WHEN** a service depends on `ITranscriptionService`
- **THEN** the DI container SHALL resolve it to the `WhisperService` implementation

### Requirement: No temporary files on disk

The system SHALL NOT write audio data to disk during transcription. The audio stream SHALL be forwarded directly to the Whisper API as multipart form content.

#### Scenario: Audio stream sent without disk I/O
- **WHEN** a transcription request is made
- **THEN** the audio stream SHALL be read and forwarded directly without intermediate file storage
