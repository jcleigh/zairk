# zAIrk
AI-powered Zork

## Description
zAIrk is an AI-powered text adventure game inspired by the classic game Zork. Using advanced language models, zAIrk generates unique game worlds with rooms, items, and descriptions, providing a different experience each time you play.

## Features
- AI-generated game content (rooms, items, descriptions)
- Classic text adventure gameplay
- Exploration through connected rooms
- Item interaction (take, drop, examine)
- Minimal dependencies

## Requirements
- .NET 8.0 SDK
- OpenAI API key

## Installation
1. Clone this repository
2. Build the project with `dotnet build`
3. Run the game with `dotnet run`

## Usage
```
dotnet run [theme]
```

Where `[theme]` is an optional parameter to set the theme of the generated world (e.g., fantasy, sci-fi, horror). If not provided, the default theme is "fantasy".

You can also set your OpenAI API key as an environment variable:
```
export OPENAI_API_KEY=your_api_key_here
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
