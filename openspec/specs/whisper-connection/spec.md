## Requirements

### Requirement: Configurable Faster Whisper endpoint

The system SHALL read the Faster Whisper server URL from configuration under the `Whisper:Endpoint` key with a default value of `http://localhost:8000`, and pass it to the SK `AddOpenAIAudioToText()` connector registration.

#### Scenario: Default endpoint used when not configured
- **WHEN** the configuration key `Whisper:Endpoint` is not present
- **THEN** the SK connector SHALL use `http://localhost:8000` as the default endpoint

#### Scenario: Custom endpoint from configuration
- **WHEN** the configuration key `Whisper:Endpoint` is set to `http://10.0.0.5:9000`
- **THEN** the SK connector SHALL be configured with the custom endpoint URL

### Requirement: Configurable Whisper model name

The system SHALL read the Whisper model identifier from configuration under `Whisper:Model` (default `medium`), passed as the `modelId` parameter to `AddOpenAIAudioToText()`.

#### Scenario: Default model used when not configured
- **WHEN** `Whisper:Model` is not configured
- **THEN** the SK connector SHALL default to `medium` as the model identifier

#### Scenario: Custom model from configuration
- **WHEN** `Whisper:Model` is set to `whisper-large-v3`
- **THEN** the SK connector SHALL be configured with that model identifier

### Requirement: API key placeholder

The system SHALL pass a placeholder API key to the SK OpenAI connector, since FasterWhisper exposes an OpenAI-compatible API locally without authentication.

#### Scenario: Placeholder key used
- **WHEN** the SK `AddOpenAIAudioToText()` connector is registered
- **THEN** a non-empty placeholder string SHALL be used as the API key

### Requirement: Whisper connection failure handling

The system SHALL report a meaningful error when the Faster Whisper server is unreachable or the SK connector returns an error.

#### Scenario: Whisper server is not running
- **WHEN** the SK connector attempts to call the Whisper server but the connection is refused
- **THEN** the system SHALL return an HTTP 500 error with a message indicating the Whisper service is unavailable

#### Scenario: SK connector returns an error
- **WHEN** the SK audio-to-text connector throws an exception during transcription
- **THEN** the system SHALL return an HTTP 500 error with details from the exception
