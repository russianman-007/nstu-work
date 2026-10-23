using DungeonCrawler.Core.Interfaces;
using DungeonCrawler.Core.Models;
using System;
using System.Xml.Linq;

namespace DungeonCrawler.Core.Models.Entities
{
    public class Hero : Entity  // Должно быть Entity, а не IEntity
    {
        public override string Symbol => "@";
        public override ConsoleColor Color => ConsoleColor.Yellow;
        public override bool IsPassable => false;
        public int Health { get; set; } = 100;
        public int Score { get; set; }
        public int AttackPower { get; set; } = 10;

        public Hero(Position position) : base(position, "Hero")
        {
        }

        public bool TryMove(Position newPosition, GameState gameState)
        {
            if (gameState == null) return false;
            var cell = gameState.Dungeon.GetCell(newPosition);

            // Если на целевой клетке враг - атакуем
            if (cell?.Entity is Enemy enemy && enemy.IsAlive)
            {
                Attack(enemy);
                gameState.Log.Add($"{Name} attacks {enemy.Name} for {AttackPower} damage!");
                if (!enemy.IsAlive)
                {
                    gameState.Log.Add($"{enemy.Name} defeated!");
                    // После убийства врага можно занять его клетку
                    cell.Entity = null;
                    return false;  // Не перемещаемся
                }
                return false; // Не перемещаемся при атаке
            }

            if (cell?.IsPassable == true)
            {
                // Освобождаем старую клетку
                var oldCell = gameState.Dungeon.GetCell(Position);
                if (oldCell != null)
                    oldCell.Entity = null;
                // Занимаем новую клетку
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
            // Герой может восстанавливать здоровье со временем
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
                if (int.TryParse(parts[3], out int health))
                    Health = health;
                if (int.TryParse(parts[4], out int score))
                    Score = score;
                if (int.TryParse(parts[5], out int attackPower))
                    AttackPower = attackPower;
            }
        }
    }
}