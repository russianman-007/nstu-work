using System;

class Program
{
    static void Main()
    {
        Console.WriteLine("================== Исходные вектора: ==================\n");

        var v1 = new Vector(new double[] { 4, 8, 1 });
        var v2 = new Vector(new double[] { 4, 1, 2 });
        var v3 = new Vector(new double[] { -4, 2, -7});
        var v4 = new Vector(new double[] { 1, 1, 1 });
        var v5 = new Vector();

        Console.WriteLine($"# V1 = {v1}");
        Console.WriteLine($"# V2 = {v2}");
        Console.WriteLine($"# V3 = {v3}");
        Console.WriteLine($"# V4 = {v4}");
        Console.WriteLine($"# V5 = {v5}\n");

        Console.WriteLine("=======================================================\n");
        Console.WriteLine("=============== Операции над векторами: ===============\n");

        var v6 = v1 + v2;
        Console.WriteLine($"# {v1} + {v2} = {v6} - Сложение векторов");

        double norm1 = v3.Norm();
        double norm2 = v4.Norm();
        if (norm1 > norm2)
        { Console.Write($"# {v3} > {v4}"); }
        else if (norm1 < norm2)
        { Console.Write($"# {v3} < {v4}"); }
        else
        { Console.Write($"# {v3} = {v4}"); }
        Console.WriteLine(" - Сравнение векторов");

        var v7 = new Vector(v1);
        Console.WriteLine($"# {v7} ~ {v1} - Копирование вектора\n");

        if (v4)
        { Console.WriteLine($"# Вектор V4 не пустой"); }
        else
        { Console.WriteLine($"# Вектор V4 пустой"); }
        if (v5)
        { Console.WriteLine($"# Вектор V5 не пустой"); }
        else
        { Console.WriteLine($"# Вектор V5 пустой"); }

        Console.WriteLine("\n=======================================================\n");
        Console.WriteLine("================= Методы расширения: ==================\n");
        
        string text = "Мед и Варенье!";
        Console.WriteLine($"# Оригинал строки: '{text}'");
        Console.WriteLine($"# Строка после учечки: '{text.CutStart(6)}'");

        var v8 = v3.RemovePositive();
        Console.WriteLine($"\n# Исходный вектор: {v3}");
        Console.WriteLine($"# Вектор без положительных элементов: {v8}");

        Console.WriteLine("\n=======================================================");
    }
}