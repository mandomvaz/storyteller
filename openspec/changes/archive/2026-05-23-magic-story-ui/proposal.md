# Proposal: Magical Child-Friendly UI & Under-Demand Model Warmup Loop

## Problem Statement

The StoryForge Single Page Application originally lacked a visual theme suitable for its primary target audience: a 4-year-old child who cannot read. Additionally, the backend pipelines faced significant cold-start latencies. Most notably:
1. **XTTS Model Unloading:** The XTTS Docker container automatically unloads its PyTorch checkpoints from GPU VRAM after exactly 300 seconds (5 minutes) of inactivity. A child starting a new story after a short break would experience a painful ~25-second delay for the first synthesized paragraph.
2. **Whisper Language Hallucinations:** Silent or noisy background recordings would occasionally lead Whisper to auto-detect Greek (`el`), resulting in Greek story generations and massive translation bottlenecks.

We need a completely text-free, highly engaging, tactile UI locked to a `7:5` tablet aspect ratio that incorporates an under-demand model preheating screen and a self-healing inactivity-reload system to ensure model weights are always warm in VRAM.

## Proposed Solution

1. **Magical Child-Friendly UI:** Rebuild the frontend using a vintage open magic book, inline vector SVGs, and responsive 3D click/touch buttons. Provide floating notes during playback and rising confetti hearts upon saving.
2. **Magical Warmup Blocker (`warming`):** Replace background warmup calls with a page-load blocking overlay screen. The child sees a gorgeous, text-free swirling magic portal and crystal ball animation. The book and record controls are revealed only after `/api/stories/warmup` returns `200 OK` (indicating XTTS weights are warm in VRAM).
3. **Inactivity Auto-Reload Guard:** Track user interactions globally. If the page is left idle for more than 5 minutes (300 seconds), any key action (like tapping the Microphone) will trigger a full page reload (`window.location.reload()`). This F5 bounce brings back the magical warming screen and preheats the VRAM, ensuring zero cold-start latencies during the child's story.

## Tech Stack & Abstractions
* **Frontend:** Alpine.js (for reactive state machine), Vanilla CSS with CSS Modules, hand-crafted inline vector SVGs.
* **Backend:** Minimal API `/api/stories/warmup` returning `200 OK` after executing `ttsService.GetAudioContentAsync("calentamiento")` to pre-warm XTTS.
* **Telemetry:** Stopwatch logging of Whisper, Ollama, XTTS, and badge pipelines.
