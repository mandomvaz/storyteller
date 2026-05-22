## Why

The current badge generates exactly 3 emojis, which is too limiting for expressing the tone of a story. Increasing to 5 emojis gives richer expression and better conveys the story's mood.

## What Changes

- Update the badge prompt from "exactly 3 emojis" to "exactly 5 emojis"
- Update the default prompt in `Program.cs` from "3 emojis" to "5 emojis"
- Update the prompt file at `Prompts/badge.txt` to request 5 emojis instead of 3
- Increase `MaxTokens` from 10 to 15 to accommodate the longer output

## Capabilities

### New Capabilities
- *(none)*

### Modified Capabilities
- `badge-generation`: Change the badge count requirement from 3 to 5 emojis

## Impact

- `Prompts/badge.txt` — prompt text changed
- `Program.cs` — default fallback prompt changed, `MaxTokens` increased to 15
- `openspec/specs/badge-generation/spec.md` — requirement updated from 3 to 5 emojis
