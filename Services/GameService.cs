using System.Text;
using ZAIrk.Models;

namespace ZAIrk.Services;

/// <summary>
/// Service for handling game state and gameplay
/// </summary>
public class GameService
{
    private readonly GameWorld _gameWorld;
    
    public GameService(GameWorld gameWorld)
    {
        _gameWorld = gameWorld;
    }
    
    /// <summary>
    /// Gets the current room
    /// </summary>
    public Room? GetCurrentRoom()
    {
        return _gameWorld.CurrentRoom;
    }
    
    /// <summary>
    /// Moves the player in the specified direction
    /// </summary>
    /// <returns>True if the move was successful, false otherwise</returns>
    public bool Move(Direction direction)
    {
        var currentRoom = _gameWorld.CurrentRoom;
        if (currentRoom == null) return false;
        
        if (!currentRoom.Exits.TryGetValue(direction, out var destinationRoomId))
        {
            return false; // No exit in that direction
        }
        
        _gameWorld.CurrentRoomId = destinationRoomId;
        
        // Mark the room as visited
        if (_gameWorld.Rooms.TryGetValue(destinationRoomId, out var room))
        {
            room.IsVisited = true;
        }
        
        return true;
    }
    
    /// <summary>
    /// Gets an item from the current room
    /// </summary>
    public Item? GetItemFromRoom(string itemName)
    {
        var currentRoom = _gameWorld.CurrentRoom;
        if (currentRoom == null) return null;
        
        return currentRoom.Items.FirstOrDefault(i => 
            i.Name.Equals(itemName, StringComparison.OrdinalIgnoreCase));
    }
    
    /// <summary>
    /// Gets an item from the player's inventory
    /// </summary>
    public Item? GetItemFromInventory(string itemName)
    {
        return _gameWorld.Inventory.FirstOrDefault(i => 
            i.Name.Equals(itemName, StringComparison.OrdinalIgnoreCase));
    }
    
    /// <summary>
    /// Takes an item from the current room and adds it to the inventory
    /// </summary>
    /// <returns>True if the item was taken, false otherwise</returns>
    public bool TakeItem(string itemName)
    {
        var item = GetItemFromRoom(itemName);
        if (item == null || !item.IsPickable) return false;
        
        // Check if the item is too large to pick up
        if (item.Size >= Size.Large) return false;
        
        if (_gameWorld.Inventory.Count >= _gameWorld.MaxInventorySize)
        {
            return false; // Inventory is full
        }
        
        var currentRoom = _gameWorld.CurrentRoom;
        if (currentRoom == null) return false;
        
        // Remove from room and add to inventory
        currentRoom.Items.Remove(item);
        _gameWorld.Inventory.Add(item);
        
        return true;
    }
    
    /// <summary>
    /// Drops an item from the inventory into the current room
    /// </summary>
    /// <returns>True if the item was dropped, false otherwise</returns>
    public bool DropItem(string itemName)
    {
        var item = GetItemFromInventory(itemName);
        if (item == null) return false;
        
        var currentRoom = _gameWorld.CurrentRoom;
        if (currentRoom == null) return false;
        
        // Remove from inventory and add to room
        _gameWorld.Inventory.Remove(item);
        currentRoom.Items.Add(item);
        
        return true;
    }
    
    /// <summary>
    /// Gets the available exits from the current room
    /// </summary>
    public IEnumerable<Direction> GetAvailableExits()
    {
        var currentRoom = _gameWorld.CurrentRoom;
        if (currentRoom == null) return Enumerable.Empty<Direction>();
        
        return currentRoom.Exits.Keys;
    }
    
    /// <summary>
    /// Gets the items in the current room
    /// </summary>
    public IEnumerable<Item> GetItemsInRoom()
    {
        var currentRoom = _gameWorld.CurrentRoom;
        if (currentRoom == null) return Enumerable.Empty<Item>();
        
        return currentRoom.Items;
    }
    
    /// <summary>
    /// Gets the items in the player's inventory
    /// </summary>
    public IEnumerable<Item> GetInventory()
    {
        return _gameWorld.Inventory;
    }
    
    /// <summary>
    /// Generates a markdown map of the game world rooms and their connections
    /// </summary>
    /// <param name="filePath">Path where the map file should be saved</param>
    /// <returns>True if the map was successfully generated and saved, false otherwise</returns>
    public bool GenerateRoomMap(string filePath)
    {
        try
        {
            var mapBuilder = new StringBuilder();
            
            // Add header
            mapBuilder.AppendLine("# Game World Map");
            mapBuilder.AppendLine();
            
            // Add information about starting room
            mapBuilder.AppendLine($"Starting Room: {_gameWorld.Rooms[_gameWorld.StartingRoomId].Name}");
            mapBuilder.AppendLine();
            
            // Add all rooms and their connections
            mapBuilder.AppendLine("## Rooms");
            mapBuilder.AppendLine();
            
            foreach (var room in _gameWorld.Rooms.Values)
            {
                mapBuilder.AppendLine($"### {room.Name} (ID: {room.Id})");
                mapBuilder.AppendLine();
                mapBuilder.AppendLine(room.Description);
                mapBuilder.AppendLine();
                
                // Add exits
                if (room.Exits.Any())
                {
                    mapBuilder.AppendLine("#### Exits:");
                    foreach (var exit in room.Exits)
                    {
                        var connectedRoom = _gameWorld.Rooms[exit.Value];
                        mapBuilder.AppendLine($"- {exit.Key.ToDisplayString()}: {connectedRoom.Name}");
                    }
                }
                else
                {
                    mapBuilder.AppendLine("No exits available.");
                }
                
                // Add items
                if (room.Items.Any())
                {
                    mapBuilder.AppendLine();
                    mapBuilder.AppendLine("#### Items:");
                    foreach (var item in room.Items)
                    {
                        mapBuilder.AppendLine($"- {item.Name}");
                    }
                }
                
                mapBuilder.AppendLine();
            }
            
            // Write to file
            File.WriteAllText(filePath, mapBuilder.ToString());
            return true;
        }
        catch (Exception)
        {
            // If there's an error, don't crash the game
            return false;
        }
    }
}