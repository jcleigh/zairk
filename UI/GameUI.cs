namespace ZAIrk.UI;

/// <summary>
/// Handles user interface rendering and interaction
/// </summary>
public class GameUI
{
    private readonly TextWriter _output;
    private readonly TextReader _input;
    
    public GameUI(TextWriter output, TextReader input)
    {
        _output = output;
        _input = input;
    }
    
    /// <summary>
    /// Displays the game title and introduction
    /// </summary>
    public void DisplayTitle()
    {
        _output.WriteLine("=========================================");
        _output.WriteLine("               zAIrk                     ");
        _output.WriteLine("      An AI-Powered Text Adventure       ");
        _output.WriteLine("=========================================");
        _output.WriteLine();
    }
    
    /// <summary>
    /// Displays the game introduction
    /// </summary>
    public void DisplayIntroduction()
    {
        _output.WriteLine("Welcome to zAIrk, an AI-powered text adventure inspired by Zork.");
        _output.WriteLine("Explore the world, collect items, and uncover the mysteries that await.");
        _output.WriteLine();
        _output.WriteLine("Type 'help' for a list of commands.");
        _output.WriteLine();
    }
    
    /// <summary>
    /// Displays help information
    /// </summary>
    public void DisplayHelp()
    {
        _output.WriteLine("Available commands:");
        _output.WriteLine("  go [direction]    - Move in a direction (north, south, east, west, up, down)");
        _output.WriteLine("  n, s, e, w, u, d  - Shortcuts for movement");
        _output.WriteLine("  look              - Look around the current location");
        _output.WriteLine("  examine [object]  - Examine an object or item");
        _output.WriteLine("  take [item]       - Pick up an item");
        _output.WriteLine("  drop [item]       - Drop an item from your inventory");
        _output.WriteLine("  inventory         - Show your inventory");
        _output.WriteLine("  map               - Show information about the map file");
        _output.WriteLine("  help              - Display this help information");
        _output.WriteLine("  quit              - Exit the game");
        _output.WriteLine();
    }
    
    /// <summary>
    /// Displays a room description
    /// </summary>
    public void DisplayRoom(string name, string description, IEnumerable<string> exits, IEnumerable<string> items)
    {
        _output.WriteLine($"=== {name} ===");
        _output.WriteLine(description);
        _output.WriteLine();
        
        // Display exits
        if (exits.Any())
        {
            _output.WriteLine("Exits: " + string.Join(", ", exits));
        }
        else
        {
            _output.WriteLine("There are no obvious exits.");
        }
        
        // Display items
        if (items.Any())
        {
            _output.WriteLine("You can see: " + string.Join(", ", items));
        }
        
        _output.WriteLine();
    }
    
    /// <summary>
    /// Displays inventory contents
    /// </summary>
    public void DisplayInventory(IEnumerable<string> items)
    {
        _output.WriteLine("Inventory:");
        
        if (!items.Any())
        {
            _output.WriteLine("  Your inventory is empty.");
        }
        else
        {
            foreach (var item in items)
            {
                _output.WriteLine($"  {item}");
            }
        }
        
        _output.WriteLine();
    }
    
    /// <summary>
    /// Displays a message to the user
    /// </summary>
    public void DisplayMessage(string message)
    {
        _output.WriteLine(message);
        _output.WriteLine();
    }
    
    /// <summary>
    /// Displays an error message to the user
    /// </summary>
    public void DisplayError(string message)
    {
        _output.WriteLine($"Error: {message}");
        _output.WriteLine();
    }
    
    /// <summary>
    /// Gets input from the user
    /// </summary>
    public string GetInput()
    {
        _output.Write("> ");
        return _input.ReadLine() ?? string.Empty;
    }
    
    /// <summary>
    /// Displays a loading message while waiting for AI generation
    /// </summary>
    public void DisplayLoading(string message)
    {
        _output.WriteLine($"[{message}]");
    }
    
    /// <summary>
    /// Displays information about the map file
    /// </summary>
    public void DisplayMapInfo(string mapFilePath)
    {
        _output.WriteLine("A map of the game world has been generated.");
        _output.WriteLine($"You can view it at: {mapFilePath}");
        _output.WriteLine();
    }
}