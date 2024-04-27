using System.Diagnostics;

namespace SynchroStats.Features.Combinations;

[DebuggerDisplay("{HandName}: {MinimumSize} // {MaximumSize}")]
public readonly struct HandElement<TCardGroupName> : IEquatable<HandElement<TCardGroupName>>
    where TCardGroupName : notnull, IEquatable<TCardGroupName>, IComparable<TCardGroupName>
{
    public required TCardGroupName HandName { get; init; }
    public required int MinimumSize { get; init; }
    public required int MaximumSize { get; init; }

    public bool Equals(HandElement<TCardGroupName> other)
    {
        return
            HandName.Equals(other.HandName) &&
            MinimumSize == other.MinimumSize &&
            MaximumSize == other.MaximumSize;
    }

    public override bool Equals(object? obj)
    {
        return obj is HandElement<TCardGroupName> other && Equals(other);
    }

    public static bool operator ==(HandElement<TCardGroupName> left, HandElement<TCardGroupName> right)
    {
        return left.Equals(right);
    }

    public static bool operator !=(HandElement<TCardGroupName> left, HandElement<TCardGroupName> right)
    {
        return !(left == right);
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(HandName, MinimumSize, MaximumSize);
    }
}
