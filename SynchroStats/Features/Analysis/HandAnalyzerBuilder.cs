using SynchroStats.Data;

namespace SynchroStats.Features.Analysis;

public static class HandAnalyzerBuildArguments
{
    public static HandAnalyzerBuildArguments<T, U> Create<T, U>(string analyzerName, int handSize, IReadOnlyCollection<T> cardGroups)
        where T : ICardGroup<U>
        where U : notnull, IEquatable<U>, IComparable<U>
    {
        return new HandAnalyzerBuildArguments<T, U>(analyzerName, handSize, cardGroups);
    }

    public static HandAnalyzerBuildArguments<T, U> Create<T, U>(string analyzerName, int handSize, CardList<T, U> cardList)
        where T : ICardGroup<U>
        where U : notnull, IEquatable<U>, IComparable<U>
    {
        return new HandAnalyzerBuildArguments<T, U>(analyzerName, handSize, cardList);
    }
}

public record HandAnalyzerBuildArguments<T, U>(string AnalyzerName, int HandSize, IReadOnlyCollection<T> CardGroups)
    where T : ICardGroup<U>
    where U : notnull, IEquatable<U>, IComparable<U>;
