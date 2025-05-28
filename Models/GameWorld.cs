namespace ZAIrk.Models;

/// <summary>
/// Represents the game world containing all rooms and items
/// </summary>
public class GameWorld
{
    /// <summary>
    /// Dictionary of all rooms in the game world, keyed by room ID
    /// </summary>
    public Dictionary<string, Room> Rooms { get; set; } = new Dictionary<string, Room>();
    
    /// <summary>
    /// ID of the starting room
    /// </summary>
    public string StartingRoomId { get; set; } = string.Empty;
    
    /// <summary>
    /// ID of the current room where the player is located
    /// </summary>
    public string CurrentRoomId { get; set; } = string.Empty;
    
    /// <summary>
    /// List of items in the player's inventory
    /// </summary>
    public List<Item> Inventory { get; set; } = new List<Item>();
    
    /// <summary>
    /// Maximum number of items the player can carry
    /// </summary>
    public int MaxInventorySize { get; set; } = 10;
    
    /// <summary>
    /// Gets the current room where the player is located
    /// </summary>
    public Room? CurrentRoom => Rooms.TryGetValue(CurrentRoomId, out var room) ? room : null;
}