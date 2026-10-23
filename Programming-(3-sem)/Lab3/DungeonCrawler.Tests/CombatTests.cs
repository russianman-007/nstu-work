using Microsoft.VisualStudio.TestTools.UnitTesting;
using DungeonCrawler.Core.Models;
using DungeonCrawler.Core.Models.Entities;

namespace DungeonCrawler.Tests
{
    [TestClass]
    public class CombatTests
    {
        [TestMethod]
        public void Hero_Attack_ReducesEnemyHealth()
        {
            var hero = new Hero(new Position(0, 0)) { AttackPower = 15 };
            var enemy = new StationaryEnemy(new Position(1, 0), health: 30, attackPower: 10, scoreReward: 50);

            hero.Attack(enemy);

            Assert.AreEqual(15, enemy.Health, "Здоровье врага должно уменьшиться на силу атаки героя");
            Assert.IsTrue(enemy.IsAlive, "Враг должен остаться жив после одного удара");
        }

        [TestMethod]
        public void Enemy_Attack_ReducesHeroHealth()
        {
            var hero = new Hero(new Position(0, 0)) { Health = 100 };
            var enemy = new StationaryEnemy(new Position(1, 0), health: 30, attackPower: 25, scoreReward: 50);
            var gameState = new GameState(5, 5);

            enemy.AttackHero(hero, gameState);

            Assert.AreEqual(75, hero.Health, "Здоровье героя должно уменьшиться на силу атаки врага");
        }

        [TestMethod]
        public void Enemy_Death_TriggersScoreReward()
        {
            var hero = new Hero(new Position(0, 0)) { AttackPower = 50, Score = 0 };
            var enemy = new MovingEnemy(new Position(1, 0), health: 20, attackPower: 10, scoreReward: 30);

            hero.Attack(enemy);

            Assert.IsFalse(enemy.IsAlive, "Враг должен быть мертв");
        }
    }
}