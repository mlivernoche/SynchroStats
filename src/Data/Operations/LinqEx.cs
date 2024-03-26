namespace SynchroStats.Data.Operations;

public static class LinqEx
{
    public static IEnumerable<T> WhereIsNotNull<T>(this IEnumerable<T?> source)
    {
        foreach (var item in source)
        {
            if (item != null)
            {
                yield return item;
            }
        }
    }
}
