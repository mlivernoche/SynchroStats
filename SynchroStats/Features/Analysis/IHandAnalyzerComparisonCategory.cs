using SynchroStats.Data;

namespace SynchroStats.Features.Analysis;

public interface IHandAnalyzerComparisonCategory<TCardGroup, TCardGroupName>
    where TCardGroup : ICardGroup<TCardGroupName>
    where TCardGroupName : notnull, IEquatable<TCardGroupName>, IComparable<TCardGroupName>
{
    string Name { get; }
    string Run(IEnumerable<HandAnalyzer<TCardGroup, TCardGroupName>> analyzers);
}
