namespace ZAIrk.Models;

/// <summary>
/// Represents a room in the game world
/// </summary>
public class Room
{
    /// <summary>
    /// Unique identifier for the room
    /// </summary>
    public string Id { get; set; } = string.Empty;
    
    /// <summary>
    /// Name of the room
    /// </summary>
    public string Name { get; set; } = string.Empty;
    
    /// <summary>
    /// Description of the room
    /// </summary>
    public string Description { get; set; } = string.Empty;
    
    /// <summary>
    /// Detailed description shown when player examines the room
    /// </summary>
    public string DetailedDescription { get; set; } = string.Empty;
    
    /// <summary>
    /// Collection of items in the room
    /// </summary>
    public List<Item> Items { get; set; } = new List<Item>();
    
    /// <summary>
    /// Dictionary of exits mapping direction to room ID
    /// </summary>
    public Dictionary<Direction, string> Exits { get; set; } = new Dictionary<Direction, string>();
    
    /// <summary>
    /// Whether the room has been visited by the player
    /// </summary>
    public bool IsVisited { get; set; } = false;
}