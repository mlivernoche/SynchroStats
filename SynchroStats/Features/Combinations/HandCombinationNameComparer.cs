using System.Diagnostics.CodeAnalysis;

namespace SynchroStats.Features.Combinations;

internal sealed class HandCombinationNameComparer<T> : IEqualityComparer<HandElement<T>>
    where T : notnull, IEquatable<T>, IComparable<T>
{
    public static IEqualityComparer<HandElement<T>> Default { get; } = new HandCombinationNameComparer<T>();

    private HandCombinationNameComparer() { }

    public bool Equals(HandElement<T> x, HandElement<T> y)
    {
        return x.HandName.Equals(y.HandName);
    }

    public int GetHashCode([DisallowNull] HandElement<T> obj)
    {
        return HashCode.Combine(obj.HandName);
    }
}
