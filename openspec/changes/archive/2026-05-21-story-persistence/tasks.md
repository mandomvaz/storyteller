## 1. Model and NuGet

- [x] 1.1 Add `Microsoft.Data.Sqlite` NuGet package to `storyforge.csproj`
- [x] 1.2 Extend `Story` model: add `Id` (Guid), `Badge` (string), `CreatedAt` (DateTime), `Paragraphs` JSON serialization

## 2. Configuration

- [x] 2.1 Add `Database:Path` and `BadgePrompt:Path` and `BadgePrompt:ReloadIntervalSeconds` to `appsettings.json`
- [x] 2.2 Create `BadgePromptSettings` and `DatabaseSettings` POCOs for strongly-typed config binding

## 3. SQLite Repository

- [x] 3.1 Create `IStoryRepository` interface with `SaveAsync`, `GetAllAsync`, `GetByIdAsync`, `InitDatabaseAsync`
- [x] 3.2 Create `StorySummary` record with `Id` (Guid) and `Badge` (string)
- [x] 3.3 Implement `SqliteStoryRepository`: `InitDatabaseAsync` (CREATE TABLE IF NOT EXISTS)
- [x] 3.4 Implement `SqliteStoryRepository.SaveAsync`: INSERT with Paragraphs as JSON
- [x] 3.5 Implement `SqliteStoryRepository.GetAllAsync`: SELECT Id, Badge ORDER BY CreatedAt DESC
- [x] 3.6 Implement `SqliteStoryRepository.GetByIdAsync`: SELECT * + Paragraphs JSON deserialization

## 4. Badge Generation

- [x] 4.1 Create `BadgeService` with `GenerateBadgeAsync(Story)` method
- [x] 4.2 Read prompt file at startup, create `KernelFunction` with `CreateFunctionFromPrompt`, register as keyed singleton in DI
- [x] 4.3 Inject `Kernel` and keyed `KernelFunction` into `BadgeService`, invoke with story title + body
- [x] 4.4 Add `BadgeService` to DI registration

## 5. Persistence Service

- [x] 5.1 Create `PersistenceService` with `CacheAsync(Story)`, `TryGetFromCache(Guid)`, `PersistAsync(Guid) -> bool`
- [x] 5.2 Implement `CacheAsync`: store in `IMemoryCache` with 10min TTL keyed by `story.Id`
- [x] 5.3 Implement `PersistAsync`: retrieve from cache, call `IStoryRepository.SaveAsync`, remove from cache, return true/false
- [x] 5.4 Register `IMemoryCache`, `PersistenceService` in DI

## 6. Pipeline Integration

- [x] 6.1 After `storyTask` completes in `StoryPipelineRunner`, call `BadgeService.GenerateBadgeAsync(story)` and set `story.Badge`
- [x] 6.2 After badge generation, call `PersistenceService.CacheAsync(story)`
- [x] 6.3 Inject `BadgeService` and `PersistenceService` into `StoryPipelineRunner`

## 7. Save API Endpoint

- [x] 7.1 Add `POST /api/stories/{id}/save` route in `Program.cs`
- [x] 7.2 Call `PersistenceService.PersistAsync(id)`, return 200 on success / 404 on miss

## 8. Verification

- [x] 8.1 Build and verify the project compiles
- [ ] 8.2 Run the application and verify SQLite database is created on startup
- [ ] 8.3 Complete a full pipeline run and verify cache stores the story after badge generation
- [ ] 8.4 Call save endpoint and verify story is persisted to SQLite
- [ ] 8.5 Restart app and verify persisted stories survive restart (GetAllAsync)
