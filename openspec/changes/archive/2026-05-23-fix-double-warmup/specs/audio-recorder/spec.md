## MODIFIED Requirements

### Requirement: Magical Audio Recording, Playback, and Warmup Control

The main page SHALL display an immersive vintage open magic book that acts as the container for storytelling. It SHALL utilize a hand-crafted inline vector SVG design locked to a `7:5` aspect ratio. The system SHALL display interactive buttons over the book crease to control recording, playback, saving, and preheating states.

#### Scenario: Warmup screen blocks access on load
- **WHEN** the page loads
- **THEN** the system SHALL display a magical warmup overlay (`.warmup-overlay`) covering the entire book wrapper
- **AND** the overlay SHALL present a rotating outer star portal and a pulsing crystal ball vector SVG animation
- **AND** the overlay SHALL NOT display any written text
- **AND** all book interaction buttons SHALL be hidden or disabled
- **AND** Alpine.js components SHALL initialize cleanly, ensuring that initialization (`init()`) executes exactly once per page load to prevent duplicate requests

#### Scenario: Preheating successfully reveals the book
- **WHEN** the SignalR connection establishes successfully
- **THEN** the frontend SHALL issue exactly one blocking HTTP POST request to `/api/stories/warmup`
- **WHEN** the request returns a successful `200 OK` (indicating XTTS checkpoints are loaded in VRAM)
- **THEN** the warmup overlay SHALL fade out smoothly (`x-transition`)
- **AND** the open magic book and active Microphone button SHALL be revealed in idle state
- **AND** the last activity timestamp (`lastActivityTime`) SHALL be initialized to `Date.now()`

#### Scenario: Concurrent warmup requests are de-duplicated by the backend
- **WHEN** the backend receives multiple concurrent HTTP POST requests to `/api/stories/warmup`
- **THEN** it SHALL execute the underlying preheating sequence exactly once
- **AND** it SHALL return `200 OK` to all concurrent callers once the single preheating operation completes successfully

#### Scenario: User activity tracks silently
- **WHEN** the user clicks, touches, moves the cursor, or presses keys on the page
- **THEN** the global `lastActivityTime` variable SHALL update immediately to `Date.now()`
- **AND** the application SHALL continue running normally without reloading

#### Scenario: Inactivity over 4 minutes triggers page reload (F5)
- **WHEN** more than 4 minutes (240 seconds) pass without any registered user interaction
- **AND** the user taps the Microphone button (or any interactive trigger)
- **THEN** the frontend SHALL execute a clean page reload (`window.location.reload()`)
- **AND** the browser SHALL re-enter the page-load warmup blocker state to pre-warm the XTTS checkpoints in VRAM
