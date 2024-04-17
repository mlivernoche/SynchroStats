using System.Collections.Immutable;
using SynchroStats.Data;

namespace SynchroStats.Features.Combinations;

internal static class HandCombinationFinder
{
    private sealed class HandStackWithSizeCounter<TCardGroupName>
        where TCardGroupName : notnull, IEquatable<TCardGroupName>, IComparable<TCardGroupName>
    {
        private int Size;
        private readonly Stack<HandElement<TCardGroupName>> Storage = new(128);

        public HandElement<TCardGroupName>[] GetHandPermutations()
        {
            return Storage.ToArray();
        }

        public int GetHandSize()
        {
            return Size;
        }

        public HandElement<TCardGroupName> Pop()
        {
            var pop = Storage.Pop();
            Size -= pop.MinimumSize;
            return pop;
        }

        public void Push(HandElement<TCardGroupName> handPermutation)
        {
            Size += handPermutation.MinimumSize;
            Storage.Push(handPermutation);
        }
    }

    internal static IImmutableSet<HandCombination<TCardGroupName>> GetCombinations<TCardGroup, TCardGroupName>(int startingHandSize, IEnumerable<TCardGroup> cardGroups)
        where TCardGroup : ICardGroup<TCardGroupName>
        where TCardGroupName : notnull, IEquatable<TCardGroupName>, IComparable<TCardGroupName>
    {
        var hand = cardGroups
                .Select(static group => new HandElement<TCardGroupName>
                {
                    HandName = group.Name,
                    MinimumSize = group.Minimum,
                    MaximumSize = group.Maximum,
                })
                .ToHashSet();

        return GetCombinations(startingHandSize, hand);
    }

    internal static IImmutableSet<HandCombination<TCardGroupName>> GetCombinations<TCardGroupName>(int startingHandSize, IEnumerable<HandElement<TCardGroupName>> cardGroups)
        where TCardGroupName : notnull, IEquatable<TCardGroupName>, IComparable<TCardGroupName>
    {
        return GetCombinations(startingHandSize, cardGroups.ToHashSet());
    }

    private static ImmutableHashSet<HandCombination<TCardGroupName>> GetCombinations<TCardGroupName>(int startingHandSize, HashSet<HandElement<TCardGroupName>> cardGroups)
        where TCardGroupName : notnull, IEquatable<TCardGroupName>, IComparable<TCardGroupName>
    {
        static void Recursive(int startingHandSize, HandStackWithSizeCounter<TCardGroupName> hand, List<HandElement<TCardGroupName>[]> storage, Stack<HandElement<TCardGroupName>> start)
        {
            if (start.Count == 0)
            {
                if (hand.GetHandSize() != startingHandSize)
                {
                    return;
                }

                storage.Add(hand.GetHandPermutations());
                return;
            }

            var group = start.Pop();

            for (var i = group.MinimumSize; i <= group.MaximumSize; i++)
            {
                hand.Push(new HandElement<TCardGroupName>()
                {
                    HandName = group.HandName,
                    MinimumSize = i,
                    MaximumSize = group.MaximumSize
                });

                Recursive(startingHandSize, hand, storage, start);

                hand.Pop();
            }

            start.Push(group);
        }

        var permutations = new List<HandElement<TCardGroupName>[]>(32768);
        var stack = new HandStackWithSizeCounter<TCardGroupName>();
        var start = new Stack<HandElement<TCardGroupName>>(cardGroups);
        Recursive(startingHandSize, stack, permutations, start);

        var emptyHand = cardGroups
            .Select(static permutation => new HandElement<TCardGroupName>
            {
                HandName = permutation.HandName,
                MinimumSize = 0,
                MaximumSize = permutation.MaximumSize,
            })
            .ToHashSet();

        var completeSet = new List<HandCombination<TCardGroupName>>();

        foreach (var handPermutation in permutations)
        {
            var handWithEmpties = new HashSet<HandElement<TCardGroupName>>(handPermutation, HandCombinationNameComparer<TCardGroupName>.Default);
            handWithEmpties.UnionWith(emptyHand);

            completeSet.Add(new HandCombination<TCardGroupName>(handWithEmpties));
        }

        return [.. completeSet];
    }
}
