using System;

public abstract class Document
{
    public string Number { get; set; }
    public Date IssueDate { get; set; }
    public Organization Issuer { get; set; }

    protected Document(string number, Date issueDate, Organization issuer)
    {
        Number = number;
        IssueDate = issueDate;
        Issuer = issuer;
    }

    public virtual void Print()
    {
        Console.WriteLine($"Печать документа: {ToString()}");
    }

    public abstract override string ToString();
}