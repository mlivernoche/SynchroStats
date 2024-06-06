using SynchroStats.Data;
using SynchroStats.Features.Analysis;
using SynchroStats.Features.Combinations;
using SynchroStats.Formatting;

namespace SynchroStats.Features.Assessment;

internal sealed class HandAssessmentWithAnalyzerComparisonCategory<TCardGroup, TCardGroupName, TReturn, TAssessment> : HandAnalyzerComparisonCategory<TCardGroup, TCardGroupName>
    where TCardGroup : ICardGroup<TCardGroupName>
    where TCardGroupName : notnull, IEquatable<TCardGroupName>, IComparable<TCardGroupName>
    where TAssessment : IHandAssessment<TCardGroupName>
{
    private AssessmentCache<TCardGroup, TCardGroupName, TAssessment> Cache { get; }
    private Func<HandCombination<TCardGroupName>, HandAnalyzer<TCardGroup, TCardGroupName>, TAssessment> AssessmentFactory { get; }
    private Func<HandAssessmentAnalyzer<TCardGroup, TCardGroupName, TAssessment>, TReturn> Function { get; }
    private IHandAnalyzerComparisonFormatter<TReturn> Formatter { get; }

    public HandAssessmentWithAnalyzerComparisonCategory(string name, IHandAnalyzerComparisonFormatter<TReturn> formatter, Func<HandCombination<TCardGroupName>, HandAnalyzer<TCardGroup, TCardGroupName>, TAssessment> assessmentFactory, Func<HandAssessmentAnalyzer<TCardGroup, TCardGroupName, TAssessment>, TReturn> func, AssessmentCache<TCardGroup, TCardGroupName, TAssessment> cache)
        : base(name)
    {
        Cache = cache;
        AssessmentFactory = assessmentFactory;
        Function = func;
        Formatter = formatter;
    }

    private Dictionary<HandAnalyzer<TCardGroup, TCardGroupName>, TReturn> RunWithExtraArgument(IEnumerable<HandAnalyzer<TCardGroup, TCardGroupName>> analyzers)
    {
        var results = new Dictionary<HandAnalyzer<TCardGroup, TCardGroupName>, TReturn>();

        foreach (var analyzer in analyzers)
        {
            var value = Cache.GetAnalyzer(analyzer, AssessmentFactory);

            results.Add(analyzer, Function(value));
        }

        return results;
    }

    public override string Run(IEnumerable<HandAnalyzer<TCardGroup, TCardGroupName>> analyzers) => Formatter.FormatData(Name, RunWithExtraArgument(analyzers));
}
