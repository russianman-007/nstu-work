using DungeonCrawler.Core.Models;
using DungeonCrawler.Core.Models.Entities;
using System;

namespace DungeonCrawler.Core.Services
{
    public class ConsoleRenderer
    {
        public void Render(GameState gameState)
        {
            Console.Clear();

            Console.WriteLine("=== ИСКАТЕЛЬ ПОДЗЕМЕЛИЙ ===");
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

            var hero = gameState.Hero;
            if (hero != null)
            {
                Console.WriteLine("Враги в радиусе атаки:");
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
                        Console.WriteLine($"  {enemy.Name} на клетке ({pos.X},{pos.Y}) - Здоровье: {enemy.Health}");
                        enemiesNearby = true;
                    }
                }
                if (!enemiesNearby)
                {
                    Console.WriteLine("  - Врагов в радиусе атаки нет");
                }
            }

            Console.WriteLine();
            Console.WriteLine("Обозначения: @ = Герой; $ = Сокровище; S = Стоячий враг; M = Движущийся враг; # = Стена; E = Выход");
            Console.WriteLine(new string('=', 40));
            Console.WriteLine(GetStatus(gameState));
            Console.WriteLine(new string('-', 40));

            foreach (var message in gameState.Log)
            {
                Console.WriteLine(message);
            }

            Console.WriteLine(new string('-', 40));
            Console.WriteLine("Управление: | Стрелки/WASD = Движение | N = Новая игра | F5 = Сохранить игру       |\n            | Пробел = Атака          | Esc = Выход    | F9 = Загрузить сохранение |");
        }

        private string GetStatus(GameState gameState)
        {
            if (gameState.IsGameWon)
                return $"ПОБЕДА! Очки: {gameState.Hero?.Score ?? 0}";
            if (gameState.IsGameOver)
                return "ИГРА ОКОНЧЕНА";
            return $"Здоровье: {gameState.Hero?.Health ?? 0} | Очки: {gameState.Hero?.Score ?? 0} | Ход: {gameState.Turn}";
        }
    }
}