// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Collections;
using PSRule.Definitions.Baselines;
using PSRule.Options;

namespace PSRule.Configuration;

/// <summary>
/// A subset of options that can be defined within a baseline.
/// These options can be passes as a baseline for use within a pipeline.
/// </summary>
public class BaselineOption
{
    internal sealed class BaselineRef : BaselineOption
    {
        public readonly string Name;

        public BaselineRef(string name)
        {
            Name = name;
        }
    }

    internal sealed class BaselineInline : BaselineOption, IBaselineV1Spec
    {
        public BaselineInline()
        {
            Binding = new BindingOption();
            Configuration = new ConfigurationOption();
            Rule = new RuleOption();
        }

        public BindingOption Binding { get; set; }

        public ConfigurationOption Configuration { get; set; }

        //public ConventionOption Convention { get; set; }

        public OverrideOption Override { get; set; }

        public RuleOption Rule { get; set; }
    }

    /// <summary>
    /// Creates a baseline option from a hashtable of key/ values.
    /// </summary>
    /// <param name="hashtable">A hashtable of key/ values.</param>
    /// <returns>A baseline option composed of provided key/ values.</returns>
    public static implicit operator BaselineOption(Hashtable hashtable)
    {
        return FromHashtable(hashtable);
    }

    /// <summary>
    /// Creates a reference to a baseline by name which is resolved at runtime.
    /// </summary>
    /// <param name="value">The name of the baseline.</param>
    /// <returns>A reference to a baseline option.</returns>
    public static implicit operator BaselineOption(string value)
    {
        return FromString(value);
    }

    /// <summary>
    /// Creates a baseline option from a hashtable of key/ values.
    /// </summary>
    /// <param name="hashtable">A hashtable of key/ values.</param>
    /// <returns>A baseline option composed of provided key/ values.</returns>
    public static BaselineOption FromHashtable(Hashtable hashtable)
    {
        var option = new BaselineInline();
        if (hashtable != null)
        {
            // Build index to allow mapping
            var index = PSRuleOption.BuildIndex(hashtable);
            Load(option, index);
        }
        return option;
    }

    /// <summary>
    /// Creates a reference to a baseline by name which is resolved at runtime.
    /// </summary>
    /// <param name="value">The name of the baseline.</param>
    /// <returns>A reference to a baseline option.</returns>
    public static BaselineOption FromString(string value)
    {
        return string.IsNullOrEmpty(value) ? null : new BaselineRef(value);
    }

    internal static void Load(IBaselineV1Spec option)
    {
        // Binding.Field - currently not supported

        if (Environment.TryBool("PSRULE_BINDING_IGNORECASE", out var ignoreCase))
            option.Binding.IgnoreCase = ignoreCase;

        if (Environment.TryString("PSRULE_BINDING_NAMESEPARATOR", out var nameSeparator))
            option.Binding.NameSeparator = nameSeparator;

        if (Environment.TryBool("PSRULE_BINDING_PREFERTARGETINFO", out var preferTargetInfo))
            option.Binding.PreferTargetInfo = preferTargetInfo;

        if (Environment.TryStringArray("PSRULE_BINDING_TARGETNAME", out var targetName))
            option.Binding.TargetName = targetName;

        if (Environment.TryStringArray("PSRULE_BINDING_TARGETTYPE", out var targetType))
            option.Binding.TargetType = targetType;

        if (Environment.TryBool("PSRULE_BINDING_USEQUALIFIEDNAME", out var useQualifiedName))
            option.Binding.UseQualifiedName = useQualifiedName;

        if (Environment.TryString("PSRULE_RULE_BASELINE", out var baseline))
            option.Rule.Baseline = baseline;

        if (Environment.TryStringArray("PSRULE_RULE_EXCLUDE", out var exclude))
            option.Rule.Exclude = exclude;

        if (Environment.TryBool("PSRULE_RULE_INCLUDELOCAL", out var includeLocal))
            option.Rule.IncludeLocal = includeLocal;

        if (Environment.TryStringArray("PSRULE_RULE_INCLUDE", out var include))
            option.Rule.Include = include;

        // Rule.Tag - currently not supported

        // Process configuration values
        option.Override.Load();
        option.Configuration.Load();
    }

    /// <summary>
    /// Load matching values
    /// </summary>
    /// <param name="option">A baseline options object to load.</param>
    /// <param name="properties">One or more indexed properties.</param>
    internal static void Load(IBaselineV1Spec option, Dictionary<string, object> properties)
    {
        if (properties.TryPopValue("Binding.Field", out Hashtable map))
            option.Binding.Field = new FieldMap(map);

        if (properties.TryPopBool("Binding.IgnoreCase", out var ignoreCase))
            option.Binding.IgnoreCase = ignoreCase;

        if (properties.TryPopString("Binding.NameSeparator", out var nameSeparator))
            option.Binding.NameSeparator = nameSeparator;

        if (properties.TryPopBool("Binding.PreferTargetInfo", out var preferTargetInfo))
            option.Binding.PreferTargetInfo = preferTargetInfo;

        if (properties.TryPopStringArray("Binding.TargetName", out var targetName))
            option.Binding.TargetName = targetName;

        if (properties.TryPopStringArray("Binding.TargetType", out var targetType))
            option.Binding.TargetType = targetType;

        if (properties.TryPopValue("Binding.UseQualifiedName", out bool useQualifiedName))
            option.Binding.UseQualifiedName = useQualifiedName;

        if (properties.TryPopString("Rule.Baseline", out var baseline))
            option.Rule.Baseline = baseline;

        if (properties.TryPopStringArray("Rule.Exclude", out var exclude))
            option.Rule.Exclude = exclude;

        if (properties.TryPopBool("Rule.IncludeLocal", out var includeLocal))
            option.Rule.IncludeLocal = includeLocal;

        if (properties.TryPopStringArray("Rule.Include", out var include))
            option.Rule.Include = include;

        if (properties.TryPopValue("Rule.Tag", out Hashtable tag))
            option.Rule.Tag = tag;

        // Process configuration values
        option.Override.Load(properties);
        option.Configuration.Load(properties);
    }
}
