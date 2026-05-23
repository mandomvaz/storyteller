## 1. Frontend: Fix Alpine.js Initialization

- [x] 1.1 Locate the `<div x-data="audioRecorder()" x-init="init()"` tag in `wwwroot/index.html`
- [x] 1.2 Remove the redundant `x-init="init()"` attribute to prevent Alpine.js from invoking `init()` twice
- [x] 1.3 Locate the inactivity check in `wwwroot/index.html` inside `toggleRecording()` (currently `5 * 60 * 1000` ms) and change it to `4 * 60 * 1000` ms (4 minutes)
- [x] 1.4 Verify that the application initializes cleanly, only one hub connection/warmup request is dispatched upon reload, and inactivity triggers reload after 4 minutes

## 2. Backend: Implement thread-safe Singleton Warmup Service

- [x] 2.1 Create the new Singleton service `StoryWarmupService` inside `Services/StoryWarmupService.cs`
- [x] 2.2 Implement a thread-safe deduplication pattern inside `StoryWarmupService.WarmupAsync()` using C# locking and cached Task sharing
- [x] 2.3 Register the `StoryWarmupService` as a Singleton service in `Program.cs`
- [x] 2.4 Inject `StoryWarmupService` into the `/api/stories/warmup` Minimal API endpoint and update its call to use `warmupService.WarmupAsync()`
- [x] 2.5 Run the backend build and test suite to ensure compilation succeeds and that simultaneous requests to the endpoint return cleanly
