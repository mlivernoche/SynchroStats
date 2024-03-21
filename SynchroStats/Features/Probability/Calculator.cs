using CommunityToolkit.Diagnostics;
using SynchroStats.Data;
using SynchroStats.Features.Combinations;

namespace SynchroStats.Features.Probability;

public static class Calculator
{
    internal static double CalculateProbability<T, U>(IEnumerable<T> cardGroups, IEnumerable<HandCombination<U>> handCombinations, int deckSize, int handSize)
        where T : ICardGroup<U>
        where U : notnull, IEquatable<U>, IComparable<U>
    {
        return CalculateProbability(cardGroups, new HashSet<HandCombination<U>>(handCombinations), deckSize, handSize);
    }

    internal static double CalculateProbability<T, U>(IEnumerable<T> cardGroups, IReadOnlyCollection<HandCombination<U>> handCombinations, int deckSize, int handSize)
        where T : ICardGroup<U>
        where U : notnull, IEquatable<U>, IComparable<U>
    {
        var cardGroupByName = new DictionaryWithGeneratedKeys<U, T>(static group => group.Name, cardGroups);
        var totalProb = 0.0;

        foreach (var handPermutation in handCombinations)
        {
            var currentCardGroup = new List<ICardGroup<U>>(handPermutation.CardNames.Count);

            foreach (var cardGroup in handPermutation.CardNames)
            {
                if (!cardGroupByName.TryGetValue(cardGroup.HandName, out var originalCardGroup))
                {
                    throw new Exception();
                }

                currentCardGroup.Add(new CardGroup<U>
                {
                    Name = originalCardGroup.Name,
                    Size = originalCardGroup.Size,
                    Minimum = cardGroup.MinimumSize,
                    Maximum = cardGroup.MaximumSize
                });
            }

            var prob = Calculate<ICardGroup<U>, U>(currentCardGroup, deckSize, handSize);
            totalProb += prob;
        }

        return totalProb;
    }

    internal static double CalculateProbability<T, U>(IEnumerable<T> cardGroups, HandCombination<U> handPermutation, int deckSize, int handSize)
        where T : ICardGroup<U>
        where U : notnull, IEquatable<U>, IComparable<U>
    {
        var cardGroupByName = new DictionaryWithGeneratedKeys<U, T>(static group => group.Name, cardGroups);
        var currentCardGroup = new List<ICardGroup<U>>(handPermutation.CardNames.Count);

        foreach (var cardGroup in handPermutation.CardNames)
        {
            if (!cardGroupByName.TryGetValue(cardGroup.HandName, out var originalCardGroup))
            {
                throw new Exception();
            }

            currentCardGroup.Add(new CardGroup<U>
            {
                Name = cardGroup.HandName,
                Size = originalCardGroup.Size,
                Minimum = cardGroup.MinimumSize,
                Maximum = cardGroup.MaximumSize,
            });
        }

        return Calculate<ICardGroup<U>, U>(currentCardGroup, deckSize, handSize);
    }

    private static double Calculate<T, U>(IEnumerable<T> cardGroups, int deckSize, int handSize)
        where T : ICardGroup<U>
        where U : notnull, IEquatable<U>, IComparable<U>
    {
        Validation<T, U>(cardGroups, deckSize, handSize);

        var stack = new Stack<T>(cardGroups);

        var top = CalculateProbabilityOfHand<T, U>(handSize, stack);
        var bottom = new CardChance()
        {
            N = deckSize,
            K = handSize
        }.Calculate();
        return top / bottom;
    }

    private static void Validation<T, U>(IEnumerable<T> cardGroups, int deckSize, int handSize)
        where T : ICardGroup<U>
        where U : notnull, IEquatable<U>, IComparable<U>
    {
        {
            var groupSize = cardGroups.Sum(static group => group.Size);

            if (deckSize < groupSize)
            {
                throw new Exception($"Card group aggregate size ({groupSize}) cannot be greater than deck size ({deckSize}).");
            }

            if (deckSize < handSize)
            {
                throw new Exception($"Hand size ({handSize}) cannot be greater than deck size ({deckSize}).");
            }
        }

        {
            foreach (var group in cardGroups)
            {
                if (group.Minimum < 0)
                {
                    throw new Exception($"Minimum ({group.Minimum}) in {group.Name} must be greater than 0.");
                }

                if (group.Maximum > group.Size)
                {
                    throw new Exception($"Maximum ({group.Maximum}) in {group.Name} cannot be greater than size ({group.Size}).");
                }
            }
        }
    }

    private static double CalculateProbabilityOfHand<T, U>(int maxHandSize, Stack<T> cardGroups)
        where T : ICardGroup<U>
        where U : notnull, IEquatable<U>, IComparable<U>
    {
        static double Impl(Stack<CardChance> hand, int currentHandSize, int maxHandSize, Stack<T> cardGroups)
        {
            if (cardGroups.Count == 0 || currentHandSize >= maxHandSize)
            {
                if (currentHandSize == maxHandSize)
                {
                    foreach (var group in cardGroups)
                    {
                        if (group.Minimum != 0)
                        {
                            return 0;
                        }
                    }
                }
                else if (currentHandSize > maxHandSize)
                {
                    return 0;
                }

                var chance = 1.0;
                foreach (var group in hand)
                {
                    chance *= group.Calculate();
                }

                return chance;
            }

            {
                var group = cardGroups.Pop();
                var probs = 0.0;

                for (var i = group.Minimum; i <= group.Maximum; i++)
                {
                    hand.Push(new CardChance()
                    {
                        N = group.Size,
                        K = i
                    });

                    probs += Impl(hand, currentHandSize + i, maxHandSize, cardGroups);

                    hand.Pop();
                }

                cardGroups.Push(group);

                return probs;
            }
        }

        var hand = new Stack<CardChance>();
        return Impl(hand, 0, maxHandSize, cardGroups);
    }

    private sealed class CardChance
    {
        public readonly static double[] FactorialCache;

        static CardChance()
        {
            double factorial = 1.0;
            FactorialCache = new double[171];

            for (var i = 0; i < FactorialCache.Length; i++)
            {
                FactorialCache[i] = factorial;
                factorial *= i + 1.0;
            }
        }

        public int N { get; init; }
        public int K { get; init; }

        public double Calculate()
        {
            var top = Factorial(N);
            var bottom = (Factorial(K) * Factorial(N - K));
            return top / bottom;
        }

        static double Factorial(int number)
        {
            Guard.HasSizeGreaterThan(FactorialCache, number);
            return FactorialCache[number];
        }
    }
}