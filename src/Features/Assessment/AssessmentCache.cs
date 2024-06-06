using SynchroStats.Data;
using SynchroStats.Features.Analysis;
using SynchroStats.Features.Combinations;
using System.Collections.Concurrent;

namespace SynchroStats.Features.Assessment;

internal sealed class AssessmentCache<TCardGroup, TCardGroupName, TAssessment>
    where TCardGroup : ICardGroup<TCardGroupName>
    where TCardGroupName : notnull, IEquatable<TCardGroupName>, IComparable<TCardGroupName>
    where TAssessment : IHandAssessment<TCardGroupName>
{
    private ConcurrentDictionary<HandAnalyzer<TCardGroup, TCardGroupName>, HandAssessmentAnalyzer<TCardGroup, TCardGroupName, TAssessment>> AssessmentAnalyzers { get; } = new();
    private readonly object _lock = new object();

    public HandAssessmentAnalyzer<TCardGroup, TCardGroupName, TAssessment> GetAnalyzer(HandAnalyzer<TCardGroup, TCardGroupName> analyzer, Func<HandCombination<TCardGroupName>, HandAnalyzer<TCardGroup, TCardGroupName>, TAssessment> assessmentFactory)
    {
        lock (_lock)
        {
            if (!AssessmentAnalyzers.TryGetValue(analyzer, out HandAssessmentAnalyzer<TCardGroup, TCardGroupName, TAssessment>? value))
            {
                value = analyzer.AssessHands(assessmentFactory);
                AssessmentAnalyzers[analyzer] = value;
            }

            return value;
        }
    }

    public HandAssessmentAnalyzer<TCardGroup, TCardGroupName, TAssessment> GetAnalyzer(HandAnalyzer<TCardGroup, TCardGroupName> analyzer, Func<HandCombination<TCardGroupName>, TAssessment> assessmentFactory)
    {
        lock (_lock)
        {
            if (!AssessmentAnalyzers.TryGetValue(analyzer, out HandAssessmentAnalyzer<TCardGroup, TCardGroupName, TAssessment>? value))
            {
                value = analyzer.AssessHands(assessmentFactory);
                AssessmentAnalyzers[analyzer] = value;
            }

            return value;
        }
    }
}
