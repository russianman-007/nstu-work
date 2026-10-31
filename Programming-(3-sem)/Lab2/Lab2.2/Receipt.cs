using System;

public class Receipt : Document
{
    public decimal Amount { get; set; }

    public Receipt(string number, Date issueDate, Organization issuer, decimal amount)
        : base(number, issueDate, issuer)
    {
        Amount = amount;
    }

    public override string ToString() =>
        $"Квитанция №{Number} от {IssueDate}, от {Issuer}, сумма: {Amount:F2}";

    public override void Print()
    {
        Console.WriteLine($"Номер: {Number}");
        Console.WriteLine($"Дата: {IssueDate}");
        Console.WriteLine($"Организация: {Issuer}");
        Console.WriteLine($"Сумма: {Amount:F2} руб.");
    }

    public bool IsExpensive()
    {
        return Amount > 10000m;
    }

    public override bool Equals(object? obj)
    {
        if (obj is Receipt other)
            return Number == other.Number && Amount == other.Amount;
        return false;
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(Number, Amount);
    }
}