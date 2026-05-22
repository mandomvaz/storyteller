## Context

Badge generation currently produces exactly 3 emojis via a `KernelFunction` injected prompt. The prompt string is defined in two places: `Prompts/badge.txt` (file on disk) and a fallback in `Program.cs`. The `MaxTokens` setting is 10.

## Goals / Non-Goals

**Goals:**
- Change badge output from 3 to 5 emojis
- Only change the number — no structural or architectural changes

**Non-Goals:**
- No changes to caching, persistence, or pipeline flow
- No new dependencies or services

## Decisions

### Prompt changes only — no code changes needed
The `BadgeService` and `KernelFunction` invocation logic is unchanged. Only the prompt text and `MaxTokens` value need to be updated. The token limit is increased from 10 to 15 since 5 emojis may produce slightly longer output.

## Risks / Trade-offs

- **[Risk] 5 emojis may not fit in 15 tokens** — Each emoji is ~1 token, plus whitespace. 15 tokens is generous for 5 emojis. If truncation occurs, increase `MaxTokens` further.
- **[Risk] Existing cached stories** — already-persisted stories with 3-emoji badges are unaffected. Only new stories will have 5-emoji badges.
