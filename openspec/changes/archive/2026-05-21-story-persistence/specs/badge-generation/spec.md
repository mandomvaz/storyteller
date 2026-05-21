## ADDED Requirements

### Requirement: Badge generation via Semantic Kernel

The system SHALL generate a 3-emojis badge for a story using a `KernelFunction` created via SK's `CreateFunctionFromPrompt`. The function SHALL receive the story title and full body text as arguments and return exactly 3 emojis.

#### Scenario: Badge generated from story content
- **WHEN** `BadgeService.GenerateBadgeAsync(story)` is called
- **THEN** the system SHALL invoke the injected `KernelFunction` with `title` and `body` arguments containing the story's title and full paragraph text
- **AND** the system SHALL return the result string (3 emojis)

#### Scenario: KernelFunction created from file on startup
- **WHEN** the application starts
- **THEN** the prompt file SHALL be read from the configured path (`BadgePrompt:Path`)
- **AND** a `KernelFunction` SHALL be created using `kernel.CreateFunctionFromPrompt(prompt, executionSettings)`
- **AND** the function SHALL be registered as a keyed singleton in DI for injection into `BadgeService`

#### Scenario: Prompt file not found uses default
- **WHEN** the prompt file does not exist at the configured path on startup
- **THEN** the system SHALL log a warning
- **AND** SHALL use a built-in default prompt: "Genera exactamente 3 emojis que representen el tono de esta historia. Devuelve solo los 3 emojis, nada más."

#### Scenario: Badge generation failure
- **WHEN** the Ollama server is unreachable or the model returns an error
- **THEN** the system SHALL propagate the exception to the caller
- **AND** the story SHALL still be cached (with empty badge) so pipeline does not block on badge failure
