// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.ComponentModel;
using PSRule.Data;
using PSRule.Definitions.Rules;

namespace PSRule.Options;

/// <summary>
/// Options that configure additional rule overrides.
/// </summary>
/// <remarks>
/// See <see href="https://aka.ms/ps-rule/options"/>.
/// </remarks>
public sealed class OverrideOption : IEquatable<OverrideOption>, IOption
{
    private const string LEVEL_KEY_PREFIX = "PSRULE_OVERRIDE_LEVEL_";

    internal static readonly OverrideOption Default = new()
    {
    };

    /// <summary>
    /// Create an option instance.
    /// </summary>
    public OverrideOption()
    {
        Level = null;
    }

    /// <summary>
    /// Create an option instance based on an existing object.
    /// </summary>
    /// <param name="option">The existing object to copy.</param>
    public OverrideOption(OverrideOption option)
    {
        if (option == null)
            return;

        Level = option.Level;
    }

    /// <inheritdoc/>
    public override bool Equals(object obj)
    {
        return obj is OverrideOption option && Equals(option);
    }

    /// <inheritdoc/>
    public bool Equals(OverrideOption other)
    {
        return other != null &&
            Level == other.Level;
    }

    /// <inheritdoc/>
    public override int GetHashCode()
    {
        unchecked // Overflow is fine
        {
            var hash = 17;
            hash = hash * 23 + (Level != null ? Level.GetHashCode() : 0);
            return hash;
        }
    }

    /// <summary>
    /// Combines two option instances into a new merged instance.
    /// The new instance uses any non-null values from <paramref name="o1"/>.
    /// Any null values from <paramref name="o1"/> are replaced with <paramref name="o2"/>.
    /// </summary>
    public static OverrideOption Combine(OverrideOption o1, OverrideOption o2)
    {
        var result = new OverrideOption(o1)
        {
            Level = o1?.Level ?? o2?.Level,
        };
        return result;
    }

    /// <inheritdoc/>
    [DefaultValue(null)]
    public EnumMap<SeverityLevel>? Level { get; set; }

    /// <summary>
    /// Load from environment variables.
    /// </summary>
    internal void Load()
    {
        var level = new EnumMap<SeverityLevel>();
        level.FromEnvironment(prefix: LEVEL_KEY_PREFIX);
        Level = level.Count > 0 ? level : null;
    }

    /// <summary>
    /// Load from a dictionary.
    /// </summary>
    internal void Load(IDictionary<string, object> index)
    {
        var level = new EnumMap<SeverityLevel>();
        level.FromDictionary(index, prefix: LEVEL_KEY_PREFIX);
        Level = level.Count > 0 ? level : null;
    }
}
