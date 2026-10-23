using System;

namespace DungeonCrawler.Core.Models.Entities
{
    [Serializable]
    public class StationaryEnemy : Enemy
    {
        public override string Symbol => "S";
        public override ConsoleColor Color => ConsoleColor.Red;

        public StationaryEnemy(Position position)
            : base(position, "Stationary Enemy", 30, 15)
        {
        }
    }
}