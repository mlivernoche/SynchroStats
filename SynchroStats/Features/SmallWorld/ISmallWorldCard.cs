using SynchroStats.Data;

namespace SynchroStats.Features.SmallWorld;

public interface ISmallWorldCard<TName> : INamedCard<TName>
    where TName : notnull, IEquatable<TName>, IComparable<TName>
{
    ISmallWorldTraits? SmallWorldTraits { get; }
}
