using System;

public class Invoice : Document
{
    public string Items { get; set; }

    public Invoice(string number, Date issueDate, Organization issuer, string items)
        : base(number, issueDate, issuer)
    {
        Items = items;
    }

    public override string ToString() =>
        $"Накладная №{Number} от {IssueDate}, от {Issuer}, товары: {Items}";

    public override void Print()
    {
        Console.WriteLine($"Номер: {Number}");
        Console.WriteLine($"Дата: {IssueDate}");
        Console.WriteLine($"Отправитель: {Issuer}");
        Console.WriteLine($"Товары: {Items}");
    }

    public int GetItemsCount()
    {
        if (string.IsNullOrEmpty(Items)) return 0;
        return Items.Split(',').Length;
    }
}