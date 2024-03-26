using System.Collections;
using System.Diagnostics.CodeAnalysis;

namespace SynchroStats;

public sealed class DictionaryWithGeneratedKeys<TKey, TValue> : IReadOnlyDictionary<TKey, TValue>
        where TKey : notnull, IEquatable<TKey>
{
    private readonly Func<TValue, TKey> _keySelector;
    private readonly Dictionary<TKey, TValue> _dictionary;

    public DictionaryWithGeneratedKeys(Func<TValue, TKey> keySelector)
    {
        _keySelector = keySelector;
        _dictionary = new Dictionary<TKey, TValue>(EqualityComparer<TKey>.Default);
    }

    public DictionaryWithGeneratedKeys(Func<TValue, TKey> keySelector, IEnumerable<TValue> values) : this(keySelector)
    {
        foreach (var value in values)
        {
            TryAdd(value);
        }
    }

    public TKey GenerateKey(TValue value)
    {
        return _keySelector(value);
    }

    public void AddOrUpdate(TValue val)
    {
        var key = _keySelector(val);
        _dictionary[key] = val;
    }

    public bool TryAdd(TValue val)
    {
        var key = _keySelector(val);
        return _dictionary.TryAdd(key, val);
    }

    public bool TryRemove(TValue val)
    {
        var key = _keySelector(val);
        return _dictionary.Remove(key);
    }

    public void Update(TValue val)
    {
        var key = _keySelector(val);
        _dictionary[key] = val;
    }

    public (bool Success, TValue? Result) TryGetValue(TValue val)
    {
        var key = _keySelector(val);

        if (_dictionary.TryGetValue(key, out var value))
        {
            return (true, value);
        }

        return (false, default);
    }

    public (bool Success, TValue? Result) TryGetValue(TKey key)
    {
        if (_dictionary.TryGetValue(key, out var value))
        {
            return (true, value);
        }

        return (false, default);
    }

    public TValue this[TKey key]
    {
        get
        {
            IReadOnlyDictionary<TKey, TValue> dictionary = _dictionary;
            return dictionary[key];
        }
    }

    public IEnumerable<TKey> Keys
    {
        get
        {
            IReadOnlyDictionary<TKey, TValue> dictionary = _dictionary;
            return dictionary.Keys;
        }
    }

    public IEnumerable<TValue> Values
    {
        get
        {
            IReadOnlyDictionary<TKey, TValue> dictionary = _dictionary;
            return dictionary.Values;
        }
    }

    public int Count
    {
        get
        {
            IReadOnlyDictionary<TKey, TValue> dictionary = _dictionary;
            return dictionary.Count;
        }
    }

    public bool ContainsKey(TKey key)
    {
        Dictionary<TKey, TValue> dictionary = _dictionary;
        return dictionary.ContainsKey(key);
    }

    public bool ContainsValue(TValue val)
    {
        var key = _keySelector(val);
        return ContainsKey(key);
    }

    public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator()
    {
        IEnumerable<KeyValuePair<TKey, TValue>> enumerable = _dictionary;
        return enumerable.GetEnumerator();
    }

    public bool TryGetValue(TKey key, [MaybeNullWhen(false)] out TValue value)
    {
        Dictionary<TKey, TValue> dictionary = _dictionary;
        return dictionary.TryGetValue(key, out value);
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        IEnumerable enumerable = _dictionary;
        return enumerable.GetEnumerator();
    }
}
