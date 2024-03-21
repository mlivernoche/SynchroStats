using SynchroStats.Features.Analysis;
using SynchroStats.Features.Combinations;

namespace SynchroStats.Data.Operations
{
    public static class Transform
    {
        public static IEnumerable<(TCardGroup, HandElement<TCardGroupName>)> GetCardGroups<TCardGroup, TCardGroupName>(this HandAnalyzer<TCardGroup, TCardGroupName> analyzer, HandCombination<TCardGroupName> hand)
            where TCardGroup : ICardGroup<TCardGroupName>
            where TCardGroupName : notnull, IEquatable<TCardGroupName>, IComparable<TCardGroupName>
        {
            foreach (var card in hand.GetCardsInHand())
            {
                if (analyzer.CardGroups.TryGetValue(card.HandName, out var cardGroup))
                {
                    yield return (cardGroup, card);
                }
            }
        }
    }
}
