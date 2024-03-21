using System.Collections;
using System.Collections.Concurrent;
using System.Text;
using SynchroStats.Data;
using SynchroStats.Formatting;

namespace SynchroStats.Features.Analysis;

public static class HandAnalyzerComparison
{
    public static HandAnalyzerComparison<T, U> Create<T, U>(IEnumerable<HandAnalyzer<T, U>> values)
        where T : ICardGroup<U>
        where U : notnull, IEquatable<U>, IComparable<U>
    {
        return new HandAnalyzerComparison<T, U>(values);
    }

    public static HandAnalyzerComparison<T, U> Run<T, U>(IHandAnalyzerOutputStream outputStream, HandAnalyzerComparison<T, U> comparison, IHandAnalyzerComparisonFormatter<string> handAnalyzerNamesFormatter)
        where T : ICardGroup<U>
        where U : notnull, IEquatable<U>, IComparable<U>
    {
        comparison.Run(outputStream, handAnalyzerNamesFormatter);
        return comparison;
    }

    public static HandAnalyzerComparison<T, U> RunInParallel<T, U>(IHandAnalyzerOutputStream outputStream, HandAnalyzerComparison<T, U> comparison, IHandAnalyzerComparisonFormatter<string> handAnalyzerNamesFormatter)
        where T : ICardGroup<U>
        where U : notnull, IEquatable<U>, IComparable<U>
    {
        comparison.RunInParallel(outputStream, handAnalyzerNamesFormatter);
        return comparison;
    }
}

public class HandAnalyzerComparison<T, U> : IEnumerable<IHandAnalyzerComparisonCategory<T, U>>
    where T : ICardGroup<U>
    where U : notnull, IEquatable<U>, IComparable<U>
{
    private List<HandAnalyzer<T, U>> Analyzers { get; } = new();
    private List<IHandAnalyzerComparisonCategory<T, U>> Categories { get; } = new List<IHandAnalyzerComparisonCategory<T, U>>();

    public HandAnalyzerComparison(IEnumerable<HandAnalyzer<T, U>> analyzers)
    {
        Analyzers.AddRange(analyzers);
    }

    public void Add(IHandAnalyzerComparisonCategory<T, U> category)
    {
        Categories.Add(category);
    }

    public void Run(IHandAnalyzerOutputStream outputStream, IHandAnalyzerComparisonFormatter<string> handAnalyzerNamesFormatter)
    {
        var sb = new StringBuilder();

        foreach (var handAnalyzer in Analyzers)
        {
            sb.AppendLine($"Analyzer: {handAnalyzer.AnalyzerName}. Cards: {handAnalyzer.DeckSize:N0}. Hand Size: {handAnalyzer.HandSize:N0}. Possible Hands: {handAnalyzer.Combinations.Count:N0}.");
        }

        {
            var names = Analyzers.ToDictionary(static key => key, static key => key.AnalyzerName);
            sb.AppendLine(handAnalyzerNamesFormatter.FormatData("Category", names));
        }

        foreach (var category in Categories)
        {
            sb.AppendLine(category.Run(Analyzers));
        }

        outputStream.Write(sb.ToString());
    }

    public void RunInParallel(IHandAnalyzerOutputStream outputStream, IHandAnalyzerComparisonFormatter<string> handAnalyzerNamesFormatter)
    {
        var list = new List<(int, IHandAnalyzerComparisonCategory<T, U>)>(Categories.Count);

        {
            int sortId = 0;
            foreach (var category in Categories)
            {
                list.Add((sortId++, category));
            }
        }

        var output = new ConcurrentBag<(int sortId, string category)>();

        Parallel.ForEach(list, tuple =>
        {
            var (sortId, category) = tuple;
            output.Add((sortId, category.Run(Analyzers)));
        });

        var sb = new StringBuilder();

        foreach (var handAnalyzer in Analyzers)
        {
            sb.AppendLine($"Analyzer: {handAnalyzer.AnalyzerName}. Cards: {handAnalyzer.DeckSize:N0}. Hand Size: {handAnalyzer.HandSize:N0}. Possible Hands: {handAnalyzer.Combinations.Count:N0}.");
        }

        sb.AppendLine();

        {
            var names = Analyzers.ToDictionary(static key => key, static key => key.AnalyzerName);
            sb.AppendLine(handAnalyzerNamesFormatter.FormatData("Category", names));
        }

        foreach (var category in output.OrderBy(static x => x.sortId).Select(static x => x.category))
        {
            sb.AppendLine(category);
        }

        outputStream.Write(sb.ToString());
    }

    public IEnumerator<IHandAnalyzerComparisonCategory<T, U>> GetEnumerator()
    {
        IEnumerable<IHandAnalyzerComparisonCategory<T, U>> enumerable = Categories;
        return enumerable.GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        IEnumerable enumerable = Categories;
        return enumerable.GetEnumerator();
    }
}
