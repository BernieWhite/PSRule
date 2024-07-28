// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Collections;
using PSRule.Converters;

namespace PSRule.Data;

/// <summary>
/// A mapping of string to string arrays.
/// </summary>
public sealed class EnumMap<T> : KeyMap<T> where T : struct, Enum
{
    /// <summary>
    /// Create an empty <see cref="EnumMap{T}"/> instance.
    /// </summary>
    public EnumMap()
        : base() { }

    /// <summary>
    /// Create an instance by copying an existing <see cref="EnumMap{T}"/>.
    /// </summary>
    internal EnumMap(EnumMap<T> map)
        : base(map) { }

    /// <summary>
    /// Create an instance by copying mapped keys from a string dictionary.
    /// </summary>
    internal EnumMap(IDictionary<string, T> map)
        : base(map) { }

    /// <summary>
    /// Create an instance by copying mapped keys from a <seealso cref="Hashtable"/>.
    /// </summary>
    /// <param name="map"></param>
    internal EnumMap(Hashtable map)
        : base(map) { }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="hashtable"></param>
    public static implicit operator EnumMap<T>(Hashtable hashtable)
    {
        return new EnumMap<T>(hashtable);
    }

    /// <summary>
    /// Convert a hashtable into a <see cref="EnumMap{T}"/> instance.
    /// </summary>
    public static EnumMap<T> FromHashtable(Hashtable hashtable)
    {
        return new EnumMap<T>(hashtable);
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="o"></param>
    /// <param name="value"></param>
    /// <returns></returns>
    protected override bool TryConvertValue(object o, out T value)
    {
        value = default;
        if (TypeConverter.TryEnum<T>(o, convert: true, out var result) && result != null)
        {
            value = result.Value;
            return true;
        }
        return false;
    }
}
