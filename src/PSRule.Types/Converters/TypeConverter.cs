// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.Collections;
using Newtonsoft.Json.Linq;

namespace PSRule.Converters;

internal static class TypeConverter
{
    private const string PROPERTY_BASEOBJECT = "BaseObject";

    public static bool TryString(object o, out string? value)
    {
        value = null;
        if (o == null) return false;
        if (o is string s)
        {
            value = s;
            return true;
        }
        else if (o is JToken token && token.Type == JTokenType.String)
        {
            value = token.Value<string>();
            return true;
        }
        else if (TryGetValue(o, PROPERTY_BASEOBJECT, out var baseValue) && baseValue is string s_baseValue)
        {
            value = s_baseValue;
            return true;
        }
        return false;
    }

    public static bool TryString(object o, bool convert, out string? value)
    {
        if (TryString(o, out value) && value != null)
            return true;

        if (convert && o is Enum evalue)
        {
            value = evalue.ToString();
            return true;
        }
        else if (convert && TryLong(o, false, out var l_value))
        {
            value = l_value.ToString(Thread.CurrentThread.CurrentCulture);
            return true;
        }
        else if (convert && TryBool(o, false, out var b_value))
        {
            value = b_value.ToString(Thread.CurrentThread.CurrentCulture);
            return true;
        }
        else if (convert && TryInt(o, false, out var i_value))
        {
            value = i_value.ToString(Thread.CurrentThread.CurrentCulture);
            return true;
        }
        return false;
    }

    public static bool TryArray(object o, out Array? value)
    {
        value = null;
        if (o is string) return false;
        if (o is Array a)
            value = a;

        else if (o is JArray jArray)
            value = jArray.Values<object>().ToArray();

        else if (o is IEnumerable e)
            value = e.OfType<object>().ToArray();

        return value != null;
    }

    public static bool TryStringOrArray(object o, bool convert, out string[]? value)
    {
        // Handle single string
        if (TryString(o, convert, value: out var s) && s != null)
        {
            value = [s];
            return true;
        }

        // Handle multiple strings
        return TryStringArray(o, convert, out value);
    }

    public static bool TryStringArray(object o, bool convert, out string[]? value)
    {
        value = null;
        if (o is Array array)
        {
            value = new string[array.Length];
            for (var i = 0; i < array.Length; i++)
            {
                if (TryString(array.GetValue(i), convert, value: out var s) && s != null)
                    value[i] = s;
            }
        }
        else if (o is JArray jArray)
        {
            value = new string[jArray.Count];
            for (var i = 0; i < jArray.Count; i++)
            {
                if (TryString(jArray[i], convert, out var s) && s != null)
                    value[i] = s;
            }
        }
        else if (o is IEnumerable<string> enumerable)
        {
            value = enumerable.ToArray();
        }
        else if (o is IEnumerable e)
        {
            value = e.OfType<string>().ToArray();
        }
        return value != null;
    }

    /// <summary>
    /// Try to get an int from the existing object.
    /// </summary>
    public static bool TryInt(object o, bool convert, out int value)
    {
        if (o is int ivalue)
        {
            value = ivalue;
            return true;
        }
        if (o is long lvalue && lvalue <= int.MaxValue && lvalue >= int.MinValue)
        {
            value = (int)lvalue;
            return true;
        }
        else if (o is JToken token && token.Type == JTokenType.Integer)
        {
            value = token.Value<int>();
            return true;
        }
        else if (convert && TryString(o, out var s) && int.TryParse(s, out ivalue))
        {
            value = ivalue;
            return true;
        }
        value = default;
        return false;
    }

    public static bool TryBool(object o, bool convert, out bool value)
    {
        if (o is bool bvalue)
        {
            value = bvalue;
            return true;
        }
        else if (o is JToken token && token.Type == JTokenType.Boolean)
        {
            value = token.Value<bool>();
            return true;
        }
        else if (convert && TryString(o, out var s) && bool.TryParse(s, out bvalue))
        {
            value = bvalue;
            return true;
        }
        else if (convert && TryLong(o, convert: false, out var lvalue))
        {
            value = lvalue > 0;
            return true;
        }
        value = default;
        return false;
    }

    public static bool TryByte(object o, bool convert, out byte value)
    {
        if (o is byte bvalue)
        {
            value = bvalue;
            return true;
        }
        else if (o is JToken token && token.Type == JTokenType.Integer)
        {
            value = token.Value<byte>();
            return true;
        }
        else if (convert && TryString(o, out var s) && byte.TryParse(s, out bvalue))
        {
            value = bvalue;
            return true;
        }
        value = default;
        return false;
    }

    public static bool TryLong(object o, bool convert, out long value)
    {
        if (o is byte b)
        {
            value = b;
            return true;
        }
        else if (o is int i)
        {
            value = i;
            return true;
        }
        else if (o is uint ui)
        {
            value = (long)ui;
            return true;
        }
        else if (o is long l)
        {
            value = l;
            return true;
        }
        else if (o is ulong ul && ul <= long.MaxValue)
        {
            value = (long)ul;
            return true;
        }
        else if (o is JToken token && token.Type == JTokenType.Integer)
        {
            value = token.Value<long>();
            return true;
        }
        else if (convert && TryString(o, out var s) && long.TryParse(s, out l))
        {
            value = l;
            return true;
        }
        value = default;
        return false;
    }

    public static bool TryFloat(object o, bool convert, out float value)
    {
        if (o is float fvalue || (convert && o is string s && float.TryParse(s, out fvalue)))
        {
            value = fvalue;
            return true;
        }
        else if (convert && o is int ivalue)
        {
            value = ivalue;
            return true;
        }
        value = default;
        return false;
    }

    public static bool TryDouble(object o, bool convert, out double value)
    {
        if (o is double dvalue || (convert && o is string s && double.TryParse(s, out dvalue)))
        {
            value = dvalue;
            return true;
        }
        value = default;
        return false;
    }

    /// <summary>
    /// Try to get the environment variable as a enum of type <typeparamref name="T"/>.
    /// </summary>
    public static bool TryEnum<T>(object o, bool convert, out T? value) where T : struct, Enum
    {
        if (o is T t || convert && o is string s && Enum.TryParse(s, ignoreCase: true, out t))
        {
            value = t;
            return true;
        }
        value = null;
        return false;
    }

    private static bool TryGetValue(object o, string propertyName, out object? value)
    {
        value = null;
        if (o == null) return false;

        var type = o.GetType();
        if (type.TryGetPropertyInfo(propertyName, out var propertyInfo) && propertyInfo != null)
        {
            value = propertyInfo.GetValue(o);
            return true;
        }
        return false;
    }
}
