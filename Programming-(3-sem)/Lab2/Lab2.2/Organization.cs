using System;

public class Organization
{
    public string Name { get; set; }
    public string Inn { get; set; }

    public Organization(string name, string inn)
    {
        Name = name;
        Inn = inn;
    }

    public override string ToString() => $"{Name} (ИНН: {Inn})";
}