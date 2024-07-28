// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace PSRule.Definitions.Rules;

/// <summary>
/// Any rule properties.
/// </summary>
public sealed class RuleProperties
{
    /// <summary>
    /// If the rule fails, how serious is the result.
    /// </summary>
    public SeverityLevel Level { get; set; }
}
