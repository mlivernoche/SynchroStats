namespace SynchroStats.Data;

public interface ICardGroup<TName> : INamedCard<TName>
    where TName : notnull, IEquatable<TName>, IComparable<TName>
{
    int Size { get; }
    int Minimum { get; }
    int Maximum { get; }
}
