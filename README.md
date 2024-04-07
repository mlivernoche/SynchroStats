# SynchroStats

SynchroStats is a framework written in C# for building empirical and statistical models that aim to optimize the opening hand of a Yu-Gi-Oh! deck. This framework allows for an analysis that is more detailed, automated, and correct.

* [SynchroStats](#synchrostats)
  * [Detailed](#detailed)
  * [Automated](#automated)
  * [Correct](#correct)
* [Card Mechanics](#card-mechanics)
  * [Small World](#small-world)
  * [Pot of Prosperity](#pot-of-prosperity)
* [How To Use](#how-to-use)
  * [Installation](#installation)
  * [Getting Started](#getting-started)

## Detailed

Using this framework, it is possible to distinguish cards in a much more useful manner. A common use case is calculating the probability for drawing a starter card.

This type of card is required in almost all modern Yu-Gi-Oh!, and most players look to draw at least one. There are typically two classes of starter cards: 1 card starters and 2 card starters. 1 card starters can, by themselves, start an entire turn of effects and summons by themselves. On the other hand, 2 card starters require a specific card and another card (how specific depends on the starter) to accompany it. Calculating the probability of drawing a 1 card starter is straightfoward, but calculating the probability of drawing a 2 card starter and the other card is more difficult. With SynchroStats, this is possible.

In order to calculate the probability of drawing a 1 card starter, something like this can work ([file](https://github.com/mlivernoche/VaylantzHandAnalysis/blob/3a8ef2fdb2b7b5920d89f31075c4c25a8b0ede4c/VaylantzHandAnalysis/Projects/VaylantzSHSProject.cs#L400)):

```
private static bool HasOneCardCombo(HandCombination<YGOCards.YGOCardName> hand)
{
    return hand.HasAnyOfTheseCards(
    [
        YGOCards.C_SuperheavySamuraiMotorbike,
        YGOCards.C_SuperheavySamuraiProdigyWakaushi,
        YGOCards.C_ShinonometheVaylantzPriestess,
    ]);
}
```

This function simply looks at the hand, and checks if that hand has at least one copy of `YGOCards.C_SuperheavySamuraiMotorbike`, `YGOCards.C_SuperheavySamuraiProdigyWakaushi`, or `YGOCards.C_ShinonometheVaylantzPriestess` (all possible hands are checked).

To find hands with a 2 card starter that lead to full combo, this works ([file](https://github.com/mlivernoche/VaylantzHandAnalysis/blob/3a8ef2fdb2b7b5920d89f31075c4c25a8b0ede4c/VaylantzHandAnalysis/Projects/VaylantzSHSProject.cs#L410)):

```
private static bool HasTwoCardCombo(HandCombination<YGOCards.YGOCardName> hand)
{
    // We want to look at hands with only two card combos.
    if (HasOneCardCombo(hand))
    {
        return false;
    }

    if (
        hand.HasThisCard(YGOCards.C_SuperheavySamuraiSoulpiercer) &&
        hand.HasAnyOfTheseCards(
        [
            YGOCards.C_SaiontheVaylantzArcher,
            YGOCards.C_VaylantzVoltageViscount,
            YGOCards.C_VaylantzWakeningSoloActivation,
        ]))
    {
        return true;
    }

    if (
        hand.HasThisCard(YGOCards.C_VaylantzWakeningSoloActivation) &&
        hand.HasAnyOfTheseCards(
        [
            YGOCards.C_KashtiraFenrir,
            YGOCards.C_SuperheavySamuraiSoulpiercer,
            YGOCards.C_HojotheVaylantzWarrior,
            YGOCards.C_NazukitheVaylantzNinja,
            YGOCards.C_SaiontheVaylantzArcher,
            YGOCards.C_VaylantzVoltageViscount,
            YGOCards.C_SuperheavySamuraiGeneralCoral,
        ]))
    {
        return true;
    }

    return false;
}
```

This function is a little more advanced, but it essentially checks if the hand has at least one specific card and at least one of a number of other cards.

These two examples uses the `IHandAssessment<TCardGroupName>` system (see [src/Features/Assessment](https://github.com/mlivernoche/SynchroStats/tree/b2f770b86bca88754f5d2d1503e7959c3ee9a26c/src/Features/Assessment)), which is currently the most powerful tool for analyzing hands.

It is also possible to look at hands on the opposite end: when the deck produces a hand that does not produce full combo, what does the hand look it? Does it have 1 hand trap and 4 garnets? Does it have 4 hand traps and 1 garnet? Does it have 4 hand traps, but 3 of them are the same card and that card has a hard once per turn and so it really only has 2 hand traps? How often do these hands happen?

## Automated

### Generating Models
Another common use case is determing how many hand traps or board breakers (sometimes called "non-engine") to run. It is very easy to create a function that automatically checks a range of quantities for non-engine for both going first and going second. It would look something like this ([file](https://github.com/mlivernoche/VaylantzHandAnalysis/blob/3a8ef2fdb2b7b5920d89f31075c4c25a8b0ede4c/VaylantzHandAnalysis/Projects/GenericProbabilityProject.cs#L15)):

```
var analyzerBuilderCollection = new List<HandAnalyzerBuildArguments<CardGroup<YGOCards.YGOCardName>, YGOCards.YGOCardName>>();

for (var i = range.Start.Value; i <= range.End.Value; i++)
{
    var engineGroup = new CardGroup<YGOCards.YGOCardName>()
    {
        Name = new("Engine", -1),
        Size = deckSize - i,
        Minimum = 0,
        Maximum = deckSize - i,
    };
    var nonEngineGroup = new CardGroup<YGOCards.YGOCardName>()
    {
        Name = new("Non-Engine", -2),
        Size = i,
        Minimum = 0,
        Maximum = i,
    };

    var cardList = CardList.Create<CardGroup<YGOCards.YGOCardName>, YGOCards.YGOCardName>([engineGroup, nonEngineGroup]);

    // Going first analyzer.
    var analyzerArgs = HandAnalyzerBuildArguments.Create($"E{engineGroup.Size:N0} v N{nonEngineGroup.Size:N0} (5)", 5, cardList);
    analyzerBuilderCollection.Add(analyzerArgs);

    // Going second analyzer.
    analyzerBuilderCollection.Add(analyzerArgs with { AnalyzerName = $"E{engineGroup.Size:N0} v N{nonEngineGroup.Size:N0} (6)", HandSize = 6 });
}
```

This code creates a number of `HandAnalyzerBuildArguments<TCardGroup, TCardGroupName>` objects. These are used to create `HandAnalyzer<TCardGroup, TCardGroupName>` objects, which is the basis for analyzing hands. An instance of `HandAnalyzer<TCardGroup, TCardGroupName>` contains the deck size, the hand size, all possible hands, and more. It can be used to calculate probabilities, but it does not output them to any stream. For doing that, we need to use `HandAnalyzerComparison<TCardGroup, TCardGroupName>` ([file](https://github.com/mlivernoche/VaylantzHandAnalysis/blob/3a8ef2fdb2b7b5920d89f31075c4c25a8b0ede4c/VaylantzHandAnalysis/Projects/GenericProbabilityProject.cs#L44)).

```
// Create hand analyzers.
var analyzerCollection = HandAnalyzer.CreateInParallel(analyzerBuilderCollection);

// Create a HandAnalyzerComparison, which is used for comparing hand analyzers and then outputting them to something.
var analyzerComparer = HandAnalyzerComparison.Create(analyzerCollection);

// Calculate probability of drawing a hand with 0 non-engine.
analyzerComparer.Add("(==0) Non-Engine", formatter, static analyzer => analyzer.CalculateProbability(static hand => hand.CountCopiesOfCardInHand(new("Non-Engine", -2)) == 0));

// Calculate probability of drawing a hand with 1 or 2 non-engine.
analyzerComparer.Add("(==1 or 2) Non-Engine", formatter, static analyzer => analyzer.CalculateProbability(static hand => hand.CountCopiesOfCardInHand(new("Non-Engine", -2)) is 1 or 2));

// Calculate probability of drawing a hand with 3 or more non-engine.
analyzerComparer.Add("(>=3) Non-Engine", formatter, static analyzer => analyzer.CalculateProbability(static hand => hand.CountCopiesOfCardInHand(new("Non-Engine", -2)) >= 3));

// Run the models.
HandAnalyzerComparison.RunInParallel(outputStream, analyzerComparer, handFormatter);
```

Using a deck with 44 cards and a range of 13 to 17, we get this output ("E" is "Engine", "N" is "Non-Engine"):

```
Analyzer: E31 v N13 (5). Cards: 44. Hand Size: 5. Possible Hands: 6.
Analyzer: E31 v N13 (6). Cards: 44. Hand Size: 6. Possible Hands: 7.
Analyzer: E30 v N14 (5). Cards: 44. Hand Size: 5. Possible Hands: 6.
Analyzer: E30 v N14 (6). Cards: 44. Hand Size: 6. Possible Hands: 7.
Analyzer: E29 v N15 (5). Cards: 44. Hand Size: 5. Possible Hands: 6.
Analyzer: E29 v N15 (6). Cards: 44. Hand Size: 6. Possible Hands: 7.
Analyzer: E28 v N16 (5). Cards: 44. Hand Size: 5. Possible Hands: 6.
Analyzer: E28 v N16 (6). Cards: 44. Hand Size: 6. Possible Hands: 7.
Analyzer: E27 v N17 (5). Cards: 44. Hand Size: 5. Possible Hands: 6.
Analyzer: E27 v N17 (6). Cards: 44. Hand Size: 6. Possible Hands: 7.

Category                 E31 v N13 (5)     E31 v N13 (6)     E30 v N14 (5)     E30 v N14 (6)     E29 v N15 (5)     E29 v N15 (6)     E28 v N16 (5)     E28 v N16 (6)     E27 v N17 (5)     E27 v N17 (6)
(==0) Non-Engine         15.65%            10.43%            13.12%            8.41%             10.94%            6.73%             9.05%             5.34%             7.43%             4.19%
(==1 or 2) Non-Engine    69.95%            66.06%            69.35%            63.59%            68.13%            60.56%            66.36%            57.08%            64.10%            53.25%
(>=3) Non-Engine         14.41%            23.51%            17.53%            28.00%            20.93%            32.71%            24.59%            37.58%            28.46%            42.55%
```

### Card Searchers

Another common scenario are cards that search for cards. A typical example is a field spell that searches for a monster. Instead of defining these relationships in the models, we can define these relationships in a data structure and then models can use that data structure. That way, we can update the data structure itself and the models will also be updated.

One way to do this, is to use `CardSearchNode<TCardGroupName>` and `CardSearchNodeCollection<TCardGroupName>`. This is collection of nodes that form a graph. This graph describes which cards look for which cards. Here is a simple example.

* `Terraforming` -> `Pressured Planet Wraitsoth` -> `Kashtira Fenrir`

We can create this graph in the following way.

```
var cardSearchGraphs = new CardSearchNodeCollection<YGOCards.YGOCardName>()
{
    {
        YGOCards.C_Terraforming,
        [
            YGOCards.C_PressuredPlanetWraitsoth,
        ]
    },
    {
        YGOCards.C_PressuredPlanetWraitsoth,
        [
            YGOCards.C_KashtiraFenrir,
        ]
    },
};
```

In the model, these searchers can be accounted for in the following way.

```
foreach (var card in possibleHand.GetCardsInHand(analyzer))
{
    if (context.CardSearchGraphs.HasPathBetweenNodes(card.Name, YGOCards.C_KashtiraFenrir))
    {
        return true;
    }
}
```

By using this method, we add more searchers without having to update each model.

## Correct

A common issue in programming is typos. This can be a problem when dealing with a cardpool as vast as Yu-Gi-Oh!. To help mitigate this, SynchroStats allows more than strings to identify cards.

All models require a type for the name. Each type has this restriction:

```where TName : notnull, IEquatable<TName>, IComparable<TName>```

You can provide your own type, but a built-in type is provided called `YGOCards.YGOCardName`. These names are auto-generated from a JSON file in the project `CardSourceGenerator`. This project utilizes .NET source code generation to take proper JSON objects and write C# that can be used during compilation. [You can read more about it here.](https://learn.microsoft.com/en-us/dotnet/csharp/roslyn-sdk/source-generators-overview) In order words, typos are way less likely because it if the name does not exist, then a compilation error occurs. Further, because this generated code is available before compilation and all names are added as static members to `YGOCards`, Visual Studio can provide IntelliSense (in other words, autocomplete). Card names had to be modified in order to fit C#'s naming convention.

`YGOCardName` also features useful methods and optimizations to increase performance. They also sort alphanumerically in ascending order. As of 4/6/2024, this is the implementation (this implementation is subject to change).

```
public readonly struct YGOCardName : IEquatable<YGOCardName>, IComparable<YGOCardName>
{
	public string Name { get; } = string.Empty;
	public int Id { get; }
	public YGOCardName(string name, int id)
	{
		Name = name;
		Id = id;
	}
	public bool Equals(YGOCardName other) => Id == other.Id;
	public override bool Equals(object obj) => obj is YGOCardName other && Equals(other);
	public override int GetHashCode() => HashCode.Combine(Id);
	public static bool operator ==(YGOCardName x, YGOCardName y) => x.Equals(y);
	public static bool operator !=(YGOCardName x, YGOCardName y) => !(x == y);
	public int CompareTo(YGOCardName other) => Id.CompareTo(other.Id);
	public static bool operator >(YGOCardName x, YGOCardName y) => x.CompareTo(y) > 0;
	public static bool operator <(YGOCardName x, YGOCardName y) => x.CompareTo(y) < 0;
	public static bool operator >=(YGOCardName x, YGOCardName y) => x.CompareTo(y) >= 0;
	public static bool operator <=(YGOCardName x, YGOCardName y) => x.CompareTo(y) <= 0;
}
```

If you are interested in utilizing this feature, you can add a reference to `SynchroStatsWithCardData`. This project uses the `CardSourceGenerator` project and any JSON files put in `/CardData`. If this feature does not interest you, then please reference `SynchroStats` to reduce build times; a custom type can be used instead.

Right now, `CardSourceGenerator` only understands JSON from YGOProDeck, because that is the only comprehensive source I could find.

# Card Mechanics

Another angle analysis for Yu-Gi-Oh! hands are consistency cards. The first one that caught my attention was Small World.

## Small World

Small World is a spell that can, in theory, search any monster in the main deck, but it requires another card in hand. In other words, it is a 2 card starter with specific card requirements. SynchroStats provides tools for analyzing the viability of Small World in a deck.

The primary tools for this analysis is `SmallWorldAnalyzer<TCardGroupName>` and `ISmallWorldCard<TCardGroupName>`. `SmallWorldAnalyzer<TCardGroupName>` provides methods for determing if a connection between a card in and a search target in deck exists. This can be used through methods like `public bool HasBridge(ISmallWorldCard<TCardGroupName> revealedCard, ISmallWorldCard<TCardGroupName> desiredCard)` ([see file](src/Features/SmallWorld/SmallWorldAnalyzer.cs#L142)). [You can read the source code in src/Features/SmallWorld.](src/Features/SmallWorld)

Here is an example of determing whether or not Small World is live in a particular hand ([see file](https://github.com/mlivernoche/VaylantzHandAnalysis/blob/3a8ef2fdb2b7b5920d89f31075c4c25a8b0ede4c/VaylantzHandAnalysis/Vaylantz.cs#L187)).

```
public static bool SmallWorldCanFindShinonome(HandAnalyzer<VaylantzCardGroup, YGOCards.YGOCardName> analyzer, VaylantzCalculationContext context, HandCombination<YGOCards.YGOCardName> possibleHand)
{
    if (!possibleHand.HasThisCard(YGOCards.C_SmallWorld))
    {
        return false;
    }

    var cardsInDeck = analyzer.CardGroups.Values.Minus(possibleHand, static (card, amount) => card with { Size = amount });
    var smallWorldAnalyzer = SmallWorldAnalyzer.Create(cardsInDeck);
```

First, the hand is skipped if Small World is not present in the current hand. Then, a `CardList<TCardGroup, TCardGroupName>` is created that does not include the current hand. Finally, a `SmallWorldAnalyzer<TCardGroupName>` is created.

```
    foreach (var card in possibleHand.GetCardsInHand())
    {
        if (smallWorldAnalyzer.HasBridge(card.HandName, YGOCards.C_ShinonometheVaylantzPriestess))
        {
            return true;
        }
```

This section simply checks if the current hand has a way to get to the search, which is `YGOCards.C_ShinonometheVaylantzPriestess`.

```
        if (!analyzer.CardGroups.TryGetValue(card.HandName, out var group))
        {
            throw new Exception($"Card in hand \"{card.HandName}\" not in card list.");
        }

        foreach (var name in context.CardSearchGraphs.GetCardsAccessibleFromName(card.HandName))
        {
            var deckWithoutCard = cardsInDeck.RemoveCardFromDeck(name, static (card, amount) => card with { Size = amount });
            var newSmallWorldAnalyzer = SmallWorldAnalyzer.Create(deckWithoutCard);

            if (newSmallWorldAnalyzer.HasBridge(name, YGOCards.C_ShinonometheVaylantzPriestess))
            {
                return true;
            }
        }
    }
```

This section is a little more advanced. Cards in Yu-Gi-Oh! can search for other cards, a lot of times without any cost or setup. This section accounts for that.

Cards searching for other cards can be modeled as a graph. The type, `CardSearchNode<TCardGroupName>`, is a single directional graph, meaning card search go one way (see the code in [src/Data/CardSearch](src/Data/CardSearch)). You can define graphs in `CardSearchNodeCollection<TCardGroupName>`, which can find subgraphs and other connections.

```
    return false;
}
```

Finally, if we get to here, then this hand has Small World, but it does not have a way to find anything. In this hand, Small World is a dead card.

## Pot Of Prosperity

There is, of course, another card that I wanted to study. Pot of Prosperity can provide a huge consistency boost, but it is limited in that it needs to excavate from the top of the deck. So, unlike Small World, it has randomness to it.

Pot of Propserity can be modeled in the following way.

```
var (context, desiredCards, optimizedAnalyzer) = projectContext;
var totalProb = 0.0;

// Check for hands that have the cards we need.
foreach (var hand in analyzer.Combinations)
{
    if (hand.HasThisCard(YGOCards.C_ShinonometheVaylantzPriestess))
    {
        totalProb += analyzer.CalculateProbability(hand);
    }
    else if (hand.HasThisCard(YGOCards.C_VaylantzWakeningSoloActivation) && Vaylantz.SoloActivationLive(analyzer, hand))
    {
        totalProb += analyzer.CalculateProbability(hand);
    }
    else if (hand.HasThisCard(YGOCards.C_SmallWorld) && Vaylantz.SmallWorldCanFindShinonome(analyzer, context, hand))
    {
        totalProb += analyzer.CalculateProbability(hand);
    }
}

// Figure out the prosperity component.
// This will be lower than it should be, because
// only the desired cards are in the optimizer. So,
// it is assumed that Solo Activation and Small World
// will work 100% of the time, which is not true.
foreach (var hand in optimizedAnalyzer.Combinations)
{
    if (hand.HasThisCard(YGOCards.C_ShinonometheVaylantzPriestess))
    {
        continue;
    }
    else if (hand.HasThisCard(YGOCards.C_VaylantzWakeningSoloActivation))
    {
        continue;
    }
    else if (hand.HasThisCard(YGOCards.C_SmallWorld))
    {
        continue;
    }
    else if (hand.HasThisCard(YGOCards.C_PotofProsperity))
    {
        var probOfHand = optimizedAnalyzer.CalculateProbability(hand);
        var prospAnalyzer = optimizedAnalyzer.Remove(hand, static (group, amount) => CardGroup.Create(group.Name, amount, group.Minimum, Math.Min(amount, group.Maximum)));
        var probOfProspTargets = 0.0;

        foreach (var prospHand in prospAnalyzer.Combinations)
        {
            if (prospHand.HasAnyOfTheseCards(desiredCards))
            {
                probOfProspTargets += prospAnalyzer.CalculateProbability(prospHand);
            }
        }

        // I believe this is a valid interpretation of these events.
        // While the probability of finding prosperity targets hinges
        // on whether it was drawn, the probability of drawing this hand
        // has nothing to do with whether or not pot prosperity finds
        // something. So, Bayes' thereom is not appropriate here.
        totalProb += probOfHand * probOfProspTargets;
    }
}

return totalProb;
```

It should be noted that this model is not 100% accurate, because of the limitations of computing power or the algorithm used to find all possible hand combinations. Those two factors are actually why the model is split into two sections, which will be explained after going through the code.

```
var (context, desiredCards, optimizedAnalyzer) = projectContext;
var totalProb = 0.0;

// Check for hands that have the cards we need.
foreach (var hand in analyzer.Combinations)
{
    if (hand.HasThisCard(YGOCards.C_ShinonometheVaylantzPriestess))
    {
        totalProb += analyzer.CalculateProbability(hand);
    }
    else if (hand.HasThisCard(YGOCards.C_VaylantzWakeningSoloActivation) && Vaylantz.SoloActivationLive(analyzer, hand))
    {
        totalProb += analyzer.CalculateProbability(hand);
    }
    else if (hand.HasThisCard(YGOCards.C_SmallWorld) && Vaylantz.SmallWorldCanFindShinonome(analyzer, context, hand))
    {
        totalProb += analyzer.CalculateProbability(hand);
    }
}
```

This section checks the actual analyzer, and the starters. This section is simple and mostly self-explanatory.

```
foreach (var hand in optimizedAnalyzer.Combinations)
{
    if (hand.HasThisCard(YGOCards.C_ShinonometheVaylantzPriestess))
    {
        continue;
    }
    else if (hand.HasThisCard(YGOCards.C_VaylantzWakeningSoloActivation))
    {
        continue;
    }
    else if (hand.HasThisCard(YGOCards.C_SmallWorld))
    {
        continue;
    }
    else if (hand.HasThisCard(YGOCards.C_PotofProsperity))
    {
        var probOfHand = optimizedAnalyzer.CalculateProbability(hand);
        var prospAnalyzer = optimizedAnalyzer.Remove(hand, static (group, amount) => CardGroup.Create(group.Name, amount, group.Minimum, Math.Min(amount, group.Maximum)));
        var probOfProspTargets = 0.0;

        foreach (var prospHand in prospAnalyzer.Combinations)
        {
            if (prospHand.HasAnyOfTheseCards(desiredCards))
            {
                probOfProspTargets += prospAnalyzer.CalculateProbability(prospHand);
            }
        }

        // I believe this is a valid interpretation of these events.
        // While the probability of finding prosperity targets hinges
        // on whether it was drawn, the probability of drawing this hand
        // has nothing to do with whether or not pot prosperity finds
        // something. So, Bayes' thereom is not appropriate here.
        totalProb += probOfHand * probOfProspTargets;
    }
}
```

This section is must more complex, and requires the creation of a `HandAnalyzer<TCardGroup, TCardGroupName>` that is optimized for this model.

The algorithm for Pot of Prosperity works in the following way (side note, it is important to skip the cards we accounted for in the first section, we are analyzing only Pot of Prosperity).

1. Create a `CardList<TCardGroup, TCardGroupName>` that does not have the current hand.
2. Create a `HandAnalyzer<TCardGroup, TCardGroupName>` from the card list (`prospAnalyzer`). This will find all possible hands.
3. Inspect each excavation (either 3 or 6) for any of the desired cards, and calculate the probability of that excavation happening (this is basically just creating a `HandAnalyzer<TCardGroup, TCardGroupName>` with the hand size set to 3 or 6).
4. Sum all of the probabilities (`probOfProspTargets`).
5. Multiple the probability of drawing the current hand (`probOfHand`) and the probabities from step 4.

Due to Step 2, without using an optimized `HandAnalyzer<TCardGroup, TCardGroupName>`, a gargantuan amount of data has to be created. The limiting factor is having to find all possible hand combinations, which is a very high number if each card name is provided. Even if the algorithm used to find all hand combinations were able to utilize multithreading, the limiting factor would still be how much data has to be created. For example, as of 4/6/2024, using [the card list provided here](https://github.com/mlivernoche/VaylantzHandAnalysis/blob/3a8ef2fdb2b7b5920d89f31075c4c25a8b0ede4c/VaylantzHandAnalysis/Projects/VaylantzSHSProject.cs#L17), with a 5 card hand, there are 19,254 possible hands. Even though the above algorithm removes 5 or so cards from the deck, the amount of possible hands to find is in the millions (this is also after consolidating everything, because a 40 card deck actually has 40! possible orders, which is a lot, but `HandAnalyzer<TCardGroup, TCardGroupName>` churns out a lot of the fat; e.g., 12234 and 12342 are considered the same hand). Providing a `HandAnalyzer<TCardGroup, TCardGroupName>` optimized for this is of the utmost importance. This can be done with the following method,

```
var cardListWithEveryNameInTheDeck = CardList.Create(...);
var prosperityTargets = [...];
var cardListOptimizedForPotOfProsperity = cardListWithEveryNameInTheDeck.CreateSimplifiedCardList(YGOCards.C_PotofProsperity, new("misc", -1), prosperityTargets);
```

# How To Use

## Installation

The best way to use this library, is to clone the repository to your computer, then create a new solution and add either `SynchroStats` or `SynchroStatsWithCardData` as a project and then reference that. Add either `SynchroStats` if you do not need or want the autogenerated `YGOCards.YGOCardName` names.

## Getting Started

### IProject

`IProject` is not necessary to create models, but it does make organizing multiple models easier. The way it works, simply create an `IEnumerable<IProject>` object composed of your `IProject` object. Then, create a `IProjectHandler` object, which has a simple implementation called `ProjectHandler`. You will also need a `IHandAnalyzerOutputStream`, which also has a simple implementation called `HandAnalyzerConsoleOutputStream` which will print the results to the console. Then call `ProjectHandler.RunProjects(IEnumerable<IProject> projectsToRun, IHandAnalyzerOutputStream outputStream)`. Here's an example.

```
var list = new List<IProject>
{
  new MyProject(),
  new MyProject2(),
};
var projectHandler = new ProjectHandler();
var consoleOutputStream = new HandAnalyzerConsoleOutputStream();
projectHandler.RunProjects(list, consoleOutputStream);
```

You can, of course, create your own `IProjectHandler`.

### ICardGroup<TCardGroupName>

This is the building block for creating models. It has four basic pieces of data:
  * `TCardGroupName Name` - this is the name of the card. It has the following restriction: `where TName : notnull, IEquatable<TName>, IComparable<TName>`. It is most often used in `HashSet<T>` and `Dictionary<TKey, TValue>` to ensure uniqueness.
  * `int Size` - this is how many copies of this card are in the deck (but not the hand). There are no restrictions on what value this can take, even though the Yu-Gi-Oh! card game has such restrictions. This is because in models, it can be useful to ignore specific card names and lump them all together; for example, instead of having all non-engine being specified, you can instead lump them all together into one `ICardGroup<string>` with name `Name` set to `"non-engine"`.
  * `int Minimum` - this is the minimum amount of copies of this card in the hand, which basically means how many copies of the card are in the card.
  * `int Maximum` - this is the amount amount of copies that can appear in the hand, typically this one is not super important.

It is possible to add other data, because SynchroStats is designed with the idea that a custom type is provided. This is why many of the types (such as `CardList<TCardGroup, TCardGroupName>` and `HandAnalyzer<TCardGroup, TCardGroupName>`) require you to specify which type you want to use. One example is providing `Enum` flags, such the following below ([see file](https://github.com/mlivernoche/VaylantzHandAnalysis/blob/3a8ef2fdb2b7b5920d89f31075c4c25a8b0ede4c/VaylantzHandAnalysis/CardTraits.cs)):

```
[Flags]
public enum CardTraits
{
    None = 0,
    Defensive = 1,
    BeyondThePendMat = 2,
    NormalSummon = 4,
    FieldSpell = 8,

    LowLevelFireVaylantz = 16,
    HighLevelFireVaylantz = 32,

    LowLevelWaterVaylantz = 64,
    HighLevelWaterVaylantz = 128,

    HighScale = 256,
    
    ProsperityTarget = 512,

    MultipleOK = 1048,
}
```

### CardList
`CardList<TCardGroup, TCardGroupName>` is an immutable collection of `TCardGroup` with the restriction `where TCardGroup : ICardGroup<TCardGroupName>` (this restriction is very common). `CardList` is immutable, in that modifying it in any way returns a new version of it. This is useful, because they can be modified without changing the original.

You can create a `CardList<TCardGroup, TCardGroupName>` by using

```
var cardList = CardList.Create(...);
```

### HandAnalyzer

This is where most of the models are created. `HandAnalyzer<TCardGroup, TCardGroupName>` is object that is composed of a `CardList<TCardGroup, TCardGroupName>`, an `int` deck size, an `int` hand size, and a collection of `HandCombination<TCardGroupName>` of all possible hands. You can then use the following methods to analyze all of the hands:

* `double CalculateProbability()`
* `double CalculateProbability(Func<HandCombination<TCardGroupName>, bool> filter)`
* `double CalculateProbability(IFilter<HandCombination<TCardGroupName>> filter)`
* `HandAssessmentAnalyzer<TCardGroup, TCardGroupName, TAssessment> AssessHands<TAssessment>(Func<HandCombination<TCardGroupName>, TAssessment> filter)`
* `HandAssessmentAnalyzer<TCardGroup, TCardGroupName, TAssessment> AssessHands<TAssessment>(Func<HandCombination<TCardGroupName>, HandAnalyzer<TCardGroup, TCardGroupName>, TAssessment> filter)`
* `int CountHands(Func<HandCombination<TCardGroupName>, bool> filter)`
* `int[] CountUniqueCardName()`

In addition, there are numerous extension methods for different situations.

You can create a `HandAnalyzer<TCardGroup, TCardGroupName>` by using

```
var analyzer = HandAnalyzer.Create(...);
```

#### HandCombination

`HandCombination<TCardGroupName>` is a `readonly struct` that contains one of the possible hands a deck can produce. Simply, it is a collection of `N` cards, where `N` is the `int HandSize` in `HandAnalyzer<TCardGroup, TCardGroupName>`. `HandCombination<TCardGroupName>` has many extension methods to help narrow things down available in [src/Data/Operations](src/Data/Operations). Some useful ones are:

* `public static bool HasThisCard<U>(this HandCombination<U> cards, U cardName) where U : notnull, IEquatable<U>, IComparable<U>`
* `public static bool HasAnyOfTheseCards<U>(this HandCombination<U> cards, IEnumerable<U> cardNames) where U : notnull, IEquatable<U>, IComparable<U>`
* `public static bool HasAllOfTheseCards<U>(this HandCombination<U> cards, IEnumerable<U> cardNames) where U : notnull, IEquatable<U>, IComparable<U>`

#### Calculating Probability

A simple way to create models is to filter hands according to a criteria and then calculate the probability of opening hands that meet that criteria. This can be done using the `double CalculateProbability()` type of methods. Here is a simple example.

```
// should be 33.76% in a 40 card deck playing 3 copies and an opening hand of 5
var probOfDrawingAshBlossom = analyzer.CalculateProbability(static hand => hand.HasThisCard(YGOCards.C_AshBlossomJoyousSpring));
```

Here is a more complicated example.

```
public static bool HasFenrir(HandAnalyzer<VaylantzCardGroup, YGOCards.YGOCardName> analyzer, VaylantzCalculationContext context, HandCombination<YGOCards.YGOCardName> possibleHand)
{
    if (possibleHand.HasThisCard(YGOCards.C_KashtiraFenrir))
    {
        return true;
    }

    foreach (var card in possibleHand.GetCardsInHand(analyzer))
    {
        if (context.CardSearchGraphs.HasPathBetweenNodes(card.Name, YGOCards.C_KashtiraFenrir))
        {
            return true;
        }
    }

    return false;
}
```

The first section simply checks if the hand has Kashtira Fenrir. The second section (the `foreach` loop) utilizes the `CardSearchNode<TCardGroupName>` ([see file](src/Data/CardSearch/CardSearchNode.cs)) and `CardSearchNodeCollection<TCardGroupName>` ([see file](src/Data/CardSearch/CardSearchNodeCollection.cs)) tools to also look at cards that can search for Kashtira Fenrir. For example:

* `Terraforming` -> `Pressured Planet Wraitsoth` -> `Kashtira Fenrir`
* `Pressured Planet Wraitsoth` -> `Kashtira Fenrir`
* `Planet Pathfinder` -> `Pressured Planet Wraitsoth` -> `Kashtira Fenrir`

This type of data is not built into SynchroStats, it needs to be provided.

#### Assessing Hands

Using `IHandAssessment<TCardGroupName` ([see file](src/Features/Assessment/IHandAssessment.cs)) and `HandAssessmentAnalyzer<TCardGroup, TCardGroupName, TAssessment>` ([see file](src/Features/Assessment/HandAssessmentAnalyzer.cs)) is similar to the previous approach, but it can be used to find more granular details about hands. The basic operation goes like this.

```
var handAssessment = handAnalyzer.AssessHands(static (hand, analyzer) => ...);
var probabilityOfHand = handAssessment.CalculateProbability(static assessment => ...);
```

The first step is create an object that implements `IHandAssessment<TCardGroupName>`. This object will be created for each possible hand, and it can have whatever data you want. Using the Kashtira Fenrir example above, we can have the following implementation:

```
public sealed class KashtiraFenrirHandAssessment : IHandAssessment<TCardGroupName>
{
  public bool Included { get; }
  public HandCombination<YGOCards.YGOCardName> Hand { get; }
  public bool LosesToDroll { get; init; }

  public KashtiraFenrirHandAssessment(bool included, HandCombination<YGOCards.YGOCardName> hand)
  {
      Included = included;
      Hand = hand;
  }
}
```

If the hand already contains Kashtira Fenrir, we can assess that the hand is not vulnerable to Droll & Lock Bird. If we have to start with Terraforming or Pressured Planet Wraitsoth, then we are vulnerable to an immediate Droll & Lock Bird. We can then calculate the amount of hands that lose to Droll.

```
// we got to Kashtira Fenrir.
var probabilityOfGettingKashtirFenrir = handAssessment.Probability;

// we got to Kashtira Fenrir, but we played into Droll (e.g. we started with Wraitsoth).
var probabilityOfHandLosingToDroll = handAssessment.CalculateProbability(static assessment => assessment.LosesToDroll);
```
