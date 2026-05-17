## ADDED Requirements

### Requirement: Text-to-speech via SK abstraction

The system SHALL register an implementation of `ITextToAudioService` in the SK Kernel that uses the XTTS v2 server to convert text to speech audio.

#### Scenario: Service registered in Kernel
- **WHEN** the application starts and the SK Kernel is built
- **THEN** the Kernel SHALL have an `ITextToAudioService` registered and resolvable

#### Scenario: Text synthesized to audio
- **WHEN** `ITextToAudioService.GetAudioContentAsync()` is called with a text string
- **THEN** the service SHALL return an `AudioContent` containing the generated speech as a `byte[]`

### Requirement: TTS audio generated entirely in memory

The system SHALL generate TTS audio entirely in memory without writing intermediate files to disk.

#### Scenario: No files written during TTS
- **WHEN** a TTS request is processed
- **THEN** the generated audio SHALL remain in memory as a `byte[]` inside `AudioContent`

### Requirement: Audio response from stories endpoint

The `POST /api/stories/new` endpoint SHALL accept audio input, transcribe it via whisper, synthesize the transcript via XTTS, and return the resulting audio as a binary `audio/mpeg` response.

#### Scenario: Successful audio pipeline
- **WHEN** a valid `multipart/form-data` request with an `audio` field is sent
- **THEN** the endpoint SHALL return HTTP 200 with `Content-Type: audio/mpeg` and the synthesized audio as the body

#### Scenario: Transcription failure
- **WHEN** whisper fails to transcribe the audio
- **THEN** the endpoint SHALL return HTTP 500 with an error message

#### Scenario: TTS failure
- **WHEN** XTTS fails to generate audio from the transcript
- **THEN** the endpoint SHALL return HTTP 500 with an error message

### Requirement: XTTS connection failure handling

The system SHALL report a meaningful error when the XTTS server is unreachable or the `AudioClient` returns an error.

#### Scenario: XTTS server is not running
- **WHEN** the `AudioClient` attempts to call the XTTS server but the connection is refused
- **THEN** the system SHALL return an HTTP 500 error with a message indicating the XTTS service is unavailable

#### Scenario: AudioClient returns an error
- **WHEN** the `AudioClient.GenerateSpeechAsync()` throws an exception during synthesis
- **THEN** the system SHALL return an HTTP 500 error with details from the exception
