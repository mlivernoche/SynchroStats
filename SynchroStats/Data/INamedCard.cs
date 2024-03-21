namespace SynchroStats.Data;

public interface INamedCard<TName>
    where TName : notnull, IEquatable<TName>, IComparable<TName>
{
    TName Name { get; }
}
