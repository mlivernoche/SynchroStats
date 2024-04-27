using System.Collections;
using System.Diagnostics.CodeAnalysis;
using SynchroStats.Data;
using SynchroStats.Features.Analysis;
using SynchroStats.Features.Combinations;

namespace SynchroStats;

public static class CardList
{
    public static CardList<T, U> Create<T, U>(IEnumerable<T> cards)
        where T : ICardGroup<U>
        where U : notnull, IEquatable<U>, IComparable<U>
    {
        return new CardList<T, U>(cards);
    }

    public static CardList<TCardGroup, TCardGroupName> Create<TCardGroup, TCardGroupName>(HandAnalyzer<TCardGroup, TCardGroupName> analyzer)
        where TCardGroup : ICardGroup<TCardGroupName>
        where TCardGroupName : notnull, IEquatable<TCardGroupName>, IComparable<TCardGroupName>
    {
        return new CardList<TCardGroup, TCardGroupName>(analyzer.CardGroups.Values);
    }

    public static CardList<T, U> CheckSize<T, U>(this CardList<T, U> cards, int size)
        where T : ICardGroup<U>
        where U : notnull, IEquatable<U>, IComparable<U>
    {
        var listSize = cards.Sum(static group => group.Size);
        if (size != listSize)
        {
            throw new Exception();
        }
        return cards;
    }

    public static CardList<T, U> Change<T, U>(this CardList<T, U> cards, T card)
        where T : ICardGroup<U>
        where U : notnull, IEquatable<U>, IComparable<U>
    {
        return cards.Change(card);
    }

    public static CardList<TCardGroup, TCardGroupName> Change<TCardGroup, TCardGroupName>(this CardList<TCardGroup, TCardGroupName> cards, TCardGroupName name, Func<TCardGroup, TCardGroup> change)
        where TCardGroup : ICardGroup<TCardGroupName>
        where TCardGroupName : notnull, IEquatable<TCardGroupName>, IComparable<TCardGroupName>
    {
        return cards.Change(name, change);
    }

    public static CardList<T, U> Remove<T, U>(this CardList<T, U> cards, U card)
        where T : ICardGroup<U>
        where U : notnull, IEquatable<U>, IComparable<U>
    {
        return cards.Remove(card);
    }

    public static CardList<T, U> Minus<T, U>(this IEnumerable<T> cards, HandCombination<U> hand, Func<T, int, T> selector)
        where T : ICardGroup<U>
        where U : notnull, IEquatable<U>, IComparable<U>
    {
        var dict = new DictionaryWithGeneratedKeys<U, T>(static group => group.Name, cards);

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

        return new CardList<T, U>(dict.Values);
    }

    public static CardList<T, U> RemoveCardFromDeck<T, U>(this IEnumerable<T> cards, U cardInHand, Func<T, int, T> selector)
        where T : ICardGroup<U>
        where U : notnull, IEquatable<U>, IComparable<U>
    {
        var dict = new DictionaryWithGeneratedKeys<U, T>(static group => group.Name, cards);

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

        return new CardList<T, U>(dict.Values);
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

public sealed class CardList<T, U> : IReadOnlyCollection<T>
    where T : ICardGroup<U>
    where U : notnull, IEquatable<U>, IComparable<U>
{
    private sealed class Comparer : IEqualityComparer<T>
    {
        public static readonly IEqualityComparer<T> Instance = new Comparer();

        public bool Equals(T? x, T? y)
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

        public int GetHashCode([DisallowNull] T obj)
        {
            return NameComparer.Instance.GetHashCode(obj.Name);
        }
    }

    private sealed class NameComparer : IEqualityComparer<U>
    {
        public static readonly IEqualityComparer<U> Instance = new NameComparer();

        public bool Equals(U? x, U? y)
        {
            if(x is null)
            {
                return y is null;
            }

            return x.Equals(y);
        }

        public int GetHashCode([DisallowNull] U obj)
        {
            return obj.GetHashCode();
        }
    }

    private HashSet<T> Cards { get; }

    public int Count => Cards.Count;

    public IReadOnlyCollection<U> Names { get; }

    public CardList(IEnumerable<T> cards)
    {
        Cards = new HashSet<T>(cards.Where(static group => group.Size > 0), Comparer.Instance);
        Names = Cards.Select(static group => group.Name).ToList();
    }

    internal CardList<T, U> Change(U name, Func<T, T> change)
    {
        T? oldCard = default;
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

    internal CardList<T, U> Change(T newValue)
    {
        var cards = new HashSet<T>(Cards, Comparer.Instance);

        // items are compared by U, so this is removing any old
        // item that have the same U as newValue.
        cards.Remove(newValue);

        // add the newValue that has the data we want.
        cards.Add(newValue);

        return new CardList<T, U>(cards);
    }

    internal CardList<T, U> Remove(U cardName)
    {
        var cards = new HashSet<T>(Cards, Comparer.Instance);
        cards.RemoveWhere(group => NameComparer.Instance.Equals(group.Name, cardName));
        return new CardList<T, U>(cards);
    }

    public IEnumerator<T> GetEnumerator()
    {
        IEnumerable<T> enumerable = Cards;
        return enumerable.GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        IEnumerable enumerable = Cards;
        return enumerable.GetEnumerator();
    }
}
