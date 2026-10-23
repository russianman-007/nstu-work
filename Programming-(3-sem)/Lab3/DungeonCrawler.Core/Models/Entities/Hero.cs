using System;
using System.Linq;
using DungeonCrawler.Core.Models;

namespace DungeonCrawler.Core.Models.Entities
{
    public class Hero : Entity
    {
        public override string Symbol => "@";
        public override ConsoleColor Color => ConsoleColor.Yellow;
        public override bool IsPassable => false;
        public int Health { get; set; } = 100;
        public int Score { get; set; } = 0;
        public int AttackPower { get; set; } = 10;

        public Hero(Position position) : base(position, "Герой")
        {
        }

        public bool TryMove(Position newPosition, GameState gameState)
        {
            if (gameState == null) return false;
            var cell = gameState.Dungeon.GetCell(newPosition);

            if (cell?.Entity is Enemy enemy && enemy.IsAlive)
            {
                Attack(enemy);
                gameState.AddLog($"{Name} атакует {enemy.Name} и наносит {AttackPower} урона!");
                if (!enemy.IsAlive)
                {
                    gameState.AddLog($"{enemy.Name} повержен!");
                    cell.Entity = null;
                    return false;
                }
                return false;
            }

            if (cell?.IsPassable == true)
            {
                var oldCell = gameState.Dungeon.GetCell(Position);
                if (oldCell != null)
                    oldCell.Entity = null;

                var treasures = gameState.Entities.Where(e => e.Position == newPosition && e is Treasure).Cast<Treasure>().ToList();
                foreach (var treasure in treasures)
                {
                    treasure.Collect(this, gameState);
                }

                Position = newPosition;
                cell.Entity = this;
                return true;
            }
            return false;
        }

        public void Attack(Enemy enemy)
        {
            if (enemy != null)
            {
                enemy.Health -= AttackPower;
            }
        }

        public override void Update(GameState gameState)
        {
            if (Health < 100)
            {
                Health = Math.Min(100, Health + 1);
            }
        }

        public override string Serialize()
        {
            return base.Serialize() + $"|{Health}|{Score}|{AttackPower}";
        }

        public override void Deserialize(string data)
        {
            base.Deserialize(data);
            var parts = data.Split('|');
            if (parts.Length >= 6)
            {
                if (int.TryParse(parts[3], out int health)) Health = health;
                if (int.TryParse(parts[4], out int score)) Score = score;
                if (int.TryParse(parts[5], out int attackPower)) AttackPower = attackPower;
            }
        }
    }
}