using SynchroStats.Features.Analysis;
using SynchroStats.Features.Combinations;

namespace SynchroStats.Data.Operations;

public static class Filters
{
    public static bool HasCard<T, U>(this HandAnalyzer<T, U> analyzer, U cardName)
        where T : ICardGroup<U>
        where U : notnull, IEquatable<U>, IComparable<U>
    {
        if(!analyzer.CardGroups.TryGetValue(cardName, out var group))
        {
            return false;
        }

        return group.Size > 0;
    }

    public static IEnumerable<HandElement<U>> GetCardsInHand<U>(this HandCombination<U> cards)
        where U : notnull, IEquatable<U>, IComparable<U>
    {
        foreach (var element in cards.CardNames)
        {
            if (element.MinimumSize == 0)
            {
                continue;
            }

            yield return element;
        }
    }

    public static IEnumerable<TCardGroup> GetCardsInHand<TCardGroup, TCardGroupName>(this HandCombination<TCardGroupName> cards, HandAnalyzer<TCardGroup, TCardGroupName> analyzer)
        where TCardGroup : ICardGroup<TCardGroupName>
        where TCardGroupName : notnull, IEquatable<TCardGroupName>, IComparable<TCardGroupName>
    {
        foreach (var element in cards.CardNames)
        {
            if (element.MinimumSize == 0)
            {
                continue;
            }

            if(!analyzer.CardGroups.TryGetValue(element.HandName, out var group))
            {
                throw new Exception($"Card in hand \"{element.HandName}\" not in card list.");
            }

            yield return group;
        }
    }

    private static bool HasCard<U>(HandElement<U> card, IEnumerable<U> contains)
        where U : notnull, IEquatable<U>, IComparable<U>
    {
        return card.MinimumSize > 0 && contains.Contains(card.HandName);
    }

    public static bool OnlyHasSingles<U>(this HandCombination<U> cards)
        where U : notnull, IEquatable<U>, IComparable<U>
    {
        return cards
            .CardNames
            .All(static card => card.MinimumSize <= 1);
    }

    public static bool HasDuplicates<U>(this HandCombination<U> cards)
        where U : notnull, IEquatable<U>, IComparable<U>
    {
        return cards
            .CardNames
            .Any(static card => card.MinimumSize > 1);
    }

    public static bool OnlyHasDuplicates<U>(this HandCombination<U> cards)
        where U : notnull, IEquatable<U>, IComparable<U>
    {
        return cards
            .CardNames
            .Where(static card => card.MinimumSize > 0)
            .All(static card => card.MinimumSize > 1);
    }

    public static bool HasThisCard<U>(this HandCombination<U> cards, U cardName)
        where U : notnull, IEquatable<U>, IComparable<U>
    {
        foreach (var card in cards.GetCardsInHand())
        {
            if (card.HandName.Equals(cardName))
            {
                return true;
            }
        }

        return false;
    }

    public static bool HasAnyOfTheseCards<U>(this HandCombination<U> cards, IEnumerable<U> cardNames)
        where U : notnull, IEquatable<U>, IComparable<U>
    {
        foreach (var card in cards.CardNames)
        {
            if (HasCard(card, cardNames))
            {
                return true;
            }
        }

        return false;
    }

    public static bool HasAllOfTheseCards<U>(this HandCombination<U> cards, IEnumerable<U> cardNames)
        where U : notnull, IEquatable<U>, IComparable<U>
    {
        var filtered = cards.GetCardsInHand().Select(static card => card.HandName);
        var set = new HashSet<U>(filtered);
        return set.IsProperSupersetOf(cardNames);
    }

    public static int CountCardNames<U>(this HandCombination<U> cards)
        where U : notnull, IEquatable<U>, IComparable<U>
    {
        var count = 0;

        foreach (var card in cards.GetCardsInHand())
        {
            count++;
        }

        return count;
    }

    public static int CountCopiesOfCardInHand<U>(this HandCombination<U> cards, U cardName)
        where U : notnull, IEquatable<U>, IComparable<U>
    {
        foreach(var card in cards.CardNames)
        {
            if(!card.HandName.Equals(cardName))
            {
                continue;
            }

            return card.MinimumSize;
        }

        return 0;
    }

    public static int CountCopiesOfCardInHand<U>(this HandCombination<U> cards, IEnumerable<U> cardNames)
        where U : notnull, IEquatable<U>, IComparable<U>
    {
        var total = 0;

        foreach(var card in cardNames)
        {
            total += cards.CountCopiesOfCardInHand(card);
        }

        return total;
    }

    public static int CountCardNameInHand<U>(this HandCombination<U> cards, U cardName)
        where U : notnull, IEquatable<U>, IComparable<U>
    {

        foreach (var card in cards.CardNames)
        {
            if (!card.HandName.Equals(cardName))
            {
                continue;
            }

            return 1;
        }

        return 0;
    }

    /// <summary>
    /// Counts each individual card names, but not their duplicates. For example, if a card
    /// name is found two times, that isn't counted as two, but one.
    /// </summary>
    /// <typeparam name="U">The card name type.</typeparam>
    /// <param name="cards">The hand to analyze.</param>
    /// <param name="cardNames">The cards to look for.</param>
    /// <returns>The amount of names found (this number does not include duplicates).</returns>
    public static int CountCardNamesInHand<U>(this HandCombination<U> cards, IEnumerable<U> cardNames)
        where U : notnull, IEquatable<U>, IComparable<U>
    {
        var count = 0;

        foreach(var card in cards.CardNames)
        {
            if(!HasCard(card, cardNames))
            {
                continue;
            }

            count++;
        }

        return count;
    }

    public static IEnumerable<T> FilterByName<T, U, C>(this IReadOnlyDictionary<U, C> cards, IEnumerable<T> names)
        where T : INamedCard<U>
        where U : notnull, IEquatable<U>, IComparable<U>
    {
        foreach(var cardName in names)
        {
            if(cards.ContainsKey(cardName.Name))
            {
                yield return cardName;
            }
        }
    }

    public static IEnumerable<T> FilterByName<T, U>(this IReadOnlyDictionary<U, T> cards, IEnumerable<U> names)
    {
        foreach (var cardName in names)
        {
            if (cards.TryGetValue(cardName, out var card))
            {
                yield return card;
            }
        }
    }
}
