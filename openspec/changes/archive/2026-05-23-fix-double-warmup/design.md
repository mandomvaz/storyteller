## Context

On load or reload, the frontend performs duplicate initializations, triggering two parallel SignalR connections and two separate HTTP POST requests to `/api/stories/warmup`. Concurrently, the backend minimum API processes these requests independently since each injection scope retrieves `ITextToAudioService` and invokes the model warmup, causing resource waste and double-execution logs.

## Goals / Non-Goals

**Goals:**
- Eliminate the double-warmup request issue completely on the frontend.
- Protect the backend from any concurrent or duplicate warmup requests using a thread-safe Singleton coordinative pattern.
- Ensure that multiple concurrent warmup requests wait for the same initial operation to finish and all return successfully with a `200 OK` (or appropriate error if failed).

**Non-Goals:**
- Modifying how the actual audio generation works or altering the `ITextToAudioService` core implementation.
- Redesigning the SignalR hub or pipeline flow itself.

## Decisions

### 1. Fix frontend Alpine.js double `init()` execution
- **Option A:** Rename Alpine.js's component method `init` to something else (e.g., `setup` or `activate`).
- **Option B:** Remove the explicit `x-init="init()"` attribute in `index.html`.
- **Decision:** **Option B**. Alpine.js automatically executes a function named `init` if it is present in the `x-data` object on initialization. Explicitly defining `x-init="init()"` causes Alpine.js to call `init()` a second time. Removing the explicit attribute leaves Alpine.js to call it automatically exactly once, preserving standard clean semantics.

### 2. Implement a Thread-Safe Singleton Warmup Service
- **Option A:** Put thread-safe locking and task storage directly in the `/api/stories/warmup` endpoint handler.
- **Option B:** Define a custom Singleton service `StoryWarmupService` injected into the endpoint.
- **Decision:** **Option B**. Encapsulating the thread safety and state management in a dedicated `StoryWarmupService` ensures Single Responsibility Principle (SRP) and separation of concerns. In ASP.NET Core, minimal API lambda handlers should remain lightweight. Injected as a Singleton, `StoryWarmupService` can cleanly hold the task state across scoped requests.

### 3. Adjust Inactivity Reload Threshold
- **Decision:** Reduce the inactivity reload threshold in the frontend from 5 minutes to 4 minutes (`4 * 60 * 1000` ms). This ensures that if a user is inactive, the client reloads the page and triggers a preheating cycle slightly earlier to keep weights warm in VRAM.

**Technical Implementation Details:**
Inside `StoryWarmupService`:
```csharp
private readonly ITextToAudioService _ttsService;
private readonly object _lock = new();
private Task? _warmupTask;
private bool _warmedUpSuccessfully;

public Task WarmupAsync()
{
    lock (_lock)
    {
        if (_warmedUpSuccessfully)
        {
            return Task.CompletedTask;
        }

        if (_warmupTask == null || _warmupTask.IsFaulted || _warmupTask.IsCanceled)
        {
            _warmupTask = RunWarmupInternalAsync();
        }

        return _warmupTask;
    }
}

private async Task RunWarmupInternalAsync()
{
    await _ttsService.GetAudioContentAsync("calentamiento");
    _warmedUpSuccessfully = true;
}
```

## Risks / Trade-offs

- **[Risk] Warmup fails permanently due to temporary XTTS server outage** → **Mitigation:** If the task fails, it transitions to `IsFaulted`. The locking block allows creating a *new* task on subsequent requests if the previous task faulted or was cancelled.
- **[Risk] Thread locking locks request threads** → **Mitigation:** The lock is synchronous but extremely fast (only checks state and returns/starts a `Task`). It doesn't block asynchronous execution since the actual model preheating runs inside an un-awaited `Task` or asynchronous C# task returned to all callers.
