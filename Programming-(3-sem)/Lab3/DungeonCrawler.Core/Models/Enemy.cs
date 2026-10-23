using DungeonCrawler.Core.Models.Entities;
using System;

namespace DungeonCrawler.Core.Models
{
    public abstract class Enemy : Entity
    {
        public int Health { get; set; }
        public int AttackPower { get; protected set; }
        public int ScoreReward { get; protected set; }
        public bool IsAlive => Health > 0;

        protected Enemy(Position position, string name, int health, int attackPower, int scoreReward)
            : base(position, name)
        {
            Health = health;
            AttackPower = attackPower;
            ScoreReward = scoreReward;
        }

        public virtual void AttackHero(Hero hero, GameState gameState)
        {
            if (hero != null)
            {
                hero.Health -= AttackPower;
                if (gameState != null)
                {
                    gameState.AddLog($"{Name} атакует {hero.Name} и наносит {AttackPower} урона!");
                    if (hero.Health <= 0)
                    {
                        gameState.AddLog($"{hero.Name} повержен!");
                        gameState.IsGameOver = true;
                    }
                }
            }
        }

        public override void Update(GameState gameState)
        {
            if (!IsAlive)
            {
                gameState?.RemoveEntity(this);
            }
        }

        public override string Serialize()
        {
            return base.Serialize() + $"|{Health}|{AttackPower}|{ScoreReward}";
        }

        public override void Deserialize(string data)
        {
            base.Deserialize(data);
            var parts = data.Split('|');
            if (parts.Length >= 6)
            {
                int.TryParse(parts[3], out int health);
                int.TryParse(parts[4], out int attackPower);
                int.TryParse(parts[5], out int scoreReward);
                Health = health;
                AttackPower = attackPower;
                ScoreReward = scoreReward;
            }
        }
    }
}