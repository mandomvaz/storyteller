## ADDED Requirements

### Requirement: Configurable Faster Whisper endpoint

The system SHALL read the Faster Whisper server URL from configuration under the `Whisper:Endpoint` key with a default value of `http://localhost:8000`.

#### Scenario: Default endpoint used when not configured
- **WHEN** the configuration key `Whisper:Endpoint` is not present
- **THEN** the system SHALL use `http://localhost:8000` as the default endpoint

#### Scenario: Custom endpoint from configuration
- **WHEN** the configuration key `Whisper:Endpoint` is set to `http://10.0.0.5:9000`
- **THEN** the system SHALL use the configured value when connecting to the Whisper server

### Requirement: Configurable Whisper model name

The system SHALL read the Whisper model identifier from configuration under `Whisper:Model` (default `whisper`), sent as the `model` field in transcription requests.

#### Scenario: Default model used when not configured
- **WHEN** `Whisper:Model` is not configured
- **THEN** the system SHALL default to `whisper` as the model identifier

#### Scenario: Custom model from configuration
- **WHEN** `Whisper:Model` is set to `whisper-large-v3`
- **THEN** the system SHALL send that model identifier in transcription requests

### Requirement: Configurable HTTP timeout

The system SHALL read a timeout value from configuration under `Whisper:TimeoutSeconds` (default `60`) and apply it to HTTP requests to the Whisper server.

#### Scenario: Default timeout used when not configured
- **WHEN** `Whisper:TimeoutSeconds` is not configured
- **THEN** the system SHALL use 60 seconds as the HTTP request timeout

#### Scenario: Custom timeout from configuration
- **WHEN** `Whisper:TimeoutSeconds` is set to `120`
- **THEN** the system SHALL use 120 seconds as the HTTP request timeout

### Requirement: Whisper connection failure handling

The system SHALL report a meaningful error when the Faster Whisper server is unreachable or returns an error.

#### Scenario: Whisper server is not running
- **WHEN** a request attempts to call the Whisper server but the connection is refused
- **THEN** the system SHALL return an HTTP 500 error with a message indicating the Whisper service is unavailable

#### Scenario: Whisper server returns an error response
- **WHEN** the Whisper server returns a non-success HTTP status
- **THEN** the system SHALL return an HTTP 500 error with details from the Whisper response
