## ADDED Requirements

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
