using System;
using DungeonCrawler.Core.Models;

namespace DungeonCrawler.Core.Services
{
    public class ConsoleRenderer
    {
        public void Render(GameState gameState)
        {
            Console.Clear();
            // Рендерим карту
            Console.WriteLine("=== DUNGEON CRAWLER ===");
            Console.WriteLine();
            var map = gameState.Dungeon.Render();
            var lines = map.Split('\n');
            for (int y = 0; y < lines.Length && y < gameState.Dungeon.Height; y++)
            {
                for (int x = 0; x < lines[y].Length && x < gameState.Dungeon.Width; x++)
                {
                    var cell = gameState.Dungeon.GetCell(new Position(x, y));
                    if (cell?.Entity != null)
                    {
                        Console.ForegroundColor = cell.Entity.Color;
                        Console.Write(cell.Entity.Symbol);
                        Console.ResetColor();
                    }
                    else
                    {
                        Console.Write(lines[y][x]);
                    }
                }
                Console.WriteLine();
            }
            // Показываем врагов в радиусе атаки
            var hero = gameState.Hero;
            if (hero != null)
            {
                System.Console.WriteLine("Enemies in attack range:");
                bool enemiesNearby = false;
                var attackPositions = new Position[]
                {
                    new Position(hero.Position.X + 1, hero.Position.Y),
                    new Position(hero.Position.X - 1, hero.Position.Y),
                    new Position(hero.Position.X, hero.Position.Y + 1),
                    new Position(hero.Position.X, hero.Position.Y - 1)
                };
                foreach (var pos in attackPositions)
                {
                    var cell = gameState.Dungeon.GetCell(pos);
                    if (cell?.Entity is Enemy enemy && enemy.IsAlive)
                    {
                        System.Console.WriteLine($"  {enemy.Name} at ({pos.X},{pos.Y}) - Health: {enemy.Health}");
                        enemiesNearby = true;
                    }
                }
                if (!enemiesNearby)
                {
                    System.Console.WriteLine("  No enemies in attack range");
                }
            }
            // Рендерим статус
            System.Console.WriteLine();
            System.Console.WriteLine("Legend: @=Hero $=Treasure S=Stationary M=Moving #=Wall E=Exit");
            System.Console.WriteLine(new string('=', 40));
            System.Console.WriteLine(GetStatus(gameState));
            System.Console.WriteLine(new string('-', 40));
            // Рендерим лог
            foreach (var message in gameState.Log)
            {
                Console.WriteLine(message);
            }
            System.Console.WriteLine(new string('-', 40));
            System.Console.WriteLine("Controls: Arrows/WASD=Move | SPACE=Attack | F5=Save | F9=Load | N=New Game | Esc=Exit");
        }

        private string GetStatus(GameState gameState)
        {
            if (gameState.IsGameWon)
                return $"VICTORY! Score: {gameState.Hero?.Score ?? 0}";
            if (gameState.IsGameOver)
                return "GAME OVER";
            return $"Health: {gameState.Hero?.Health ?? 0} | Score: {gameState.Hero?.Score ?? 0} | Turn: {gameState.Turn}";
        }
    }
}