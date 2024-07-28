// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace PSRule.Definitions.Rules;

/// <summary>
/// Any overrides for the rule.
/// </summary>
public sealed class RuleOverride
{
    /// <summary>
    /// If the rule fails, how serious is the result.
    /// </summary>
    public SeverityLevel? Level { get; set; }
}
