using System.Diagnostics.CodeAnalysis;

namespace SynchroStats.Features.Combinations;

internal sealed class HandCombinationNameComparer<TCardGroupName> : IEqualityComparer<HandElement<TCardGroupName>>
    where TCardGroupName : notnull, IEquatable<TCardGroupName>, IComparable<TCardGroupName>
{
    public static IEqualityComparer<HandElement<TCardGroupName>> Default { get; } = new HandCombinationNameComparer<TCardGroupName>();

    private HandCombinationNameComparer() { }

    public bool Equals(HandElement<TCardGroupName> x, HandElement<TCardGroupName> y)
    {
        return x.HandName.Equals(y.HandName);
    }

    public int GetHashCode([DisallowNull] HandElement<TCardGroupName> obj)
    {
        return HashCode.Combine(obj.HandName);
    }
}
