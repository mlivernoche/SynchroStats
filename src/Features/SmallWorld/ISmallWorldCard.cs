using SynchroStats.Data;

namespace SynchroStats.Features.SmallWorld;

public interface ISmallWorldCard<TCardGroupName> : INamedCard<TCardGroupName>
    where TCardGroupName : notnull, IEquatable<TCardGroupName>, IComparable<TCardGroupName>
{
    ISmallWorldTraits? SmallWorldTraits { get; }
}
