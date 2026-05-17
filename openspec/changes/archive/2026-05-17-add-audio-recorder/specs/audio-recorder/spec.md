## ADDED Requirements

### Requirement: Audio recording button

The main page SHALL display a microphone button that controls voice recording.

#### Scenario: Button shows in idle state
- **WHEN** the page loads
- **THEN** the microphone button SHALL be visible in idle (not recording) state

#### Scenario: Click starts recording
- **WHEN** the user clicks the microphone button
- **THEN** the system SHALL request microphone permission and start recording audio

#### Scenario: Recording state is indicated
- **WHEN** recording is active
- **THEN** the button SHALL visually change to indicate recording state (e.g., pulsing animation, color change)

#### Scenario: Click during recording stops and sends
- **WHEN** the user clicks the button while recording
- **THEN** the system SHALL stop recording and upload the audio data to `/api/stories/new`

#### Scenario: Microphone permission denied shown
- **WHEN** the user denies microphone permission
- **THEN** an error message SHALL be displayed instructing the user to enable microphone access

#### Scenario: Upload progress feedback
- **WHEN** audio is being uploaded
- **THEN** the button SHALL show a loading/spinner state

#### Scenario: Success feedback
- **WHEN** the upload completes successfully
- **THEN** a success indicator SHALL be shown briefly, then the button returns to idle state

#### Scenario: Upload error feedback
- **WHEN** the upload fails
- **THEN** an error message SHALL be displayed and the button returns to idle state
