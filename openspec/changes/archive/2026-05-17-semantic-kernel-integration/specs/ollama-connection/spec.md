## ADDED Requirements

### Requirement: Configurable Ollama endpoint

The system SHALL read the Ollama server URL from configuration under the `Ollama:Endpoint` key with a default value of `http://localhost:11434`.

#### Scenario: Default endpoint used when not configured
- **WHEN** the configuration key `Ollama:Endpoint` is not present
- **THEN** the system SHALL use `http://localhost:11434` as the default endpoint

#### Scenario: Custom endpoint from configuration
- **WHEN** the configuration key `Ollama:Endpoint` is set to `http://192.168.1.100:11434`
- **THEN** the system SHALL use the configured value when connecting to Ollama

### Requirement: Configurable text model name

The system SHALL read the text model name from configuration under `Ollama:TextModel` (default `llama3.2`), used by Semantic Kernel for text-based AI interactions.

#### Scenario: Default text model used when not configured
- **WHEN** `Ollama:TextModel` is not configured
- **THEN** the system SHALL default to `llama3.2` for text generation

#### Scenario: Custom text model from configuration
- **WHEN** `Ollama:TextModel` is set to `mistral`
- **THEN** the system SHALL use that model name when creating the Semantic Kernel Ollama connector

### Requirement: Ollama connection failure handling

The system SHALL report a meaningful error when Ollama is unreachable or returns an error during SK initialization or usage.

#### Scenario: Ollama server is not running
- **WHEN** a request attempts to call Ollama but the connection is refused
- **THEN** the system SHALL return an HTTP 500 error with a message indicating Ollama is unavailable

#### Scenario: Ollama returns an error response
- **WHEN** Ollama returns a non-success HTTP status
- **THEN** the system SHALL return an HTTP 500 error with details from the Ollama response

### Requirement: Semantic Kernel registration with Ollama

The system SHALL register a Semantic Kernel instance in DI with the Ollama chat completion service pointing to the configured `TextModel`.

#### Scenario: Kernel available in DI
- **WHEN** the application starts
- **THEN** a `Kernel` instance SHALL be registered in DI with `AddOllamaChatCompletionService` using the configured `Ollama:Endpoint` and `Ollama:TextModel`

#### Scenario: Kernel injectable into services
- **WHEN** a service constructor accepts a `Kernel` parameter
- **THEN** DI SHALL resolve the registered kernel instance
