using System.Collections;
using System.Collections.Concurrent;
using System.Text;
using SynchroStats.Data;
using SynchroStats.Formatting;

namespace SynchroStats.Features.Analysis;

public static class HandAnalyzerComparison
{
    public static HandAnalyzerComparison<TCardGroup, TCardGroupName> Create<TCardGroup, TCardGroupName>(IEnumerable<HandAnalyzer<TCardGroup, TCardGroupName>> values)
        where TCardGroup : ICardGroup<TCardGroupName>
        where TCardGroupName : notnull, IEquatable<TCardGroupName>, IComparable<TCardGroupName>
    {
        return new HandAnalyzerComparison<TCardGroup, TCardGroupName>(values);
    }

    public static HandAnalyzerComparison<TCardGroup, TCardGroupName> Run<TCardGroup, TCardGroupName>(IHandAnalyzerOutputStream outputStream, HandAnalyzerComparison<TCardGroup, TCardGroupName> comparison, IHandAnalyzerComparisonFormatter<string> handAnalyzerNamesFormatter)
        where TCardGroup : ICardGroup<TCardGroupName>
        where TCardGroupName : notnull, IEquatable<TCardGroupName>, IComparable<TCardGroupName>
    {
        comparison.Run(outputStream, handAnalyzerNamesFormatter);
        return comparison;
    }

    public static HandAnalyzerComparison<TCardGroup, TCardGroupName> RunInParallel<TCardGroup, TCardGroupName>(IHandAnalyzerOutputStream outputStream, HandAnalyzerComparison<TCardGroup, TCardGroupName> comparison, IHandAnalyzerComparisonFormatter<string> handAnalyzerNamesFormatter)
        where TCardGroup : ICardGroup<TCardGroupName>
        where TCardGroupName : notnull, IEquatable<TCardGroupName>, IComparable<TCardGroupName>
    {
        comparison.RunInParallel(outputStream, handAnalyzerNamesFormatter);
        return comparison;
    }
}

public class HandAnalyzerComparison<TCardGroup, TCardGroupName> : IEnumerable<IHandAnalyzerComparisonCategory<TCardGroup, TCardGroupName>>
    where TCardGroup : ICardGroup<TCardGroupName>
    where TCardGroupName : notnull, IEquatable<TCardGroupName>, IComparable<TCardGroupName>
{
    private List<HandAnalyzer<TCardGroup, TCardGroupName>> Analyzers { get; } = new();
    private List<IHandAnalyzerComparisonCategory<TCardGroup, TCardGroupName>> Categories { get; } = new List<IHandAnalyzerComparisonCategory<TCardGroup, TCardGroupName>>();

    public HandAnalyzerComparison(IEnumerable<HandAnalyzer<TCardGroup, TCardGroupName>> analyzers)
    {
        Analyzers.AddRange(analyzers);
    }

    public void Add(IHandAnalyzerComparisonCategory<TCardGroup, TCardGroupName> category)
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
        var list = new List<(int, IHandAnalyzerComparisonCategory<TCardGroup, TCardGroupName>)>(Categories.Count);

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

    public IEnumerator<IHandAnalyzerComparisonCategory<TCardGroup, TCardGroupName>> GetEnumerator()
    {
        IEnumerable<IHandAnalyzerComparisonCategory<TCardGroup, TCardGroupName>> enumerable = Categories;
        return enumerable.GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        IEnumerable enumerable = Categories;
        return enumerable.GetEnumerator();
    }
}
