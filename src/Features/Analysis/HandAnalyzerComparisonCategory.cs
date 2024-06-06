using SynchroStats.Data;
using SynchroStats.Formatting;

namespace SynchroStats.Features.Analysis;

public static class HandAnalyzerComparisonCategory
{
    public static IHandAnalyzerComparisonCategory<TCardGroup, TCardGroupName> Create<TCardGroup, TCardGroupName, TReturn>(string name, IHandAnalyzerComparisonFormatter<TReturn> formatter, Func<HandAnalyzer<TCardGroup, TCardGroupName>, TReturn> func)
        where TCardGroup : ICardGroup<TCardGroupName>
        where TCardGroupName : notnull, IEquatable<TCardGroupName>, IComparable<TCardGroupName>
    {
        return new HandAnalyzerComparisonCategory<TCardGroup, TCardGroupName, TReturn>(name, formatter, func);
    }

    public static HandAnalyzerComparison<TCardGroup, TCardGroupName> Add<TCardGroup, TCardGroupName, TReturn>(this HandAnalyzerComparison<TCardGroup, TCardGroupName> comparison, string name, IHandAnalyzerComparisonFormatter<TReturn> formatter, Func<HandAnalyzer<TCardGroup, TCardGroupName>, TReturn> func)
        where TCardGroup : ICardGroup<TCardGroupName>
        where TCardGroupName : notnull, IEquatable<TCardGroupName>, IComparable<TCardGroupName>
    {
        var category = new HandAnalyzerComparisonCategory<TCardGroup, TCardGroupName, TReturn>(name, formatter, func);
        comparison.Add(category);
        return comparison;
    }

    public static IHandAnalyzerComparisonCategory<TCardGroup, TCardGroupName> Create<TCardGroup, TCardGroupName, TArgs, TReturn>(string name, IHandAnalyzerComparisonFormatter<TReturn> formatter, TArgs args, Func<HandAnalyzer<TCardGroup, TCardGroupName>, TArgs, TReturn> func)
        where TCardGroup : ICardGroup<TCardGroupName>
        where TCardGroupName : notnull, IEquatable<TCardGroupName>, IComparable<TCardGroupName>
    {
        return new HandAnalyzerComparisonCategory<TCardGroup, TCardGroupName, TArgs, TReturn>(name, formatter, args, func);
    }

    public static HandAnalyzerComparison<TCardGroup, TCardGroupName> Add<TCardGroup, TCardGroupName, TArgs, TReturn>(this HandAnalyzerComparison<TCardGroup, TCardGroupName> comparison, string name, IHandAnalyzerComparisonFormatter<TReturn> formatter, TArgs args, Func<HandAnalyzer<TCardGroup, TCardGroupName>, TArgs, TReturn> func)
        where TCardGroup : ICardGroup<TCardGroupName>
        where TCardGroupName : notnull, IEquatable<TCardGroupName>, IComparable<TCardGroupName>
    {
        var category = new HandAnalyzerComparisonCategory<TCardGroup, TCardGroupName, TArgs, TReturn>(name, formatter, args, func);
        comparison.Add(category);
        return comparison;
    }
}

internal abstract class HandAnalyzerComparisonCategory<TCardGroup, TCardGroupName> : IHandAnalyzerComparisonCategory<TCardGroup, TCardGroupName>
    where TCardGroup : ICardGroup<TCardGroupName>
    where TCardGroupName : notnull, IEquatable<TCardGroupName>, IComparable<TCardGroupName>
{
    public string Name { get; }

    protected HandAnalyzerComparisonCategory(string name)
    {
        Name = name;
    }

    public abstract string Run(IEnumerable<HandAnalyzer<TCardGroup, TCardGroupName>> analyzers);

    protected static Dictionary<HandAnalyzer<TCardGroup, TCardGroupName>, R> Run<R>(IEnumerable<HandAnalyzer<TCardGroup, TCardGroupName>> analyzers, Func<HandAnalyzer<TCardGroup, TCardGroupName>, R> func)
    {
        var results = new Dictionary<HandAnalyzer<TCardGroup, TCardGroupName>, R>();

        foreach (var analyzer in analyzers)
        {
            results.Add(analyzer, func(analyzer));
        }

        return results;
    }
}

internal sealed class HandAnalyzerComparisonCategory<TCardGroup, TCardGroupName, TReturn> : HandAnalyzerComparisonCategory<TCardGroup, TCardGroupName>
    where TCardGroup : ICardGroup<TCardGroupName>
    where TCardGroupName : notnull, IEquatable<TCardGroupName>, IComparable<TCardGroupName>
{
    private Func<HandAnalyzer<TCardGroup, TCardGroupName>, TReturn> Function { get; }
    private IHandAnalyzerComparisonFormatter<TReturn> Formatter { get; }

    public HandAnalyzerComparisonCategory(string name, IHandAnalyzerComparisonFormatter<TReturn> formatter, Func<HandAnalyzer<TCardGroup, TCardGroupName>, TReturn> func)
        : base(name)
    {
        Function = func;
        Formatter = formatter;
    }

    public override string Run(IEnumerable<HandAnalyzer<TCardGroup, TCardGroupName>> analyzers) => Formatter.FormatData(Name, Run(analyzers, Function));
}

internal sealed class HandAnalyzerComparisonCategory<TCardGroup, TCardGroupName, TArgs, TReturn> : HandAnalyzerComparisonCategory<TCardGroup, TCardGroupName>
    where TCardGroup : ICardGroup<TCardGroupName>
    where TCardGroupName : notnull, IEquatable<TCardGroupName>, IComparable<TCardGroupName>
{
    private TArgs Args { get; }
    private Func<HandAnalyzer<TCardGroup, TCardGroupName>, TArgs, TReturn> Function { get; }
    private IHandAnalyzerComparisonFormatter<TReturn> Formatter { get; }

    public HandAnalyzerComparisonCategory(string name, IHandAnalyzerComparisonFormatter<TReturn> formatter, TArgs args, Func<HandAnalyzer<TCardGroup, TCardGroupName>, TArgs, TReturn> func)
        : base(name)
    {
        Function = func;
        Formatter = formatter;
        Args = args;
    }

    private Dictionary<HandAnalyzer<TCardGroup, TCardGroupName>, TReturn> RunWithExtraArgument(IEnumerable<HandAnalyzer<TCardGroup, TCardGroupName>> analyzers)
    {
        var results = new Dictionary<HandAnalyzer<TCardGroup, TCardGroupName>, TReturn>();

        foreach (var analyzer in analyzers)
        {
            results.Add(analyzer, Function(analyzer, Args));
        }

        return results;
    }

    public override string Run(IEnumerable<HandAnalyzer<TCardGroup, TCardGroupName>> analyzers) => Formatter.FormatData(Name, RunWithExtraArgument(analyzers));
}
