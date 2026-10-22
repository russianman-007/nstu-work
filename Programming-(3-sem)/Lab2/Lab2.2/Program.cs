using System;
using static System.Runtime.InteropServices.JavaScript.JSType;

class Program
{
    static void Main()
    {
        var org1 = new Organization("АО 'Географ'", "5295169165");
        var date1 = new Date(1, 1, 2026);
        var date2 = new Date(1, 1, 2026);

        var org2 = new Organization("ООО 'Техно-ДНС'", "6328158627");
        var date3 = new Date(25, 1, 2026);
        var date4 = new Date(10, 2, 2026);

        Document[] documents =
        {
            new Receipt("КВ-012", date1, org1, 1500.50m),
            new Invoice("НК-212", date1, org1, "Мониторы, клавиатуры"),
            new Check("ЧК-427", date2, org1, "Банковская Карта"),

            new Receipt("КВ-112", date3, org2, 2000.00m),
            new Invoice("НК-087", date3, org2, "Цемент, арматура, доски"),
            new Check("ЧК-481", date4, org2, "Наличные")
        };

        foreach (var doc in documents)
        {
            Console.WriteLine(doc.ToString());
            doc.Print();
            Console.WriteLine(new string('-', 50));
        }

        var r1 = new Receipt("КВ-012", date1, org1, 1500.50m);
        var r2 = new Receipt("КВ-012", date1, org1, 1500.50m);
        Console.WriteLine($"Квитанции равны? {r1.Equals(r2)}");
    }
}