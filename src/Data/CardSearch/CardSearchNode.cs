using System.Diagnostics;

namespace SynchroStats.Data.CardSearch;

public static class CardSearchNode
{
    public static CardSearchNode<TCardGroupName>? CreateSearchGraph<TCardGroupName>(IEnumerable<TCardGroupName> names)
        where TCardGroupName : notnull, IEquatable<TCardGroupName>, IComparable<TCardGroupName>
    {
        static CardSearchNode<TCardGroupName>? CreateGraph(int currentNode, TCardGroupName[] cardNames)
        {
            if (currentNode == cardNames.Length)
            {
                return null;
            }

            return new CardSearchNode<TCardGroupName>(cardNames[currentNode], CreateGraph(currentNode + 1, cardNames));
        }

        return CreateGraph(0, names.ToArray());
    }

    public static CardSearchNode<TCardGroupName> GetLastNode<TCardGroupName>(this CardSearchNode<TCardGroupName> node)
        where TCardGroupName : notnull, IEquatable<TCardGroupName>, IComparable<TCardGroupName>
    {
        while(node.Next != null)
        {
            node = node.Next;
        }

        return node;
    }
}

[DebuggerDisplay("{Name} -> {Next}")]
public class CardSearchNode<TCardGroupName>
    where TCardGroupName : notnull, IEquatable<TCardGroupName>, IComparable<TCardGroupName>
{
    public TCardGroupName Name { get; }
    public CardSearchNode<TCardGroupName>? Next { get; }

    public CardSearchNode(TCardGroupName name, CardSearchNode<TCardGroupName>? next)
    {
        Name = name;
        Next = next;
    }
}
