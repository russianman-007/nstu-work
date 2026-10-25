// Враг, который перемещается в сторону героя и атакует его
using System;
using DungeonCrawler.Core.Models;

namespace DungeonCrawler.Core.Models.Entities
{
    public class MovingEnemy : Enemy
    {
        public override string Symbol => "M";
        public override ConsoleColor Color => ConsoleColor.Magenta;

        public MovingEnemy(Position position, int health = 20, int attackPower = 10, int scoreReward = 30)
            : base(position, "Движущийся враг", health, attackPower, scoreReward)
        {
        }

        // Попытка перемещения на новую свободную клетку
        public bool TryMove(Position newPosition, GameState gameState)
        {
            var cell = gameState.Dungeon.GetCell(newPosition);
            if (cell?.IsPassable == true && !cell.IsOccupied)
            {
                var oldCell = gameState.Dungeon.GetCell(Position);
                if (oldCell != null) oldCell.Entity = null;
                Position = newPosition;
                cell.Entity = this;
                return true;
            }
            return false;
        }

        // Движение к герою и атака при сближении
        public override void Update(GameState gameState)
        {
            base.Update(gameState);
            if (!IsAlive) return;

            var hero = gameState.Hero;
            if (hero != null)
            {
                var direction = Position.GetDirectionTo(hero.Position);
                var newPosition = Position + direction;
                TryMove(newPosition, gameState);

                if (Position.DistanceTo(hero.Position) <= 1.0)
                {
                    AttackHero(hero, gameState);
                }
            }
        }
    }
}