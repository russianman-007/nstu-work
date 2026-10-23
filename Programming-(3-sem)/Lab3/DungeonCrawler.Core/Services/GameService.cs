using System;
using System.IO;
using System.Text.Json;
using DungeonCrawler.Core.Models;
using DungeonCrawler.Core.Models.Entities;

namespace DungeonCrawler.Core.Services
{
    public class GameService
    {
        private const string SaveFileName = "savegame.json";
        public GameState CurrentGame { get; private set; }

        public GameService()
        {
            CurrentGame = new GameState();
        }

        public bool ProcessInput(ConsoleKey key)
        {
            if (CurrentGame.IsGameOver || CurrentGame.IsGameWon)
                return false;

            Position direction = key switch
            {
                ConsoleKey.UpArrow => new Position(0, -1),
                ConsoleKey.DownArrow => new Position(0, 1),
                ConsoleKey.LeftArrow => new Position(-1, 0),
                ConsoleKey.RightArrow => new Position(1, 0),
                ConsoleKey.W => new Position(0, -1),
                ConsoleKey.S => new Position(0, 1),
                ConsoleKey.A => new Position(-1, 0),
                ConsoleKey.D => new Position(1, 0),
                ConsoleKey.Spacebar => new Position(0, 0),  // Для атаки
                _ => new Position(0, 0)
            };

            if (direction.X != 0 || direction.Y != 0)
            {
                // Движение героя
                var newPosition = CurrentGame.Hero.Position + direction;
                CurrentGame.Hero.TryMove(newPosition, CurrentGame);
                CurrentGame.Update();
                return true;
            }
            else if (key == ConsoleKey.Spacebar)
            {
                // Атака героя
                PerformAttack();
                CurrentGame.Update();
                return true;
            }
            return false;
        }

        // Выполнение атаки героем
        private void PerformAttack()
        {
            var hero = CurrentGame.Hero;
            if (hero == null) return;

            bool attacked = false;
            // Проверяем 4 соседние клетки
            var positionsToCheck = new Position[]
            {
                new Position(hero.Position.X + 1, hero.Position.Y),  // Вправо
                new Position(hero.Position.X - 1, hero.Position.Y),  // Влево
                new Position(hero.Position.X, hero.Position.Y + 1),  // Вниз
                new Position(hero.Position.X, hero.Position.Y - 1)   // Вверх
            };

            foreach (var pos in positionsToCheck)
            {
                var cell = CurrentGame.Dungeon.GetCell(pos);
                if (cell?.Entity is Enemy enemy && enemy.IsAlive)
                {
                    hero.Attack(enemy);
                    CurrentGame.Log.Add($"{hero.Name} attacks {enemy.Name} for {hero.AttackPower} damage!");
                    attacked = true;
                    if (!enemy.IsAlive)
                    {
                        CurrentGame.Log.Add($"{enemy.Name} defeated!");
                    }
                }
            }

            if (!attacked)
            {
                CurrentGame.Log.Add("No enemies in attack range!");
            }
        }

        public void SaveGame()
        {
            try
            {
                var saveData = new SaveData
                {
                    HeroX = CurrentGame.Hero.Position.X,
                    HeroY = CurrentGame.Hero.Position.Y,
                    HeroHealth = CurrentGame.Hero.Health,
                    HeroScore = CurrentGame.Hero.Score,
                    Turn = CurrentGame.Turn,
                    IsGameOver = CurrentGame.IsGameOver,
                    IsGameWon = CurrentGame.IsGameWon
                };
                var json = JsonSerializer.Serialize(saveData, new JsonSerializerOptions { WriteIndented = true });
                File.WriteAllText(SaveFileName, json);
                CurrentGame.Log.Add("Game saved successfully!");
            }
            catch (Exception ex)
            {
                CurrentGame.Log.Add($"Save failed: {ex.Message}");
            }
        }

        public bool LoadGame()
        {
            try
            {
                if (!File.Exists(SaveFileName))
                {
                    CurrentGame.Log.Add("No save file found!");
                    return false;
                }

                var json = File.ReadAllText(SaveFileName);
                var saveData = JsonSerializer.Deserialize<SaveData>(json);

                if (saveData == null)
                {
                    CurrentGame.Log.Add("Save file is corrupted!");
                    return false;
                }

                // Создаем новую игру с загруженными параметрами
                var newGame = new GameState();

                // Перемещаем героя на сохраненную позицию
                var newPosition = new Position(saveData.HeroX, saveData.HeroY);
                var targetCell = newGame.Dungeon.GetCell(newPosition);

                if (targetCell?.IsPassable == true)
                {
                    // Освобождаем старую позицию
                    var oldCell = newGame.Dungeon.GetCell(newGame.Hero.Position);
                    if (oldCell != null) oldCell.Entity = null;

                    // Занимаем новую позицию
                    newGame.Hero.Position = newPosition;
                    targetCell.Entity = newGame.Hero;

                    // Восстанавливаем параметры героя
                    newGame.Hero.Health = Math.Min(100, Math.Max(0, saveData.HeroHealth));
                    newGame.Hero.Score = Math.Max(0, saveData.HeroScore);

                    // Восстанавливаем состояние игры
                    newGame.Turn = Math.Max(0, saveData.Turn);
                    newGame.IsGameOver = saveData.IsGameOver;
                    newGame.IsGameWon = saveData.IsGameWon;

                    CurrentGame = newGame;
                    CurrentGame.Log.Add("Game loaded successfully!");
                    return true;
                }
                else
                {
                    CurrentGame.Log.Add("Cannot load - saved position is blocked!");
                    return false;
                }
            }
            catch (Exception ex)
            {
                CurrentGame.Log.Add($"Load failed: {ex.Message}");
                return false;
            }
        }

        public void NewGame()
        {
            CurrentGame = new GameState();
            CurrentGame.Log.Add("New game started!");
        }

        private class SaveData
        {
            public int HeroX { get; set; }
            public int HeroY { get; set; }
            public int HeroHealth { get; set; }
            public int HeroScore { get; set; }
            public int Turn { get; set; }
            public bool IsGameOver { get; set; }
            public bool IsGameWon { get; set; }
        }
    }
}