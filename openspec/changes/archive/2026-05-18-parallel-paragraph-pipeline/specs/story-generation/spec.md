## MODIFIED Requirements

### Requirement: Story text sent to TTS

The system SHALL send each paragraph of the story text (including the title as the first paragraph) as an individual `TextUnit` to `Channel<TextUnit>` for per-paragraph audio generation.

#### Scenario: Each paragraph written as a TextUnit
- **WHEN** a paragraph boundary is detected (double newline `\n\n`) during story generation streaming
- **THEN** the system SHALL write a `TextUnit` with the paragraph text to `Channel<TextUnit>`
- **AND** the first `TextUnit` SHALL contain the title

#### Scenario: Writing continues until stream ends
- **WHEN** the Ollama streaming response is complete
- **THEN** the system SHALL call `Channel<TextUnit>.Writer.Complete()` to signal no more paragraphs

#### Scenario: Story generation failure
- **WHEN** the Ollama server is unreachable or the model returns an error
- **THEN** the system SHALL send an `"error"` event via SignalR with `{ jobId, connectionId, step: "story", message }`
