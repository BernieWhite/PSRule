// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace PSRule.Runtime;

/// <summary>
/// An interface for context available while executing a convention.
/// </summary>
public interface IConventionContext
{
    /// <summary>
    /// A collection of items to add to the run summary.
    /// </summary>
    SummaryCollection Summary { get; }
}
