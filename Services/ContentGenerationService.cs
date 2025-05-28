using System.Text;
using System.Text.Json;
using ZAIrk.Models;
using ZAIrk.Models.Ollama;

namespace ZAIrk.Services;

/// <summary>
/// Service for generating game content using AI (Ollama)
/// </summary>
public class ContentGenerationService : IDisposable
{
    private readonly HttpClient _httpClient;
    private readonly string _modelName;
    private readonly string _baseUrl;
    
    public ContentGenerationService(string baseUrl = "http://localhost:11434", string modelName = "gemma2")
    {
        _httpClient = new HttpClient();
        _baseUrl = baseUrl;
        _modelName = modelName;
    }
    
    public void Dispose()
    {
        _httpClient?.Dispose();
    }
    
    /// <summary>
    /// Makes a chat completion request to Ollama
    /// </summary>
    private async Task<string> GetChatCompletionAsync(string systemPrompt, string userPrompt, float temperature = 0.7f, int maxTokens = 500)
    {
        var request = new OllamaRequest
        {
            Model = _modelName,
            Messages = new List<OllamaMessage>
            {
                new OllamaMessage { Role = "system", Content = systemPrompt },
                new OllamaMessage { Role = "user", Content = userPrompt }
            },
            Stream = false,
            Options = new OllamaOptions
            {
                Temperature = temperature,
                NumPredict = maxTokens
            }
        };
        
        var json = JsonSerializer.Serialize(request);
        var content = new StringContent(json, Encoding.UTF8, "application/json");
        
        var response = await _httpClient.PostAsync($"{_baseUrl}/api/chat", content);
        response.EnsureSuccessStatusCode();
        
        var responseJson = await response.Content.ReadAsStringAsync();
        var ollamaResponse = JsonSerializer.Deserialize<OllamaResponse>(responseJson);
        
        return ollamaResponse?.Message?.Content ?? "";
    }
    
    /// <summary>
    /// Generates a room description using AI
    /// </summary>
    public async Task<string> GenerateRoomDescriptionAsync(string roomName, string theme)
    {
        var systemPrompt = "You are a text adventure game designer. Create vivid, concise room descriptions " +
                          "similar to those in the classic game Zork. Keep descriptions brief, under 100 words. " +
                          "Avoid repetitive patterns like starting descriptions with 'The air...' or similar phrases. " +
                          "Use varied language to describe atmosphere and environments. " +
                          "Return ONLY the room description text with no additional commentary, explanations, " +
                          "formatting, or meta-text. Do not include design notes or ask questions.";
        
        var userPrompt = $"Create a short, distinctive description for a room called '{roomName}' in a {theme}-themed text adventure. " +
                        "Focus on unique characteristics rather than generic atmosphere. Be concise.";
        
        var response = await GetChatCompletionAsync(systemPrompt, userPrompt, 0.7f, 400);
        
        // Post-process the response to remove any markdown or meta-commentary
        return CleanRoomDescription(response);
    }
    
    /// <summary>
    /// Cleans a room description by removing markdown formatting and meta-commentary
    /// </summary>
    private string CleanRoomDescription(string description)
    {
        // Remove markdown code blocks
        description = System.Text.RegularExpressions.Regex.Replace(description, "```[\\s\\S]*?```", match => 
        {
            // Extract just the content inside the code block
            var content = match.Value.Replace("```", "").Trim();
            return content;
        });
        
        // Remove single backticks
        description = description.Replace("`", "");
        
        // Remove lines that appear to be meta-commentary
        var lines = description.Split('\n');
        var cleanedLines = new List<string>();
        
        foreach (var line in lines)
        {
            var trimmedLine = line.Trim();
            
            // Skip empty lines
            if (string.IsNullOrWhiteSpace(trimmedLine))
                continue;
                
            // Skip lines that look like meta-commentary
            if (trimmedLine.StartsWith("Note:") || 
                trimmedLine.StartsWith("Design:") || 
                trimmedLine.StartsWith("*") ||
                trimmedLine.StartsWith("-") ||
                trimmedLine.StartsWith("#") ||
                trimmedLine.StartsWith(">") ||
                trimmedLine.Contains("Would you like") ||
                trimmedLine.Contains("I can") && trimmedLine.Contains("?"))
                continue;
                
            cleanedLines.Add(trimmedLine);
        }
        
        return string.Join("\n", cleanedLines);
    }
    
    /// <summary>
    /// Generates an item description using AI
    /// </summary>
    public async Task<string> GenerateItemDescriptionAsync(string itemName, string theme, string roomDescription = "")
    {
        var systemPrompt = "You are a text adventure game designer. Create vivid, concise object descriptions " +
                          "similar to those in the classic game Zork. Keep descriptions under 75 words. " +
                          "Return ONLY the item description text with no additional commentary, explanations, " +
                          "formatting, or meta-text. Do not include design notes or ask questions.";
        
        var userPrompt = $"Create a description for an item called '{itemName}' in a {theme}-themed text adventure.";
        
        if (!string.IsNullOrWhiteSpace(roomDescription))
        {
            userPrompt += $" The item is found in a room with this description: \"{roomDescription}\". " +
                         "Make sure the item description fits naturally with the room's atmosphere and environment.";
        }
        
        userPrompt += " Describe its appearance, material, and any notable features.";
        
        var response = await GetChatCompletionAsync(systemPrompt, userPrompt, 0.7f, 300);
        
        // Post-process the response to remove any markdown or meta-commentary
        return CleanItemDescription(response);
    }
    
    /// <summary>
    /// Cleans an item description by removing markdown formatting and meta-commentary
    /// </summary>
    private string CleanItemDescription(string description)
    {
        // Use the same cleaning logic as room descriptions
        return CleanRoomDescription(description);
    }
    
    /// <summary>
    /// Determines the size of an item based on its name
    /// </summary>
    private Size DetermineItemSize(string itemName)
    {
        // List of keywords that suggest large objects that shouldn't be pickable
        var hugeItemKeywords = new[] 
        { 
            "bridge", "wall", "statue", "boulder", "pillar", "throne", "altar", 
            "fountain", "waterfall", "tree", "door", "gate", "archway"
        };
        
        var largeItemKeywords = new[] 
        { 
            "table", "desk", "chair", "bed", "shelf", "chest", "trunk", 
            "cabinet", "bench", "stool"
        };
        
        var mediumItemKeywords = new[] 
        { 
            "book", "lamp", "lantern", "vase", "pot", "pan", "jug", 
            "staff", "sword", "shield", "helmet", "boots"
        };
        
        // Check against keywords - case insensitive
        foreach (var keyword in hugeItemKeywords)
        {
            if (itemName.Contains(keyword, StringComparison.OrdinalIgnoreCase))
            {
                return Size.Huge;
            }
        }
        
        foreach (var keyword in largeItemKeywords)
        {
            if (itemName.Contains(keyword, StringComparison.OrdinalIgnoreCase))
            {
                return Size.Large;
            }
        }
        
        foreach (var keyword in mediumItemKeywords)
        {
            if (itemName.Contains(keyword, StringComparison.OrdinalIgnoreCase))
            {
                return Size.Medium;
            }
        }
        
        // Default to small
        return Size.Small;
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
        
        // Extract items mentioned in room descriptions (Issue 1)
        await ExtractItemsFromRoomDescriptions(gameWorld, theme);
        
        return gameWorld;
    }
    
    /// <summary>
    /// Generates a list of room names for the game world
    /// </summary>
    private async Task<List<string>> GenerateRoomNamesAsync(string theme, int count)
    {
        var systemPrompt = "You are a text adventure game designer creating room names for a Zork-like game. " +
                          "Return only a numbered list with no additional text, commentary, or formatting.";
        
        var userPrompt = $"Create {count} unique and interesting room names for a {theme}-themed text adventure. " +
                        "Each name should be brief (1-4 words) and evocative. Format as a numbered list.";
        
        var content = await GetChatCompletionAsync(systemPrompt, userPrompt, 0.8f, 500);
        
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
        var roomDescriptions = gameWorld.Rooms.Select(r => $"Room {r.Key}: {r.Value.Name}").ToList();
        var roomsDescription = string.Join("\n", roomDescriptions);
        
        var systemPrompt = "You are a text adventure game designer creating the layout for a Zork-like game. " +
                          "Create connections between rooms using north, south, east, west, up, and down directions. " +
                          "Return only a list of connections in the format 'RoomID Direction RoomID' with no additional text, " +
                          "commentary, or formatting.";
        
        var userPrompt = $"Create connections between these rooms for a {theme}-themed text adventure:\n\n" +
                        $"{roomsDescription}\n\n" +
                        "Each room should have 1-3 connections. Ensure all rooms are reachable from room_0. " +
                        "Format each connection as 'RoomID Direction RoomID' (e.g., 'room_0 north room_1').";
        
        var content = await GetChatCompletionAsync(systemPrompt, userPrompt, 0.7f, 1000);
        
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
        // Keep track of all item names to prevent duplicates (Issue 5)
        var allItemNames = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        
        foreach (var room in gameWorld.Rooms.Values)
        {
            // Generate 0-1 items per room (70% chance of 0, 30% chance of 1) (Issue 2)
            // This follows Zork's design where many rooms have no items
            var random = new Random();
            var hasItem = random.NextDouble() < 0.3;
            
            if (!hasItem)
                continue;
            
            var itemName = await GenerateItemNameAsync(room.Name, theme);
            
            // Skip if this item name already exists elsewhere (Issue 5)
            if (allItemNames.Contains(itemName))
                continue;
                
            allItemNames.Add(itemName);
            var itemId = $"item_{room.Id}_0";
            
            var item = new Item
            {
                Id = itemId,
                Name = itemName,
                Description = await GenerateItemDescriptionAsync(itemName, theme, room.Description),
                IsPickable = random.NextDouble() < 0.9, // 90% chance of being pickable (Issue 4)
                Size = DetermineItemSize(itemName)
            };
            
            room.Items.Add(item);
        }
    }
    
    /// <summary>
    /// Extracts items mentioned in room descriptions and adds them to the item list (Issue 1)
    /// </summary>
    private async Task ExtractItemsFromRoomDescriptions(GameWorld gameWorld, string theme)
    {
        // Track all existing item names to prevent duplicates
        var allItemNames = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        
        // First, collect all existing item names
        foreach (var room in gameWorld.Rooms.Values)
        {
            foreach (var item in room.Items)
            {
                allItemNames.Add(item.Name);
            }
        }
        
        // Now extract items from room descriptions
        foreach (var room in gameWorld.Rooms.Values)
        {
            var systemPrompt = "You are a text adventure game item extractor. " +
                             "Identify simple, concrete objects mentioned in a room description that players might want to interact with. " +
                             "Return only a comma-separated list of simple object names (1-2 words maximum). " +
                             "If no clear items are mentioned, return 'none'. " +
                             "Focus on distinct physical objects, not features or descriptions.";
            
            var userPrompt = $"Extract a list of simple, interactable objects from this room description: \"{room.Description}\"";
            
            var extractedItemsText = await GetChatCompletionAsync(systemPrompt, userPrompt, 0.7f, 100);
            
            if (string.IsNullOrWhiteSpace(extractedItemsText) || extractedItemsText.Trim().Equals("none", StringComparison.OrdinalIgnoreCase))
                continue;
            
            var extractedItems = extractedItemsText.Split(',', StringSplitOptions.RemoveEmptyEntries)
                .Select(i => i.Trim())
                .Where(i => !string.IsNullOrWhiteSpace(i))
                .ToList();
            
            foreach (var extractedItemName in extractedItems)
            {
                // Skip if we already have this item somewhere else (Issue 5)
                if (allItemNames.Contains(extractedItemName))
                    continue;
                
                // Skip if the room already has too many items (part of Issue 2)
                if (room.Items.Count >= 2)
                    break;
                
                allItemNames.Add(extractedItemName);
                
                var itemId = $"item_{room.Id}_{room.Items.Count}";
                var random = new Random();
                
                var item = new Item
                {
                    Id = itemId,
                    Name = extractedItemName,
                    Description = await GenerateItemDescriptionAsync(extractedItemName, theme, room.Description),
                    IsPickable = random.NextDouble() < 0.7, // 70% chance for extracted items to be pickable
                    Size = DetermineItemSize(extractedItemName)
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
        var systemPrompt = "You are a text adventure game designer creating simple item names for a Zork-like game. " +
                          "Return only the item name with no additional text, commentary, or formatting. " +
                          "Keep names very simple, preferably single words like 'lantern', 'key', or 'sword'.";
        
        var userPrompt = $"Create a simple name for an item that might be found in a room called '{roomName}' " +
                        $"in a {theme}-themed text adventure. The name should ideally be a single word without adjectives. " +
                        $"If you must use an adjective, use at most one, like 'silver key' but never 'ancient silver key'.";
        
        var content = await GetChatCompletionAsync(systemPrompt, userPrompt, 0.8f, 100);
        return content.Trim();
    }
}