using Azure.AI.OpenAI;
using ZAIrk.Models;

namespace ZAIrk.Services;

/// <summary>
/// Service for generating game content using AI
/// </summary>
public class ContentGenerationService
{
    private readonly OpenAIClient _openAIClient;
    private readonly string _deploymentName;
    
    public ContentGenerationService(string apiKey, string deploymentName)
    {
        _openAIClient = new OpenAIClient(apiKey);
        _deploymentName = deploymentName;
    }
    
    /// <summary>
    /// Generates a room description using AI
    /// </summary>
    public async Task<string> GenerateRoomDescriptionAsync(string roomName, string theme)
    {
        var chatCompletionsOptions = new ChatCompletionsOptions()
        {
            Temperature = 0.7f,
            MaxTokens = 500
        };
        
        chatCompletionsOptions.Messages.Add(new ChatMessage(
            ChatRole.System,
            "You are a text adventure game designer. Create vivid, concise room descriptions " +
            "similar to those in the classic game Zork. Keep descriptions under 150 words."
        ));
        
        chatCompletionsOptions.Messages.Add(new ChatMessage(
            ChatRole.User,
            $"Create a description for a room called '{roomName}' in a {theme}-themed text adventure. " +
            "Describe the room's appearance, atmosphere, and notable features."
        ));
        
        var response = await _openAIClient.GetChatCompletionsAsync(_deploymentName, chatCompletionsOptions);
        return response.Value.Choices[0].Message.Content;
    }
    
    /// <summary>
    /// Generates an item description using AI
    /// </summary>
    public async Task<string> GenerateItemDescriptionAsync(string itemName, string theme)
    {
        var chatCompletionsOptions = new ChatCompletionsOptions()
        {
            Temperature = 0.7f,
            MaxTokens = 300
        };
        
        chatCompletionsOptions.Messages.Add(new ChatMessage(
            ChatRole.System,
            "You are a text adventure game designer. Create vivid, concise object descriptions " +
            "similar to those in the classic game Zork. Keep descriptions under 75 words."
        ));
        
        chatCompletionsOptions.Messages.Add(new ChatMessage(
            ChatRole.User,
            $"Create a description for an item called '{itemName}' in a {theme}-themed text adventure. " +
            "Describe its appearance, material, and any notable features."
        ));
        
        var response = await _openAIClient.GetChatCompletionsAsync(_deploymentName, chatCompletionsOptions);
        return response.Value.Choices[0].Message.Content;
    }
    
    /// <summary>
    /// Generates a complete game world with rooms and items
    /// </summary>
    public async Task<GameWorld> GenerateGameWorldAsync(string theme, int numberOfRooms = 10)
    {
        // First, generate room names
        var roomNames = await GenerateRoomNamesAsync(theme, numberOfRooms);
        
        // Create the game world
        var gameWorld = new GameWorld();
        
        // Generate rooms
        for (int i = 0; i < roomNames.Count; i++)
        {
            var roomId = $"room_{i}";
            var roomName = roomNames[i];
            
            var room = new Room
            {
                Id = roomId,
                Name = roomName,
                Description = await GenerateRoomDescriptionAsync(roomName, theme)
            };
            
            gameWorld.Rooms[roomId] = room;
        }
        
        // Set starting room
        gameWorld.StartingRoomId = "room_0";
        gameWorld.CurrentRoomId = gameWorld.StartingRoomId;
        
        // Generate connections between rooms
        await GenerateRoomConnectionsAsync(gameWorld, theme);
        
        // Generate items for rooms
        await GenerateItemsAsync(gameWorld, theme);
        
        return gameWorld;
    }
    
    /// <summary>
    /// Generates a list of room names for the game world
    /// </summary>
    private async Task<List<string>> GenerateRoomNamesAsync(string theme, int count)
    {
        var chatCompletionsOptions = new ChatCompletionsOptions()
        {
            Temperature = 0.8f,
            MaxTokens = 500
        };
        
        chatCompletionsOptions.Messages.Add(new ChatMessage(
            ChatRole.System,
            "You are a text adventure game designer creating room names for a Zork-like game. " +
            "Return only a numbered list with no additional text."
        ));
        
        chatCompletionsOptions.Messages.Add(new ChatMessage(
            ChatRole.User,
            $"Create {count} unique and interesting room names for a {theme}-themed text adventure. " +
            "Each name should be brief (1-4 words) and evocative. Format as a numbered list."
        ));
        
        var response = await _openAIClient.GetChatCompletionsAsync(_deploymentName, chatCompletionsOptions);
        var content = response.Value.Choices[0].Message.Content;
        
        // Parse the numbered list
        var lines = content.Split('\n', StringSplitOptions.RemoveEmptyEntries);
        var roomNames = new List<string>();
        
        foreach (var line in lines)
        {
            var trimmedLine = line.Trim();
            if (string.IsNullOrEmpty(trimmedLine)) continue;
            
            // Extract room name from the numbered list (format: "1. Room Name")
            var parts = trimmedLine.Split('.', 2);
            if (parts.Length > 1)
            {
                roomNames.Add(parts[1].Trim());
            }
            else
            {
                roomNames.Add(trimmedLine);
            }
            
            if (roomNames.Count >= count) break;
        }
        
        // If we didn't get enough room names, add generic ones
        while (roomNames.Count < count)
        {
            roomNames.Add($"{theme} Room {roomNames.Count + 1}");
        }
        
        return roomNames;
    }
    
    /// <summary>
    /// Generates connections between rooms in the game world
    /// </summary>
    private async Task GenerateRoomConnectionsAsync(GameWorld gameWorld, string theme)
    {
        var chatCompletionsOptions = new ChatCompletionsOptions()
        {
            Temperature = 0.7f,
            MaxTokens = 1000
        };
        
        var roomDescriptions = gameWorld.Rooms.Select(r => $"Room {r.Key}: {r.Value.Name}").ToList();
        var roomsDescription = string.Join("\n", roomDescriptions);
        
        chatCompletionsOptions.Messages.Add(new ChatMessage(
            ChatRole.System,
            "You are a text adventure game designer creating the layout for a Zork-like game. " +
            "Create connections between rooms using north, south, east, west, up, and down directions. " +
            "Return only a list of connections in the format 'RoomID Direction RoomID'."
        ));
        
        chatCompletionsOptions.Messages.Add(new ChatMessage(
            ChatRole.User,
            $"Create connections between these rooms for a {theme}-themed text adventure:\n\n" +
            $"{roomsDescription}\n\n" +
            "Each room should have 1-3 connections. Ensure all rooms are reachable from room_0. " +
            "Format each connection as 'RoomID Direction RoomID' (e.g., 'room_0 north room_1')."
        ));
        
        var response = await _openAIClient.GetChatCompletionsAsync(_deploymentName, chatCompletionsOptions);
        var content = response.Value.Choices[0].Message.Content;
        
        // Parse the connections
        var lines = content.Split('\n', StringSplitOptions.RemoveEmptyEntries);
        
        foreach (var line in lines)
        {
            var trimmedLine = line.Trim();
            if (string.IsNullOrEmpty(trimmedLine)) continue;
            
            // Extract connection info (format: "room_0 north room_1")
            var parts = trimmedLine.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length >= 3 && 
                gameWorld.Rooms.ContainsKey(parts[0]) && 
                DirectionExtensions.TryParse(parts[1], out var direction) && 
                gameWorld.Rooms.ContainsKey(parts[2]))
            {
                var fromRoomId = parts[0];
                var toRoomId = parts[2];
                
                // Add connections in both directions
                gameWorld.Rooms[fromRoomId].Exits[direction] = toRoomId;
                gameWorld.Rooms[toRoomId].Exits[direction.GetOpposite()] = fromRoomId;
            }
        }
        
        // Ensure all rooms are connected
        EnsureAllRoomsAreConnected(gameWorld);
    }
    
    /// <summary>
    /// Ensures all rooms in the game world are connected
    /// </summary>
    private void EnsureAllRoomsAreConnected(GameWorld gameWorld)
    {
        var visited = new HashSet<string>();
        var startingRoom = gameWorld.StartingRoomId;
        
        // Perform DFS to find connected rooms
        void DFS(string roomId)
        {
            if (visited.Contains(roomId)) return;
            visited.Add(roomId);
            
            foreach (var exit in gameWorld.Rooms[roomId].Exits)
            {
                DFS(exit.Value);
            }
        }
        
        DFS(startingRoom);
        
        // Connect any unconnected rooms to the starting room
        var unconnectedRooms = gameWorld.Rooms.Keys.Where(id => !visited.Contains(id)).ToList();
        foreach (var roomId in unconnectedRooms)
        {
            var direction = (Direction)(new Random().Next(6)); // Random direction
            gameWorld.Rooms[startingRoom].Exits[direction] = roomId;
            gameWorld.Rooms[roomId].Exits[direction.GetOpposite()] = startingRoom;
        }
    }
    
    /// <summary>
    /// Generates items for rooms in the game world
    /// </summary>
    private async Task GenerateItemsAsync(GameWorld gameWorld, string theme)
    {
        foreach (var room in gameWorld.Rooms.Values)
        {
            // Generate 0-3 items per room
            var itemCount = new Random().Next(4);
            
            for (int i = 0; i < itemCount; i++)
            {
                var itemName = await GenerateItemNameAsync(room.Name, theme);
                var itemId = $"item_{room.Id}_{i}";
                
                var item = new Item
                {
                    Id = itemId,
                    Name = itemName,
                    Description = await GenerateItemDescriptionAsync(itemName, theme),
                    IsPickable = new Random().Next(10) > 2 // 80% chance of being pickable
                };
                
                room.Items.Add(item);
            }
        }
    }
    
    /// <summary>
    /// Generates a name for an item based on the room and theme
    /// </summary>
    private async Task<string> GenerateItemNameAsync(string roomName, string theme)
    {
        var chatCompletionsOptions = new ChatCompletionsOptions()
        {
            Temperature = 0.8f,
            MaxTokens = 100
        };
        
        chatCompletionsOptions.Messages.Add(new ChatMessage(
            ChatRole.System,
            "You are a text adventure game designer creating item names for a Zork-like game. " +
            "Return only the item name with no additional text."
        ));
        
        chatCompletionsOptions.Messages.Add(new ChatMessage(
            ChatRole.User,
            $"Create a name for an item that might be found in a room called '{roomName}' " +
            $"in a {theme}-themed text adventure. The name should be 1-3 words."
        ));
        
        var response = await _openAIClient.GetChatCompletionsAsync(_deploymentName, chatCompletionsOptions);
        return response.Value.Choices[0].Message.Content.Trim();
    }
}