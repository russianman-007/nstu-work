using DungeonCrawler.Core.Interfaces;
using Microsoft.VisualBasic;
using System;

namespace DungeonCrawler.Core.Models
{
    public class Cell
    {
        public CellType Type { get; set; }
        public Entity Entity { get; set; }
        public bool IsPassable => Type != CellType.Wall && (Entity == null || Entity.IsPassable);
        public bool IsOccupied => Entity != null;

        public Cell(CellType type = CellType.Floor)
        {
            Type = type;
            Entity = null;
        }

        public override string ToString()
        {
            return Entity?.Symbol ?? (Type == CellType.Wall ? "#" : Type == CellType.Exit ? "E" : ".");
        }
    }
}