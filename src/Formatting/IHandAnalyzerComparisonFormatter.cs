using SynchroStats.Data;
using SynchroStats.Features.Analysis;

namespace SynchroStats.Formatting;

public interface IHandAnalyzerComparisonFormatter<R>
{
    string FormatData<TCardGroup, TCardGroupName>(string categoryName, IReadOnlyDictionary<HandAnalyzer<TCardGroup, TCardGroupName>, R> analyzers)
        where TCardGroup : ICardGroup<TCardGroupName>
        where TCardGroupName : notnull, IEquatable<TCardGroupName>, IComparable<TCardGroupName>;
}
