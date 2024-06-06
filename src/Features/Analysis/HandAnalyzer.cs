using CommunityToolkit.Diagnostics;
using System.Collections.Concurrent;
using System.Collections.Immutable;
using SynchroStats.Data;
using SynchroStats.Data.Operations;
using SynchroStats.Features.Combinations;
using SynchroStats.Features.Probability;
using SynchroStats.Features.Assessment;

namespace SynchroStats.Features.Analysis;

public static class HandAnalyzer
{
    public static HandAnalyzer<TCardGroup, TCardGroupName> Create<TCardGroup, TCardGroupName>(HandAnalyzerBuildArguments<TCardGroup, TCardGroupName> buildArguments)
        where TCardGroup : ICardGroup<TCardGroupName>
        where TCardGroupName : notnull, IEquatable<TCardGroupName>, IComparable<TCardGroupName>
    {
        return new HandAnalyzer<TCardGroup, TCardGroupName>(buildArguments);
    }

    public static IReadOnlyCollection<HandAnalyzer<TCardGroup, TCardGroupName>> CreateInParallel<TCardGroup, TCardGroupName>(IEnumerable<HandAnalyzerBuildArguments<TCardGroup, TCardGroupName>> buildArguments)
        where TCardGroup : ICardGroup<TCardGroupName>
        where TCardGroupName : notnull, IEquatable<TCardGroupName>, IComparable<TCardGroupName>
    {
        var list = new List<(int, HandAnalyzerBuildArguments<TCardGroup, TCardGroupName>)>();

        {
            int sortId = 0;
            foreach (var buildArgument in buildArguments)
            {
                list.Add((sortId++, buildArgument));
            }
        }

        var analyzers = new ConcurrentBag<(int, HandAnalyzer<TCardGroup, TCardGroupName>)>();

        Parallel.ForEach(list, tuple =>
        {
            var (sortId, buildArgument) = tuple;
            var analyzer = new HandAnalyzer<TCardGroup, TCardGroupName>(buildArgument);
            analyzers.Add((sortId, analyzer));
        });

        return analyzers
            .OrderBy(static x => x.Item1)
            .Select(static x => x.Item2)
            .ToList();
    }



    public static HandAnalyzer<TCardGroup, TCardGroupName> Remove<TCardGroup, TCardGroupName>(this HandAnalyzer<TCardGroup, TCardGroupName> orgAnalyzer, HandCombination<TCardGroupName> hand, Func<TCardGroup, int, TCardGroup> selector)
        where TCardGroup : ICardGroup<TCardGroupName>
        where TCardGroupName : notnull, IEquatable<TCardGroupName>, IComparable<TCardGroupName>
    {
        var cardList = CardList.Create(orgAnalyzer).Minus(hand, selector);
        var args = HandAnalyzerBuildArguments.Create(orgAnalyzer.AnalyzerName, orgAnalyzer.HandSize, cardList);
        return Create(args);
    }

    private static IEnumerable<HandCombination<TCardGroupName>> Filter<TCardGroup, TCardGroupName>(HandAnalyzer<TCardGroup, TCardGroupName> analyzer, Func<HandAnalyzer<TCardGroup, TCardGroupName>, HandCombination<TCardGroupName>, bool> filter)
        where TCardGroup : ICardGroup<TCardGroupName>
        where TCardGroupName : notnull, IEquatable<TCardGroupName>, IComparable<TCardGroupName>
    {
        foreach (var hand in analyzer.Combinations)
        {
            if (filter(analyzer, hand))
            {
                yield return hand;
            }
        }
    }

    public static double CalculateProbability<TCardGroup, TCardGroupName>(this HandAnalyzer<TCardGroup, TCardGroupName> handAnalyzer, Func<HandAnalyzer<TCardGroup, TCardGroupName>, HandCombination<TCardGroupName>, bool> filter)
        where TCardGroup : ICardGroup<TCardGroupName>
        where TCardGroupName : notnull, IEquatable<TCardGroupName>, IComparable<TCardGroupName>
    {
        return Calculator.CalculateProbability(handAnalyzer.CardGroups.Values, Filter(handAnalyzer, filter), handAnalyzer.DeckSize, handAnalyzer.HandSize);
    }

    public static double CalculateProbability<TCardGroup, TCardGroupName, TArgs>(this HandAnalyzer<TCardGroup, TCardGroupName> handAnalyzer, TArgs args, Func<HandAnalyzer<TCardGroup, TCardGroupName>, TArgs, HandCombination<TCardGroupName>, bool> filter)
        where TCardGroup : ICardGroup<TCardGroupName>
        where TCardGroupName : notnull, IEquatable<TCardGroupName>, IComparable<TCardGroupName>
    {
        IEnumerable<HandCombination<TCardGroupName>> GetCombinations()
        {
            foreach (var hand in handAnalyzer.Combinations)
            {
                if (filter(handAnalyzer, args, hand))
                {
                    yield return hand;
                }
            }
        }

        return Calculator.CalculateProbability(handAnalyzer.CardGroups.Values, GetCombinations(), handAnalyzer.DeckSize, handAnalyzer.HandSize);
    }

    public static double CalculateProbability<TCardGroup, TCardGroupName, TArgs>(this HandAnalyzer<TCardGroup, TCardGroupName> handAnalyzer, TArgs args, Func<TArgs, HandCombination<TCardGroupName>, bool> filter)
        where TCardGroup : ICardGroup<TCardGroupName>
        where TCardGroupName : notnull, IEquatable<TCardGroupName>, IComparable<TCardGroupName>
    {
        IEnumerable<HandCombination<TCardGroupName>> GetCombinations()
        {
            foreach (var hand in handAnalyzer.Combinations)
            {
                if (filter(args, hand))
                {
                    yield return hand;
                }
            }
        }

        return Calculator.CalculateProbability(handAnalyzer.CardGroups.Values, GetCombinations(), handAnalyzer.DeckSize, handAnalyzer.HandSize);
    }

    public static double CalculateProbability<TCardGroup, TCardGroupName>(this HandAnalyzer<TCardGroup, TCardGroupName> handAnalyzer, HandCombination<TCardGroupName> hand)
        where TCardGroup : ICardGroup<TCardGroupName>
        where TCardGroupName : notnull, IEquatable<TCardGroupName>, IComparable<TCardGroupName>
    {
        return Calculator.CalculateProbability(handAnalyzer.CardGroups.Values, hand, handAnalyzer.DeckSize, handAnalyzer.HandSize);
    }

    public static double CalculateExpectedValue<TCardGroup, TCardGroupName>(this HandAnalyzer<TCardGroup, TCardGroupName> handAnalyzer, Func<HandCombination<TCardGroupName>, double> valueFunction)
        where TCardGroup : ICardGroup<TCardGroupName>
        where TCardGroupName : notnull, IEquatable<TCardGroupName>, IComparable<TCardGroupName>
    {
        var expectedValue = 0.0;

        foreach(var hand in handAnalyzer.Combinations)
        {
            var value = valueFunction(hand);
            if(value > 0)
            {
                expectedValue += handAnalyzer.CalculateProbability(hand) * value;
            }
        }

        return expectedValue;
    }

    public static double CalculateExpectedValue<TCardGroup, TCardGroupName>(this HandAnalyzer<TCardGroup, TCardGroupName> handAnalyzer, Func<HandAnalyzer<TCardGroup, TCardGroupName>, HandCombination<TCardGroupName>, double> valueFunction)
        where TCardGroup : ICardGroup<TCardGroupName>
        where TCardGroupName : notnull, IEquatable<TCardGroupName>, IComparable<TCardGroupName>
    {
        var expectedValue = 0.0;

        foreach (var hand in handAnalyzer.Combinations)
        {
            var value = valueFunction(handAnalyzer, hand);
            if (value > 0)
            {
                expectedValue += handAnalyzer.CalculateProbability(hand) * value;
            }
        }

        return expectedValue;
    }

    public static double Aggregate<TCardGroup, TCardGroupName, TAggregate>(this HandAnalyzer<TCardGroup, TCardGroupName> handAnalyzer, Func<HandCombination<TCardGroupName>, TAggregate> aggregator, Func<IReadOnlyDictionary<HandCombination<TCardGroupName>, TAggregate>, double> calculator)
        where TCardGroup : ICardGroup<TCardGroupName>
        where TCardGroupName : notnull, IEquatable<TCardGroupName>, IComparable<TCardGroupName>
    {
        var aggregateValues = new Dictionary<HandCombination<TCardGroupName>, TAggregate>(handAnalyzer.Combinations.Count);

        foreach (var hand in handAnalyzer.Combinations)
        {
            aggregateValues[hand] = aggregator(hand);
        }

        return calculator(aggregateValues);
    }

    public static double Aggregate<TCardGroup, TCardGroupName, TAggregate>(this HandAnalyzer<TCardGroup, TCardGroupName> handAnalyzer, Func<HandCombination<TCardGroupName>, HandAnalyzer<TCardGroup, TCardGroupName>, TAggregate> aggregator, Func<IReadOnlyDictionary<HandCombination<TCardGroupName>, TAggregate>, double> calculator)
        where TCardGroup : ICardGroup<TCardGroupName>
        where TCardGroupName : notnull, IEquatable<TCardGroupName>, IComparable<TCardGroupName>
    {
        var aggregateValues = new Dictionary<HandCombination<TCardGroupName>, TAggregate>(handAnalyzer.Combinations.Count);

        foreach (var hand in handAnalyzer.Combinations)
        {
            aggregateValues[hand] = aggregator(hand, handAnalyzer);
        }

        return calculator(aggregateValues);
    }
}

public sealed class HandAnalyzer<TCardGroup, TCardGroupName>
    where TCardGroup : ICardGroup<TCardGroupName>
    where TCardGroupName : notnull, IEquatable<TCardGroupName>, IComparable<TCardGroupName>
{
    public int DeckSize { get; }
    public int HandSize { get; }
    public IReadOnlyDictionary<TCardGroupName, TCardGroup> CardGroups { get; }
    public IImmutableSet<HandCombination<TCardGroupName>> Combinations { get; }
    public string AnalyzerName { get; }

    public HandAnalyzer(HandAnalyzerBuildArguments<TCardGroup, TCardGroupName> args)
    {
        AnalyzerName = args.AnalyzerName;
        HandSize = args.HandSize;

        CardGroups = new DictionaryWithGeneratedKeys<TCardGroupName, TCardGroup>(static group => group.Name, args.CardGroups);

        DeckSize = CardGroups.Values.Sum(static group => group.Size);
        Guard.IsGreaterThanOrEqualTo(DeckSize, 1, nameof(DeckSize));
        Guard.IsLessThanOrEqualTo(DeckSize, 60, nameof(DeckSize));

        Combinations = HandCombinationFinder.GetCombinations<TCardGroup, TCardGroupName>(args.HandSize, CardGroups.Values);
    }

    public double CalculateProbability()
    {
        return Calculator.CalculateProbability(CardGroups.Values, Combinations, DeckSize, HandSize);
    }

    public double CalculateProbability(Func<HandCombination<TCardGroupName>, bool> filter)
    {
        return Calculator.CalculateProbability(CardGroups.Values, Combinations.Where(filter), DeckSize, HandSize);
    }

    public double CalculateProbability(IFilter<HandCombination<TCardGroupName>> filter)
    {
        return Calculator.CalculateProbability(CardGroups.Values, filter.GetResults(Combinations), DeckSize, HandSize);
    }

    public HandAssessmentAnalyzer<TCardGroup, TCardGroupName, TAssessment> AssessHands<TAssessment>(Func<HandCombination<TCardGroupName>, TAssessment> filter)
        where TAssessment : IHandAssessment<TCardGroupName>
    {
        var assessments = Combinations.Select(filter).ToList();
        var includedHands = assessments
            .Where(static assessment => assessment.Included)
            .Select(static assessment => assessment.Hand);
        var prob = Calculator.CalculateProbability(CardGroups.Values, includedHands, DeckSize, HandSize);

        return new HandAssessmentAnalyzer<TCardGroup, TCardGroupName, TAssessment>(this, prob, assessments);
    }

    public HandAssessmentAnalyzer<TCardGroup, TCardGroupName, TAssessment> AssessHands<TAssessment>(Func<HandCombination<TCardGroupName>, HandAnalyzer<TCardGroup, TCardGroupName>, TAssessment> filter)
        where TAssessment : IHandAssessment<TCardGroupName>
    {
        var assessments = new List<TAssessment>();

        foreach(var hand in Combinations)
        {
            assessments.Add(filter(hand, this));
        }

        var includedHands = assessments
            .Where(static assessment => assessment.Included)
            .Select(static assessment => assessment.Hand);
        var prob = Calculator.CalculateProbability(CardGroups.Values, includedHands, DeckSize, HandSize);

        return new HandAssessmentAnalyzer<TCardGroup, TCardGroupName, TAssessment>(this, prob, assessments);
    }

    public int CountHands(Func<HandCombination<TCardGroupName>, bool> filter)
    {
        return Combinations.Count(filter);
    }

    public int[] CountUniqueCardName()
    {
        var counts = new int[HandSize + 1];

        foreach (var combination in Combinations)
        {
            var count = combination.CountCardNames();
            counts[count]++;
        }

        return counts;
    }
}
