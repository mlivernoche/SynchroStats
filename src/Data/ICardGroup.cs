namespace SynchroStats.Data;

public interface ICardGroup<TCardGroupName> : INamedCard<TCardGroupName>
    where TCardGroupName : notnull, IEquatable<TCardGroupName>, IComparable<TCardGroupName>
{
    int Size { get; }
    int Minimum { get; }
    int Maximum { get; }
}
