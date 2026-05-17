### Requirement: Configurable XTTS endpoint

The system SHALL read the XTTS v2 server URL from configuration under the `Xtts:Endpoint` key with a default value of `http://localhost:8020`, and construct an `OpenAIClient` pointing to that endpoint with the `/v1` suffix appended.

#### Scenario: Default endpoint used when not configured
- **WHEN** the configuration key `Xtts:Endpoint` is not present
- **THEN** the `OpenAIClient` SHALL use `http://localhost:8020/v1` as the base URL

#### Scenario: Custom endpoint from configuration
- **WHEN** the configuration key `Xtts:Endpoint` is set to `http://10.0.0.5:9020`
- **THEN** the `OpenAIClient` SHALL be configured with `http://10.0.0.5:9020/v1` as the base URL

### Requirement: Configurable XTTS model name

The system SHALL read the TTS model identifier from configuration under `Xtts:Model` (default `tts-1`), passed as the `modelId` parameter when creating the `AudioClient`.

#### Scenario: Default model used when not configured
- **WHEN** `Xtts:Model` is not configured
- **THEN** the system SHALL default to `tts-1` as the model identifier

#### Scenario: Custom model from configuration
- **WHEN** `Xtts:Model` is set to `tts-1-hd`
- **THEN** the `AudioClient` SHALL be created with that model identifier

### Requirement: Configurable XTTS voice

The system SHALL read the TTS voice name from configuration under `Xtts:Voice` (default `alloy`), passed as the `Voice` parameter when generating speech.

#### Scenario: Default voice used when not configured
- **WHEN** `Xtts:Voice` is not configured
- **THEN** the system SHALL default to `alloy` as the voice

#### Scenario: Custom voice from configuration
- **WHEN** `Xtts:Voice` is set to `coral`
- **THEN** the system SHALL use `coral` as the voice for speech generation

### Requirement: API key placeholder

The system SHALL pass a placeholder API key to the `OpenAIClient`, since XTTS v2 exposes an OpenAI-compatible API locally without authentication.

#### Scenario: Placeholder key used
- **WHEN** the `OpenAIClient` is created for the XTTS endpoint
- **THEN** a non-empty placeholder string SHALL be used as the API key
