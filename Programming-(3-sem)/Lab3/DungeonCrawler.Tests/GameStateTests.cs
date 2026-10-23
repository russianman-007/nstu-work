using Microsoft.VisualStudio.TestTools.UnitTesting;
using DungeonCrawler.Core.Models;

namespace DungeonCrawler.Tests
{
    [TestClass]
    public class GameStateTests
    {
        [TestMethod]
        public void GameState_Initialization_CreatesEntities()
        {
            var game = new GameState(10, 10);

            Assert.IsNotNull(game.Hero, "Герой должен быть создан");
            Assert.IsTrue(game.Entities.Count >= 6, "На карте должны быть герой, сокровища и враги");
            Assert.AreEqual(0, game.Turn, "Начальный ход должен быть 0");
            Assert.IsFalse(game.IsGameOver, "Игра не должна быть окончена при старте");
        }

        [TestMethod]
        public void GameState_AddLog_PreventsSpam()
        {
            var game = new GameState(5, 5);
            game.Log.Clear();

            game.AddLog("Test message");
            game.AddLog("Test message");
            game.AddLog("Test message");

            Assert.AreEqual(1, game.Log.Count, "Одинаковые сообщения подряд не должны добавляться");
            Assert.AreEqual("Test message", game.Log[0]);
        }

        [TestMethod]
        public void GameState_AddLog_LimitsSize()
        {
            var game = new GameState(5, 5);
            game.Log.Clear();

            for (int i = 0; i < 15; i++)
            {
                game.AddLog($"Message {i}");
            }

            Assert.IsTrue(game.Log.Count <= 8, "Лог не должен превышать 8 записей");
            Assert.AreEqual("Message 14", game.Log[game.Log.Count - 1], "Последним должно быть новейшее сообщение");
        }
    }
}