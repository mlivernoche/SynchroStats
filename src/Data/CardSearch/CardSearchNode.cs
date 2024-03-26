using System.Diagnostics;

namespace SynchroStats.Data.CardSearch;

public static class CardSearchNode
{
    public static CardSearchNode<TName>? CreateSearchGraph<TName>(IEnumerable<TName> names)
        where TName : notnull, IEquatable<TName>, IComparable<TName>
    {
        static CardSearchNode<TName>? CreateGraph(int currentNode, TName[] cardNames)
        {
            if (currentNode == cardNames.Length)
            {
                return null;
            }

            return new CardSearchNode<TName>(cardNames[currentNode], CreateGraph(currentNode + 1, cardNames));
        }

        return CreateGraph(0, names.ToArray());
    }

    public static CardSearchNode<TName> GetLastNode<TName>(this CardSearchNode<TName> node)
        where TName : notnull, IEquatable<TName>, IComparable<TName>
    {
        while(node.Next != null)
        {
            node = node.Next;
        }

        return node;
    }
}

[DebuggerDisplay("{Name} -> {Next}")]
public class CardSearchNode<TName>
    where TName : notnull, IEquatable<TName>, IComparable<TName>
{
    public TName Name { get; }
    public CardSearchNode<TName>? Next { get; }

    public CardSearchNode(TName name, CardSearchNode<TName>? next)
    {
        Name = name;
        Next = next;
    }
}
