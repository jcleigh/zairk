namespace ZAIrk.Models;

/// <summary>
/// Represents the physical size of an item
/// </summary>
public enum Size
{
    /// <summary>
    /// Small items that can be easily carried (e.g., keys, coins, small tools)
    /// </summary>
    Small = 0,
    
    /// <summary>
    /// Medium-sized items that can be carried but take up more space (e.g., books, lamps)
    /// </summary>
    Medium = 1,
    
    /// <summary>
    /// Large items that are difficult to carry (e.g., furniture, large equipment)
    /// </summary>
    Large = 2,
    
    /// <summary>
    /// Huge items that cannot be physically carried (e.g., bridges, walls, statues)
    /// </summary>
    Huge = 3
}