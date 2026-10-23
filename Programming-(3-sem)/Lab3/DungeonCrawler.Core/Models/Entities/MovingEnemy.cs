using System;
using DungeonCrawler.Core.Models;

namespace DungeonCrawler.Core.Models.Entities
{
    public class MovingEnemy : Enemy
    {
        public override string Symbol => "M";
        public override ConsoleColor Color => ConsoleColor.Magenta;

        public MovingEnemy(Position position)
            : base(position, "Moving Enemy", 20, 10)
        {
        }

        public bool TryMove(Position newPosition, GameState gameState)
        {
            var cell = gameState.Dungeon.GetCell(newPosition);
            if (cell?.IsPassable == true && !cell.IsOccupied)
            {
                var oldCell = gameState.Dungeon.GetCell(Position);
                if (oldCell != null)
                    oldCell.Entity = null;
                Position = newPosition;
                cell.Entity = this;
                return true;
            }
            return false;
        }

        public override void Update(GameState gameState)
        {
            base.Update(gameState);
            if (!IsAlive) return;

            // ИИ: двигаться к герою
            var hero = gameState.Hero;
            if (hero != null)
            {
                var direction = Position.GetDirectionTo(hero.Position);
                var newPosition = Position + direction;
                TryMove(newPosition, gameState);

                // Если рядом с героем - атаковать
                // Используем правильный метод AttackHero
                if (Position.DistanceTo(hero.Position) <= 1.5)
                {
                    AttackHero(hero, gameState);   // Теперь этот метод существует
                }
            }
        }
    }
}