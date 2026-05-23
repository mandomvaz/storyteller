## Why

On page load or reload, the magic storytelling frontend launches two concurrent warmup requests to `/api/stories/warmup`. This double call causes duplicate backend operations, redundant logs, and wastes system resources. Fixes are needed both on the frontend to address the root cause, and on the backend to elegantly coordinate and de-duplicate concurrent warmup requests.

## What Changes

- **Frontend (Alpine.js):** Correct the double-initialization root cause where Alpine.js's auto-magic `init()` execution and the explicit `x-init="init()"` attribute combination trigger the initialization sequence (and thus SignalR and warmup API requests) twice. Also adjust the inactivity reload threshold from 5 minutes to 4 minutes to ensure the XTTS checkpoint is preheated slightly sooner.
- **Backend (ASP.NET Core):** Implement a thread-safe, Singleton-coordinated `StoryWarmupService` that intercepts concurrent warmup requests. If a warmup is already in progress, the service will prevent launching a duplicate task, and await the ongoing task, returning a successful `200 OK` to all concurrent and subsequent callers.

## Capabilities

### New Capabilities
<!-- Capabilities being introduced. Replace <name> with kebab-case identifier (e.g., user-auth, data-export, api-rate-limiting). Each creates specs/<name>/spec.md -->

### Modified Capabilities
<!-- Existing capabilities whose REQUIREMENTS are changing (not just implementation).
     Only list here if spec-level behavior changes. Each needs a delta spec file.
     Use existing spec names from openspec/specs/. Leave empty if no requirement changes. -->
- `audio-recorder`: Prevent duplicate frontend warmup invocations, adjust inactivity reload threshold to 4 minutes, and optimize API endpoint resiliency.

## Impact

- **Frontend:** `wwwroot/index.html` (Alpine.js component binding updated).
- **Backend:** `Program.cs` (DI and endpoint mapping updated), and a new Singleton service `Services/StoryWarmupService.cs`.
