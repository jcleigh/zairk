using ZAIrk.Models;

namespace ZAIrk.Services;

/// <summary>
/// Service for parsing player commands
/// </summary>
public class CommandParserService
{
    /// <summary>
    /// Parses a command string into a command and arguments
    /// </summary>
    public (string Command, string[] Arguments) ParseCommand(string input)
    {
        if (string.IsNullOrWhiteSpace(input))
        {
            return (string.Empty, Array.Empty<string>());
        }
        
        var parts = input.Trim().Split(' ', StringSplitOptions.RemoveEmptyEntries);
        if (parts.Length == 0)
        {
            return (string.Empty, Array.Empty<string>());
        }
        
        var command = parts[0].ToLower();
        var arguments = parts.Skip(1).ToArray();
        
        // Handle movement shortcuts (n, s, e, w, u, d)
        if (DirectionExtensions.TryParse(command, out _))
        {
            return ("go", new[] { command });
        }
        
        return (command, arguments);
    }
    
    /// <summary>
    /// Checks if the input is a movement command
    /// </summary>
    public bool IsMovementCommand(string command, string[] arguments, out Direction direction)
    {
        direction = Direction.North; // Default
        
        if (command == "go" && arguments.Length > 0)
        {
            return DirectionExtensions.TryParse(arguments[0], out direction);
        }
        
        return false;
    }
    
    /// <summary>
    /// Checks if the input is a take command
    /// </summary>
    public bool IsTakeCommand(string command, string[] arguments, out string itemName)
    {
        itemName = string.Empty;
        
        if ((command == "take" || command == "get") && arguments.Length > 0)
        {
            itemName = string.Join(" ", arguments);
            return true;
        }
        
        return false;
    }
    
    /// <summary>
    /// Checks if the input is a drop command
    /// </summary>
    public bool IsDropCommand(string command, string[] arguments, out string itemName)
    {
        itemName = string.Empty;
        
        if (command == "drop" && arguments.Length > 0)
        {
            itemName = string.Join(" ", arguments);
            return true;
        }
        
        return false;
    }
    
    /// <summary>
    /// Checks if the input is a look command
    /// </summary>
    public bool IsLookCommand(string command)
    {
        return command == "look" || command == "l";
    }
    
    /// <summary>
    /// Checks if the input is an examine command
    /// </summary>
    public bool IsExamineCommand(string command, string[] arguments, out string target)
    {
        target = string.Empty;
        
        if ((command == "examine" || command == "x") && arguments.Length > 0)
        {
            target = string.Join(" ", arguments);
            return true;
        }
        
        return false;
    }
    
    /// <summary>
    /// Checks if the input is an inventory command
    /// </summary>
    public bool IsInventoryCommand(string command)
    {
        return command == "inventory" || command == "i";
    }
    
    /// <summary>
    /// Checks if the input is a help command
    /// </summary>
    public bool IsHelpCommand(string command)
    {
        return command == "help" || command == "h" || command == "?";
    }
    
    /// <summary>
    /// Checks if the input is a quit command
    /// </summary>
    public bool IsQuitCommand(string command)
    {
        return command == "quit" || command == "q" || command == "exit";
    }
    
    /// <summary>
    /// Checks if the input is a map command
    /// </summary>
    public bool IsMapCommand(string command)
    {
        return command == "map" || command == "m";
    }
}