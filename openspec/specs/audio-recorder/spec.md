## MODIFIED Requirements

### Requirement: Magical Audio Recording, Playback, and Warmup Control

The main page SHALL display an immersive vintage open magic book that acts as the container for storytelling. It SHALL utilize a hand-crafted inline vector SVG design locked to a `7:5` aspect ratio. The system SHALL display interactive buttons over the book crease to control recording, playback, saving, and preheating states.

---

### Part 1: Warmup & Initial Page Load

#### Scenario: Warmup screen blocks access on load
- **WHEN** the page loads
- **THEN** the system SHALL display a magical warmup overlay (`.warmup-overlay`) covering the entire book wrapper
- **AND** the overlay SHALL present a rotating outer star portal and a pulsing crystal ball vector SVG animation
- **AND** the overlay SHALL NOT display any written text
- **AND** all book interaction buttons SHALL be hidden or disabled

#### Scenario: Preheating successfully reveals the book
- **WHEN** the SignalR connection establishes successfully
- **THEN** the frontend SHALL issue a blocking HTTP POST request to `/api/stories/warmup`
- **WHEN** the request returns a successful `200 OK` (indicating XTTS checkpoints are loaded in VRAM)
- **THEN** the warmup overlay SHALL fade out smoothly (`x-transition`)
- **AND** the open magic book and active Microphone button SHALL be revealed in idle state
- **AND** the last activity timestamp (`lastActivityTime`) SHALL be initialized to `Date.now()`

---

### Part 2: Recording and Stop Actions

#### Scenario: Click Microphone starts recording
- **WHEN** the page is in idle state
- **AND** the user clicks the Microscope button
- **THEN** the system SHALL check microphone permissions
- **AND** start recording the audio stream
- **AND** the button SHALL visually transform into a crimson stop button (`.btn-stop`) with a pulsing wave ring

#### Scenario: Click Stop stops and uploads recording
- **WHEN** recording is active
- **AND** the user clicks the Stop button
- **THEN** the system SHALL terminate the audio stream
- **AND** upload the webm audio payload to `/api/stories/new` including the connection ID
- **AND** transition to waiting state

---

### Part 3: Waiting and Playback States

#### Scenario: Waiting star rotates during pipeline execution
- **WHEN** the audio is being processed (Ollama generating the story paragraphs, XTTS converting them to WAV)
- **THEN** the central control SHALL transform into a disabled golden spinning magic star (`.btn-wait`)
- **AND** the star SHALL spin clockwise to entertain the child

#### Scenario: Playback wiggles button and emits notes
- **WHEN** the first audio chunk is received via SignalR
- **THEN** the central control SHALL transform into a playing note indicator (`.btn-play`)
- **AND** the button SHALL wiggle playfully (`@keyframes dancePlay`)
- **AND** pastel music notes (`🎵`, `🎶`) SHALL emerge from the book crease and float upwards

---

### Part 4: Decision and Persistence States

#### Scenario: Completed story displays Save/Discard options
- **WHEN** all story paragraphs have finished playing
- **AND** the SignalR `storyComplete` event is received
- **THEN** the central play note SHALL fade out
- **AND** the system SHALL present two slide-in options:
  - A green heartbeat Heart button (`.btn-yes`) on the left page (Save)
  - A wiggling red Trash button (`.btn-no`) on the right page (Discard)

#### Scenario: Confirming Save triggers celebration
- **WHEN** the user taps the green Heart button
- **THEN** the frontend SHALL make a POST request to `/api/stories/{jobId}/save`
- **AND** trigger an explosion of rising heart particles (`.confetti-heart`) from the bottom of the screen
- **AND** return cleanly to idle state after 3 seconds

#### Scenario: Discard wiggles out and resets
- **WHEN** the user taps the red Trash button
- **THEN** the options SHALL fade out
- **AND** the system SHALL reset directly to idle state without making save calls

---

### Part 5: Inactivity Guard & Self-Healing Auto-Reload

#### Scenario: User activity tracks silently
- **WHEN** the user clicks, touches, moves the cursor, or presses keys on the page
- **THEN** the global `lastActivityTime` variable SHALL update immediately to `Date.now()`
- **AND** the application SHALL continue running normally without reloading

#### Scenario: Inactivity over 5 minutes triggers page reload (F5)
- **WHEN** more than 5 minutes (300 seconds) pass without any registered user interaction
- **AND** the user taps the Microphone button (or any interactive trigger)
- **THEN** the frontend SHALL execute a clean page reload (`window.location.reload()`)
- **AND** the browser SHALL re-enter the page-load warmup blocker state to pre-warm the XTTS checkpoints in VRAM
