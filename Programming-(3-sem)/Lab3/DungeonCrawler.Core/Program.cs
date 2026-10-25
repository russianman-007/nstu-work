// Точка входа в приложение. Управляет главным циклом игры
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
            Console.WriteLine("=== ИСКАТЕЛЬ ПОДЗЕМЕЛИЙ ===");
            Console.WriteLine("Нажмите любую кнопку для начала игры...");
            Console.ReadKey();

            renderer.Render(gameService.CurrentGame);

            bool running = true;
            while (running)
            {
                if (Console.KeyAvailable)
                {
                    var key = Console.ReadKey(true).Key;
                    bool needsRender = false;

                    switch (key)
                    {
                        case ConsoleKey.Escape:
                            running = false;
                            Console.WriteLine("До свидания!");
                            break;
                        case ConsoleKey.F5:
                            gameService.SaveGame();
                            needsRender = true;
                            break;
                        case ConsoleKey.F9:
                            gameService.LoadGame();
                            needsRender = true;
                            break;
                        case ConsoleKey.N:
                            gameService.NewGame();
                            needsRender = true;
                            break;
                        default:
                            needsRender = gameService.ProcessInput(key);
                            break;
                    }

                    if (needsRender)
                    {
                        renderer.Render(gameService.CurrentGame);
                    }
                }
                System.Threading.Thread.Sleep(50);
            }
            Console.WriteLine("Нажмите любую кнопку для выхода из игры...");
            Console.ReadKey();
        }
    }
}