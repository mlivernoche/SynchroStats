using SynchroStats.Data;
using SynchroStats.Features.Analysis;
using SynchroStats.Features.Combinations;
using SynchroStats.Formatting;

namespace SynchroStats.Features.Assessment;

public static class HandAssessment
{
    public static HandAnalyzerComparison<TCardGroup, TCardGroupName> AddAssessment<TCardGroup, TCardGroupName, TReturn, TAssessment>(
        this HandAnalyzerComparison<TCardGroup, TCardGroupName> comparison,
        string categoryName,
        IHandAnalyzerComparisonFormatter<TReturn> formatter,
        Func<HandCombination<TCardGroupName>, TAssessment> assessmentFactory,
        Func<HandAssessmentAnalyzer<TCardGroup, TCardGroupName, TAssessment>, TReturn> func)
        where TCardGroup : ICardGroup<TCardGroupName>
        where TCardGroupName : notnull, IEquatable<TCardGroupName>, IComparable<TCardGroupName>
        where TAssessment : IHandAssessment<TCardGroupName>
    {
        var category = new HandAssessmentComparisonCategory<TCardGroup, TCardGroupName, TReturn, TAssessment>(categoryName, formatter, assessmentFactory, func);
        comparison.Add(category);
        return comparison;
    }

    public static HandAnalyzerComparison<TCardGroup, TCardGroupName> AddAssessment<TCardGroup, TCardGroupName, TAssessment>(
        this HandAnalyzerComparison<TCardGroup, TCardGroupName> comparison,
        string categoryName,
        IHandAnalyzerComparisonFormatter<double> formatter,
        Func<HandCombination<TCardGroupName>, TAssessment> assessmentFactory,
        Func<TAssessment, bool> predicate)
        where TCardGroup : ICardGroup<TCardGroupName>
        where TCardGroupName : notnull, IEquatable<TCardGroupName>, IComparable<TCardGroupName>
        where TAssessment : IHandAssessment<TCardGroupName>
    {
        var category = new HandAssessmentComparisonCategory<TCardGroup, TCardGroupName, double, TAssessment>(categoryName, formatter, assessmentFactory, analyzer => analyzer.CalculateProbability(predicate));
        comparison.Add(category);
        return comparison;
    }

    public static HandAnalyzerComparison<TCardGroup, TCardGroupName> AddAssessment<TCardGroup, TCardGroupName, TReturn, TAssessment>(
        this HandAnalyzerComparison<TCardGroup, TCardGroupName> comparison,
        IHandAnalyzerComparisonFormatter<TReturn> formatter,
        Func<HandCombination<TCardGroupName>, TAssessment> assessmentFactory,
        params (string Name, Func<HandAssessmentAnalyzer<TCardGroup, TCardGroupName, TAssessment>, TReturn> Method)[] func)
        where TCardGroup : ICardGroup<TCardGroupName>
        where TCardGroupName : notnull, IEquatable<TCardGroupName>, IComparable<TCardGroupName>
        where TAssessment : IHandAssessment<TCardGroupName>
    {
        foreach(var calculator in func)
        {
            var category = new HandAssessmentComparisonCategory<TCardGroup, TCardGroupName, TReturn, TAssessment>(calculator.Name, formatter, assessmentFactory, calculator.Method);
            comparison.Add(category);
        }

        return comparison;
    }

    public static HandAnalyzerComparison<TCardGroup, TCardGroupName> AddAssessment<TCardGroup, TCardGroupName, TAssessment>(
        this HandAnalyzerComparison<TCardGroup, TCardGroupName> comparison,
        IHandAnalyzerComparisonFormatter<double> formatter,
        Func<HandCombination<TCardGroupName>, TAssessment> assessmentFactory,
        params (string Name, Func<TAssessment, bool> Predicate)[] func)
        where TCardGroup : ICardGroup<TCardGroupName>
        where TCardGroupName : notnull, IEquatable<TCardGroupName>, IComparable<TCardGroupName>
        where TAssessment : IHandAssessment<TCardGroupName>
    {
        foreach (var calculator in func)
        {
            var category = new HandAssessmentComparisonCategory<TCardGroup, TCardGroupName, double, TAssessment>(calculator.Name, formatter, assessmentFactory, analyzer => analyzer.CalculateProbability(calculator.Predicate));
            comparison.Add(category);
        }

        return comparison;
    }
}
