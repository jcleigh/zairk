namespace ZAIrk.Models;

/// <summary>
/// Represents directions for movement in the game
/// </summary>
public enum Direction
{
    North,
    South,
    East,
    West,
    Up,
    Down
}

/// <summary>
/// Extension methods for the Direction enum
/// </summary>
public static class DirectionExtensions
{
    /// <summary>
    /// Gets the opposite direction
    /// </summary>
    public static Direction GetOpposite(this Direction direction)
    {
        return direction switch
        {
            Direction.North => Direction.South,
            Direction.South => Direction.North,
            Direction.East => Direction.West,
            Direction.West => Direction.East,
            Direction.Up => Direction.Down,
            Direction.Down => Direction.Up,
            _ => throw new ArgumentOutOfRangeException(nameof(direction))
        };
    }
    
    /// <summary>
    /// Converts a string to a Direction enum value
    /// </summary>
    public static bool TryParse(string input, out Direction direction)
    {
        direction = Direction.North; // Default
        
        if (string.IsNullOrEmpty(input))
            return false;
            
        return input.ToLower() switch
        {
            "north" or "n" => (direction = Direction.North, true).Item2,
            "south" or "s" => (direction = Direction.South, true).Item2,
            "east" or "e" => (direction = Direction.East, true).Item2,
            "west" or "w" => (direction = Direction.West, true).Item2,
            "up" or "u" => (direction = Direction.Up, true).Item2,
            "down" or "d" => (direction = Direction.Down, true).Item2,
            _ => false
        };
    }
    
    /// <summary>
    /// Gets the string representation of the direction
    /// </summary>
    public static string ToDisplayString(this Direction direction)
    {
        return direction switch
        {
            Direction.North => "north",
            Direction.South => "south",
            Direction.East => "east",
            Direction.West => "west",
            Direction.Up => "up",
            Direction.Down => "down",
            _ => throw new ArgumentOutOfRangeException(nameof(direction))
        };
    }
}