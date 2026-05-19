## Requirements

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

### Requirement: Per-paragraph TTS via channel consumption

The `TextToAudioService` SHALL consume `TextUnit` objects from `Channel<TextUnit>` and produce `AudioUnit` objects to `Channel<AudioUnit>` via the SK `ITextToAudioService`.

#### Scenario: Each TextUnit produces an AudioUnit
- **WHEN** a `TextUnit` is read from `Channel<TextUnit>`
- **THEN** its text SHALL be passed to `ITextToAudioService.GetAudioContentAsync()` individually
- **AND** the resulting `AudioUnit` SHALL retain the original `JobId` and `ConnectionId`

### Requirement: XTTS connection failure handling

The system SHALL report a meaningful error when the XTTS server is unreachable or the synthesis request returns an error.

#### Scenario: XTTS server is not running
- **WHEN** the `HttpClient` attempts to call `POST /tts_to_audio/` but the connection is refused
- **THEN** the system SHALL send an `"error"` SignalR event with `{ step: "tts", message }`

#### Scenario: Synthesis request returns an error
- **WHEN** the `POST /tts_to_audio/` endpoint returns a non-200 HTTP status or the audio download fails
- **THEN** the system SHALL send an `"error"` SignalR event with `{ step: "tts", message }`
