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

        public void AddLog(string message)
        {
            if (Log.Count > 0 && Log[Log.Count - 1] == message)
                return;

            Log.Add(message);

            while (Log.Count > 8)
            {
                Log.RemoveAt(0);
            }
        }

        private void InitializeGame()
        {
            var emptyPositions = Dungeon.GetEmptyPositions();
            if (emptyPositions.Count == 0) return;
            var random = new Random();

            var heroPos = emptyPositions[random.Next(emptyPositions.Count)];
            Hero = new Hero(heroPos);
            Dungeon.PlaceEntity(Hero);
            Entities.Add(Hero);
            emptyPositions.Remove(heroPos);

            int treasureCount = random.Next(3, 6);
            for (int i = 0; i < treasureCount && emptyPositions.Count > 0; i++)
            {
                var pos = emptyPositions[random.Next(emptyPositions.Count)];
                var treasure = new Treasure(pos, random.Next(10, 101), 25);
                Dungeon.PlaceEntity(treasure);
                Entities.Add(treasure);
                emptyPositions.Remove(pos);
            }

            int stationaryCount = random.Next(2, 4);
            for (int i = 0; i < stationaryCount && emptyPositions.Count > 0; i++)
            {
                var pos = emptyPositions[random.Next(emptyPositions.Count)];
                var enemy = new StationaryEnemy(pos);
                Dungeon.PlaceEntity(enemy);
                Entities.Add(enemy);
                emptyPositions.Remove(pos);
            }

            int movingCount = random.Next(1, 3);
            for (int i = 0; i < movingCount && emptyPositions.Count > 0; i++)
            {
                var pos = emptyPositions[random.Next(emptyPositions.Count)];
                var enemy = new MovingEnemy(pos);
                Dungeon.PlaceEntity(enemy);
                Entities.Add(enemy);
                emptyPositions.Remove(pos);
            }

            AddLog("Добро пожаловать в ПОДЗЕМЕЛЬЕ!");
        }

        public void Update()
        {
            if (IsGameOver || IsGameWon) return;
            Turn++;
            Log.Clear();
            AddLog($"Ход {Turn}");

            foreach (var entity in Entities.ToList())
            {
                entity.Update(this);
            }

            var heroCell = Dungeon.GetCell(Hero.Position);
            if (heroCell?.Type == CellType.Exit)
            {
                IsGameWon = true;
                AddLog("ПОЗДРАВЛЯЕМ! Вы нашли выход!");
                AddLog($"Итоговый счет: {Hero.Score}");
            }

            Entities.RemoveAll(e => e is Enemy enemy && !enemy.IsAlive);
        }

        public void RemoveEntity(Entity entity)
        {
            Entities.Remove(entity);
            Dungeon.RemoveEntity(entity.Position);
        }
    }
}