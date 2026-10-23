using System;

namespace DungeonCrawler.Core.Models
{
    public abstract class Entity   // Ключевое слово abstract class
    {
        public abstract string Symbol { get; }
        public abstract ConsoleColor Color { get; }
        public Position Position { get; set; }
        public virtual bool IsPassable => false;
        public string Name { get; protected set; }

        protected Entity(Position position, string name = "Entity")
        {
            Position = position;
            Name = name;
        }

        public virtual void Update(GameState gameState) { }

        public virtual string Serialize()
        {
            return $"{GetType().Name}|{Position.X},{Position.Y}|{Name}";
        }

        public virtual void Deserialize(string data)
        {
            var parts = data.Split('|');
            if (parts.Length >= 2)
            {
                var posParts = parts[1].Split(',');
                if (posParts.Length == 2 &&
                    int.TryParse(posParts[0], out int x) &&
                    int.TryParse(posParts[1], out int y))
                {
                    Position = new Position(x, y);
                }
            }
            if (parts.Length >= 3)
            {
                Name = parts[2];
            }
        }
    }
}