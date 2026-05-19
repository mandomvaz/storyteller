## ADDED Requirements

### Requirement: Per-paragraph TTS via channel consumption

The `TextToAudioService` SHALL consume `TextUnit` objects from `Channel<TextUnit>` and produce `AudioUnit` objects to `Channel<AudioUnit>` via the SK `ITextToAudioService`.

#### Scenario: Each TextUnit produces an AudioUnit
- **WHEN** a `TextUnit` is read from `Channel<TextUnit>`
- **THEN** its text SHALL be passed to `ITextToAudioService.GetAudioContentAsync()` individually
- **AND** the resulting `AudioUnit` SHALL retain the original `JobId` and `ConnectionId`

## REMOVED Requirements

### Requirement: Audio response from stories endpoint

**Reason**: The endpoint no longer returns audio synchronously. Audio is now delivered via SignalR `"audioChunk"` events through the `AudioDeliveryService`.

**Migration**: Frontend must listen for `"audioChunk"` events instead of awaiting the POST response body.
