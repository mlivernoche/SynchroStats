using CommunityToolkit.Diagnostics;
using SynchroStats.Features.Assessment;

namespace SynchroStats.Data.Math;

public static class StandardDeviation
{
    public static double CalculateStandardDeviation<TCardGroup, TCardGroupName, TAssessment>(this HandAssessmentAnalyzer<TCardGroup, TCardGroupName, TAssessment> analyzer, Func<TAssessment, double> selector)
        where TCardGroup : ICardGroup<TCardGroupName>
        where TCardGroupName : notnull, IEquatable<TCardGroupName>, IComparable<TCardGroupName>
        where TAssessment : IHandAssessment<TCardGroupName>
    {
        var mean = analyzer.Assessments.Average(selector);

        Guard.IsGreaterThanOrEqualTo(mean, 0.0);

        var total = 0.0;
        var count = 0;

        foreach (var data in analyzer.Assessments)
        {
            total += System.Math.Pow(selector(data) - mean, 2.0);
            count++;
        }

        Guard.IsGreaterThan(count, 0);
        Guard.IsGreaterThanOrEqualTo(total, 0.0);

        return System.Math.Sqrt(total / count);
    }

    public static double CalculateStandardDeviation<T>(this IEnumerable<T> assessments, Func<T, double> selector)
    {
        var count = assessments.Count();
        Guard.IsGreaterThan(count, 0);

        var mean = assessments.Average(selector);
        Guard.IsGreaterThanOrEqualTo(mean, 0.0);

        var total = 0.0;

        foreach (var data in assessments)
        {
            total += System.Math.Pow(selector(data) - mean, 2.0);
        }

        Guard.IsGreaterThanOrEqualTo(total, 0.0);

        return System.Math.Sqrt(total / count);
    }
}
