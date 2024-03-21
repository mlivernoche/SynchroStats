using System.Collections.Immutable;

namespace SynchroStats.Features.SmallWorld;

public sealed class SmallWorldCollection<TSmallWorldValue, TName>
    where TSmallWorldValue : IEquatable<TSmallWorldValue>, IComparable<TSmallWorldValue>
    where TName : notnull, IEquatable<TName>, IComparable<TName>
{
    private Func<ISmallWorldTraits, TSmallWorldValue> ValueSelector { get; }
    private Dictionary<TSmallWorldValue, DictionaryWithGeneratedKeys<TName, ISmallWorldCard<TName>>> Cards { get; }

    public SmallWorldCollection(Func<ISmallWorldTraits, TSmallWorldValue> selector, IEnumerable<ISmallWorldCard<TName>> cards)
    {
        ValueSelector = selector;
        Cards = new Dictionary<TSmallWorldValue, DictionaryWithGeneratedKeys<TName, ISmallWorldCard<TName>>>();

        foreach (var card in cards)
        {
            var traits = card.SmallWorldTraits;

            if (traits is null)
            {
                continue;
            }

            var value = ValueSelector(traits);

            if (!Cards.TryGetValue(value, out var collection))
            {
                collection = new DictionaryWithGeneratedKeys<TName, ISmallWorldCard<TName>>(static card => card.Name);
                Cards[value] = collection;
            }

            if (!collection.TryAdd(card))
            {
                throw new Exception("Duplicate cards found.");
            }
        }
    }

    public IReadOnlyDictionary<TName, ISmallWorldCard<TName>> GetCardsBySmallWorldValue(ISmallWorldCard<TName> card)
    {
        var traits = card.SmallWorldTraits;

        if (traits is null)
        {
            return ImmutableDictionary<TName, ISmallWorldCard<TName>>.Empty;
        }

        return GetCardsBySmallWorldValue(ValueSelector(traits));
    }

    public IReadOnlyDictionary<TName, ISmallWorldCard<TName>> GetCardsBySmallWorldValue(TSmallWorldValue smallWorldValue)
    {
        if (!Cards.TryGetValue(smallWorldValue, out var collection))
        {
            return ImmutableDictionary<TName, ISmallWorldCard<TName>>.Empty;
        }

        return collection;
    }

    public bool IsCardPresent(ISmallWorldCard<TName> card)
    {
        var traits = card.SmallWorldTraits;

        if (traits is null)
        {
            return false;
        }

        var collection = GetCardsBySmallWorldValue(ValueSelector(traits));
        return collection.ContainsKey(card.Name);
    }
}
