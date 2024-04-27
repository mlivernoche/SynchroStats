namespace SynchroStats.Data;

public interface INamedCard<TCardGroupName>
    where TCardGroupName : notnull, IEquatable<TCardGroupName>, IComparable<TCardGroupName>
{
    TCardGroupName Name { get; }
}
