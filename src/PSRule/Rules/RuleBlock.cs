// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Collections;
using System.Diagnostics;
using Newtonsoft.Json;
using PSRule.Definitions;
using PSRule.Definitions.Rules;
using PSRule.Pipeline;
using PSRule.Runtime;
using YamlDotNet.Serialization;

namespace PSRule.Rules;

#nullable enable

internal delegate bool RulePrecondition();

internal delegate RuleConditionResult RuleCondition();

/// <summary>
/// Define an instance of a rule block. Each rule block has a unique id.
/// </summary>
[DebuggerDisplay("{Id} @{Source.Path}")]
internal sealed class RuleBlock : ILanguageBlock, IDependencyTarget, IDisposable, IResource, IRuleV1
{
    internal RuleBlock(ISourceFile source, ResourceId id, ResourceId? @ref, RuleProperties @default, RuleOverride @override, RuleHelpInfo info, ICondition condition, IResourceTags tag, ResourceId[] alias, ResourceId[] dependsOn, Hashtable configuration, ISourceExtent extent, ResourceFlags flags, IResourceLabels labels)
    {
        Source = source;
        Name = id.Name;

        // Get fully qualified Id, either RuleName or Module\RuleName
        Id = id;
        Ref = @ref;
        Alias = alias;

        Default = @default;
        Override = @override;
        Level = Override != null && Override.Level.HasValue && Override.Level != SeverityLevel.None ? Override.Level.Value : Default.Level;
        Info = info;
        Condition = condition;
        Tag = tag;
        DependsOn = dependsOn;
        Configuration = configuration;
        Extent = extent;
        Flags = flags;
        Labels = labels;
    }

    /// <summary>
    /// A unique identifier for the rule.
    /// </summary>
    public ResourceId Id { get; }

    /// <inheritdoc/>
    public ResourceId? Ref { get; }

    /// <inheritdoc/>
    public ResourceId[]? Alias { get; }

    /// <summary>
    /// If the rule fails, how serious is the result.
    /// </summary>
    public SeverityLevel Level { get; }

    /// <summary>
    /// The name of the rule.
    /// </summary>
    public readonly string Name;

    /// <summary>
    /// The body of the rule definition where conditions are provided that either pass or fail the rule.
    /// </summary>
    [JsonIgnore]
    [YamlIgnore]
    public readonly ICondition Condition;

    /// <summary>
    /// Other rules that must completed successfully before calling this rule.
    /// </summary>
    public readonly ResourceId[] DependsOn;

    /// <summary>
    /// Tags assigned to block. Tags are additional metadata used to select rules to execute and identify results.
    /// </summary>
    public readonly IResourceTags Tag;

    /// <inheritdoc/>
    public IResourceLabels? Labels { get; }

    /// <summary>
    /// Configuration defaults for the rule definition.
    /// </summary>
    /// <remarks>
    /// These defaults are used when the value does not exist in the baseline configuration.
    /// </remarks>
    public readonly Hashtable Configuration;

    public readonly RuleHelpInfo Info;

    /// <inheritdoc/>
    public ISourceFile Source { get; }

    /// <inheritdoc/>
    public ISourceExtent Extent { get; }

    /// <inheritdoc/>
    [JsonIgnore]
    [YamlIgnore]
    public ResourceFlags Flags { get; }

    [JsonIgnore]
    [YamlIgnore]
    public RuleProperties Default { get; }

    [JsonIgnore]
    [YamlIgnore]
    public RuleOverride Override { get; }

    ResourceId[] IDependencyTarget.DependsOn => DependsOn;

    bool IDependencyTarget.Dependency => Source.IsDependency();

    ResourceKind IResource.Kind => ResourceKind.Rule;

    string IResource.ApiVersion => Specs.V1;

    string IResource.Name => Name;

    IResourceTags? IResource.Tags => Tag;

    IResourceHelpInfo IResource.Info => Info;

    IResourceTags IRuleV1.Tag => Tag;

    string IRuleV1.Synopsis => Info.Synopsis;

    InfoString IRuleV1.Recommendation => ((IRuleHelpInfoV2)Info)!.Recommendation;

    #region IDisposable

    public void Dispose()
    {
        Condition.Dispose();
    }

    #endregion IDisposable
}

#nullable restore
