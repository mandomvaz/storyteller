# StoryForge

Voice-to-story pipeline powered by Semantic Kernel. Records audio, transcribes with Whisper, generates stories via Ollama, and delivers text-to-speech audio through XTTS in real-time.

## Features

- Audio recording via browser (MediaRecorder API)
- Speech-to-text transcription (Whisper / Faster-Whisper)
- Story generation via Ollama + LLM
- Per-paragraph streaming TTS (XTTS)
- Real-time delivery over SignalR + MessagePack

## Tech Stack

- .NET 10 + Minimal APIs
- Semantic Kernel 1.76
- SignalR + MessagePack
- Ollama (LLM backend)
- Whisper (ASR backend)
- XTTS (TTS backend)
- Alpine.js (frontend)

## Prerequisites

- [.NET 10 SDK](https://dotnet.microsoft.com/download)
- [Ollama](https://ollama.com) running on `localhost:11434`
- [Faster-Whisper](https://github.com/SYSTRAN/faster-whisper) server on `localhost:8000`
- [XTTS API](https://github.com/coqui-ai/TTS) server on `localhost:8020`

## Quick Start

```bash
cd storyforge
dotnet run --launch-profile http
```

Open `http://localhost:5147` in a browser.

## Configuration

Edit `appsettings.json` or set environment variables:

| Key | Default |
|---|---|
| `Ollama:Endpoint` | `http://localhost:11434` |
| `Ollama:TextModel` | `storyteller` |
| `Whisper:Endpoint` | `http://localhost:8000` |
| `Whisper:Model` | `medium` |
| `Xtts:Endpoint` | `http://localhost:8020` |
| `Xtts:Speaker` | `laura.wav` |
| `Xtts:Language` | `es` |

## Architecture

See [ARCHITECTURE.md](ARCHITECTURE.md) for a full breakdown.

## Disclaimer

**This project is provided as-is, without any warranty or support.**  
Once finalized, no further updates, bug fixes, security patches, or feature additions will be made by the author.  
External pull requests, modifications, or contributions will **not** be reviewed, accepted, or merged.  
If you wish to fork and maintain your own version, you are free to do so under the terms of the GPL-3.0 license.

## License

[GNU General Public License v3.0](LICENSE)
