## ADDED Requirements

### Requirement: Story repository initialization

The system SHALL create the SQLite database and the `Stories` table on application startup if they do not already exist.

#### Scenario: Database file created on startup
- **WHEN** the application starts
- **THEN** the system SHALL create the SQLite database file at the configured path if it does not exist
- **AND** the system SHALL execute `CREATE TABLE IF NOT EXISTS Stories`

#### Scenario: Table schema
- **WHEN** the `Stories` table is created
- **THEN** it SHALL have columns: `Id TEXT PRIMARY KEY`, `Title TEXT NOT NULL`, `Badge TEXT NOT NULL DEFAULT ''`, `Paragraphs TEXT NOT NULL DEFAULT '[]'`, `CreatedAt TEXT NOT NULL`

### Requirement: Save story to repository

The system SHALL persist a `Story` to the SQLite database.

#### Scenario: Story saved successfully
- **WHEN** `SaveAsync(story)` is called
- **THEN** a new row SHALL be inserted into the `Stories` table with all fields mapped

### Requirement: List story summaries

The system SHALL return a list of story summaries, each containing only `Id` (Guid) and `Badge` (string).

#### Scenario: Stories listed
- **WHEN** `GetAllAsync()` is called
- **THEN** the system SHALL return `List<StorySummary>` where each `StorySummary` contains `Id` and `Badge`
- **AND** results SHALL be ordered by `CreatedAt` descending

### Requirement: Get story by id

The system SHALL retrieve a full `Story` by its `Id`.

#### Scenario: Story found
- **WHEN** `GetByIdAsync(id)` is called with an existing story id
- **THEN** the system SHALL return the `Story` with all fields populated, including deserialized `Paragraphs`

#### Scenario: Story not found
- **WHEN** `GetByIdAsync(id)` is called with a non-existent story id
- **THEN** the system SHALL return `null`
