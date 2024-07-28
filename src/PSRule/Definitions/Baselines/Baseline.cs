// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Management.Automation;
using Newtonsoft.Json;
using PSRule.Configuration;
using PSRule.Options;
using PSRule.Pipeline;
using PSRule.Resources;
using YamlDotNet.Serialization;

namespace PSRule.Definitions.Baselines;

/// <summary>
/// A specification for a V1 baseline resource.
/// </summary>
internal interface IBaselineV1Spec
{
    /// <summary>
    /// Options that affect property binding.
    /// </summary>
    BindingOption Binding { get; set; }

    /// <summary>
    /// Allows configuration key/ values to be specified that can be used within rule definitions.
    /// </summary>
    ConfigurationOption Configuration { get; set; }

    ///// <summary>
    ///// Options that configure conventions.
    ///// </summary>
    //ConventionOption Convention { get; set; }

    /// <summary>
    /// Options that configure additional rule overrides.
    /// </summary>
    OverrideOption Override { get; set; }

    /// <summary>
    /// Options for that affect which rules are executed by including and filtering discovered rules.
    /// </summary>
    RuleOption Rule { get; set; }
}

/// <summary>
/// A baseline resource V1.
/// </summary>
[Spec(Specs.V1, Specs.Baseline)]
public sealed class Baseline : InternalResource<BaselineSpec>, IResource
{
    /// <summary>
    /// Create a baseline instance.
    /// </summary>
    public Baseline(string apiVersion, SourceFile source, ResourceMetadata metadata, IResourceHelpInfo info, ISourceExtent extent, BaselineSpec spec)
        : base(ResourceKind.Baseline, apiVersion, source, metadata, info, extent, spec) { }

    /// <summary>
    /// The unique identifier for the baseline.
    /// </summary>
    [YamlIgnore()]
    public string BaselineId => Name;

    /// <summary>
    /// A human readable block of text, used to identify the purpose of the rule.
    /// </summary>
    [JsonIgnore]
    [YamlIgnore]
    public string Synopsis => Info.Synopsis.Text;
}

/// <summary>
/// A specification for a V1 baseline resource.
/// </summary>
public sealed class BaselineSpec : Spec, IBaselineV1Spec
{
    /// <inheritdoc/>
    public BindingOption Binding { get; set; }

    /// <inheritdoc/>
    public ConfigurationOption Configuration { get; set; }

    ///// <inheritdoc/>
    //public ConventionOption Convention { get; set; }

    /// <inheritdoc/>
    public OverrideOption Override { get; set; }

    /// <inheritdoc/>
    public RuleOption Rule { get; set; }
}

internal sealed class BaselineFilter : IResourceFilter
{
    private readonly HashSet<string> _Include;
    private readonly WildcardPattern _WildcardMatch;

    public BaselineFilter(string[] include)
    {
        _Include = include == null || include.Length == 0 ? null : new HashSet<string>(include, StringComparer.OrdinalIgnoreCase);
        _WildcardMatch = null;
        if (include != null && include.Length > 0 && WildcardPattern.ContainsWildcardCharacters(include[0]))
        {
            if (include.Length > 1)
                throw new NotSupportedException(PSRuleResources.MatchSingleName);

            _WildcardMatch = new WildcardPattern(include[0]);
        }
    }

    ResourceKind IResourceFilter.Kind => ResourceKind.Baseline;

    public bool Match(IResource resource)
    {
        return _Include == null || _Include.Contains(resource.Name) || MatchWildcard(resource.Name);
    }

    private bool MatchWildcard(string name)
    {
        return _WildcardMatch != null && _WildcardMatch.IsMatch(name);
    }
}

internal sealed class BaselineRef : ResourceRef
{
    public readonly ScopeType Type;

    public BaselineRef(string id, ScopeType scopeType)
        : base(id, ResourceKind.Baseline)
    {
        Type = scopeType;
    }
}
