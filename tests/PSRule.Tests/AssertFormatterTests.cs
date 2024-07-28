// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Management.Automation;
using PSRule.Configuration;
using PSRule.Definitions;
using PSRule.Definitions.Rules;
using PSRule.Pipeline;
using PSRule.Pipeline.Formatters;
using PSRule.Rules;

namespace PSRule;

/// <summary>
/// Tests for formatters used by assert pipeline.
/// </summary>
public sealed class AssertFormatterTests
{
    [Fact]
    public void Plain()
    {
        var option = GetOption();
        option.Output.Banner = BannerFormat.None;
        option.Output.Footer = FooterFormat.None;
        var writer = GetWriter();

        // Check output is empty
        var formatter = new PlainFormatter(null, writer, option);
        formatter.Begin();
        formatter.End(0, 0, 0);
        Assert.Equal("", writer.Output);

        // Check pass output
        writer.Clear();
        formatter = new PlainFormatter(null, writer, option);
        formatter.Begin();
        formatter.Result(GetPassResult());
        formatter.End(0, 0, 0);
        Assert.Equal(@" -> TestObject : TestType [1/1]

    [PASS] Test
", writer.Output);

        // Check fail output
        writer.Clear();
        formatter = new PlainFormatter(null, writer, option);
        formatter.Begin();
        formatter.Result(GetFailResult());
        formatter.End(0, 0, 0);
        Assert.Equal(@" -> TestObject : TestType [0/2]

    [FAIL] Test1

    [FAIL] Test2

", writer.Output);

        // Check fail output as warning
        writer.Clear();
        formatter = new PlainFormatter(null, writer, option);
        formatter.Begin();
        formatter.Result(GetFailResult(SeverityLevel.Warning));
        formatter.End(0, 0, 0);
        Assert.Equal(@" -> TestObject : TestType [0/2]

    [FAIL] Test1

    [FAIL] Test2

", writer.Output);

        // Check fail output as information
        writer.Clear();
        formatter = new PlainFormatter(null, writer, option);
        formatter.Begin();
        formatter.Result(GetFailResult(SeverityLevel.Information));
        formatter.End(0, 0, 0);
        Assert.Equal(@" -> TestObject : TestType [0/2]

    [FAIL] Test1

    [FAIL] Test2

", writer.Output);
    }

    [Fact]
    public void Client()
    {
        var option = GetOption();
        option.Output.Banner = BannerFormat.None;
        option.Output.Footer = FooterFormat.None;
        var writer = GetWriter();

        // Check output is empty
        var formatter = new ClientFormatter(null, writer, option);
        formatter.Begin();
        formatter.End(0, 0, 0);
        Assert.Equal("", writer.Output);

        // Check pass output
        writer.Clear();
        formatter = new ClientFormatter(null, writer, option);
        formatter.Begin();
        formatter.Result(GetPassResult());
        formatter.End(0, 0, 0);
        Assert.Equal(@" -> TestObject : TestType [1/1]

    [PASS] Test
", writer.Output);

        // Check fail output
        writer.Clear();
        formatter = new ClientFormatter(null, writer, option);
        formatter.Begin();
        formatter.Result(GetFailResult());
        formatter.End(0, 0, 0);
        Assert.Equal(@" -> TestObject : TestType [0/2]

    [FAIL] Test1

    [FAIL] Test2

", writer.Output);

        // Check fail output as warning
        writer.Clear();
        formatter = new ClientFormatter(null, writer, option);
        formatter.Begin();
        formatter.Result(GetFailResult(SeverityLevel.Warning));
        formatter.End(0, 0, 0);
        Assert.Equal(@" -> TestObject : TestType [0/2]

    [FAIL] Test1

    [FAIL] Test2

", writer.Output);

        // Check fail output as information
        writer.Clear();
        formatter = new ClientFormatter(null, writer, option);
        formatter.Begin();
        formatter.Result(GetFailResult(SeverityLevel.Information));
        formatter.End(0, 0, 0);
        Assert.Equal(@" -> TestObject : TestType [0/2]

    [FAIL] Test1

    [FAIL] Test2

", writer.Output);
    }

    [Fact]
    public void AzurePipelines()
    {
        var option = GetOption();
        option.Output.Banner = BannerFormat.None;
        option.Output.Footer = FooterFormat.None;
        var writer = GetWriter();

        // Check output is empty
        var formatter = new AzurePipelinesFormatter(null, writer, option);
        formatter.Begin();
        formatter.End(0, 0, 0);
        Assert.Equal("", writer.Output);

        // Check pass output
        writer.Clear();
        formatter = new AzurePipelinesFormatter(null, writer, option);
        formatter.Begin();
        formatter.Result(GetPassResult());
        formatter.End(0, 0, 0);
        Assert.Equal(@" -> TestObject : TestType [1/1]

    [PASS] Test
", writer.Output);

        // Check fail output
        writer.Clear();
        formatter = new AzurePipelinesFormatter(null, writer, option);
        formatter.Begin();
        formatter.Result(GetFailResult());
        formatter.End(0, 0, 0);
        Assert.Equal(@" -> TestObject : TestType [0/2]

    [FAIL] Test1

##vso[task.logissue type=error]TestObject failed Test1. 

    [FAIL] Test2

##vso[task.logissue type=error]TestObject failed Test2. 

", writer.Output);

        // Check fail output as warning
        writer.Clear();
        formatter = new AzurePipelinesFormatter(null, writer, option);
        formatter.Begin();
        formatter.Result(GetFailResult(SeverityLevel.Warning));
        formatter.End(0, 0, 0);
        Assert.Equal(@" -> TestObject : TestType [0/2]

    [FAIL] Test1

##vso[task.logissue type=warning]TestObject failed Test1. 

    [FAIL] Test2

##vso[task.logissue type=warning]TestObject failed Test2. 

", writer.Output);

        // Check fail output as information
        writer.Clear();
        formatter = new AzurePipelinesFormatter(null, writer, option);
        formatter.Begin();
        formatter.Result(GetFailResult(SeverityLevel.Information));
        formatter.End(0, 0, 0);
        Assert.Equal(@" -> TestObject : TestType [0/2]

    [FAIL] Test1

    [FAIL] Test2

", writer.Output);
    }

    [Fact]
    public void GitHubActions()
    {
        var option = GetOption();
        option.Output.Banner = BannerFormat.None;
        option.Output.Footer = FooterFormat.None;
        var writer = GetWriter();

        // Check output is empty
        var formatter = new GitHubActionsFormatter(null, writer, option);
        formatter.Begin();
        formatter.End(0, 0, 0);
        Assert.Equal("", writer.Output);

        // Check pass output
        writer.Clear();
        formatter = new GitHubActionsFormatter(null, writer, option);
        formatter.Begin();
        formatter.Result(GetPassResult());
        formatter.End(0, 0, 0);
        Assert.Equal(@" -> TestObject : TestType [1/1]

    [PASS] Test
", writer.Output);

        // Check fail output
        writer.Clear();
        formatter = new GitHubActionsFormatter(null, writer, option);
        formatter.Begin();
        formatter.Result(GetFailResult());
        formatter.End(0, 0, 0);
        Assert.Equal(@" -> TestObject : TestType [0/2]

    [FAIL] Test1

::error::TestObject failed Test1. 

    [FAIL] Test2

::error::TestObject failed Test2. 

", writer.Output);

        // Check fail output as warning
        writer.Clear();
        formatter = new GitHubActionsFormatter(null, writer, option);
        formatter.Begin();
        formatter.Result(GetFailResult(SeverityLevel.Warning));
        formatter.End(0, 0, 0);
        Assert.Equal(@" -> TestObject : TestType [0/2]

    [FAIL] Test1

::warning::TestObject failed Test1. 

    [FAIL] Test2

::warning::TestObject failed Test2. 

", writer.Output);

        // Check fail output as information
        writer.Clear();
        formatter = new GitHubActionsFormatter(null, writer, option);
        formatter.Begin();
        formatter.Result(GetFailResult(SeverityLevel.Information));
        formatter.End(0, 0, 0);
        Assert.Equal(@" -> TestObject : TestType [0/2]

    [FAIL] Test1

::notice::TestObject failed Test1. 

    [FAIL] Test2

::notice::TestObject failed Test2. 

", writer.Output);
    }

    [Fact]
    public void VisualStudioCode()
    {
        var option = GetOption();
        option.Output.Banner = BannerFormat.None;
        option.Output.Footer = FooterFormat.None;
        var writer = GetWriter();

        // Check output is empty
        var formatter = new VisualStudioCodeFormatter(null, writer, option);
        formatter.Begin();
        formatter.End(0, 0, 0);
        Assert.Equal("", writer.Output);

        // Check pass output
        writer.Clear();
        formatter = new VisualStudioCodeFormatter(null, writer, option);
        formatter.Begin();
        formatter.Result(GetPassResult());
        formatter.End(0, 0, 0);
        Assert.Equal(@"> TestObject : TestType [1/1]

   PASS  Test
", writer.Output);

        // Check fail output
        writer.Clear();
        formatter = new VisualStudioCodeFormatter(null, writer, option);
        formatter.Begin();
        formatter.Result(GetFailResult());
        formatter.End(0, 0, 0);
        Assert.Equal(@"> TestObject : TestType [0/2]

   FAIL  Test1

   FAIL  Test2

", writer.Output);

        // Check fail output as warning
        writer.Clear();
        formatter = new VisualStudioCodeFormatter(null, writer, option);
        formatter.Begin();
        formatter.Result(GetFailResult(SeverityLevel.Warning));
        formatter.End(0, 0, 0);
        Assert.Equal(@"> TestObject : TestType [0/2]

   FAIL  Test1

   FAIL  Test2

", writer.Output);

        // Check fail output as information
        writer.Clear();
        formatter = new VisualStudioCodeFormatter(null, writer, option);
        formatter.Begin();
        formatter.Result(GetFailResult(SeverityLevel.Information));
        formatter.End(0, 0, 0);
        Assert.Equal(@"> TestObject : TestType [0/2]

   FAIL  Test1

   FAIL  Test2

", writer.Output);
    }

    #region Helper methods

    private static InvokeResult GetPassResult()
    {
        var result = new InvokeResult();
        result.Add(new RuleRecord
        (
            runId: "run-001",
            ruleId: ResourceId.Parse(".\\Test"),
            @ref: "",
            targetObject: new TargetObject(new PSObject()),
            targetName: "TestObject",
            targetType: "TestType",
            tag: new ResourceTags(),
            info: new RuleHelpInfo("Test", "Test rule", null),
            field: null,
            @default: new RuleProperties
            {
                Level = SeverityLevel.Error
            },
            extent: null,
            outcome: RuleOutcome.Pass,
            reason: RuleOutcomeReason.Processed
        ));
        return result;
    }

    private static InvokeResult GetFailResult(SeverityLevel level = SeverityLevel.Error)
    {
        var result = new InvokeResult();
        result.Add(new RuleRecord
        (
            runId: "run-001",
            ruleId: ResourceId.Parse(".\\Test1"),
            @ref: "",
            targetObject: new TargetObject(new PSObject()),
            targetName: "TestObject",
            targetType: "TestType",
            tag: new ResourceTags(),
            info: new RuleHelpInfo("Test1", "Test rule", null),
            field: null,
            @default: new RuleProperties
            {
                Level = level
            },
            extent: null,
            outcome: RuleOutcome.Fail,
            reason: RuleOutcomeReason.Processed
        ));
        result.Add(new RuleRecord
        (
            runId: "run-001",
            ruleId: ResourceId.Parse(".\\Test2"),
            @ref: "",
            targetObject: new TargetObject(new PSObject()),
            targetName: "TestObject",
            targetType: "TestType",
            tag: new ResourceTags(),
            info: new RuleHelpInfo("Test2", "Test rule", null),
            field: null,
            @default: new RuleProperties
            {
                Level = level
            },
            extent: null,
            outcome: RuleOutcome.Fail,
            reason: RuleOutcomeReason.Processed
        ));
        return result;
    }

    private static PSRuleOption GetOption()
    {
        return PSRuleOption.FromDefault();
    }

    private static TestAssertWriter GetWriter()
    {
        return new TestAssertWriter(GetOption());
    }

    #endregion Helper methods
}
