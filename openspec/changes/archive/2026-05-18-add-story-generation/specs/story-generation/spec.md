## ADDED Requirements

### Requirement: Story generation from transcribed text

The system SHALL generate a children's story from transcribed audio text using the Ollama `storyteller` model, without any additional prompt. The model response SHALL contain the story title on the first line, followed by the story body.

#### Scenario: Story generated successfully
- **WHEN** the transcribed text is sent to the `storyteller` model
- **THEN** the system SHALL receive a text response containing a story

#### Scenario: Title extracted from first line
- **WHEN** the model returns a multi-line story
- **THEN** the system SHALL extract the first line as the title and keep the rest as the story body

#### Scenario: Story generation failure
- **WHEN** the Ollama server is unreachable or the model returns an error
- **THEN** the system SHALL propagate the exception to the caller

### Requirement: Story text sent to TTS

The system SHALL send the full story text (including the title) to the TTS service for audio generation, regardless of how the title is extracted.

#### Scenario: Full story sent to TTS
- **WHEN** the story is generated and the title is extracted
- **THEN** the complete story text SHALL be passed to `ITextToAudioService.GetAudioContentAsync`
