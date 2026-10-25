// Проверка корректности реализации структуры Position
using Microsoft.VisualStudio.TestTools.UnitTesting;
using DungeonCrawler.Core.Models;
using System;

namespace DungeonCrawler.Tests
{
    [TestClass]
    public class PositionTests
    {
        // Тест операторов равенства
        [TestMethod]
        public void Position_Equality_Works()
        {
            var pos1 = new Position(5, 10);
            var pos2 = new Position(5, 10);
            var pos3 = new Position(5, 11);

            Assert.IsTrue(pos1 == pos2, "Одинаковые позиции должны быть равны");
            Assert.IsFalse(pos1 == pos3, "Разные позиции не должны быть равны");
        }

        // Тест вычисления расстояния
        [TestMethod]
        public void Position_Distance_CalculatedCorrectly()
        {
            var pos1 = new Position(0, 0);
            var pos2 = new Position(3, 4);

            double distance = pos1.DistanceTo(pos2);

            Assert.AreEqual(5.0, distance, 0.0001, "Расстояние вычислено неверно");
        }

        // Тест сложения позиций
        [TestMethod]
        public void Position_Addition_Works()
        {
            var pos1 = new Position(2, 3);
            var pos2 = new Position(4, 5);
            var result = pos1 + pos2;

            Assert.AreEqual(6, result.X);
            Assert.AreEqual(8, result.Y);
        }
    }
}