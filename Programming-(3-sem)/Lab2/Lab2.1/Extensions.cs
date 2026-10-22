using System;
using System.Collections.Generic;
using System.Linq;

// Методы расширения для строки
public static class StringExtensions
{
    public static string CutStart(this string str, int count)
    {
        if (str == null) throw new ArgumentNullException(nameof(str));
        if (count < 0) throw new ArgumentException("Счетчик не должен быть отрицательным");
        if (count >= str.Length) return string.Empty;
        return str.Substring(count);
    }
}

// Методы расширения для вектора
public static class VectorExtensions
{
    public static Vector RemovePositive(this Vector vector)
    {
        if (vector is null) throw new ArgumentNullException(nameof(vector));

        var nonPositive = new List<double>();
        for (int i = 0; i < vector.Length; i++)
        {
            if (vector[i] <= 0)
                nonPositive.Add(vector[i]);
        }
        return new Vector(nonPositive.ToArray());
    }
}