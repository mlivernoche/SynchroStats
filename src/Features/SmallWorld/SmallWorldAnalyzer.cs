using System.Collections.Immutable;

namespace SynchroStats.Features.SmallWorld;

public static class SmallWorldAnalyzer
{
    public static SmallWorldAnalyzer<TCardGroupName> Create<TCardGroupName>(IEnumerable<ISmallWorldCard<TCardGroupName>> cards)
        where TCardGroupName : notnull, IEquatable<TCardGroupName>, IComparable<TCardGroupName>
    {
        return new SmallWorldAnalyzer<TCardGroupName>(cards);
    }

    public static bool IsBridgeWith<TCardGroupName>(this ISmallWorldCard<TCardGroupName> first, ISmallWorldCard<TCardGroupName> second)
        where TCardGroupName : notnull, IEquatable<TCardGroupName>, IComparable<TCardGroupName>
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

public sealed class SmallWorldAnalyzer<TCardGroupName>
    where TCardGroupName : notnull, IEquatable<TCardGroupName>, IComparable<TCardGroupName>
{
    private DictionaryWithGeneratedKeys<TCardGroupName, ISmallWorldCard<TCardGroupName>> Cards { get; }
    public SmallWorldCollection<int, TCardGroupName> ByLevel { get; }
    public SmallWorldCollection<int, TCardGroupName> ByAttackPoints { get; }
    public SmallWorldCollection<int, TCardGroupName> ByDefensePoints { get; }
    public SmallWorldCollection<string, TCardGroupName> ByMonsterType { get; }
    public SmallWorldCollection<string, TCardGroupName> ByMonsterAttribute { get; }

    public SmallWorldAnalyzer(IEnumerable<ISmallWorldCard<TCardGroupName>> cards)
    {
        Cards = new DictionaryWithGeneratedKeys<TCardGroupName, ISmallWorldCard<TCardGroupName>>(static card => card.Name, cards);

        ByLevel = new SmallWorldCollection<int, TCardGroupName>(static card => card.Level, cards);
        ByAttackPoints = new SmallWorldCollection<int, TCardGroupName>(static card => card.AttackPoints, cards);
        ByDefensePoints = new SmallWorldCollection<int, TCardGroupName>(static card => card.DefensePoints, cards);
        ByMonsterType = new SmallWorldCollection<string, TCardGroupName>(static card => card.MonsterType, cards);
        ByMonsterAttribute = new SmallWorldCollection<string, TCardGroupName>(static card => card.MonsterAttribute, cards);
    }

    public IReadOnlyDictionary<TCardGroupName, ISmallWorldCard<TCardGroupName>> FindBridges(TCardGroupName revealedCardName, TCardGroupName desiredCardName)
    {
        if(Cards.TryGetValue(revealedCardName) is not (true, ISmallWorldCard<TCardGroupName> revealedCard))
        {
            return ImmutableDictionary<TCardGroupName, ISmallWorldCard<TCardGroupName>>.Empty;
        }

        if (Cards.TryGetValue(desiredCardName) is not (true, ISmallWorldCard<TCardGroupName> desiredCard))
        {
            return ImmutableDictionary<TCardGroupName, ISmallWorldCard<TCardGroupName>>.Empty;
        }

        return FindBridges(revealedCard, desiredCard);
    }

    public IReadOnlyDictionary<TCardGroupName, ISmallWorldCard<TCardGroupName>> FindBridges(ISmallWorldCard<TCardGroupName> revealedCard, ISmallWorldCard<TCardGroupName> desiredCard)
    {
        var dict = new DictionaryWithGeneratedKeys<TCardGroupName, ISmallWorldCard<TCardGroupName>>(static card => card.Name);

        void SearchForBridges<TSmallWorldValue>(SmallWorldCollection<TSmallWorldValue, TCardGroupName> collection)
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

    public bool HasBridge(TCardGroupName revealedCardName, TCardGroupName desiredCardName)
    {
        if (Cards.TryGetValue(revealedCardName) is not (true, ISmallWorldCard<TCardGroupName> revealedCard))
        {
            return false;
        }

        if (Cards.TryGetValue(desiredCardName) is not (true, ISmallWorldCard<TCardGroupName> desiredCard))
        {
            return false;
        }

        return HasBridge(revealedCard, desiredCard);
    }

    public bool HasBridge(ISmallWorldCard<TCardGroupName> revealedCard, ISmallWorldCard<TCardGroupName> desiredCard)
    {
        bool SearchForBridge<TSmallWorldValue>(SmallWorldCollection<TSmallWorldValue, TCardGroupName> collection)
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
