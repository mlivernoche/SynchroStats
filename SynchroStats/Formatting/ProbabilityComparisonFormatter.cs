using System.Text;
using SynchroStats.Data;
using SynchroStats.Features.Analysis;

namespace SynchroStats.Formatting;

internal sealed class ProbabilityComparisonFormatter<R> : IHandAnalyzerComparisonFormatter<R>
    where R : IComparable, IComparable<R>, IEquatable<R>
{
    private int CategoryNameLength { get; }
    private int ValueLength { get; }

    public ProbabilityComparisonFormatter(int categoryNameLength, int valueLength)
    {
        CategoryNameLength = categoryNameLength;
        ValueLength = valueLength;
    }

    public string FormatData<T, U>(string categoryName, IReadOnlyDictionary<HandAnalyzer<T, U>, R> analyzers)
        where T : ICardGroup<U>
        where U : notnull, IEquatable<U>, IComparable<U>
    {
        var sb = new StringBuilder();

        sb.Append(categoryName.PadRight(CategoryNameLength));

        foreach(var result in analyzers.Values)
        {
            sb.Append($"{result:P2}".PadRight(ValueLength));
        }

        return sb.ToString();
    }
}

internal sealed class HandAnalyzerNameFormatter : IHandAnalyzerComparisonFormatter<string>
{
    private int CategoryNameLength { get; }
    private int ValueLength { get; }

    public HandAnalyzerNameFormatter(int categoryNameLength, int valueLength)
    {
        CategoryNameLength = categoryNameLength;
        ValueLength = valueLength;
    }

    public string FormatData<T, U>(string categoryName, IReadOnlyDictionary<HandAnalyzer<T, U>, string> analyzers)
        where T : ICardGroup<U>
        where U : notnull, IEquatable<U>, IComparable<U>
    {
        var sb = new StringBuilder();

        sb.Append(categoryName.PadRight(CategoryNameLength));

        foreach (var result in analyzers.Values)
        {
            sb.Append(result.PadRight(ValueLength));
        }

        return sb.ToString();
    }
}
