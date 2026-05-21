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

### Requirement: Story object captured after generation

After story generation completes, the pipeline SHALL capture the returned `Story` object and pass it through badge generation and caching steps before the pipeline scope ends.

#### Scenario: Story passed to BadgeService after generation
- **WHEN** `GenerateStoryFromTextAsync` completes successfully
- **THEN** the pipeline SHALL call `BadgeService.GenerateBadgeAsync(story)` with the captured `Story`
- **AND** the `Story.Badge` SHALL be set to the returned emojis

#### Scenario: Story cached after badge generation
- **WHEN** badge generation completes (success or failure)
- **THEN** the pipeline SHALL call `PersistenceService.CacheAsync(story)` with the `Story` (badge may be empty if generation failed)
- **AND** this SHALL NOT block the concurrent TTS and delivery tasks
