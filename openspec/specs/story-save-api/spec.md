## ADDED Requirements

### Requirement: Save story endpoint

The system SHALL provide a `POST /api/stories/{id}/save` endpoint that persists a cached story to the database.

#### Scenario: Story saved successfully
- **WHEN** a POST request is sent to `/api/stories/{id}/save` with a valid story id that exists in cache
- **THEN** the system SHALL persist the story to SQLite
- **AND** return HTTP 200 with `{ "saved": true }`

#### Scenario: Story not found (TTL expired)
- **WHEN** a POST request is sent to `/api/stories/{id}/save` with a story id not in cache
- **THEN** the system SHALL return HTTP 404 with `{ "error": "Story not found or expired" }`
