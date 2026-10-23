using DungeonCrawler.Core.Interfaces;
using DungeonCrawler.Core.Models.Entities;
using System;
using System.Xml.Linq;

namespace DungeonCrawler.Core.Models
{
    public abstract class Enemy : Entity
    {
        public int Health { get; set; }
        public int AttackPower { get; protected set; }
        public bool IsAlive => Health > 0;

        protected Enemy(Position position, string name, int health, int attackPower)
            : base(position, name)
        {
            Health = health;
            AttackPower = attackPower;
        }

        // ЭТОТ МЕТОД ДОЛЖЕН БЫТЬ!
        public virtual void AttackHero(Hero hero, GameState gameState)
        {
            if (hero != null)
            {
                hero.Health -= AttackPower;
                if (gameState != null)
                {
                    gameState.Log.Add($"{Name} attacks {hero.Name} for {AttackPower} damage!");
                    if (hero.Health <= 0)
                    {
                        gameState.Log.Add($"{hero.Name} has been defeated!");
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
            return base.Serialize() + $"|{Health}|{AttackPower}";
        }

        public override void Deserialize(string data)
        {
            base.Deserialize(data);
            var parts = data.Split('|');
            if (parts.Length >= 5)
            {
                int.TryParse(parts[3], out int health);
                int.TryParse(parts[4], out int attackPower);
                Health = health;
                AttackPower = attackPower;
            }
        }
    }
}