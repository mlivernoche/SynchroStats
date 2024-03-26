namespace SynchroStats.Data;

public static class CardGroup
{
    public static CardGroup<TName> Create<TName>(TName name, int size, int minimum, int maximum)
        where TName : notnull, IEquatable<TName>, IComparable<TName>
    {
        return new CardGroup<TName>()
        {
            Name = name,
            Size = size,
            Minimum = minimum,
            Maximum = maximum,
        };
    }
}

public class CardGroup<TName> : ICardGroup<TName>
    where TName : notnull, IEquatable<TName>, IComparable<TName>
{
    public required TName Name { get; init; }
    public required int Size { get; init; }
    public required int Minimum { get; init; }
    public required int Maximum { get; init; }

    public CardGroup() { }

    public CardGroup(TName name, int size, int minimum, int maximum)
    {
        Name = name;
        Size = size;
        Minimum = minimum;
        Maximum = maximum;
    }

    public CardGroup(TName name)
    {
        Name = name;
    }
}
