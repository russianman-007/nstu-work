using System;
using DungeonCrawler.Core.Services;

namespace DungeonCrawler
{
    class Program
    {
        static void Main(string[] args)
        {
            var gameService = new GameService();
            var renderer = new ConsoleRenderer();
            Console.Clear();
            Console.WriteLine("=== DUNGEON CRAWLER ===");
            Console.WriteLine("Press any key to start...");
            Console.ReadKey();
            bool running = true;
            while (running)
            {
                renderer.Render(gameService.CurrentGame);
                if (Console.KeyAvailable)
                {
                    var key = Console.ReadKey(true).Key;
                    switch (key)
                    {
                        case ConsoleKey.Escape:
                            running = false;
                            Console.WriteLine("Goodbye!");
                            break;
                        case ConsoleKey.F5:
                            gameService.SaveGame();
                            break;
                        case ConsoleKey.F9:
                            gameService.LoadGame();
                            break;
                        case ConsoleKey.N:
                            gameService.NewGame();
                            break;
                        default:
                            gameService.ProcessInput(key);
                            break;
                    }
                }
                System.Threading.Thread.Sleep(50);
            }
            Console.WriteLine("Press any key to exit...");
            Console.ReadKey();
        }
    }
}