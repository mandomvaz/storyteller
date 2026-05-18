## Context

The app currently processes audio synchronously: `POST /api/stories/new` blocks until Whisper transcribes, Ollama generates a story, and XTTS produces speech audio (~20-40s total). The frontend shows a generic "loading" spinner with no progress feedback. If the pipeline fails at any step, the user gets a 500 error with no indication of what went wrong.

This design introduces an asynchronous pipeline using `System.Threading.Channels` for in-memory job queuing, `BackgroundService` for processing, and SignalR with MessagePack for real-time status events. The frontend's state machine expands from 5 states to handle the async flow.

## Goals / Non-Goals

**Goals:**
- `POST /api/stories/new` returns immediately (200) after audio validation
- Background worker processes: transcribe → generate story → generate audio
- SignalR pushes real-time step progress to the frontend
- Audio delivered as binary via MessagePack protocol
- Button disabled during recording AND during playback (enabled only when idle)
- Per-step error granularity: if transcription fails, the user sees "transcription failed" not a generic server error
- MessagePack protocol for efficient binary transfer of audio data

**Non-Goals:**
- Parallel pipeline execution (remains sequential for now)
- Job persistence or history (no database, in-memory only)
- Job cancellation on disconnect
- Multiple concurrent user support (assumes single user, no user identity)
- Story content saved when audio generation fails (loss acceptable)
- SSE or polling alternatives (SignalR chosen deliberately)

## Decisions

**SignalR over SSE**: SSE is simpler (native EventSource API, no library), but requires a `while(true)` loop pinned to an HTTP connection — fragile, untestable, and blocks a thread. SignalR provides a clean hub abstraction, proper connection lifecycle, and the bidirectional channel is ready if we later need cancel/control commands. SignalR also handles reconnection, transport fallback, and scale-out backplanes natively.

**MessagePack over JSON**: Audio data as base64 in JSON adds ~33% overhead and parsing cost. MessagePack sends raw binary (`ArrayBuffer` on JS side) with zero encoding overhead. Once we parallelize XTTS per-paragraph, multiple audio chunks will flow through this same channel — MessagePack makes that efficient from day one.

**`Channel<Job>` over `ConcurrentQueue`**: `Channel<T>` provides backpressure-aware async reading (`ReadAsync` blocks until data available), bounded/unbounded capacity control, and completion semantics — exactly what a producer-consumer pipeline needs. `ConcurrentQueue` would require manual signaling with `SemaphoreSlim` or similar.

**`BackgroundService` over direct `IHostedService`**: `BackgroundService` provides `ExecuteAsync` with a structured `StopAsync`/`cancellationToken` pattern. The base class handles startup/shutdown lifecycle correctly. Direct `IHostedService` requires manual `Task.Run` management.

**Three separate try/catch blocks over one wrapper**: Each pipeline step (transcribe, story, audio) gets its own try/catch so the error event identifies exactly which step failed. A single wrapper would require parsing exception messages to determine the failing step.

**No job cancellation on disconnect**: The worker runs to completion regardless of client disconnection. If the client reconnects (e.g., refreshes the page), the audio is lost — acceptable for a single-user domestic app. A future optimization could cache the last result keyed by connection ID.

## Risks / Trade-offs

| Risk | Mitigation |
|---|---|
| SignalR CDN unavailable | Pin a specific version, consider bundling into wwwroot |
| MessagePack needs two CDN scripts | Same mitigation — pin versions, bundle if offline matters |
| Browser closes tab mid-playback | `audio.onended` fires normally; button resets. If tab closes, connection drops, worker still finishes |
| XTTS generates 0-byte audio | Caught by the audio step's try/catch, sent as `error` event with `step: "audio"` |
| Channel grows unbounded if production faster than consumption | For single-user with sequential pipeline, backpressure from the worker naturally throttles the producer. If needed later, `Channel.CreateBounded<T>(capacity)` |
| SignalR connection negotiation adds latency to page load | Negotiation is a single lightweight HTTP round-trip (~50ms). Negligible |
