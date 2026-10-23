using DungeonCrawler.Core.Models;
using System;

namespace DungeonCrawler.Core.Interfaces
{
    public interface IEntity
    {
        string Symbol { get; }
        ConsoleColor Color { get; }
        Position Position { get; set; }
        bool IsPassable { get; }
        void Update(GameState gameState);
        string Serialize();
        void Deserialize(string data);
    }

    public interface IMovable
    {
        bool TryMove(Position newPosition, GameState gameState);
    }

    public interface IInteractable
    {
        void Interact(Entity entity, GameState gameState);
    }
}