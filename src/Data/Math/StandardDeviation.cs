namespace SynchroStats.Data.Math;

public static class StandardDeviation
{
    public static double CalculateStandardDeviation<T>(this IEnumerable<T> assessments, Func<T, double> selector)
    {
        var count = assessments.Count();
        if (count == 0)
        {
            return 0.0;
        }

        var mean = assessments.Average(selector);
        var total = 0.0;

        foreach (var data in assessments)
        {
            total += System.Math.Pow(selector(data) - mean, 2.0);
        }

        return System.Math.Sqrt(total / count);
    }
}
