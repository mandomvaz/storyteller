## ADDED Requirements

### Requirement: Text-to-audio via dedicated scoped service

The system SHALL provide a `TextToAudioService` (scoped) that reads `TextUnit` objects from a `Channel<TextUnit>` and writes `AudioUnit` objects to a `Channel<AudioUnit>` using Semantic Kernel's `ITextToAudioService`.

#### Scenario: Service consumes TextUnits and produces AudioUnits
- **WHEN** a `TextUnit` is available in `Channel<TextUnit>`
- **THEN** `TextToAudioService` SHALL read it, call `ITextToAudioService.GetAudioContentAsync()`, and write the resulting `AudioUnit` to `Channel<AudioUnit>`

#### Scenario: Service completes audioChannel when textChannel completes
- **WHEN** `Channel<TextUnit>.Reader.ReadAllAsync()` completes (the writer called `Complete()`)
- **THEN** `TextToAudioService` SHALL call `Channel<AudioUnit>.Writer.Complete()` after all pending `TextUnit`s have been processed

#### Scenario: Error during TTS is reported via SignalR
- **WHEN** `ITextToAudioService.GetAudioContentAsync()` throws an exception
- **THEN** `TextToAudioService` SHALL send an `"error"` event via SignalR with `{ jobId, connectionId, step: "tts", message }` and stop processing the current job

### Requirement: Audio delivery via dedicated scoped service

The system SHALL provide an `AudioDeliveryService` (scoped) that reads `AudioUnit` objects from a `Channel<AudioUnit>` and delivers them to the browser via SignalR.

#### Scenario: Each AudioUnit delivered as audioChunk
- **WHEN** an `AudioUnit` is available in `Channel<AudioUnit>`
- **THEN** `AudioDeliveryService` SHALL send an `"audioChunk"` event to the client via SignalR containing the raw binary audio data (MessagePack)

#### Scenario: storyComplete sent when channel completes
- **WHEN** `Channel<AudioUnit>.Reader.ReadAllAsync()` completes
- **THEN** `AudioDeliveryService` SHALL send a `"storyComplete"` event to the client via SignalR

#### Scenario: Error during delivery is reported via SignalR
- **WHEN** sending an event via SignalR fails
- **THEN** `AudioDeliveryService` SHALL send an `"error"` event via SignalR with `{ jobId, connectionId, step: "delivery", message }`

### Requirement: Channels registered as scoped dependencies

The system SHALL register `Channel<TextUnit>` and `Channel<AudioUnit>` as scoped services in the DI container.

#### Scenario: Channels are scoped
- **WHEN** a new DI scope is created for each PipelineJob
- **THEN** the container SHALL provide a fresh `Channel<TextUnit>` and `Channel<AudioUnit>` to all services within that scope

#### Scenario: Single reader per channel
- **WHEN** registering channels
- **THEN** `Channel<TextUnit>` SHALL have `SingleReader = true` (consumed only by `TextToAudioService`)
- **AND** `Channel<AudioUnit>` SHALL have `SingleReader = true` (consumed only by `AudioDeliveryService`)
