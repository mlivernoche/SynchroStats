using SynchroStats.Data;

namespace SynchroStats.Features.Analysis;

public static class HandAnalyzerBuildArguments
{
    public static HandAnalyzerBuildArguments<TCardGroup, TCardGroupName> Create<TCardGroup, TCardGroupName>(string analyzerName, int handSize, IReadOnlyCollection<TCardGroup> cardGroups)
        where TCardGroup : ICardGroup<TCardGroupName>
        where TCardGroupName : notnull, IEquatable<TCardGroupName>, IComparable<TCardGroupName>
    {
        return new HandAnalyzerBuildArguments<TCardGroup, TCardGroupName>(analyzerName, handSize, cardGroups);
    }

    public static HandAnalyzerBuildArguments<TCardGroup, TCardGroupName> Create<TCardGroup, TCardGroupName>(string analyzerName, int handSize, CardList<TCardGroup, TCardGroupName> cardList)
        where TCardGroup : ICardGroup<TCardGroupName>
        where TCardGroupName : notnull, IEquatable<TCardGroupName>, IComparable<TCardGroupName>
    {
        return new HandAnalyzerBuildArguments<TCardGroup, TCardGroupName>(analyzerName, handSize, cardList);
    }
}

public record HandAnalyzerBuildArguments<TCardGroup, TCardGroupName>(string AnalyzerName, int HandSize, IReadOnlyCollection<TCardGroup> CardGroups)
    where TCardGroup : ICardGroup<TCardGroupName>
    where TCardGroupName : notnull, IEquatable<TCardGroupName>, IComparable<TCardGroupName>;
