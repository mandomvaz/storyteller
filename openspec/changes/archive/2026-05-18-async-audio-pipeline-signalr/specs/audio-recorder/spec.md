## MODIFIED Requirements

### Requirement: Audio recording button

The main page SHALL display a microphone button that controls voice recording and playback states. The button SHALL be disabled while audio is being sent, being processed, or during playback.

#### Scenario: Button shows in idle state
- **WHEN** the page loads
- **THEN** the microphone button SHALL be visible in idle (not recording) state
- **AND** the button SHALL be enabled

#### Scenario: Click starts recording
- **WHEN** the user clicks the microphone button in idle state
- **THEN** the system SHALL request microphone permission and start recording audio

#### Scenario: Recording state is indicated
- **WHEN** recording is active
- **THEN** the button SHALL visually change to indicate recording state

#### Scenario: Click during recording stops and sends
- **WHEN** the user clicks the button while recording
- **THEN** the system SHALL stop recording and upload the audio data to `/api/stories/new` including the SignalR connection ID

#### Scenario: Microphone permission denied shown
- **WHEN** the user denies microphone permission
- **THEN** an error message SHALL be displayed instructing the user to enable microphone access

#### Scenario: Loading state during upload
- **WHEN** audio is being uploaded
- **THEN** the button SHALL show a loading/spinner state

#### Scenario: Button disabled during pipeline processing
- **WHEN** the POST returns successfully
- **THEN** the button SHALL remain in loading state and disabled until the pipeline completes and audio arrives via SignalR

#### Scenario: Audio playback disables button
- **WHEN** audio data is received via SignalR `audioReady` event
- **THEN** the button SHALL switch to a playing state
- **AND** the button SHALL be disabled
- **AND** the system SHALL play the audio through the browser's audio element

#### Scenario: Button re-enables after playback ends
- **WHEN** the audio playback finishes
- **THEN** the button SHALL return to idle state and become enabled

#### Scenario: Pipeline error returns button to idle
- **WHEN** a pipeline error event is received via SignalR
- **THEN** the button SHALL return to idle state and become enabled
- **AND** an error message SHALL be displayed indicating which step failed

#### Scenario: Success feedback during playback
- **WHEN** the audio playback starts
- **THEN** a brief "playing..." indicator SHALL be visible
