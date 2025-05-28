namespace ZAIrk.Models;

/// <summary>
/// Represents an item in the game world
/// </summary>
public class Item
{
    /// <summary>
    /// Unique identifier for the item
    /// </summary>
    public string Id { get; set; } = string.Empty;
    
    /// <summary>
    /// Name of the item
    /// </summary>
    public string Name { get; set; } = string.Empty;
    
    /// <summary>
    /// Description of the item
    /// </summary>
    public string Description { get; set; } = string.Empty;
    
    /// <summary>
    /// Detailed description shown when player examines the item
    /// </summary>
    public string DetailedDescription { get; set; } = string.Empty;
    
    /// <summary>
    /// Whether the item can be picked up
    /// </summary>
    public bool IsPickable { get; set; } = true;
    
    /// <summary>
    /// Weight of the item (may affect inventory capacity)
    /// </summary>
    public int Weight { get; set; } = 1;
    
    /// <summary>
    /// Physical size of the item (affects whether it can be picked up)
    /// </summary>
    public Size Size { get; set; } = Size.Small;
    
    /// <summary>
    /// The intended purpose or use of this item (e.g., "unlock doors", "provide light", "combat")
    /// </summary>
    public string Purpose { get; set; } = string.Empty;
}