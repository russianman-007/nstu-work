using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

namespace Lab._2._2.Tests
{
    [TestClass]
    public class Tests
    {
        // ================ Date =================

        [TestMethod]
        public void Date_ToString_ReturnsCorrectFormat()
        {
            var date = new Date(5, 12, 2025);
            string result = date.ToString();
            Assert.AreEqual("05.12.2025", result);
        }

        // ================ Organization =================

        [TestMethod]
        public void Organization_ToString_ReturnsCorrectFormat()
        {
            var org = new Organization("ООО Тест", "1234567890");
            string result = org.ToString();
            Assert.AreEqual("ООО Тест (ИНН: 1234567890)", result);
        }

        // ================ Receipt =================

        [TestMethod]
        public void Receipt_ToString_ReturnsCorrectString()
        {
            var org = new Organization("ООО Ромашка", "1234567890");
            var date = new Date(1, 12, 2025);
            var receipt = new Receipt("R-001", date, org, 1500.50m);

            string actual = receipt.ToString();
            Assert.IsTrue(actual.Contains("Квитанция"));
            Assert.IsTrue(actual.Contains("R-001"));
            Assert.IsTrue(actual.Contains("01.12.2025"));
            Assert.IsTrue(actual.Contains("ООО Ромашка"));

            string expectedAmount = 1500.50m.ToString("F2");
            Assert.IsTrue(actual.Contains(expectedAmount),
                $"Ожидалось содержимое '{expectedAmount}', но фактически: '{actual}'");
        }

        [TestMethod]
        public void Receipt_Equals_ReturnsTrueForSameNumberAndAmount()
        {
            var org = new Organization("ООО А", "111");
            var date = new Date(1, 1, 2025);
            var r1 = new Receipt("R-1", date, org, 100m);
            var r2 = new Receipt("R-1", date, org, 100m);

            Assert.IsTrue(r1.Equals(r2));
        }

        [TestMethod]
        public void Receipt_Equals_ReturnsFalseForDifferentNumber()
        {
            var org = new Organization("ООО А", "111");
            var date = new Date(1, 1, 2025);
            var r1 = new Receipt("R-1", date, org, 100m);
            var r2 = new Receipt("R-2", date, org, 100m);

            Assert.IsFalse(r1.Equals(r2));
        }

        [TestMethod]
        public void Receipt_Equals_ReturnsFalseForDifferentAmount()
        {
            var org = new Organization("ООО А", "111");
            var date = new Date(1, 1, 2025);
            var r1 = new Receipt("R-1", date, org, 100m);
            var r2 = new Receipt("R-1", date, org, 200m);

            Assert.IsFalse(r1.Equals(r2));
        }

        // ================ Invoice =================

        [TestMethod]
        public void Invoice_ToString_ReturnsCorrectString()
        {
            var org = new Organization("ООО Строй", "0987654321");
            var date = new Date(10, 12, 2025);
            var inv = new Invoice("INV-555", date, org, "Цемент, доски");

            string actual = inv.ToString();
            Assert.IsTrue(actual.Contains("Накладная"));
            Assert.IsTrue(actual.Contains("INV-555"));
            Assert.IsTrue(actual.Contains("Цемент, доски"));
        }

        // ================= Check ==================

        [TestMethod]
        public void Check_ToString_ReturnsCorrectString()
        {
            var org = new Organization("ИП Иванов", "1122334455");
            var date = new Date(15, 12, 2025);
            var check = new Check("CHK-777", date, org, "Наличные");

            string actual = check.ToString();
            Assert.IsTrue(actual.Contains("Чек"));
            Assert.IsTrue(actual.Contains("CHK-777"));
            Assert.IsTrue(actual.Contains("Наличные"));
        }

        [TestMethod]
        public void Check_IsSealed()
        {
            var type = typeof(Check);
            Assert.IsTrue(type.IsSealed);
        }

        // ================ Полиморфизм Document[] =================

        [TestMethod]
        public void Document_Array_PolymorphicBehavior()
        {
            var org = new Organization("Тест", "000");
            var date = new Date(1, 1, 2025);

            Document[] docs =
            {
                new Receipt("R", date, org, 100m),
                new Invoice("I", date, org, "Товар"),
                new Check("C", date, org, "Карта")
            };

            Assert.IsTrue(docs[0].ToString().StartsWith("Квитанция"));
            Assert.IsTrue(docs[1].ToString().StartsWith("Накладная"));
            Assert.IsTrue(docs[2].ToString().StartsWith("Чек"));
        }

        // =============== Print вызывает ToString =================

        [TestMethod]
        public void Document_Print_CallsToString()
        {
            var org = new Organization("Тест", "000");
            var date = new Date(1, 1, 2025);
            var receipt = new Receipt("R", date, org, 100m);

            using (var sw = new StringWriter())
            {
                Console.SetOut(sw);
                receipt.Print();
                string output = sw.ToString();
                Assert.IsTrue(output.Contains("Квитанция"));
            }
        }
    }
}