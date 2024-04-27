using System.Collections;

namespace SynchroStats.Data.CardSearch;

public static class CardSearchNodeCollection
{
    public static CardSearchNodeCollection<TCardGroupName> Add<TCardGroupName>(this CardSearchNodeCollection<TCardGroupName> collection, IEnumerable<TCardGroupName> names)
        where TCardGroupName : notnull, IEquatable<TCardGroupName>, IComparable<TCardGroupName>
    {
        var graph = CardSearchNode.CreateSearchGraph(names);
        collection.Add(graph);
        return collection;
    }

    public static CardSearchNodeCollection<TCardGroupName> Add<TCardGroupName>(this CardSearchNodeCollection<TCardGroupName> collection, TCardGroupName start, IEnumerable<TCardGroupName> directSearches)
        where TCardGroupName : notnull, IEquatable<TCardGroupName>, IComparable<TCardGroupName>
    {
        foreach (var direct in directSearches)
        {
            var graph = CardSearchNode.CreateSearchGraph(new[] { start, direct });
            collection.Add(graph);
        }

        return collection;
    }

    public static IEnumerable<CardSearchNode<TCardGroupName>> GetGraphsByName<TCardGroupName>(this CardSearchNodeCollection<TCardGroupName> collection, TCardGroupName name)
        where TCardGroupName : notnull, IEquatable<TCardGroupName>, IComparable<TCardGroupName>
    {
        foreach(var graph in collection)
        {
            if(!graph.Name.Equals(name))
            {
                continue;
            }

            yield return graph;
        }
    }

    public static IEnumerable<CardSearchNode<TCardGroupName>> GetGraphsByEnd<TCardGroupName>(this CardSearchNodeCollection<TCardGroupName> collection, TCardGroupName name)
        where TCardGroupName : notnull, IEquatable<TCardGroupName>, IComparable<TCardGroupName>
    {
        foreach (var graph in collection)
        {
            var end = graph.GetLastNode();

            if(!end.Name.Equals(name))
            {
                continue;
            }

            yield return graph;
        }
    }
}

/// <summary>
/// This class helps describe "card equivalencies." It basically provides a
/// means to say, "Card X searches for or gets card Y." This is done using a single directional
/// graph, where each node has the name of the card and the card it can search.
/// e.g., Terraforming -> Pressured Planet Wraitsoth -> Kashtira Fenrir -> Kashtira Riseheart.
/// Card X doesn't have to explicitly add, of course. For example,
/// Diviner of the Herald -> Trias Hierarchia -> DoSolfachord Cutia -> ReSolfachord Dreamia
/// is also fine.
/// </summary>
/// <typeparam name="TCardGroupName"></typeparam>
public sealed class CardSearchNodeCollection<TCardGroupName> : IEnumerable<CardSearchNode<TCardGroupName>>
    where TCardGroupName : notnull, IEquatable<TCardGroupName>, IComparable<TCardGroupName>
{
    private sealed class NodeComparer : IEqualityComparer<CardSearchNode<TCardGroupName>>
    {
        public static IEqualityComparer<CardSearchNode<TCardGroupName>> Instance { get; } = new NodeComparer();

        private static int GetNodeHashCode(CardSearchNode<TCardGroupName>? node)
        {
            var code = 0;

            while (node != null)
            {
                code = HashCode.Combine(code, node.Name.GetHashCode());
                node = node.Next;
            }

            return code;
        }

        bool IEqualityComparer<CardSearchNode<TCardGroupName>>.Equals(CardSearchNode<TCardGroupName>? x, CardSearchNode<TCardGroupName>? y)
        {
            return GetNodeHashCode(x) == GetNodeHashCode(y);
        }

        int IEqualityComparer<CardSearchNode<TCardGroupName>>.GetHashCode(CardSearchNode<TCardGroupName> obj) => GetNodeHashCode(obj);
    }

    private HashSet<CardSearchNode<TCardGroupName>> Nodes { get; }

    public CardSearchNodeCollection()
    {
        Nodes = new HashSet<CardSearchNode<TCardGroupName>>(NodeComparer.Instance);
    }

    public CardSearchNodeCollection<TCardGroupName> Add(CardSearchNode<TCardGroupName>? cardSearchNode)
    {
        var node = cardSearchNode;

        //add the graph and the subgraphs.
        // e.g., a -> b -> c -> d but also
        // b -> c -> d
        // c -> d
        // are added
        while (node != null)
        {
            var next = node.Next;
            if (next != null)
            {
                Nodes.Add(node);
            }
            node = next;
        }

        return this;
    }

    public IEnumerable<CardSearchNode<TCardGroupName>> FindNodes(TCardGroupName cardName)
    {
        foreach (var graph in Nodes)
        {
            var node = graph;
            while (node != null)
            {
                if (node.Name.Equals(cardName))
                {
                    yield return node;
                }
            }
        }
    }

    public bool HasPathBetweenNodes(TCardGroupName start, TCardGroupName end)
    {
        return GetPathBetweenNodes(start, end);
    }

    public IReadOnlySet<TCardGroupName> GetCardsAccessibleFromName(TCardGroupName start)
    {
        var maxDepth = MaxDepth();
        var destination = new Queue<TCardGroupName>();
        var found = new HashSet<TCardGroupName>();
        destination.Enqueue(start);

        for (var depth = 0; depth < maxDepth; depth++)
        {
            // This is gonna be name for depth + 1.
            var newDestinations = new Queue<TCardGroupName>();

            while (destination.TryDequeue(out var head))
            {
                // we don't need to iterate through graphs,
                // because subgraphs are automatically added
                // when graphs are added.
                // e.g., a -> b -> c -> d but also
                // b -> c -> d
                // c -> d
                // are added

                // could use FindNodes(head) if needed
                foreach (var graph in Nodes)
                {
                    if (!graph.Name.Equals(head))
                    {
                        continue;
                    }

                    var next = graph.Next;

                    if (next != null)
                    {
                        found.Add(next.Name);
                        newDestinations.Enqueue(next.Name);
                    }
                }
            }

            // Start looking at destinations for the next depth.
            destination = newDestinations;
        }

        return found;
    }

    private bool GetPathBetweenNodes(TCardGroupName start, TCardGroupName end)
    {
        var maxDepth = MaxDepth();
        var destination = new Queue<TCardGroupName>();
        destination.Enqueue(start);
        
        for(var depth = 0; depth < maxDepth; depth++)
        {
            // This is gonna be name for depth + 1.
            var newDestinations = new Queue<TCardGroupName>();

            while (destination.TryDequeue(out var head))
            {
                // we don't need to iterate through graphs,
                // because subgraphs are automatically added
                // when graphs are added.
                // e.g., a -> b -> c -> d but also
                // b -> c -> d
                // c -> d
                // are added

                // could use FindNodes(head) if needed
                foreach (var graph in Nodes)
                {
                    if(!graph.Name.Equals(head))
                    {
                        continue;
                    }

                    var node = graph;
                    while(node != null)
                    {
                        if(node.Name.Equals(end))
                        {
                            return true;
                        }

                        node = node.Next;
                    }

                    if(graph.Next != null)
                    {
                        newDestinations.Enqueue(graph.Next.Name);
                    }
                }
            }

            // Start looking at destinations for the next depth.
            destination = newDestinations;
        }

        return false;
    }

    private int MaxDepth()
    {
        int maxDepth = 0;

        foreach (var graph in Nodes)
        {
            var node = graph;
            int depth = 0;
            while (node != null)
            {
                depth++;
                node = node.Next;
            }

            maxDepth = Math.Max(maxDepth, depth);
        }

        return maxDepth;
    }

    public IEnumerator<CardSearchNode<TCardGroupName>> GetEnumerator()
    {
        IEnumerable<CardSearchNode<TCardGroupName>> nodes = Nodes;
        return nodes.GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        IEnumerable nodes = Nodes;
        return nodes.GetEnumerator();
    }
}
