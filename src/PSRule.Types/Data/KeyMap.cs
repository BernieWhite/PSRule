// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Collections;

namespace PSRule.Data;

/// <summary>
/// A mapping of string to string arrays.
/// </summary>
public abstract class KeyMap<T> : IEnumerable<KeyValuePair<string, T>>
{
    private readonly Dictionary<string, T> _Map;

    /// <summary>
    /// Create an empty <see cref="StringArrayMap"/> instance.
    /// </summary>
    protected KeyMap()
    {
        _Map = new Dictionary<string, T>(StringComparer.OrdinalIgnoreCase);
    }

    /// <summary>
    /// Create an instance by copying an existing <see cref="StringArrayMap"/>.
    /// </summary>
    protected KeyMap(KeyMap<T> map)
    {
        _Map = new Dictionary<string, T>(map._Map, StringComparer.OrdinalIgnoreCase);
    }

    /// <summary>
    /// Create an instance by copying mapped keys from a string dictionary.
    /// </summary>
    protected KeyMap(IDictionary<string, T> map)
    {
        _Map = new Dictionary<string, T>(map, StringComparer.OrdinalIgnoreCase);
    }

    /// <summary>
    /// Create an instance by copying mapped keys from a <seealso cref="Hashtable"/>.
    /// </summary>
    /// <param name="map"></param>
    protected KeyMap(Hashtable map)
        : this()
    {
        if (map != null) FromDictionary(map.IndexByString());
    }

    /// <summary>
    /// The number of mapped keys.
    /// </summary>
    public int Count => _Map.Count;

    /// <summary>
    /// Get or set mapping for a specified key.
    /// </summary>
    public T? this[string key]
    {
        get
        {
            return !string.IsNullOrEmpty(key) && _Map.TryGetValue(key, out var value) ? value : GetValueDefault();
        }
        set
        {
            if (!string.IsNullOrEmpty(key) && value != null)
                _Map[key] = value;
        }
    }

    /// <summary>
    /// Try to get a mapping by key.
    /// </summary>
    /// <param name="key">The key.</param>
    /// <param name="value">Returns an array of mapped keys.</param>
    /// <returns>Returns <c>true</c> if the key was found. Otherwise <c>false</c> is returned.</returns>
    public bool TryGetValue(string key, out T value)
    {
        return _Map.TryGetValue(key, out value);
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="key"></param>
    /// <param name="value"></param>
    public void Add(string key, T value)
    {
        _Map.Add(key, value);
    }

    /// <inheritdoc/>
    public IEnumerator<KeyValuePair<string, T>> GetEnumerator()
    {
        return ((IEnumerable<KeyValuePair<string, T>>)_Map).GetEnumerator();
    }

    /// <inheritdoc/>
    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }

    /// <summary>
    /// Convert the instance into a dictionary.
    /// </summary>
    /// <returns></returns>
    public IDictionary<string, T> ToDictionary()
    {
        return _Map;
    }

    /// <summary>
    /// Get the default for the type.
    /// </summary>
    protected virtual T? GetValueDefault()
    {
        return default;
    }

    /// <summary>
    /// Convert the type.
    /// </summary>
    protected virtual bool TryConvertValue(object o, out T? value)
    {
        value = default;
        if (o is T t)
        {
            value = t;
            return true;
        }
        return false;
    }

    /// <summary>
    /// Load a key map from an existing dictionary.
    /// </summary>
    internal void FromDictionary(IDictionary<string, object> dictionary, string? prefix = null, Func<string, string>? format = null)
    {
        foreach (var kv in dictionary)
        {
            if (TryKeyPrefix(kv.Key, prefix, out var suffix) && TryConvertValue(kv.Value, out var value) && value != null)
            {
                if (format != null)
                    suffix = format(suffix);

                _Map[suffix] = value;
            }
        }
    }

    /// <summary>
    /// Load values from environment variables into the option.
    /// Keys that appear in both will replaced by environment variable values.
    /// </summary>
    /// <exception cref="ArgumentNullException">Is raised if the environment helper is null.</exception>
    internal void FromEnvironment(string? prefix = null, Func<string, string>? format = null)
    {
        foreach (var kv in Environment.GetByPrefix(prefix))
        {
            if (TryKeyPrefix(kv.Key, prefix, out var suffix) && TryConvertValue(kv.Value, out var value) && value != null)
            {
                if (format != null)
                    suffix = format(suffix);

                _Map[suffix] = value;
            }
        }
    }

    /// <summary>
    /// Try a key prefix.
    /// </summary>
    private static bool TryKeyPrefix(string key, string? prefix, out string suffix)
    {
        suffix = key;
        if (prefix == null || prefix.Length == 0)
            return true;

        if (key.StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
        {
            suffix = key.Substring(prefix.Length);
            return true;
        }
        return false;
    }
}
