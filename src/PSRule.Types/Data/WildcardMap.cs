// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace PSRule.Data;

/// <summary>
/// 
/// </summary>
/// <typeparam name="T"></typeparam>
public sealed class WildcardMap<T>
{
    private readonly Dictionary<string, T> _Map;
    private List<KeyValuePair<string, T>>? _Wildcard;

    /// <summary>
    /// 
    /// </summary>
    public WildcardMap()
    {
        _Map = new Dictionary<string, T>(StringComparer.OrdinalIgnoreCase);
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="values"></param>
    public WildcardMap(IEnumerable<KeyValuePair<string, T>> values)
    {
        _Map = new Dictionary<string, T>(StringComparer.OrdinalIgnoreCase);
        Load(values);
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="key"></param>
    /// <param name="value"></param>
    /// <returns></returns>
    public bool TryGetValue(string key, out T? value)
    {
        value = default;
        if (string.IsNullOrEmpty(key)) return false;
        if (_Map.TryGetValue(key, out value)) return true;

        for (var i = 0; _Wildcard != null && i < _Wildcard.Count; i++)
        {
            if (key.Length > _Wildcard[i].Key.Length && key.StartsWith(_Wildcard[i].Key))
            {
                value = _Wildcard[i].Value;
                return true;
            }
        }
        return false;
    }

    private void Load(IEnumerable<KeyValuePair<string, T>> dictionary)
    {
        Dictionary<string, T>? wildcardIndex = null;
        foreach (var kv in dictionary)
        {
            var index = kv.Key.IndexOf('*');

            // Simple keys
            if (index < 0)
            {
                _Map[kv.Key] = kv.Value;
            }
            // Wildcard keys
            else
            {
                var key = kv.Key.Substring(0, index);
                wildcardIndex ??= new Dictionary<string, T>(StringComparer.OrdinalIgnoreCase);
                wildcardIndex[key] = kv.Value;
            }
        }

        if (wildcardIndex != null)
        {
            _Wildcard = new List<KeyValuePair<string, T>>();
            foreach (var kv in wildcardIndex.OrderByDescending(s => s.Key))
                _Wildcard.Add(kv);
        }
    }
}
