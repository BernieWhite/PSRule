// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace PSRule.Runtime;

/// <summary>
/// A simple summary item with a title and a body.
/// </summary>
public sealed class TextSummary : ISummaryItem
{
    /// <summary>
    /// Create an instance of a text summary.
    /// </summary>
    public TextSummary(string title, string body)
    {
        Title = title;
        Body = body;
    }

    /// <summary>
    /// The title of the summary item.
    /// </summary>
    public string Title { get; }

    /// <summary>
    /// The body of the summary item.
    /// </summary>
    public string Body { get; }
}
