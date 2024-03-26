namespace SynchroStats.Formatting;

public static class HandAnalyzerComparisonFormatter
{
    public static IHandAnalyzerComparisonFormatter<R> CreateProbabilityFormatter<R>(int categoryNameLength, int valueLength)
        where R : IComparable, IComparable<R>, IEquatable<R>
    {
        return new ProbabilityComparisonFormatter<R>(categoryNameLength, valueLength);
    }

    public static IHandAnalyzerComparisonFormatter<R> CreateNumericalFormatter<R>(int categoryNameLength, int valueLength)
        where R : IComparable, IComparable<R>, IEquatable<R>
    {
        return new NumericalComparisonFormatter<R>(categoryNameLength, valueLength);
    }

    public static IHandAnalyzerComparisonFormatter<string> CreateHandAnalyzerNamesFormatter(int categoryNameLength, int valueLength)
    {
        return new HandAnalyzerNameFormatter(categoryNameLength, valueLength);
    }
}
