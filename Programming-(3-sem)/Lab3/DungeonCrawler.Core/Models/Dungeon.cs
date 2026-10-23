using DungeonCrawler.Core.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;

namespace DungeonCrawler.Core.Models
{
    public class Dungeon
    {
        public int Width { get; private set; }
        public int Height { get; private set; }
        public Cell[,] Grid { get; private set; }

        public Dungeon(int width, int height)
        {
            Width = width;
            Height = height;
            Grid = new Cell[width, height];
            Initialize();
        }

        private void Initialize()
        {
            var random = new Random();
            for (int x = 0; x < Width; x++)
            {
                for (int y = 0; y < Height; y++)
                {
                    if (x == 0 || y == 0 || x == Width - 1 || y == Height - 1)
                    {
                        Grid[x, y] = new Cell(CellType.Wall);
                    }
                    else
                    {
                        Grid[x, y] = new Cell(random.Next(5) == 0 ? CellType.Wall : CellType.Floor);
                    }
                }
            }
            Grid[Width - 2, Height - 2] = new Cell(CellType.Exit);
        }

        public Cell GetCell(Position position)
        {
            if (IsValidPosition(position))
                return Grid[position.X, position.Y];
            return null;
        }

        public bool IsValidPosition(Position position)
        {
            return position.X >= 0 && position.X < Width &&
                   position.Y >= 0 && position.Y < Height;
        }

        public void PlaceEntity(Entity entity)
        {
            var cell = GetCell(entity.Position);
            if (cell != null)
            {
                cell.Entity = entity;
            }
        }

        public void RemoveEntity(Position position)
        {
            var cell = GetCell(position);
            if (cell != null)
            {
                cell.Entity = null;
            }
        }

        public string Render()
        {
            var sb = new StringBuilder();
            for (int y = 0; y < Height; y++)
            {
                for (int x = 0; x < Width; x++)
                {
                    sb.Append(Grid[x, y].ToString());
                }
                sb.AppendLine();
            }
            return sb.ToString();
        }

        public List<Position> GetEmptyPositions()
        {
            var positions = new List<Position>();
            for (int x = 1; x < Width - 1; x++)
            {
                for (int y = 1; y < Height - 1; y++)
                {
                    if (Grid[x, y].Type == CellType.Floor && !Grid[x, y].IsOccupied)
                    {
                        positions.Add(new Position(x, y));
                    }
                }
            }
            return positions;
        }

        public string Serialize()
        {
            var data = new List<string>
            {
                $"{Width},{Height}"
            };
            for (int x = 0; x < Width; x++)
            {
                for (int y = 0; y < Height; y++)
                {
                    data.Add($"{x},{y}:{(int)Grid[x, y].Type}");
                }
            }
            return string.Join(";", data);
        }

        public void Deserialize(string data)
        {
            var parts = data.Split(';');
            if (parts.Length > 0)
            {
                var size = parts[0].Split(',');
                if (size.Length == 2 &&
                    int.TryParse(size[0], out int width) &&
                    int.TryParse(size[1], out int height))
                {
                    Width = width;
                    Height = height;
                    Grid = new Cell[width, height];
                    for (int i = 1; i < parts.Length; i++)
                    {
                        var cellData = parts[i].Split(':');
                        if (cellData.Length == 2)
                        {
                            var pos = cellData[0].Split(',');
                            if (pos.Length == 2 &&
                                int.TryParse(pos[0], out int x) &&
                                int.TryParse(pos[1], out int y) &&
                                int.TryParse(cellData[1], out int cellType))
                            {
                                Grid[x, y] = new Cell((CellType)cellType);
                            }
                        }
                    }
                }
            }
        }
    }
}