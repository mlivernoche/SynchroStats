using System.Text;
using SynchroStats.Data;
using SynchroStats.Features.Analysis;

namespace SynchroStats.Formatting;

internal sealed class NumericalComparisonFormatter<R> : IHandAnalyzerComparisonFormatter<R>
    where R : IComparable, IComparable<R>, IEquatable<R>
{
    private int CategoryNameLength { get; }
    private int ValueLength { get; }

    public NumericalComparisonFormatter(int categoryNameLength, int valueLength)
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

        foreach (var result in analyzers.Keys)
        {
            sb.Append($"{result:N0}".PadRight(ValueLength));
        }

        return sb.ToString();
    }
}
