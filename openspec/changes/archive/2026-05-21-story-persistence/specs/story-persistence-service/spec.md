## ADDED Requirements

### Requirement: Cache story after generation

The system SHALL temporarily cache a completed `Story` in memory after badge generation, so it can be persisted later on user confirmation.

#### Scenario: Story cached with 10 minute TTL
- **WHEN** `CacheAsync(story)` is called
- **THEN** the story SHALL be stored in `IMemoryCache` keyed by `story.Id`
- **AND** the cache entry SHALL expire after 10 minutes
- **AND** the method SHALL return immediately without waiting for user confirmation

### Requirement: Persist story from cache

The system SHALL persist a story from cache to the SQLite repository when the user confirms.

#### Scenario: Story found in cache and persisted
- **WHEN** `PersistAsync(id)` is called with an id that exists in cache
- **THEN** the system SHALL retrieve the `Story` from cache
- **AND** call `IStoryRepository.SaveAsync(story)`
- **AND** remove the entry from cache
- **AND** return `true`

#### Scenario: Story not found in cache
- **WHEN** `PersistAsync(id)` is called with an id not in cache (expired or never cached)
- **THEN** the system SHALL return `false`

### Requirement: Get story from cache (for future listing)

The system SHALL expose cached stories for potential listing on the frontend (e.g., "pending stories awaiting save").

#### Scenario: Cached story retrieved
- **WHEN** `TryGetFromCache(id)` is called
- **THEN** the system SHALL return the `Story` if found in cache
- **AND** return `null` if not found
