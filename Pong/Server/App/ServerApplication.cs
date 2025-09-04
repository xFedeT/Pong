using System;
using System.Threading.Tasks;

namespace Pong.Server.App
{
    /// <summary>
    /// Console application entry point for the Pong multiplayer server
    /// </summary>
    static class ServerApplication
    {
        private static GameServer gameServer;
        private static bool isRunning = true;

        public static async Task Main(string[] args)
        {
            DisplayWelcomeMessage();
            
            try
            {
                await StartServerAsync();
                await HandleUserInputAsync();
            }
            catch (Exception ex)
            {
                DisplayErrorMessage($"Critical server error: {ex.Message}");
            }
            finally
            {
                await ShutdownServerAsync();
            }

            DisplayExitMessage();
        }

        private static void DisplayWelcomeMessage()
        {
            System.Console.WriteLine("=====================================");
            System.Console.WriteLine("    Pong Multiplayer Server v1.0    ");
            System.Console.WriteLine("=====================================");
            System.Console.WriteLine();
            System.Console.WriteLine("Server is initializing...");
            System.Console.WriteLine("Type 'help' for available commands or 'quit' to stop the server.");
            System.Console.WriteLine();
        }

        private static async Task StartServerAsync()
        {
            gameServer = new GameServer();
            
            var serverStartTask = gameServer.StartAsync();
            
            // Give the server a moment to start
            await Task.Delay(1000);
            
            if (!serverStartTask.IsCompleted)
            {
                System.Console.WriteLine("✓ Server started successfully!");
                System.Console.WriteLine($"✓ Listening on port 12346");
                System.Console.WriteLine("✓ Ready to accept player connections");
                System.Console.WriteLine();
            }
        }

        private static async Task HandleUserInputAsync()
        {
            while (isRunning)
            {
                System.Console.Write("Server> ");
                var userInput = System.Console.ReadLine()?.Trim().ToLowerInvariant();

                switch (userInput)
                {
                    case "help":
                    case "h":
                        DisplayHelpMessage();
                        break;

                    case "status":
                    case "s":
                        DisplayServerStatus();
                        break;

                    case "clear":
                    case "cls":
                        System.Console.Clear();
                        DisplayWelcomeMessage();
                        break;

                    case "quit":
                    case "exit":
                    case "q":
                        isRunning = false;
                        System.Console.WriteLine("Shutting down server...");
                        break;

                    case "restart":
                    case "r":
                        await RestartServerAsync();
                        break;

                    case "":
                        // Ignore empty input
                        break;

                    default:
                        System.Console.WriteLine($"Unknown command: '{userInput}'. Type 'help' for available commands.");
                        break;
                }
            }
        }

        private static void DisplayHelpMessage()
        {
            System.Console.WriteLine();
            System.Console.WriteLine("Available Commands:");
            System.Console.WriteLine("  help, h        - Show this help message");
            System.Console.WriteLine("  status, s      - Display current server status");
            System.Console.WriteLine("  restart, r     - Restart the server");
            System.Console.WriteLine("  clear, cls     - Clear the console screen");
            System.Console.WriteLine("  quit, exit, q  - Stop the server and exit");
            System.Console.WriteLine();
        }

        private static void DisplayServerStatus()
        {
            System.Console.WriteLine();
            System.Console.WriteLine("Server Status:");
            System.Console.WriteLine($"  Server State: {(gameServer != null ? "Running" : "Stopped")}");
            System.Console.WriteLine($"  Port: 12346");
            System.Console.WriteLine($"  Game Mode: Pong Multiplayer");
            System.Console.WriteLine($"  Max Players: 2");
            System.Console.WriteLine();
        }

        private static async Task RestartServerAsync()
        {
            System.Console.WriteLine("Restarting server...");
            
            try
            {
                gameServer?.Stop();
                await Task.Delay(2000); // Allow cleanup time
                
                gameServer?.Dispose();
                gameServer = new GameServer();
                
                await gameServer.StartAsync();
                System.Console.WriteLine("✓ Server restarted successfully!");
            }
            catch (Exception ex)
            {
                DisplayErrorMessage($"Failed to restart server: {ex.Message}");
            }
        }

        private static async Task ShutdownServerAsync()
        {
            try
            {
                gameServer?.Stop();
                gameServer?.Dispose();
                
                // Allow time for cleanup
                await Task.Delay(1000);
            }
            catch (Exception ex)
            {
                DisplayErrorMessage($"Error during shutdown: {ex.Message}");
            }
        }

        private static void DisplayErrorMessage(string message)
        {
            var originalColor = System.Console.ForegroundColor;
            System.Console.ForegroundColor = ConsoleColor.Red;
            System.Console.WriteLine($"ERROR: {message}");
            System.Console.ForegroundColor = originalColor;
        }

        private static void DisplayExitMessage()
        {
            System.Console.WriteLine();
            System.Console.WriteLine("Server shutdown complete. Press any key to exit...");
            System.Console.ReadKey();
        }
    }
}