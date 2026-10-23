using DungeonCrawler.Core.Interfaces;
using DungeonCrawler.Core.Models;
using System;

namespace DungeonCrawler.Core.Models.Entities
{
    public class Treasure : Entity
    {
        public override string Symbol => "$";
        public override ConsoleColor Color => ConsoleColor.Green;
        public override bool IsPassable => true;
        public int Value { get; private set; }

        public Treasure(Position position, int value = 50) : base(position, $"Treasure({value})")
        {
            Value = value;
        }

        public void Collect(Hero hero, GameState gameState)
        {
            if (hero != null && gameState != null)
            {
                hero.Score += Value;
                gameState.Log.Add($"{hero.Name} found treasure worth {Value} points!");
                // Удаляем сокровище с карты
                var cell = gameState.Dungeon.GetCell(Position);
                if (cell != null)
                    cell.Entity = null;
                gameState.RemoveEntity(this);
            }
        }

        public override string Serialize()
        {
            return base.Serialize() + $"|{Value}";
        }

        public override void Deserialize(string data)
        {
            base.Deserialize(data);
            var parts = data.Split('|');
            if (parts.Length >= 4)
            {
                int.TryParse(parts[3], out int value);
                Value = value;
            }
        }
    }
}