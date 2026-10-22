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
}