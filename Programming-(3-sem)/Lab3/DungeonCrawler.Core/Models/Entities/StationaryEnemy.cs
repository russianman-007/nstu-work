// Враг, который не двигается, но атакует героя на близкой дистанции
using System;
using DungeonCrawler.Core.Models;

namespace DungeonCrawler.Core.Models.Entities
{
    public class StationaryEnemy : Enemy
    {
        public override string Symbol => "S";
        public override ConsoleColor Color => ConsoleColor.Red;

        public StationaryEnemy(Position position, int health = 30, int attackPower = 15, int scoreReward = 50)
            : base(position, "Стоячий враг", health, attackPower, scoreReward)
        {
        }

        // Атака героя, если расстояние <= 1.0
        public override void Update(GameState gameState)
        {
            base.Update(gameState);
            if (!IsAlive) return;

            var hero = gameState.Hero;
            if (hero != null && Position.DistanceTo(hero.Position) <= 1.0)
            {
                AttackHero(hero, gameState);
            }
        }
    }
}