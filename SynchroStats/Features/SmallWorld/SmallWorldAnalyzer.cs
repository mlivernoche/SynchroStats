using System.Collections.Immutable;

namespace SynchroStats.Features.SmallWorld;

public static class SmallWorldAnalyzer
{
    public static SmallWorldAnalyzer<TName> Create<TName>(IEnumerable<ISmallWorldCard<TName>> cards)
        where TName : notnull, IEquatable<TName>, IComparable<TName>
    {
        return new SmallWorldAnalyzer<TName>(cards);
    }

    public static bool IsBridgeWith<TName>(this ISmallWorldCard<TName> first, ISmallWorldCard<TName> second)
        where TName : notnull, IEquatable<TName>, IComparable<TName>
    {
        if(first.SmallWorldTraits is null)
        {
            return false;
        }

        if(second.SmallWorldTraits is null)
        {
            return false;
        }

        return IsBridgeWith(first.SmallWorldTraits, second.SmallWorldTraits);
    }

    public static bool IsBridgeWith(this ISmallWorldTraits first, ISmallWorldTraits second)
    {
        var matches = 0;

        if (first.Level == second.Level)
        {
            matches++;
        }

        if (first.AttackPoints == second.AttackPoints)
        {
            matches++;
            if (matches > 1) return false;
        }

        if (first.DefensePoints == second.DefensePoints)
        {
            matches++;
            if (matches > 1) return false;
        }

        if (first.MonsterType.Equals(second.MonsterType, StringComparison.OrdinalIgnoreCase))
        {
            matches++;
            if (matches > 1) return false;
        }

        if (first.MonsterAttribute.Equals(second.MonsterAttribute, StringComparison.OrdinalIgnoreCase))
        {
            matches++;
            if (matches > 1) return false;
        }

        return matches == 1;
    }
}

public sealed class SmallWorldAnalyzer<TName>
    where TName : notnull, IEquatable<TName>, IComparable<TName>
{
    private DictionaryWithGeneratedKeys<TName, ISmallWorldCard<TName>> Cards { get; }
    public SmallWorldCollection<int, TName> ByLevel { get; }
    public SmallWorldCollection<int, TName> ByAttackPoints { get; }
    public SmallWorldCollection<int, TName> ByDefensePoints { get; }
    public SmallWorldCollection<string, TName> ByMonsterType { get; }
    public SmallWorldCollection<string, TName> ByMonsterAttribute { get; }

    public SmallWorldAnalyzer(IEnumerable<ISmallWorldCard<TName>> cards)
    {
        Cards = new DictionaryWithGeneratedKeys<TName, ISmallWorldCard<TName>>(static card => card.Name, cards);

        ByLevel = new SmallWorldCollection<int, TName>(static card => card.Level, cards);
        ByAttackPoints = new SmallWorldCollection<int, TName>(static card => card.AttackPoints, cards);
        ByDefensePoints = new SmallWorldCollection<int, TName>(static card => card.DefensePoints, cards);
        ByMonsterType = new SmallWorldCollection<string, TName>(static card => card.MonsterType, cards);
        ByMonsterAttribute = new SmallWorldCollection<string, TName>(static card => card.MonsterAttribute, cards);
    }

    public IReadOnlyDictionary<TName, ISmallWorldCard<TName>> FindBridges(TName revealedCardName, TName desiredCardName)
    {
        if(Cards.TryGetValue(revealedCardName) is not (true, ISmallWorldCard<TName> revealedCard))
        {
            return ImmutableDictionary<TName, ISmallWorldCard<TName>>.Empty;
        }

        if (Cards.TryGetValue(desiredCardName) is not (true, ISmallWorldCard<TName> desiredCard))
        {
            return ImmutableDictionary<TName, ISmallWorldCard<TName>>.Empty;
        }

        return FindBridges(revealedCard, desiredCard);
    }

    public IReadOnlyDictionary<TName, ISmallWorldCard<TName>> FindBridges(ISmallWorldCard<TName> revealedCard, ISmallWorldCard<TName> desiredCard)
    {
        var dict = new DictionaryWithGeneratedKeys<TName, ISmallWorldCard<TName>>(static card => card.Name);

        void SearchForBridges<TSmallWorldValue>(SmallWorldCollection<TSmallWorldValue, TName> collection)
            where TSmallWorldValue : IEquatable<TSmallWorldValue>, IComparable<TSmallWorldValue>
        {
            foreach(var (name, possibleBridge) in collection.GetCardsBySmallWorldValue(revealedCard))
            {
                if(revealedCard.IsBridgeWith(possibleBridge) && possibleBridge.IsBridgeWith(desiredCard))
                {
                    dict.TryAdd(possibleBridge);
                }
            }
        }

        SearchForBridges(ByLevel);
        SearchForBridges(ByAttackPoints);
        SearchForBridges(ByDefensePoints);
        SearchForBridges(ByMonsterType);
        SearchForBridges(ByMonsterAttribute);

        return dict;
    }

    public bool HasBridge(TName revealedCardName, TName desiredCardName)
    {
        if (Cards.TryGetValue(revealedCardName) is not (true, ISmallWorldCard<TName> revealedCard))
        {
            return false;
        }

        if (Cards.TryGetValue(desiredCardName) is not (true, ISmallWorldCard<TName> desiredCard))
        {
            return false;
        }

        return HasBridge(revealedCard, desiredCard);
    }

    public bool HasBridge(ISmallWorldCard<TName> revealedCard, ISmallWorldCard<TName> desiredCard)
    {
        bool SearchForBridge<TSmallWorldValue>(SmallWorldCollection<TSmallWorldValue, TName> collection)
            where TSmallWorldValue : IEquatable<TSmallWorldValue>, IComparable<TSmallWorldValue>
        {
            foreach (var (name, possibleBridge) in collection.GetCardsBySmallWorldValue(revealedCard))
            {
                if (revealedCard.IsBridgeWith(possibleBridge) && possibleBridge.IsBridgeWith(desiredCard))
                {
                    return true;
                }
            }

            return false;
        }

        if(SearchForBridge(ByLevel))
        {
            return true;
        }

        if (SearchForBridge(ByAttackPoints))
        {
            return true;
        }

        if (SearchForBridge(ByDefensePoints))
        {
            return true;
        }

        if (SearchForBridge(ByMonsterType))
        {
            return true;
        }

        if (SearchForBridge(ByMonsterAttribute))
        {
            return true;
        }

        return false;
    }
}
