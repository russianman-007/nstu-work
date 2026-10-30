using System;
using System.Linq;

public class Vector
{
    private double[] _data;

    // Конструктор по умолчанию
    public Vector()
    {
        _data = Array.Empty<double>();
    }

    // Конструктор с заданным размером
    public Vector(int size)
    {
        if (size < 0) throw new ArgumentException("Размер не может быть отрицательным");
        _data = new double[size];
    }

    // Конструктор из массива
    public Vector(double[] data)
    {
        _data = data ?? throw new ArgumentNullException(nameof(data));
    }

    // Конструктор копирования
    public Vector(Vector other)
    {
        if (other is null)
            throw new ArgumentNullException(nameof(other));
        _data = new double[other._data.Length];
        Array.Copy(other._data, _data, other._data.Length);
    }

    // Индексатор
    public double this[int index]
    {
        get => _data[index];
        set => _data[index] = value;
    }

    // Свойство для получения длины
    public int Length => _data.Length;

    // Оператор сложения векторов
    public static Vector operator +(Vector v1, Vector v2)
    {
        if (v1 is null || v2 is null)
            throw new ArgumentNullException("Вектор не может принимать значение NULL");

        if (v1.Length != v2.Length)
            throw new ArgumentException("Вектора должны быть одного размера");

        var result = new double[v1.Length];
        for (int i = 0; i < v1.Length; i++)
        {
            result[i] = v1[i] + v2[i];
        }
        return new Vector(result);
    }

    // Оператор сравнения векторов (>)
    public static bool operator >(Vector v1, Vector v2)
    {
        if (v1 is null || v2 is null)
            throw new ArgumentNullException("Вектор не может быть null");
        return v1.Norm() > v2.Norm();
    }

    // Оператор сравнения векторов (<)
    public static bool operator <(Vector v1, Vector v2)
    {
        if (v1 is null || v2 is null)
            throw new ArgumentNullException("Вектор не может быть null");
        return v1.Norm() < v2.Norm();
    }

    // Вычисление евклидовой нормы для сравнения векторов
    public double Norm()
    {
        return Math.Sqrt(_data.Sum(x => x * x));
    }

    // Оператор копирования вектора (~)
    public static Vector operator ~(Vector v)
    {
        if (v is null)
            throw new ArgumentNullException("Вектор не может быть null");
        return new Vector(v);
    }

    // Определение пустоты вектора
    public static bool operator true(Vector v) => v is not null && v.Length > 0;
    public static bool operator false(Vector v) => v is null || v.Length == 0;

    // Строковое представление вектора
    public override string ToString()
    {
        return $"[{string.Join(", ", _data)}]";
    }
}