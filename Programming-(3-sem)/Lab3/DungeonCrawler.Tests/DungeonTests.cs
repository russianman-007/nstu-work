using Microsoft.VisualStudio.TestTools.UnitTesting;
using DungeonCrawler.Core.Models;

namespace DungeonCrawler.Tests
{
    [TestClass]
    public class DungeonTests
    {
        [TestMethod]
        public void Dungeon_Initialization_CreatesValidGrid()
        {
            var dungeon = new Dungeon(10, 10);

            Assert.AreEqual(10, dungeon.Width);
            Assert.AreEqual(10, dungeon.Height);
            Assert.IsNotNull(dungeon.Grid);
        }

        [TestMethod]
        public void Dungeon_Boundaries_AreWalls()
        {
            var dungeon = new Dungeon(5, 5);

            Assert.AreEqual(CellType.Wall, dungeon.Grid[0, 0].Type);
            Assert.AreEqual(CellType.Wall, dungeon.Grid[4, 4].Type);
            Assert.AreEqual(CellType.Wall, dungeon.Grid[0, 2].Type);
            Assert.AreEqual(CellType.Wall, dungeon.Grid[2, 4].Type);
        }

        [TestMethod]
        public void Dungeon_Exit_IsPlacedCorrectly()
        {
            var dungeon = new Dungeon(10, 10);

            Assert.AreEqual(CellType.Exit, dungeon.Grid[8, 8].Type);
        }
    }
}