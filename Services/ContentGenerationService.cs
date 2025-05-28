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
    /// Generates a description for an item with a specific purpose
    /// </summary>
    public async Task<string> GeneratePurposefulItemDescriptionAsync(string itemName, string purpose, string theme, string roomDescription = "")
    {
        var systemPrompt = "You are a text adventure game designer. Create vivid, concise object descriptions " +
                          "similar to those in the classic game Zork. Keep descriptions under 75 words. " +
                          "Return ONLY the item description text with no additional commentary, explanations, " +
                          "formatting, or meta-text. Do not include design notes or ask questions. " +
                          "Subtly hint at the item's purpose without being overly obvious.";
        
        var userPrompt = $"Create a description for an item called '{itemName}' in a {theme}-themed text adventure. " +
                        $"This item is intended to {purpose}. ";
        
        if (!string.IsNullOrWhiteSpace(roomDescription))
        {
            userPrompt += $"The item is found in a room with this description: \"{roomDescription}\". " +
                         "Make sure the item description fits naturally with the room's atmosphere and environment. ";
        }
        
        userPrompt += "Describe its appearance, material, and any notable features that subtly suggest its usefulness.";
        
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
            "fountain", "waterfall", "tree", "door", "gate", "archway",
            // Living creatures that shouldn't be pickable
            "bird", "raven", "crow", "eagle", "hawk", "owl", "animal", "creature",
            "person", "human", "elf", "dwarf", "orc", "goblin", "troll", "dragon",
            "cat", "dog", "wolf", "bear", "lion", "tiger", "fox", "deer",
            "snake", "lizard", "frog", "toad", "fish", "insect", "spider", "monster"
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
    /// Plans purposeful items across the game world, creating items that serve specific functions
    /// </summary>
    private Dictionary<string, List<(string ItemType, string Purpose)>> PlanPurposefulItems(GameWorld gameWorld, string theme)
    {
        var itemPlan = new Dictionary<string, List<(string ItemType, string Purpose)>>();
        
        // Define item types and their purposes for different themes
        var itemPurposes = new Dictionary<string, List<(string ItemType, string Purpose)>>
        {
            ["fantasy"] = new List<(string, string)>
            {
                ("lantern", "provide light in dark areas"),
                ("key", "unlock doors and containers"),
                ("sword", "protection and combat"),
                ("map", "navigation and exploration"),
                ("rope", "climbing and traversal"),
                ("potion", "healing and restoration"),
                ("scroll", "knowledge and spells")
            },
            ["sci-fi"] = new List<(string, string)>
            {
                ("flashlight", "provide light in dark areas"),
                ("keycard", "unlock doors and access systems"),
                ("scanner", "detect and analyze objects"),
                ("battery", "power devices and equipment"),
                ("toolkit", "repair and maintenance"),
                ("datapad", "information and communication"),
                ("oxygen tank", "survival in hostile environments")
            },
            ["horror"] = new List<(string, string)>
            {
                ("candle", "provide light in dark areas"),
                ("key", "unlock doors and containers"),
                ("crucifix", "protection from evil"),
                ("journal", "knowledge and clues"),
                ("matches", "create light and fire"),
                ("medicine", "healing and recovery"),
                ("charm", "protection and luck")
            }
        };
        
        // Get appropriate item list for theme
        var availableItems = itemPurposes.ContainsKey(theme.ToLower()) 
            ? itemPurposes[theme.ToLower()] 
            : itemPurposes["fantasy"];
        
        // Select 3-5 item types to place across the world
        var random = new Random();
        var selectedItems = availableItems.OrderBy(x => random.Next()).Take(random.Next(3, 6)).ToList();
        
        // Distribute items across rooms, ensuring earlier rooms get items needed for later rooms
        var roomIds = gameWorld.Rooms.Keys.OrderBy(id => int.Parse(id.Split('_')[1])).ToList();
        
        for (int i = 0; i < selectedItems.Count && i < roomIds.Count - 1; i++)
        {
            var roomId = roomIds[i];
            if (!itemPlan.ContainsKey(roomId))
                itemPlan[roomId] = new List<(string, string)>();
            
            itemPlan[roomId].Add(selectedItems[i]);
        }
        
        return itemPlan;
    }
    
    /// <summary>
    /// Generates items for rooms in the game world
    /// </summary>
    private async Task GenerateItemsAsync(GameWorld gameWorld, string theme)
    {
        // First, plan purposeful items across the game world
        var itemPlan = PlanPurposefulItems(gameWorld, theme);
        
        // Keep track of all item names to prevent duplicates (Issue 5)
        var allItemNames = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        
        // Generate purposeful items according to the plan
        foreach (var (roomId, plannedItems) in itemPlan)
        {
            if (!gameWorld.Rooms.ContainsKey(roomId))
                continue;
                
            var room = gameWorld.Rooms[roomId];
            
            foreach (var (itemType, purpose) in plannedItems)
            {
                // Skip if this item name or a near-duplicate already exists elsewhere (Issue 5)
                if (HasNearDuplicate(itemType, allItemNames))
                    continue;
                    
                allItemNames.Add(itemType);
                var itemId = $"item_{room.Id}_{room.Items.Count}";
                
                var item = new Item
                {
                    Id = itemId,
                    Name = itemType,
                    Purpose = purpose,
                    Description = await GeneratePurposefulItemDescriptionAsync(itemType, purpose, theme, room.Description),
                    IsPickable = true, // Purposeful items should generally be pickable
                    Size = DetermineItemSize(itemType)
                };
                
                room.Items.Add(item);
            }
        }
        
        // Fill remaining rooms with some random items (but fewer than before)
        var random = new Random();
        foreach (var room in gameWorld.Rooms.Values)
        {
            // Only add random items if the room doesn't already have purposeful items
            if (room.Items.Count == 0)
            {
                // Reduced chance of random items (20% instead of 30%)
                var hasRandomItem = random.NextDouble() < 0.2;
                
                if (!hasRandomItem)
                    continue;
                
                var itemName = await GenerateItemNameAsync(room.Name, theme);
                
                // Skip if this item name or a near-duplicate already exists elsewhere (Issue 5)
                if (HasNearDuplicate(itemName, allItemNames))
                    continue;
                    
                allItemNames.Add(itemName);
                var itemId = $"item_{room.Id}_0";
                
                var item = new Item
                {
                    Id = itemId,
                    Name = itemName,
                    Purpose = "decoration", // Random items are mostly decorative
                    Description = await GenerateItemDescriptionAsync(itemName, theme, room.Description),
                    IsPickable = random.NextDouble() < 0.9, // 90% chance of being pickable (Issue 4)
                    Size = DetermineItemSize(itemName)
                };
                
                room.Items.Add(item);
            }
        }
    }
    
    /// <summary>
    /// Checks if two item names are near-duplicates (e.g., singular/plural forms)
    /// </summary>
    private bool AreNearDuplicates(string itemName1, string itemName2)
    {
        // Normalize both names to lowercase for comparison
        var name1 = itemName1.ToLower().Trim();
        var name2 = itemName2.ToLower().Trim();
        
        // If they're exactly the same, they're duplicates
        if (name1 == name2)
            return true;
        
        // Check if one is the singular and the other is the plural form
        return IsSingularPluralPair(name1, name2);
    }
    
    /// <summary>
    /// Checks if two words form a singular/plural pair
    /// </summary>
    private bool IsSingularPluralPair(string word1, string word2)
    {
        // Check both directions: word1 as singular, word2 as plural, and vice versa
        return IsPluralOf(word2, word1) || IsPluralOf(word1, word2);
    }
    
    /// <summary>
    /// Checks if pluralWord is the plural form of singularWord
    /// </summary>
    private bool IsPluralOf(string pluralWord, string singularWord)
    {
        // Handle common English pluralization rules
        
        // Rule 1: Just add 's' (most common)
        if (pluralWord == singularWord + "s")
            return true;
        
        // Rule 2: Add 'es' for words ending in s, ss, sh, ch, x, z
        if (pluralWord == singularWord + "es" && 
            (singularWord.EndsWith("s") || singularWord.EndsWith("ss") || 
             singularWord.EndsWith("sh") || singularWord.EndsWith("ch") || 
             singularWord.EndsWith("x") || singularWord.EndsWith("z")))
            return true;
        
        // Rule 3: Words ending in 'y' preceded by consonant: change 'y' to 'ies'
        if (singularWord.EndsWith("y") && singularWord.Length > 1 &&
            !IsVowel(singularWord[singularWord.Length - 2]) &&
            pluralWord == singularWord.Substring(0, singularWord.Length - 1) + "ies")
            return true;
        
        // Rule 4: Words ending in 'f' or 'fe': change to 'ves'
        if (singularWord.EndsWith("f") && 
            pluralWord == singularWord.Substring(0, singularWord.Length - 1) + "ves")
            return true;
        
        if (singularWord.EndsWith("fe") && 
            pluralWord == singularWord.Substring(0, singularWord.Length - 2) + "ves")
            return true;
        
        // Rule 5: Words ending in 'o' preceded by consonant: add 'es'
        if (singularWord.EndsWith("o") && singularWord.Length > 1 &&
            !IsVowel(singularWord[singularWord.Length - 2]) &&
            pluralWord == singularWord + "es")
            return true;
        
        return false;
    }
    
    /// <summary>
    /// Checks if a character is a vowel
    /// </summary>
    private bool IsVowel(char c)
    {
        return "aeiouAEIOU".Contains(c);
    }
    
    /// <summary>
    /// Checks if an item name has a near-duplicate in the given collection
    /// </summary>
    private bool HasNearDuplicate(string itemName, IEnumerable<string> existingNames)
    {
        return existingNames.Any(existingName => AreNearDuplicates(itemName, existingName));
    }

    /// <summary>
    /// Validates if an extracted item name represents an interactable object vs environmental feature or living creature
    /// </summary>
    private async Task<bool> ValidateItemIsInteractableAsync(string itemName, string roomDescription)
    {
        var systemPrompt = "You are a text adventure game validator. " +
                          "Determine if an item name represents something a player can interact with and potentially pick up, " +
                          "versus an environmental feature that is part of the scenery or a living creature. " +
                          "Environmental features include: ground, floors, walls, ceilings, ledges, paths, bridges, " +
                          "cliffs, passages, corridors, doorways, openings, surfaces, formations, structures that are " +
                          "part of the room architecture. " +
                          "Living creatures include: animals, birds, insects, people, monsters, or any animate beings that are alive. " +
                          "Interactable items include: inanimate objects a person could pick up, examine closely, or manipulate. " +
                          "Respond with only 'YES' if it's an inanimate interactable object or 'NO' if it's an environmental feature or living creature.";
        
        var userPrompt = $"Item name: '{itemName}'\nRoom context: \"{roomDescription}\"\n\n" +
                        $"Is '{itemName}' an inanimate interactable object that a player could pick up, rather than an environmental feature or living creature?";
        
        var response = await GetChatCompletionAsync(systemPrompt, userPrompt, 0.3f, 50);
        
        return response.Trim().Equals("YES", StringComparison.OrdinalIgnoreCase);
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
                allItemNames.Add(item.Name.ToLower());
            }
        }
        
        // Now extract items from room descriptions
        foreach (var room in gameWorld.Rooms.Values)
        {
            var systemPrompt = "You are a text adventure game item extractor. " +
                             "Identify simple, concrete, inanimate objects mentioned in a room description that players might want to interact with. " +
                             "Return only a comma-separated list of simple object names (1-2 words maximum). " +
                             "If no clear items are mentioned, return 'none'. " +
                             "Focus on distinct physical inanimate objects, not features, descriptions, or living creatures. " +
                             "Never include animals, people, birds, insects, or any other living beings in your list.";
            
            var userPrompt = $"Extract a list of simple, interactable inanimate objects from this room description: \"{room.Description}\"";
            
            var extractedItemsText = await GetChatCompletionAsync(systemPrompt, userPrompt, 0.7f, 100);
            
            if (string.IsNullOrWhiteSpace(extractedItemsText) || extractedItemsText.Trim().Equals("none", StringComparison.OrdinalIgnoreCase))
                continue;
            
            var extractedItems = extractedItemsText.Split(',', StringSplitOptions.RemoveEmptyEntries)
                .Select(i => i.Trim())
                .Where(i => !string.IsNullOrWhiteSpace(i))
                .ToList();
            
            foreach (var extractedItemName in extractedItems)
            {
                // Normalize item name to lowercase for consistency
                var normalizedItemName = extractedItemName.ToLower();
                
                // Skip if we already have this item or a near-duplicate somewhere else (Issue 5)
                if (HasNearDuplicate(normalizedItemName, allItemNames))
                    continue;
                
                // Skip if the room already has too many items (part of Issue 2)
                if (room.Items.Count >= 2)
                    break;
                
                // Validate that this is an interactable item, not an environmental feature
                if (!await ValidateItemIsInteractableAsync(normalizedItemName, room.Description))
                    continue;
                
                allItemNames.Add(normalizedItemName);
                
                var itemId = $"item_{room.Id}_{room.Items.Count}";
                var random = new Random();
                
                var item = new Item
                {
                    Id = itemId,
                    Name = normalizedItemName,
                    Purpose = "found in room", // Items extracted from descriptions are contextual
                    Description = await GenerateItemDescriptionAsync(normalizedItemName, theme, room.Description),
                    IsPickable = random.NextDouble() < 0.7, // 70% chance for extracted items to be pickable
                    Size = DetermineItemSize(normalizedItemName)
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
                          "Only suggest inanimate objects that could be picked up, never living creatures. " +
                          "Keep names very simple, preferably single words like 'lantern', 'key', or 'sword'.";
        
        var userPrompt = $"Create a simple name for an inanimate item that might be found in a room called '{roomName}' " +
                        $"in a {theme}-themed text adventure. The name should ideally be a single word without adjectives. " +
                        $"If you must use an adjective, use at most one, like 'silver key' but never 'ancient silver key'. " +
                        $"Never suggest animals, people, or any living creatures.";
        
        var content = await GetChatCompletionAsync(systemPrompt, userPrompt, 0.8f, 100);
        return content.Trim().ToLower();
    }
}