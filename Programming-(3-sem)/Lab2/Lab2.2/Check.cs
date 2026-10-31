using System;

public sealed class Check : Document
{
    public string PaymentMethod { get; set; }

    public Check(string number, Date issueDate, Organization issuer, string paymentMethod)
        : base(number, issueDate, issuer)
    {
        PaymentMethod = paymentMethod;
    }

    public override string ToString() =>
        $"Чек №{Number} от {IssueDate}, от {Issuer}, оплата: {PaymentMethod}";

    public override void Print()
    {
        Console.WriteLine($"Номер: {Number}");
        Console.WriteLine($"Дата: {IssueDate}");
        Console.WriteLine($"Организация: {Issuer}");
        Console.WriteLine($"Способ оплаты: {PaymentMethod}");
    }

    public bool IsCashPayment()
    {
        return PaymentMethod.Equals("Наличные", StringComparison.OrdinalIgnoreCase);
    }
}