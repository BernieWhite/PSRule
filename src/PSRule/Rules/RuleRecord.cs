// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Collections;
using System.ComponentModel;
using System.Diagnostics;
using System.Management.Automation;
using Newtonsoft.Json;
using PSRule.Data;
using PSRule.Definitions;
using PSRule.Definitions.Rules;
using PSRule.Pipeline;
using YamlDotNet.Serialization;

namespace PSRule.Rules;

/// <summary>
/// A detailed format for rule results.
/// </summary>
[DebuggerDisplay("{RuleId}, Outcome = {Outcome}")]
[JsonObject]
public sealed class RuleRecord : IDetailedRuleResultV2
{
    private readonly TargetObject _TargetObject;

    internal readonly ResultDetail _Detail;

    internal RuleRecord(string runId, ResourceId ruleId, string @ref, TargetObject targetObject, string targetName, string targetType, IResourceTags tag, RuleHelpInfo info, Hashtable field, RuleProperties @default, ISourceExtent extent, RuleOutcome outcome = RuleOutcome.None, RuleOutcomeReason reason = RuleOutcomeReason.None, RuleOverride @override = null)
    {
        _TargetObject = targetObject;
        RunId = runId;
        RuleId = ruleId.Value;
        RuleName = ruleId.Name;
        Ref = @ref;
        TargetObject = targetObject.Value;
        TargetName = targetName;
        TargetType = targetType;
        Outcome = outcome;
        OutcomeReason = reason;
        Info = info;
        Source = targetObject.Source.GetSourceInfo();
        Extent = extent;
        Default = @default;
        Override = @override;
        Level = Override?.Level ?? Default.Level;

        _Detail = new ResultDetail();
        if (tag != null)
            Tag = tag.ToHashtable();

        if (field != null && field.Count > 0)
            Field = field;
    }

    /// <summary>
    /// A unique identifier for the run.
    /// </summary>
    [JsonProperty(PropertyName = "runId")]
    public string RunId { get; }

    /// <summary>
    /// A unique identifier for the rule.
    /// </summary>
    /// <remarks>
    /// An additional opaque identifier may also be provided by by <see cref="Ref"/>.
    /// </remarks>
    [JsonIgnore]
    [YamlIgnore]
    public readonly string RuleId;

    /// <summary>
    /// The name of the rule.
    /// </summary>
    [JsonProperty(PropertyName = "ruleName")]
    public readonly string RuleName;

    /// <summary>
    /// A stable opaque unique identifier for the rule in addition to <see cref="RuleId"/>.
    /// </summary>
    public string Ref { get; }

    /// <summary>
    /// If the rule fails, how serious is the result.
    /// </summary>
    [JsonProperty(PropertyName = "level")]
    public SeverityLevel Level { get; }

    /// <summary>
    /// A source location for the rule that executed.
    /// </summary>
    [JsonIgnore]
    [YamlIgnore]
    public ISourceExtent Extent { get; }

    /// <summary>
    /// The outcome after the rule processes an object.
    /// </summary>
    [JsonProperty(PropertyName = "outcome")]
    public RuleOutcome Outcome { get; internal set; }

    /// <summary>
    /// An additional reason code for the <see cref="Outcome"/>.
    /// </summary>
    [JsonProperty(PropertyName = "outcomeReason")]
    public RuleOutcomeReason OutcomeReason { get; internal set; }

    /// <summary>
    /// A localized recommendation for the rule.
    /// </summary>
    [JsonIgnore]
    [YamlIgnore]
    public string Recommendation => Info.Recommendation?.Text ?? Info.Synopsis?.Text;

    /// <summary>
    /// The reason for the failed condition.
    /// </summary>
    [DefaultValue(null)]
    [JsonProperty(PropertyName = "reason")]
    public string[] Reason => _Detail.Count > 0 ? _Detail.GetReasonStrings() : null;

    /// <summary>
    /// A name to identify the target object.
    /// </summary>
    [JsonProperty(PropertyName = "targetName")]
    public string TargetName { get; }

    /// <summary>
    /// The type of the target object.
    /// </summary>
    [JsonProperty(PropertyName = "targetType")]
    public string TargetType { get; }

    /// <summary>
    /// The current target object.
    /// </summary>
    [JsonIgnore]
    [YamlIgnore]
    public PSObject TargetObject { get; }

    /// <summary>
    /// Custom data set by the rule for this target object.
    /// </summary>
    [JsonProperty(PropertyName = "data")]
    public Hashtable Data => _TargetObject.GetData();

    /// <summary>
    /// A set of custom fields bound for the target object.
    /// </summary>
    [JsonProperty(PropertyName = "field")]
    public Hashtable Field { get; }

    /// <summary>
    /// Tags set for the rule.
    /// </summary>
    [DefaultValue(null)]
    [JsonProperty(PropertyName = "tag")]
    public Hashtable Tag { get; }

    /// <summary>
    /// Help info for the rule.
    /// </summary>
    [DefaultValue(null)]
    [JsonProperty(PropertyName = "info")]
    public IRuleHelpInfoV2 Info { get; }

    /// <summary>
    /// The execution time of the rule in millisecond.
    /// </summary>
    [DefaultValue(0f)]
    [JsonProperty(PropertyName = "time")]
    public long Time { get; internal set; }

    /// <summary>
    /// Additional information if the rule errored. If the rule passed or failed this value is null.
    /// </summary>
    [DefaultValue(null)]
    [JsonProperty(PropertyName = "error")]
    public ErrorInfo Error { get; internal set; }

    /// <summary>
    /// Source of target object.
    /// </summary>
    [DefaultValue(null)]
    [JsonProperty(PropertyName = "source")]
    public TargetSourceInfo[] Source { get; }

    /// <summary>
    /// Rule reason details.
    /// </summary>
    [DefaultValue(null)]
    [JsonProperty(PropertyName = "detail")]
    [YamlMember()]
    public IResultDetailV2 Detail => _Detail;

    /// <summary>
    /// Any default properties for the rule.
    /// </summary>
    [JsonIgnore]
    [YamlIgnore]
    public RuleProperties Default { get; set; }

    /// <summary>
    /// Any overrides for the rule.
    /// </summary>
    [JsonIgnore]
    [YamlIgnore]
    public RuleOverride Override { get; }

    /// <summary>
    /// Determine if the rule is successful or skipped.
    /// </summary>
    public bool IsSuccess()
    {
        return Outcome == RuleOutcome.Pass || Outcome == RuleOutcome.None;
    }

    /// <summary>
    /// Determine if the rule was executed and resulted in an outcome.
    /// </summary>
    public bool IsProcessed()
    {
        return Outcome == RuleOutcome.Pass || Outcome == RuleOutcome.Fail || Outcome == RuleOutcome.Error;
    }

    /// <summary>
    /// Gets a string for output views in PowerShell.
    /// </summary>
    /// <remarks>
    /// This method is called by PowerShell.
    /// </remarks>
    public string GetReasonViewString()
    {
        return Reason == null || Reason.Length == 0 ? string.Empty : string.Join(System.Environment.NewLine, Reason);
    }

    internal bool HasSource()
    {
        return Source != null && Source.Length > 0;
    }
}
