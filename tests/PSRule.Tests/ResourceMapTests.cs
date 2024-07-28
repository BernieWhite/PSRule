// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Collections.Generic;
using PSRule.Data;
using PSRule.Definitions.Rules;

namespace PSRule;

public sealed class ResourceMapTests
{
    [Fact]
    public void Get()
    {
        var map = new WildcardMap<SeverityLevel>();
        Assert.False(map.TryGetValue("Rule1", out _));

        map = new WildcardMap<SeverityLevel>(new Dictionary<string, SeverityLevel>
        {
            { "Rule1", SeverityLevel.Warning },
            { "Rule2", SeverityLevel.Error },
            { "Rules.1", SeverityLevel.Error },
            { "Rules.*", SeverityLevel.Information },
            { "Rules.z*", SeverityLevel.Warning },
            { "Rules.2", SeverityLevel.Error },
        });

        Assert.True(map.TryGetValue("Rule1", out var level));
        Assert.Equal(SeverityLevel.Warning, level);
        Assert.True(map.TryGetValue("Rule2", out level));
        Assert.Equal(SeverityLevel.Error, level);
        Assert.True(map.TryGetValue("Rules.1", out level));
        Assert.Equal(SeverityLevel.Error, level);
        Assert.True(map.TryGetValue("Rules.2", out level));
        Assert.Equal(SeverityLevel.Error, level);
        Assert.True(map.TryGetValue("Rules.3", out level));
        Assert.Equal(SeverityLevel.Information, level);
        Assert.True(map.TryGetValue("Rules.zzzz", out level));
        Assert.Equal(SeverityLevel.Warning, level);
        Assert.False(map.TryGetValue("Rules.", out _));
        Assert.False(map.TryGetValue("Rules", out _));
    }
}
