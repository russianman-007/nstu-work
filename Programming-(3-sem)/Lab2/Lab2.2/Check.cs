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
}