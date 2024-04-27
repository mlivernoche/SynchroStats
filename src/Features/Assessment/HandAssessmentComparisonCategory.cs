using SynchroStats.Data;
using SynchroStats.Features.Analysis;
using SynchroStats.Features.Combinations;
using SynchroStats.Formatting;

namespace SynchroStats.Features.Assessment;

public sealed class HandAssessmentComparisonCategory<TCardGroup, TCardGroupName, TReturn, TAssessment> : HandAnalyzerComparisonCategory<TCardGroup, TCardGroupName>
    where TCardGroup : ICardGroup<TCardGroupName>
    where TCardGroupName : notnull, IEquatable<TCardGroupName>, IComparable<TCardGroupName>
    where TAssessment : IHandAssessment<TCardGroupName>
{
    private Func<HandCombination<TCardGroupName>, TAssessment> AssessmentFactory { get; }
    private Func<HandAssessmentAnalyzer<TCardGroup, TCardGroupName, TAssessment>, TReturn> Function { get; }
    private IHandAnalyzerComparisonFormatter<TReturn> Formatter { get; }

    public HandAssessmentComparisonCategory(string name, IHandAnalyzerComparisonFormatter<TReturn> formatter, Func<HandCombination<TCardGroupName>, TAssessment> assessmentFactory, Func<HandAssessmentAnalyzer<TCardGroup, TCardGroupName, TAssessment>, TReturn> func)
        : base(name)
    {
        AssessmentFactory = assessmentFactory;
        Function = func;
        Formatter = formatter;
    }

    private Dictionary<HandAnalyzer<TCardGroup, TCardGroupName>, TReturn> RunWithExtraArgument(IEnumerable<HandAnalyzer<TCardGroup, TCardGroupName>> analyzers)
    {
        var results = new Dictionary<HandAnalyzer<TCardGroup, TCardGroupName>, TReturn>();

        foreach (var analyzer in analyzers)
        {
            var assessment = analyzer.AssessHands(AssessmentFactory);
            results.Add(analyzer, Function(assessment));
        }

        return results;
    }

    public override string Run(IEnumerable<HandAnalyzer<TCardGroup, TCardGroupName>> analyzers) => Formatter.FormatData(Name, RunWithExtraArgument(analyzers));
}
