using CommunityToolkit.Diagnostics;
using SynchroStats.Data;
using SynchroStats.Features.Analysis;
using SynchroStats.Features.Combinations;

namespace SynchroStats.Features.Assessment;

public class HandAssessmentAnalyzer<TCardGroup, TCardGroupName, TAssessment>
    where TCardGroup : ICardGroup<TCardGroupName>
    where TCardGroupName : notnull, IEquatable<TCardGroupName>, IComparable<TCardGroupName>
    where TAssessment : IHandAssessment<TCardGroupName>
{
    private HandAnalyzer<TCardGroup, TCardGroupName> Analyzer { get; }
    public double Probability { get; }
    public IReadOnlyCollection<TAssessment> Assessments { get; }

    public HandAssessmentAnalyzer(HandAnalyzer<TCardGroup, TCardGroupName> analyzer, double prob, IReadOnlyCollection<TAssessment> assessments)
    {
        Analyzer = analyzer;
        Probability = prob;
        Assessments = new DictionaryWithGeneratedKeys<HandCombination<TCardGroupName>, TAssessment>(static assessment => assessment.Hand, assessments)
            .Values
            .ToList();
    }

    public double CalculateProbability(TAssessment assessment)
    {
        return Analyzer.CalculateProbability(assessment.Hand);
    }

    public double CalculateProbability(Func<TAssessment, bool> filter)
    {
        var prob = 0.0;

        foreach (var assessment in Assessments.Where(filter))
        {
            prob += Analyzer.CalculateProbability(assessment.Hand);
        }

        Guard.IsGreaterThanOrEqualTo(prob, 0.0);
        Guard.IsLessThanOrEqualTo(prob, 1.0);

        return prob;
    }

    public double CalculateExpectedValue(Func<TAssessment, double> valueFunction)
    {
        var expectedValue = 0.0;

        foreach (var assessment in Assessments)
        {
            var count = valueFunction(assessment);
            if(count > 0)
            {
                expectedValue += count * Analyzer.CalculateProbability(assessment.Hand);
            }
        }

        return expectedValue;
    }

    public TAggregate Aggregate<TAggregate>(Func<IReadOnlyCollection<TAssessment>, TAggregate> calculator)
    {
        return calculator(Assessments);
    }
}
