## UNCHANGED Requirements

This refactor does not change any requirements. The existing `openspec/specs/tts-integration/spec.md` already specifies:

> "The system SHALL register an implementation of `ITextToAudioService` in the SK Kernel"

The implementation was misaligned with this requirement. This change fixes the implementation to match the existing spec.
