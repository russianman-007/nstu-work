// Управляет игровой логикой: ввод, сохранение, загрузка и боевая система
using System;
using System.Collections.Generic;
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

        // Обработка ввода пользователя (движение, атака)
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
                ConsoleKey.Spacebar => new Position(0, 0),
                _ => new Position(0, 0)
            };

            if (direction.X != 0 || direction.Y != 0)
            {
                var newPosition = CurrentGame.Hero.Position + direction;
                CurrentGame.Hero.TryMove(newPosition, CurrentGame);
                CurrentGame.Update();
                return true;
            }
            else if (key == ConsoleKey.Spacebar)
            {
                PerformAttack();
                CurrentGame.Update();
                return true;
            }
            return false;
        }

        // Выполнение атаки героем по 4 соседним клеткам
        private void PerformAttack()
        {
            var hero = CurrentGame.Hero;
            if (hero == null) return;

            bool attacked = false;
            var positionsToCheck = new Position[]
            {
                new Position(hero.Position.X + 1, hero.Position.Y),
                new Position(hero.Position.X - 1, hero.Position.Y),
                new Position(hero.Position.X, hero.Position.Y + 1),
                new Position(hero.Position.X, hero.Position.Y - 1)
            };

            foreach (var pos in positionsToCheck)
            {
                var cell = CurrentGame.Dungeon.GetCell(pos);
                if (cell?.Entity is Enemy enemy && enemy.IsAlive)
                {
                    hero.Attack(enemy);
                    CurrentGame.AddLog($"{hero.Name} атакует {enemy.Name} и наносит {hero.AttackPower} урона!");
                    attacked = true;
                    if (!enemy.IsAlive)
                    {
                        hero.Score += enemy.ScoreReward;
                        CurrentGame.AddLog($"{enemy.Name} повержен! +{enemy.ScoreReward} очков!");
                    }
                }
            }

            if (!attacked)
            {
                CurrentGame.AddLog("В радиусе атаки нет врагов!");
            }
        }

        // Сохранение игры. Блокируется, если герой погиб или стоит на выходе
        public void SaveGame()
        {
            try
            {
                var heroCell = CurrentGame.Dungeon.GetCell(CurrentGame.Hero.Position);
                if (CurrentGame.IsGameOver || heroCell?.Type == CellType.Exit)
                {
                    CurrentGame.AddLog("В данный момент сохраниться невозможно! Начните новую игру.");
                    return;
                }

                var saveData = new SaveData
                {
                    Width = CurrentGame.Dungeon.Width,
                    Height = CurrentGame.Dungeon.Height,
                    MapData = new List<string>(),
                    Hero = new SaveHeroData
                    {
                        X = CurrentGame.Hero.Position.X,
                        Y = CurrentGame.Hero.Position.Y,
                        Health = CurrentGame.Hero.Health,
                        Score = CurrentGame.Hero.Score,
                        AttackPower = CurrentGame.Hero.AttackPower
                    },
                    Entities = new List<SaveEntityData>(),
                    Turn = CurrentGame.Turn,
                    IsGameOver = CurrentGame.IsGameOver,
                    IsGameWon = CurrentGame.IsGameWon
                };

                for (int x = 0; x < saveData.Width; x++)
                {
                    for (int y = 0; y < saveData.Height; y++)
                    {
                        saveData.MapData.Add($"{x},{y},{(int)CurrentGame.Dungeon.Grid[x, y].Type}");
                    }
                }

                foreach (var ent in CurrentGame.Entities)
                {
                    if (ent is Hero) continue;

                    var savedEnt = new SaveEntityData
                    {
                        Type = ent.GetType().Name,
                        X = ent.Position.X,
                        Y = ent.Position.Y
                    };

                    if (ent is Enemy enemy)
                    {
                        savedEnt.Health = enemy.Health;
                        savedEnt.AttackPower = enemy.AttackPower;
                        savedEnt.ScoreReward = enemy.ScoreReward;
                    }
                    else if (ent is Treasure treasure)
                    {
                        savedEnt.Value = treasure.Value;
                        savedEnt.ScoreReward = treasure.ScoreReward;
                    }

                    saveData.Entities.Add(savedEnt);
                }

                var json = JsonSerializer.Serialize(saveData, new JsonSerializerOptions { WriteIndented = true });
                File.WriteAllText(SaveFileName, json);
                CurrentGame.AddLog("Игра успешно сохранена!");
            }
            catch (Exception ex)
            {
                CurrentGame.AddLog($"Ошибка сохранения: {ex.Message}");
            }
        }

        // Полная загрузка карты, сущностей и статов из файла сохранения
        public bool LoadGame()
        {
            try
            {
                if (!File.Exists(SaveFileName))
                {
                    CurrentGame.AddLog("Файл сохранения не найден!");
                    return false;
                }

                var json = File.ReadAllText(SaveFileName);
                var saveData = JsonSerializer.Deserialize<SaveData>(json);

                if (saveData == null)
                {
                    CurrentGame.AddLog("Файл сохранения поврежден!");
                    return false;
                }

                var newGame = new GameState(saveData.Width, saveData.Height);

                for (int x = 0; x < newGame.Dungeon.Width; x++)
                {
                    for (int y = 0; y < newGame.Dungeon.Height; y++)
                    {
                        newGame.Dungeon.Grid[x, y].Entity = null;
                    }
                }

                newGame.Entities.Clear();
                newGame.Log.Clear();

                foreach (var md in saveData.MapData)
                {
                    var parts = md.Split(',');
                    if (parts.Length != 3) continue;

                    int x = int.Parse(parts[0]);
                    int y = int.Parse(parts[1]);
                    newGame.Dungeon.Grid[x, y].Type = (CellType)int.Parse(parts[2]);
                }

                var hero = new Hero(new Position(saveData.Hero.X, saveData.Hero.Y))
                {
                    Health = saveData.Hero.Health,
                    Score = saveData.Hero.Score,
                    AttackPower = saveData.Hero.AttackPower
                };
                newGame.Dungeon.PlaceEntity(hero);
                newGame.Entities.Add(hero);
                newGame.Hero = hero;

                foreach (var entData in saveData.Entities)
                {
                    Entity ent = null;
                    var pos = new Position(entData.X, entData.Y);

                    if (entData.Type == "Treasure")
                    {
                        ent = new Treasure(pos, entData.Value, entData.ScoreReward);
                    }
                    else if (entData.Type == "MovingEnemy")
                    {
                        ent = new MovingEnemy(pos, entData.Health, entData.AttackPower, entData.ScoreReward);
                    }
                    else if (entData.Type == "StationaryEnemy")
                    {
                        ent = new StationaryEnemy(pos, entData.Health, entData.AttackPower, entData.ScoreReward);
                    }

                    if (ent != null)
                    {
                        newGame.Dungeon.PlaceEntity(ent);
                        newGame.Entities.Add(ent);
                    }
                }

                newGame.Turn = saveData.Turn;
                newGame.IsGameOver = saveData.IsGameOver;
                newGame.IsGameWon = saveData.IsGameWon;

                CurrentGame = newGame;
                CurrentGame.AddLog("Сохранение успешно загружено!");
                return true;
            }
            catch (Exception ex)
            {
                CurrentGame.AddLog($"Ошибка загрузки: {ex.Message}");
                return false;
            }
        }

        // Сброс и создание нового экземпляра GameState
        public void NewGame()
        {
            CurrentGame = new GameState();
            CurrentGame.AddLog("Начало новой игры!");
        }

        private class SaveData
        {
            public int Width { get; set; }
            public int Height { get; set; }
            public List<string> MapData { get; set; }
            public SaveHeroData Hero { get; set; }
            public List<SaveEntityData> Entities { get; set; }
            public int Turn { get; set; }
            public bool IsGameOver { get; set; }
            public bool IsGameWon { get; set; }
        }

        private class SaveHeroData
        {
            public int X { get; set; }
            public int Y { get; set; }
            public int Health { get; set; }
            public int Score { get; set; }
            public int AttackPower { get; set; }
        }

        private class SaveEntityData
        {
            public string Type { get; set; }
            public int X { get; set; }
            public int Y { get; set; }
            public int Health { get; set; }
            public int AttackPower { get; set; }
            public int Value { get; set; }
            public int ScoreReward { get; set; }
        }
    }
}