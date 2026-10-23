using System;
using DungeonCrawler.Core.Models;

namespace DungeonCrawler.Core.Models.Entities
{
    public class Treasure : Entity
    {
        public override string Symbol => "$";
        public override ConsoleColor Color => ConsoleColor.Green;
        public override bool IsPassable => true;
        public int Value { get; private set; }
        public int ScoreReward { get; private set; } // Бонусные очки опыта за сбор

        public Treasure(Position position, int value = 50, int scoreReward = 25) : base(position, $"Treasure({value})")
        {
            Value = value;
            ScoreReward = scoreReward;
        }

        public void Collect(Hero hero, GameState gameState)
        {
            if (hero != null && gameState != null)
            {
                hero.Score += Value + ScoreReward; // Суммируем стоимость и бонус опыта
                gameState.AddLog($"{hero.Name} found treasure! +{Value + ScoreReward} очков (опыта)!");

                var cell = gameState.Dungeon.GetCell(Position);
                if (cell != null)
                    cell.Entity = null;
                gameState.RemoveEntity(this);
            }
        }

        public override string Serialize()
        {
            return base.Serialize() + $"|{Value}|{ScoreReward}";
        }

        public override void Deserialize(string data)
        {
            base.Deserialize(data);
            var parts = data.Split('|');
            if (parts.Length >= 5)
            {
                int.TryParse(parts[3], out int value);
                int.TryParse(parts[4], out int scoreReward);
                Value = value;
                ScoreReward = scoreReward;
            }
        }
    }
}