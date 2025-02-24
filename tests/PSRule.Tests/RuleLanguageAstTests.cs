// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using PSRule.Host;

namespace PSRule;

public sealed class RuleLanguageAstTests : BaseTests
{
    [Fact]
    public void RuleName()
    {
        var scriptAst = System.Management.Automation.Language.Parser.ParseFile(GetSourcePath("FromFileName.Rule.ps1"), out _, out _);
        var visitor = new RuleLanguageAst();
        scriptAst.Visit(visitor);

        Assert.Equal("PSRule.Parse.InvalidResourceName", visitor.Errors[0].FullyQualifiedErrorId);
    }
}
