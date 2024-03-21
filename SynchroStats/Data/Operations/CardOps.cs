using CommunityToolkit.Diagnostics;
using SynchroStats.Features.Analysis;

namespace SynchroStats.Data.Operations;

public static partial class CardOps
{
    public static CardList<CardGroup<TCardGroupName>, TCardGroupName> CreateSimplifiedCardList<TCardGroup, TCardGroupName>(this CardList<TCardGroup, TCardGroupName> cardList, TCardGroupName potOfProsperityName, TCardGroupName miscName, IEnumerable<TCardGroupName> desiredCards)
        where TCardGroup : ICardGroup<TCardGroupName>
        where TCardGroupName : notnull, IEquatable<TCardGroupName>, IComparable<TCardGroupName>
    {
        var dict = new DictionaryWithGeneratedKeys<TCardGroupName, TCardGroup>(static group => group.Name, cardList);
        return CreateSimplifiedCardList(dict, potOfProsperityName, miscName, desiredCards);
    }

    public static CardList<CardGroup<TCardGroupName>, TCardGroupName> CreateSimplifiedCardList<TCardGroup, TCardGroupName>(this HandAnalyzer<TCardGroup, TCardGroupName> cardList, TCardGroupName potOfProsperityName, TCardGroupName miscName, IEnumerable<TCardGroupName> desiredCards)
        where TCardGroup : ICardGroup<TCardGroupName>
        where TCardGroupName : notnull, IEquatable<TCardGroupName>, IComparable<TCardGroupName>
    {
        var dict = new DictionaryWithGeneratedKeys<TCardGroupName, TCardGroup>(static group => group.Name, cardList.CardGroups.Values);
        return CreateSimplifiedCardList(dict, potOfProsperityName, miscName, desiredCards);
    }

    private static CardList<CardGroup<TCardGroupName>, TCardGroupName> CreateSimplifiedCardList<TCardGroup, TCardGroupName>(DictionaryWithGeneratedKeys<TCardGroupName, TCardGroup> orgCardList, TCardGroupName potOfProsperityName, TCardGroupName miscName, IEnumerable<TCardGroupName> desiredCards)
        where TCardGroup : ICardGroup<TCardGroupName>
        where TCardGroupName : notnull, IEquatable<TCardGroupName>, IComparable<TCardGroupName>
    {
        var prospCards = new List<CardGroup<TCardGroupName>>();
        var deckSize = orgCardList.Values.GetNumberOfCards<TCardGroup, TCardGroupName>();

        if (orgCardList.TryGetValue(potOfProsperityName, out var prospCardGroup))
        {
            prospCards.Add(new CardGroup<TCardGroupName>()
            {
                Name = potOfProsperityName,
                Size = prospCardGroup.Size,
                Minimum = prospCardGroup.Minimum,
                Maximum = prospCardGroup.Maximum,
            });
        }
        else
        {
            throw new Exception($"{potOfProsperityName} not present in deck.");
        }

        foreach (var desiredCard in desiredCards)
        {
            if (orgCardList.TryGetValue(desiredCard, out var desiredCardGroup))
            {
                prospCards.Add(new CardGroup<TCardGroupName>()
                {
                    Name = desiredCard,
                    Size = desiredCardGroup.Size,
                    Minimum = desiredCardGroup.Minimum,
                    Maximum = desiredCardGroup.Maximum,
                });
            }
        }

        var cardSize = prospCards.Sum(static group => group.Size);
        Guard.IsLessThanOrEqualTo(cardSize, deckSize);
        var miscSize = deckSize - cardSize;
        prospCards.Add(new CardGroup<TCardGroupName>()
        {
            Name = miscName,
            Size = miscSize,
            Minimum = 0,
            Maximum = miscSize,
        });

        return CardList.Create<CardGroup<TCardGroupName>, TCardGroupName>(prospCards);
    }

    public static double PotOfProsperityHitProbability<TCardGroup, TCardGroupName>(this HandAnalyzer<TCardGroup, TCardGroupName> analyzer, TCardGroupName potOfProsperityName, int depth, IEnumerable<TCardGroupName> desiredCards, Func<TCardGroup, int, TCardGroup> selector)
        where TCardGroup : ICardGroup<TCardGroupName>
        where TCardGroupName : struct, IEquatable<TCardGroupName>, IComparable<TCardGroupName>
    {
        var prospFindsSomethingGood = 0.0;

        foreach (var hand in analyzer.Combinations)
        {
            if (!hand.HasThisCard(potOfProsperityName))
            {
                continue;
            }

            var prob = analyzer.CalculateProbability(hand);
            var prospAnalyzer = analyzer.Remove(hand, selector);
            prob *= prospAnalyzer.CalculateProbability(desiredCards, static (analyzer, cardNames, hand) => hand.HasAnyOfTheseCards(cardNames));

            prospFindsSomethingGood += prob;
        }

        var hasProsp = analyzer.CalculateProbability(potOfProsperityName, static (cardName, hand) => hand.HasThisCard(cardName));
        return prospFindsSomethingGood / hasProsp;
    }
}
