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

    public override bool Equals(object? obj)
    {
        if (obj is Receipt other)
            return Number == other.Number && Amount == other.Amount;
        return false;
    }
}