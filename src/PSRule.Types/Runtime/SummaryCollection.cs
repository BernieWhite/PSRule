// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace PSRule.Runtime;

/// <summary>
/// A collection of summary items to add to the output summary.
/// </summary>
public sealed class SummaryCollection : ISummaryCollection
{
    private readonly List<ISummaryItem> _Items;

    /// <summary>
    /// An empty collection of items.
    /// </summary>
    public SummaryCollection()
    {
        _Items = [];
    }

    /// <summary>
    /// Populate the collection will some initial items.
    /// </summary>
    /// <param name="items"></param>
    public SummaryCollection(IEnumerable<ISummaryItem> items)
    {
        _Items = [.. items];
    }

    /// <summary>
    /// Append a new summary based on provided content.
    /// </summary>
    /// <param name="title">The title of the summary.</param>
    /// <param name="body">The body of the summary.</param>
    public void Append(string title, string body)
    {
        _Items.Add(new TextSummary(title, body));
    }

    /// <summary>
    /// Append a new custom summary item.
    /// </summary>
    /// <param name="item">The specified custom item.</param>
    public void Append(ISummaryItem item)
    {
        if (item == null) return;
        _Items.Add(item);
    }
}
