## MODIFIED Requirements

### Requirement: Audio transcription via Semantic Kernel IAudioToTextService

The system SHALL transcribe uploaded audio files to text by sending them through Semantic Kernel's `IAudioToTextService` connector, configured to target the local FasterWhisper endpoint via the OpenAI-compatible protocol.

#### Scenario: Successful transcription
- **WHEN** an audio file is uploaded and processed through `IAudioToTextService`
- **THEN** the system SHALL return the transcribed text from the SK connector response

#### Scenario: Audio content sent as AudioContent
- **WHEN** an audio stream is received for transcription
- **THEN** the system SHALL create an `AudioContent` from the stream and pass it to `IAudioToTextService.GetTextAsync()`

#### Scenario: Transcription returns empty result
- **WHEN** the SK connector returns an empty or null text result
- **THEN** the system SHALL return an empty string as the transcript

### Requirement: Transcription error handling

The system SHALL handle transcription failures gracefully and report the error.

#### Scenario: SK connector throws during transcription
- **WHEN** the SK audio-to-text service throws an exception during transcription
- **THEN** the system SHALL bubble the error up to the API endpoint

### Requirement: No temporary files on disk

The system SHALL NOT write audio data to disk during transcription. The audio stream SHALL be forwarded directly to the SK connector without intermediate file storage.

#### Scenario: Audio stream sent without disk I/O
- **WHEN** a transcription request is made
- **THEN** the audio stream SHALL be read and forwarded directly to the SK connector without intermediate file storage

## REMOVED Requirements

### Requirement: Audio transcription via Faster Whisper (OpenAI-compatible API)
**Reason**: Replaced by Semantic Kernel's IAudioToTextService connector
**Migration**: Audio is now sent through SK's native abstraction instead of a direct HTTP multipart POST

#### Scenario: Audio sent as multipart form data
**Reason**: The multipart form encoding is handled internally by the SK OpenAI connector
**Migration**: No action needed — the connector manages the wire protocol

### Requirement: ITranscriptionService interface
**Reason**: The SK IAudioToTextService provides the abstraction layer; the custom interface is no longer needed
**Migration**: Replace `ITranscriptionService` dependency with `Kernel` and resolve `IAudioToTextService` from it
