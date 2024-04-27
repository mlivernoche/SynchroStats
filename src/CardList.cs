using System.Collections;
using System.Diagnostics.CodeAnalysis;
using SynchroStats.Data;
using SynchroStats.Features.Analysis;
using SynchroStats.Features.Combinations;

namespace SynchroStats;

public static class CardList
{
    public static CardList<TCardGroup, TCardGroupName> Create<TCardGroup, TCardGroupName>(IEnumerable<TCardGroup> cards)
        where TCardGroup : ICardGroup<TCardGroupName>
        where TCardGroupName : notnull, IEquatable<TCardGroupName>, IComparable<TCardGroupName>
    {
        return new CardList<TCardGroup, TCardGroupName>(cards);
    }

    public static CardList<TCardGroup, TCardGroupName> Create<TCardGroup, TCardGroupName>(HandAnalyzer<TCardGroup, TCardGroupName> analyzer)
        where TCardGroup : ICardGroup<TCardGroupName>
        where TCardGroupName : notnull, IEquatable<TCardGroupName>, IComparable<TCardGroupName>
    {
        return new CardList<TCardGroup, TCardGroupName>(analyzer.CardGroups.Values);
    }

    public static CardList<TCardGroup, TCardGroupName> CheckSize<TCardGroup, TCardGroupName>(this CardList<TCardGroup, TCardGroupName> cards, int size)
        where TCardGroup : ICardGroup<TCardGroupName>
        where TCardGroupName : notnull, IEquatable<TCardGroupName>, IComparable<TCardGroupName>
    {
        var listSize = cards.Sum(static group => group.Size);
        if (size != listSize)
        {
            throw new Exception();
        }
        return cards;
    }

    public static CardList<TCardGroup, TCardGroupName> Change<TCardGroup, TCardGroupName>(this CardList<TCardGroup, TCardGroupName> cards, TCardGroup card)
        where TCardGroup : ICardGroup<TCardGroupName>
        where TCardGroupName : notnull, IEquatable<TCardGroupName>, IComparable<TCardGroupName>
    {
        return cards.Change(card);
    }

    public static CardList<TCardGroup, TCardGroupName> Change<TCardGroup, TCardGroupName>(this CardList<TCardGroup, TCardGroupName> cards, TCardGroupName name, Func<TCardGroup, TCardGroup> change)
        where TCardGroup : ICardGroup<TCardGroupName>
        where TCardGroupName : notnull, IEquatable<TCardGroupName>, IComparable<TCardGroupName>
    {
        return cards.Change(name, change);
    }

    public static CardList<TCardGroup, TCardGroupName> Remove<TCardGroup, TCardGroupName>(this CardList<TCardGroup, TCardGroupName> cards, TCardGroupName card)
        where TCardGroup : ICardGroup<TCardGroupName>
        where TCardGroupName : notnull, IEquatable<TCardGroupName>, IComparable<TCardGroupName>
    {
        return cards.Remove(card);
    }

    public static CardList<TCardGroup, TCardGroupName> Minus<TCardGroup, TCardGroupName>(this IEnumerable<TCardGroup> cards, HandCombination<TCardGroupName> hand, Func<TCardGroup, int, TCardGroup> selector)
        where TCardGroup : ICardGroup<TCardGroupName>
        where TCardGroupName : notnull, IEquatable<TCardGroupName>, IComparable<TCardGroupName>
    {
        var dict = new DictionaryWithGeneratedKeys<TCardGroupName, TCardGroup>(static group => group.Name, cards);

        foreach (var cardInHand in hand.CardNames)
        {
            if (!dict.TryGetValue(cardInHand.HandName, out var card))
            {
                continue;
            }

            var newAmount = card.Size - cardInHand.MinimumSize;
            if (newAmount == 0)
            {
                dict.TryRemove(card);
            }
            else
            {
                dict.AddOrUpdate(selector(card, newAmount));
            }
        }

        return new CardList<TCardGroup, TCardGroupName>(dict.Values);
    }

    public static CardList<TCardGroup, TCardGroupName> RemoveCardFromDeck<TCardGroup, TCardGroupName>(this IEnumerable<TCardGroup> cards, TCardGroupName cardInHand, Func<TCardGroup, int, TCardGroup> selector)
        where TCardGroup : ICardGroup<TCardGroupName>
        where TCardGroupName : notnull, IEquatable<TCardGroupName>, IComparable<TCardGroupName>
    {
        var dict = new DictionaryWithGeneratedKeys<TCardGroupName, TCardGroup>(static group => group.Name, cards);

        if (dict.TryGetValue(cardInHand, out var card))
        {
            var newAmount = card.Size - 1;
            if (newAmount == 0)
            {
                dict.TryRemove(card);
            }
            else
            {
                dict.AddOrUpdate(selector(card, newAmount));
            }
        }

        return new CardList<TCardGroup, TCardGroupName>(dict.Values);
    }

    public static int GetNumberOfCards<TCardGroup, TCardGroupName>(this CardList<TCardGroup, TCardGroupName> cardList)
        where TCardGroup : ICardGroup<TCardGroupName>
        where TCardGroupName : notnull, IEquatable<TCardGroupName>, IComparable<TCardGroupName>
    {
        return cardList.Sum(static group => group.Size);
    }

    public static int GetNumberOfCards<TCardGroup, TCardGroupName>(this IEnumerable<TCardGroup> cardList)
        where TCardGroup : ICardGroup<TCardGroupName>
        where TCardGroupName : notnull, IEquatable<TCardGroupName>, IComparable<TCardGroupName>
    {
        return cardList.Sum(static group => group.Size);
    }
}

public sealed class CardList<TCardGroup, TCardGroupName> : IReadOnlyCollection<TCardGroup>
    where TCardGroup : ICardGroup<TCardGroupName>
    where TCardGroupName : notnull, IEquatable<TCardGroupName>, IComparable<TCardGroupName>
{
    private sealed class Comparer : IEqualityComparer<TCardGroup>
    {
        public static readonly IEqualityComparer<TCardGroup> Instance = new Comparer();

        public bool Equals(TCardGroup? x, TCardGroup? y)
        {
            if (x is null && y is null)
            {
                return true;
            }

            if (x is null || y is null)
            {
                return false;
            }

            return NameComparer.Instance.Equals(x.Name, y.Name);
        }

        public int GetHashCode([DisallowNull] TCardGroup obj)
        {
            return NameComparer.Instance.GetHashCode(obj.Name);
        }
    }

    private sealed class NameComparer : IEqualityComparer<TCardGroupName>
    {
        public static readonly IEqualityComparer<TCardGroupName> Instance = new NameComparer();

        public bool Equals(TCardGroupName? x, TCardGroupName? y)
        {
            if(x is null)
            {
                return y is null;
            }

            return x.Equals(y);
        }

        public int GetHashCode([DisallowNull] TCardGroupName obj)
        {
            return obj.GetHashCode();
        }
    }

    private HashSet<TCardGroup> Cards { get; }

    public int Count => Cards.Count;

    public IReadOnlyCollection<TCardGroupName> Names { get; }

    public CardList(IEnumerable<TCardGroup> cards)
    {
        Cards = new HashSet<TCardGroup>(cards.Where(static group => group.Size > 0), Comparer.Instance);
        Names = Cards.Select(static group => group.Name).ToList();
    }

    internal CardList<TCardGroup, TCardGroupName> Change(TCardGroupName name, Func<TCardGroup, TCardGroup> change)
    {
        TCardGroup? oldCard = default;
        foreach(var card in Cards)
        {
            if(card.Name.Equals(name))
            {
                oldCard = card;
                break;
            }
        }

        if(oldCard != null)
        {
            var chg = change(oldCard);
            return Change(chg);
        }

        return this;
    }

    internal CardList<TCardGroup, TCardGroupName> Change(TCardGroup newValue)
    {
        var cards = new HashSet<TCardGroup>(Cards, Comparer.Instance);

        // items are compared by U, so this is removing any old
        // item that have the same U as newValue.
        cards.Remove(newValue);

        // add the newValue that has the data we want.
        cards.Add(newValue);

        return new CardList<TCardGroup, TCardGroupName>(cards);
    }

    internal CardList<TCardGroup, TCardGroupName> Remove(TCardGroupName cardName)
    {
        var cards = new HashSet<TCardGroup>(Cards);
        cards.RemoveWhere(group => NameComparer.Instance.Equals(group.Name, cardName));
        return new CardList<TCardGroup, TCardGroupName>(Cards);
    }

    public IEnumerator<TCardGroup> GetEnumerator()
    {
        IEnumerable<TCardGroup> enumerable = Cards;
        return enumerable.GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        IEnumerable enumerable = Cards;
        return enumerable.GetEnumerator();
    }
}
