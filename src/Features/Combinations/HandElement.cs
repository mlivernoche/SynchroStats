namespace SynchroStats.Features.Combinations;

public readonly struct HandElement<T> : IEquatable<HandElement<T>>
    where T : notnull, IEquatable<T>, IComparable<T>
{
    public required T HandName { get; init; }
    public required int MinimumSize { get; init; }
    public required int MaximumSize { get; init; }

    public bool Equals(HandElement<T> other)
    {
        return
            HandName.Equals(other.HandName) &&
            MinimumSize == other.MinimumSize &&
            MaximumSize == other.MaximumSize;
    }

    public override bool Equals(object? obj)
    {
        return obj is HandElement<T> other && Equals(other);
    }

    public static bool operator ==(HandElement<T> left, HandElement<T> right)
    {
        return left.Equals(right);
    }

    public static bool operator !=(HandElement<T> left, HandElement<T> right)
    {
        return !(left == right);
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(HandName, MinimumSize, MaximumSize);
    }
}
