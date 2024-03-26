using System.Collections.Immutable;

namespace SynchroStats.Features.Combinations;

public readonly struct HandCombination<TCardGroupName> : IEquatable<HandCombination<TCardGroupName>>
    where TCardGroupName : notnull, IEquatable<TCardGroupName>, IComparable<TCardGroupName>
{
    private sealed class Comparer : IComparer<HandElement<TCardGroupName>>
    {
        public static readonly Comparer Instance = new Comparer();

        public int Compare(HandElement<TCardGroupName> x, HandElement<TCardGroupName> y)
        {
            return x.HandName.CompareTo(y.HandName);
        }
    }

    internal ImmutableSortedSet<HandElement<TCardGroupName>> CardNames { get; }

    public HandCombination()
    {
        CardNames = [];
    }

    public HandCombination(IEnumerable<HandElement<TCardGroupName>> permutations)
    {
        CardNames = permutations.ToImmutableSortedSet(Comparer.Instance);
    }

    public bool Equals(HandCombination<TCardGroupName> other)
    {
        return other.CardNames == CardNames || CardNames.SetEquals(other.CardNames);
    }

    public override bool Equals(object? obj)
    {
        return obj is HandCombination<TCardGroupName> other && Equals(other);
    }

    public override int GetHashCode()
    {
        var hashCode = 0;

        foreach (var permutation in CardNames)
        {
            hashCode = HashCode.Combine(hashCode, permutation);
        }

        return hashCode;
    }

    public static bool operator ==(HandCombination<TCardGroupName> left, HandCombination<TCardGroupName> right)
    {
        return left.Equals(right);
    }

    public static bool operator !=(HandCombination<TCardGroupName> left, HandCombination<TCardGroupName> right)
    {
        return !(left == right);
    }
}
