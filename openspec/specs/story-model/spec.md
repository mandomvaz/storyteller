## ADDED Requirements

### Requirement: Story entity

The system SHALL define a `Story` entity with the following fields: `Id` (Guid), `Title` (string), `Badge` (string, 3 emojis), `Paragraphs` (`List<string>`), and `CreatedAt` (DateTime).

#### Scenario: Story created with default values
- **WHEN** a new `Story` instance is created
- **THEN** `Id` SHALL be a new Guid
- **AND** `CreatedAt` SHALL be `DateTime.UtcNow`
- **AND** `Title` SHALL be empty string
- **AND** `Badge` SHALL be empty string
- **AND** `Paragraphs` SHALL be an empty list

#### Scenario: Paragraphs serialized to JSON
- **WHEN** a `Story` is stored in SQLite
- **THEN** `Paragraphs` SHALL be serialized to a JSON array string (e.g., `["p1","p2"]`)
- **AND** when retrieved from SQLite, the JSON SHALL be deserialized back to `List<string>`

#### Scenario: CreatedAt stored as ISO 8601
- **WHEN** a `Story` is stored in SQLite
- **THEN** `CreatedAt` SHALL be serialized as ISO 8601 string (`yyyy-MM-ddTHH:mm:ssZ`)
