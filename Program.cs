using ZAIrk.Models;
using ZAIrk.Services;
using ZAIrk.UI;

namespace ZAIrk;

/// <summary>
/// Main program class for the zAIrk game
/// </summary>
public class Program
{
    /// <summary>
    /// Entry point for the application
    /// </summary>
    public static async Task Main(string[] args)
    {
        // Default theme if none provided
        string theme = "fantasy";
        
        // Parse command line arguments
        if (args.Length > 0)
        {
            theme = args[0];
        }
        
        // Create UI
        var ui = new GameUI(Console.Out, Console.In);
        
        // Display title and introduction
        ui.DisplayTitle();
        ui.DisplayIntroduction();
        
        // Get API key from environment variable or prompt the user
        string? apiKey = Environment.GetEnvironmentVariable("OPENAI_API_KEY");
        if (string.IsNullOrEmpty(apiKey))
        {
            Console.Write("Enter your OpenAI API key: ");
            apiKey = Console.ReadLine();
            
            if (string.IsNullOrEmpty(apiKey))
            {
                ui.DisplayError("API key is required to generate game content.");
                return;
            }
        }
        
        // Create content generation service
        var contentGenerationService = new ContentGenerationService(apiKey, "gpt-4");
        
        // Generate game world
        ui.DisplayLoading("Generating game world...");
        GameWorld gameWorld;
        
        try
        {
            gameWorld = await contentGenerationService.GenerateGameWorldAsync(theme);
        }
        catch (Exception ex)
        {
            ui.DisplayError($"Failed to generate game world: {ex.Message}");
            return;
        }
        
        // Create game service
        var gameService = new GameService(gameWorld);
        
        // Create command parser
        var commandParser = new CommandParserService();
        
        // Main game loop
        bool isRunning = true;
        
        // Display the starting room
        DisplayCurrentRoom(ui, gameService);
        
        while (isRunning)
        {
            // Get player input
            var input = ui.GetInput();
            
            // Parse command
            var (command, arguments) = commandParser.ParseCommand(input);
            
            // Process command
            if (string.IsNullOrEmpty(command))
            {
                continue;
            }
            else if (commandParser.IsMovementCommand(command, arguments, out var direction))
            {
                if (gameService.Move(direction))
                {
                    DisplayCurrentRoom(ui, gameService);
                }
                else
                {
                    ui.DisplayMessage($"You can't go {direction.ToDisplayString()} from here.");
                }
            }
            else if (commandParser.IsTakeCommand(command, arguments, out var takeItemName))
            {
                if (gameService.TakeItem(takeItemName))
                {
                    ui.DisplayMessage($"You take the {takeItemName}.");
                }
                else
                {
                    ui.DisplayMessage($"You can't take the {takeItemName}.");
                }
            }
            else if (commandParser.IsDropCommand(command, arguments, out var dropItemName))
            {
                if (gameService.DropItem(dropItemName))
                {
                    ui.DisplayMessage($"You drop the {dropItemName}.");
                }
                else
                {
                    ui.DisplayMessage($"You don't have a {dropItemName} to drop.");
                }
            }
            else if (commandParser.IsLookCommand(command))
            {
                DisplayCurrentRoom(ui, gameService);
            }
            else if (commandParser.IsExamineCommand(command, arguments, out var examineTarget))
            {
                ExamineObject(ui, gameService, examineTarget);
            }
            else if (commandParser.IsInventoryCommand(command))
            {
                var inventory = gameService.GetInventory().Select(i => i.Name);
                ui.DisplayInventory(inventory);
            }
            else if (commandParser.IsHelpCommand(command))
            {
                ui.DisplayHelp();
            }
            else if (commandParser.IsQuitCommand(command))
            {
                ui.DisplayMessage("Thanks for playing zAIrk!");
                isRunning = false;
            }
            else
            {
                ui.DisplayMessage("I don't understand that command. Type 'help' for a list of commands.");
            }
        }
    }
    
    /// <summary>
    /// Displays the current room
    /// </summary>
    private static void DisplayCurrentRoom(GameUI ui, GameService gameService)
    {
        var currentRoom = gameService.GetCurrentRoom();
        if (currentRoom == null)
        {
            ui.DisplayError("Current room not found.");
            return;
        }
        
        var exits = gameService.GetAvailableExits().Select(e => e.ToDisplayString());
        var items = gameService.GetItemsInRoom().Select(i => i.Name);
        
        ui.DisplayRoom(currentRoom.Name, currentRoom.Description, exits, items);
    }
    
    /// <summary>
    /// Examines an object (item or room)
    /// </summary>
    private static void ExamineObject(GameUI ui, GameService gameService, string target)
    {
        // Check if the target is an item in the inventory
        var inventoryItem = gameService.GetItemFromInventory(target);
        if (inventoryItem != null)
        {
            ui.DisplayMessage(inventoryItem.DetailedDescription ?? inventoryItem.Description);
            return;
        }
        
        // Check if the target is an item in the room
        var roomItem = gameService.GetItemFromRoom(target);
        if (roomItem != null)
        {
            ui.DisplayMessage(roomItem.DetailedDescription ?? roomItem.Description);
            return;
        }
        
        // Check if the target is the room itself
        if (target == "room" || target == "here" || target == "around")
        {
            var currentRoom = gameService.GetCurrentRoom();
            if (currentRoom != null)
            {
                ui.DisplayMessage(currentRoom.DetailedDescription ?? currentRoom.Description);
                return;
            }
        }
        
        ui.DisplayMessage($"You don't see a {target} here.");
    }
}
