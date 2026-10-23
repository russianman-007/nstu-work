using System;
using System.Collections.Generic;
using System.Linq;
using DungeonCrawler.Core.Models.Entities;

namespace DungeonCrawler.Core.Models
{
    public class GameState
    {
        public Dungeon Dungeon { get; set; }
        public Hero Hero { get; set; }
        public List<Entity> Entities { get; set; }
        public List<string> Log { get; set; }
        public int Turn { get; set; }
        public bool IsGameOver { get; set; }
        public bool IsGameWon { get; set; }

        public GameState(int width = 20, int height = 10)
        {
            Dungeon = new Dungeon(width, height);
            Entities = new List<Entity>();
            Log = new List<string>();
            Turn = 0;
            IsGameOver = false;
            IsGameWon = false;
            InitializeGame();
        }

        private void InitializeGame()
        {
            var emptyPositions = Dungeon.GetEmptyPositions();
            if (emptyPositions.Count == 0) return;
            var random = new Random();

            // Размещаем героя
            var heroPos = emptyPositions[random.Next(emptyPositions.Count)];
            Hero = new Hero(heroPos);
            Dungeon.PlaceEntity(Hero);
            Entities.Add(Hero);
            emptyPositions.Remove(heroPos);

            // Размещаем сокровища (3-5 штук)
            int treasureCount = random.Next(3, 6);
            for (int i = 0; i < treasureCount && emptyPositions.Count > 0; i++)
            {
                var pos = emptyPositions[random.Next(emptyPositions.Count)];
                var treasure = new Treasure(pos, random.Next(10, 101));
                Dungeon.PlaceEntity(treasure);
                Entities.Add(treasure);
                emptyPositions.Remove(pos);
            }

            // Размещаем неподвижных врагов (2-3 штуки)
            int stationaryCount = random.Next(2, 4);
            for (int i = 0; i < stationaryCount && emptyPositions.Count > 0; i++)
            {
                var pos = emptyPositions[random.Next(emptyPositions.Count)];
                var enemy = new StationaryEnemy(pos);
                Dungeon.PlaceEntity(enemy);
                Entities.Add(enemy);
                emptyPositions.Remove(pos);
            }

            // Размещаем подвижных врагов (1-2 штуки)
            int movingCount = random.Next(1, 3);
            for (int i = 0; i < movingCount && emptyPositions.Count > 0; i++)
            {
                var pos = emptyPositions[random.Next(emptyPositions.Count)];
                var enemy = new MovingEnemy(pos);
                Dungeon.PlaceEntity(enemy);
                Entities.Add(enemy);
                emptyPositions.Remove(pos);
            }

            Log.Add("Welcome to Dungeon Crawler!");
        }

        public void Update()
        {
            if (IsGameOver || IsGameWon) return;
            Turn++;
            Log.Clear();
            Log.Add($"Turn {Turn}");

            // Обновляем всех сущностей
            foreach (var entity in Entities.ToList())
            {
                entity.Update(this);
            }

            // Проверяем победу (герой на выходе)
            var heroCell = Dungeon.GetCell(Hero.Position);
            if (heroCell?.Type == CellType.Exit)
            {
                IsGameWon = true;
                Log.Add("Congratulations! You found the exit!");
                Log.Add($"Final Score: {Hero.Score}");
            }

            // Очищаем мертвые сущности
            Entities.RemoveAll(e => e is Enemy enemy && !enemy.IsAlive);
        }

        public void RemoveEntity(Entity entity)
        {
            Entities.Remove(entity);
            Dungeon.RemoveEntity(entity.Position);
        }

        public string Serialize()
        {
            try
            {
                var data = new List<string>();
                // Только основные данные
                data.Add($"Turn:{Turn}");
                data.Add($"IsGameOver:{IsGameOver}");
                data.Add($"IsGameWon:{IsGameWon}");
                if (Hero != null)
                {
                    data.Add($"Hero:{Hero.Position.X},{Hero.Position.Y},{Hero.Health},{Hero.Score}");
                }
                return string.Join("|", data);
            }
            catch
            {
                return "Turn:0|IsGameOver:false|IsGameWon:false|Hero:1,1,100,0";
            }
        }

        public void Deserialize(string data)
        {
            try
            {
                // Очищаем текущие данные
                Entities?.Clear();
                Log?.Clear();
                var parts = data.Split('|', StringSplitOptions.RemoveEmptyEntries);
                if (parts.Length == 0) return;

                // Временные списки для загрузки
                var heroes = new List<Hero>();
                var entitiesToAdd = new List<Entity>();

                foreach (var part in parts)
                {
                    var keyValue = part.Split(':', 2);
                    if (keyValue.Length != 2) continue;
                    var key = keyValue[0];
                    var value = keyValue[1];

                    switch (key)
                    {
                        case "Turn":
                            if (int.TryParse(value, out int turn))
                                Turn = turn;
                            break;
                        case "IsGameOver":
                            if (bool.TryParse(value, out bool isGameOver))
                                IsGameOver = isGameOver;
                            break;
                        case "IsGameWon":
                            if (bool.TryParse(value, out bool isGameWon))
                                IsGameWon = isGameWon;
                            break;
                        case "Dungeon":
                            // Создаем временный Dungeon
                            var tempDungeon = new Dungeon(1, 1);
                            tempDungeon.Deserialize(value);
                            Dungeon = tempDungeon;
                            break;
                        case "Hero":
                            var heroData = value.Split('|');
                            if (heroData.Length >= 3)
                            {
                                // Позиция из первых частей данных
                                var posParts = heroData[1].Split(',');
                                if (posParts.Length == 2 &&
                                    int.TryParse(posParts[0], out int x) &&
                                    int.TryParse(posParts[1], out int y))
                                {
                                    var hero = new Hero(new Position(x, y));
                                    hero.Deserialize(value);
                                    heroes.Add(hero);
                                }
                            }
                            break;
                        case "Entity":
                            var entityData = value.Split('|');
                            if (entityData.Length > 0)
                            {
                                Entity entity = null;
                                var entityType = entityData[0];
                                // Базовые координаты для создания
                                var basePos = new Position(0, 0);
                                switch (entityType)
                                {
                                    case "Treasure":
                                        entity = new Treasure(basePos);
                                        break;
                                    case "StationaryEnemy":
                                        entity = new StationaryEnemy(basePos);
                                        break;
                                    case "MovingEnemy":
                                        entity = new MovingEnemy(basePos);
                                        break;
                                }
                                if (entity != null)
                                {
                                    entity.Deserialize(value);
                                    entitiesToAdd.Add(entity);
                                }
                            }
                            break;
                    }
                }

                // Восстанавливаем героя
                if (heroes.Count > 0)
                {
                    Hero = heroes[0];
                    Dungeon.PlaceEntity(Hero);
                }

                // Восстанавливаем остальные сущности
                Entities = new List<Entity>();
                if (Hero != null) Entities.Add(Hero);
                foreach (var entity in entitiesToAdd)
                {
                    // Проверяем, что позиция валидна
                    if (Dungeon.IsValidPosition(entity.Position))
                    {
                        Dungeon.PlaceEntity(entity);
                        Entities.Add(entity);
                    }
                }
                Log = new List<string>();
                Log.Add("Game loaded from save.");
            }
            catch (Exception ex)
            {
                // Создаем новую игру в случае ошибки
                var width = 20;
                var height = 10;
                Dungeon = new Dungeon(width, height);
                Entities = new List<Entity>();
                Log = new List<string>();
                Turn = 0;
                IsGameOver = false;
                IsGameWon = false;
                InitializeGame();
                Log.Add($"Load error: {ex.Message}");
                Log.Add("Created new game instead.");
            }
        }
    }
}