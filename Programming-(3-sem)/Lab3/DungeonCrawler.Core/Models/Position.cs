using System;

namespace DungeonCrawler.Core.Models
{
    public struct Position
    {
        public int X { get; set; }
        public int Y { get; set; }

        public Position(int x, int y)
        {
            X = x;
            Y = y;
        }

        public bool Equals(Position other) => X == other.X && Y == other.Y;
        public override bool Equals(object obj) => obj is Position other && Equals(other);
        public override int GetHashCode() => HashCode.Combine(X, Y);
        public static bool operator ==(Position left, Position right) => left.Equals(right);
        public static bool operator !=(Position left, Position right) => !left.Equals(right);
        public static Position operator +(Position a, Position b) => new(a.X + b.X, a.Y + b.Y);
        public static Position operator -(Position a, Position b) => new(a.X - b.X, a.Y - b.Y);

        public double DistanceTo(Position other)
        {
            int dx = X - other.X;
            int dy = Y - other.Y;
            return Math.Sqrt(dx * dx + dy * dy);
        }

        public Position GetDirectionTo(Position target)
        {
            int dx = Math.Sign(target.X - X);
            int dy = Math.Sign(target.Y - Y);
            return new Position(dx, dy);
        }

        public override string ToString() => $"({X},{Y})";
    }
}