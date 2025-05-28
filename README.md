# zAIrk
AI-powered Zork

## Description
zAIrk is an AI-powered text adventure game inspired by the classic game Zork. Using local AI models through Ollama, zAIrk generates unique game worlds with rooms, items, and descriptions, providing a different experience each time you play.

## Features
- AI-generated game content (rooms, items, descriptions) using local Ollama models
- Classic text adventure gameplay
- Exploration through connected rooms
- Item interaction (take, drop, examine)
- No external API dependencies - runs completely locally

## Requirements
- .NET 8.0 SDK
- Ollama running locally with a compatible model (Gemma2 or Deepseek-R1 recommended)

## Installation
1. Install [Ollama](https://ollama.ai) and pull a compatible model:
   ```bash
   ollama pull gemma2
   # or
   ollama pull deepseek-r1
   ```
2. Clone this repository
3. Build the project with `dotnet build`
4. Run the game with `dotnet run`

## Usage
```
dotnet run [theme]
```

Where `[theme]` is an optional parameter to set the theme of the generated world (e.g., fantasy, sci-fi, horror). If not provided, the default theme is "fantasy".

You can configure Ollama settings using environment variables:
```bash
export OLLAMA_URL=http://localhost:11434
export OLLAMA_MODEL=gemma2
```

## Commands
- `go [direction]` - Move in a direction (north, south, east, west, up, down)
- `n, s, e, w, u, d` - Shortcuts for movement
- `look` - Look around the current location
- `examine [object]` - Examine an object or item
- `take [item]` - Pick up an item
- `drop [item]` - Drop an item from your inventory
- `inventory` - Show your inventory
- `help` - Display help information
- `quit` - Exit the game

## License
MIT License - See LICENSE file for details
