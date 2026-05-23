# Tasks: Magical Child-Friendly UI & Warmup/Inactivity Implementation

This document lists the checklist of implemented tasks to deliver a magical UI, page-load blocking warmup, and self-healing inactivity-reload system.

- [x] **Backend Stream Lifetime Fix**
  - [x] Refactored `Program.cs` and `StoryPipelineRunner.cs` to copy audio files to isolated `MemoryStream` blocks.
  - [x] Resolved `ObjectDisposedException` when pipeline reads from disposed request threads on subsequent recordings.
- [x] **Telemetry & ASR Language Hallucination Resolution**
  - [x] Injected stopwatch diagnostic checkpoints throughout the audio pipeline (`Whisper`, `Ollama`, `XTTS`, `SK Badge`).
  - [x] Locked Whisper translation to `"es"` (Spanish) to speed up transcription and avoid auto-detection Greek hallucinations.
- [x] **Magical Child-Friendly UI Markup**
  - [x] Designed responsive aspect-ratio locked container (`7:5`) matching the OnePlusPad 3 resolution (3392*2400).
  - [x] Replaced generic frontend layout with a highly detailed hand-drawn vector inline SVG open magic book.
  - [x] Drafted glossy kid-friendly 3D buttons (offset borders, scale transformations on click/tap active states).
  - [x] Developed floating playback note emitter (`🎵`, `🎶`) rising out of the book's center crease during narration.
  - [x] Built the Save confirmation heartbeat heart button and Discard red trash button.
  - [x] Created the `celebration-overlay` with high-performance rising heart particles upon saving.
- [x] **Magical Warmup Blocker & Inactivity Reload**
  - [x] Developed dark-fantasy `.warmup-overlay` blocker with rotating outer star portal and pulsing crystal ball inline SVG.
  - [x] Initialized Alpine.js state in `'warming'`.
  - [x] Linked SignalR websocket connect callback with blocking `/api/stories/warmup` POST endpoint fetch.
  - [x] Configured window event listeners for clicks, touches, moves, and key presses to update `lastActivityTime`.
  - [x] Injected inactivity guard in `toggleRecording()` to reload page (`window.location.reload()`) if idle over 5 minutes.
  - [x] Cleaned up redundant trigger calls on recording start.
- [x] **Verification & Compilation**
  - [x] Compiled project successfully with `dotnet build` with 0 compile errors.
  - [x] Manually validated portal blocker on load, page reload on 5m inactivity, SignalR stream, and database saving.
