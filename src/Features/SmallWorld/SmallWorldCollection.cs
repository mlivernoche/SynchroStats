using System.Collections.Immutable;

namespace SynchroStats.Features.SmallWorld;

public sealed class SmallWorldCollection<TSmallWorldValue, TCardGroupName>
    where TSmallWorldValue : IEquatable<TSmallWorldValue>, IComparable<TSmallWorldValue>
    where TCardGroupName : notnull, IEquatable<TCardGroupName>, IComparable<TCardGroupName>
{
    private Func<ISmallWorldTraits, TSmallWorldValue> ValueSelector { get; }
    private Dictionary<TSmallWorldValue, DictionaryWithGeneratedKeys<TCardGroupName, ISmallWorldCard<TCardGroupName>>> Cards { get; }

    public SmallWorldCollection(Func<ISmallWorldTraits, TSmallWorldValue> selector, IEnumerable<ISmallWorldCard<TCardGroupName>> cards)
    {
        ValueSelector = selector;
        Cards = new Dictionary<TSmallWorldValue, DictionaryWithGeneratedKeys<TCardGroupName, ISmallWorldCard<TCardGroupName>>>();

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
                collection = new DictionaryWithGeneratedKeys<TCardGroupName, ISmallWorldCard<TCardGroupName>>(static card => card.Name);
                Cards[value] = collection;
            }

            if (!collection.TryAdd(card))
            {
                throw new Exception("Duplicate cards found.");
            }
        }
    }

    public IReadOnlyDictionary<TCardGroupName, ISmallWorldCard<TCardGroupName>> GetCardsBySmallWorldValue(ISmallWorldCard<TCardGroupName> card)
    {
        var traits = card.SmallWorldTraits;

        if (traits is null)
        {
            return ImmutableDictionary<TCardGroupName, ISmallWorldCard<TCardGroupName>>.Empty;
        }

        return GetCardsBySmallWorldValue(ValueSelector(traits));
    }

    public IReadOnlyDictionary<TCardGroupName, ISmallWorldCard<TCardGroupName>> GetCardsBySmallWorldValue(TSmallWorldValue smallWorldValue)
    {
        if (!Cards.TryGetValue(smallWorldValue, out var collection))
        {
            return ImmutableDictionary<TCardGroupName, ISmallWorldCard<TCardGroupName>>.Empty;
        }

        return collection;
    }

    public bool IsCardPresent(ISmallWorldCard<TCardGroupName> card)
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
