using System.Collections.Immutable;
using SynchroStats.Data;

namespace SynchroStats.Features.Combinations;

internal static class HandCombinationFinder
{
    private sealed class HandStackWithSizeCounter<T>
        where T : notnull, IEquatable<T>, IComparable<T>
    {
        private int Size;
        private readonly Stack<HandElement<T>> Storage = new(128);

        public HandElement<T>[] GetHandPermutations()
        {
            return Storage.ToArray();
        }

        public int GetHandSize()
        {
            return Size;
        }

        public HandElement<T> Pop()
        {
            var pop = Storage.Pop();
            Size -= pop.MinimumSize;
            return pop;
        }

        public void Push(HandElement<T> handPermutation)
        {
            Size += handPermutation.MinimumSize;
            Storage.Push(handPermutation);
        }
    }

    internal static IImmutableSet<HandCombination<U>> GetCombinations<T, U>(int startingHandSize, IEnumerable<T> cardGroups)
        where T : ICardGroup<U>
        where U : notnull, IEquatable<U>, IComparable<U>
    {
        var hand = cardGroups
                .Select(static group => new HandElement<U>
                {
                    HandName = group.Name,
                    MinimumSize = group.Minimum,
                    MaximumSize = group.Maximum,
                })
                .ToHashSet();

        return GetCombinations(startingHandSize, hand);
    }

    internal static IImmutableSet<HandCombination<T>> GetCombinations<T>(int startingHandSize, IEnumerable<HandElement<T>> cardGroups)
        where T : notnull, IEquatable<T>, IComparable<T>
    {
        return GetCombinations(startingHandSize, cardGroups.ToHashSet());
    }

    private static ImmutableHashSet<HandCombination<T>> GetCombinations<T>(int startingHandSize, HashSet<HandElement<T>> cardGroups)
        where T : notnull, IEquatable<T>, IComparable<T>
    {
        static void Recursive(int startingHandSize, HandStackWithSizeCounter<T> hand, List<HandElement<T>[]> storage, Stack<HandElement<T>> start)
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
                hand.Push(new HandElement<T>()
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

        var permutations = new List<HandElement<T>[]>(32768);
        var stack = new HandStackWithSizeCounter<T>();
        var start = new Stack<HandElement<T>>(cardGroups);
        Recursive(startingHandSize, stack, permutations, start);

        var emptyHand = cardGroups
            .Select(static permutation => new HandElement<T>
            {
                HandName = permutation.HandName,
                MinimumSize = 0,
                MaximumSize = permutation.MaximumSize,
            })
            .ToHashSet();

        var completeSet = new List<HandCombination<T>>();

        foreach (var handPermutation in permutations)
        {
            var handWithEmpties = new HashSet<HandElement<T>>(handPermutation, HandCombinationNameComparer<T>.Default);
            handWithEmpties.UnionWith(emptyHand);

            completeSet.Add(new HandCombination<T>(handWithEmpties));
        }

        return completeSet.ToImmutableHashSet();
    }
}
