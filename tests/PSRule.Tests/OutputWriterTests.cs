// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Collections;
using System.IO;
using System.Linq;
using System.Management.Automation;
using System.Xml;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using PSRule.Configuration;
using PSRule.Definitions;
using PSRule.Definitions.Rules;
using PSRule.Pipeline;
using PSRule.Pipeline.Output;
using PSRule.Rules;

namespace PSRule;

public sealed class OutputWriterTests
{
    [Fact]
    public void Sarif()
    {
        var option = GetOption();
        option.Output.SarifProblemsOnly = false;
        option.Repository.Url = "https://github.com/microsoft/PSRule.UnitTest";
        var output = new TestWriter(option);
        var result = new InvokeResult();
        result.Add(GetPass());
        result.Add(GetFail());
        result.Add(GetFail("rid-003", SeverityLevel.Warning));
        result.Add(GetFail("rid-004", SeverityLevel.Information));
        var writer = new SarifOutputWriter(null, output, option, null);
        writer.Begin();
        writer.WriteObject(result, false);
        writer.End(new DefaultPipelineResult(null, Options.BreakLevel.None));

        var actual = JsonConvert.DeserializeObject<JObject>(output.Output.OfType<string>().FirstOrDefault());
        Assert.NotNull(actual);
        Assert.Equal("PSRule", actual["runs"][0]["tool"]["driver"]["name"].Value<string>());
        Assert.Equal("0.0.1", actual["runs"][0]["tool"]["driver"]["semanticVersion"].Value<string>().Split('+')[0]);
        Assert.Equal("https://github.com/microsoft/PSRule.UnitTest", actual["runs"][0]["versionControlProvenance"][0]["repositoryUri"].Value<string>());

        // Pass
        Assert.Equal("TestModule\\rule-001", actual["runs"][0]["results"][0]["ruleId"].Value<string>());
        Assert.Equal("none", actual["runs"][0]["results"][0]["level"].Value<string>());

        // Fail with error
        Assert.Equal("rid-002", actual["runs"][0]["results"][1]["ruleId"].Value<string>());
        Assert.Equal("error", actual["runs"][0]["results"][1]["level"].Value<string>());
        Assert.Equal("Custom annotation", actual["runs"][0]["results"][1]["properties"]["annotations"]["annotation-data"].Value<string>());
        Assert.Equal("Custom field data", actual["runs"][0]["results"][1]["properties"]["fields"]["field-data"].Value<string>());

        // Fail with warning
        Assert.Equal("rid-003", actual["runs"][0]["results"][2]["ruleId"].Value<string>());
        Assert.Null(actual["runs"][0]["results"][2]["level"]);

        // Fail with note
        Assert.Equal("rid-004", actual["runs"][0]["results"][3]["ruleId"].Value<string>());
        Assert.Equal("note", actual["runs"][0]["results"][3]["level"].Value<string>());

        // Check options
        Assert.Equal(option.Repository.Url, actual["runs"][0]["properties"]["options"]["workspace"]["Repository"]["Url"].Value<string>());
        Assert.False(actual["runs"][0]["properties"]["options"]["workspace"]["Output"]["SarifProblemsOnly"].Value<bool>());
    }

    [Fact]
    public void SarifProblemsOnly()
    {
        var option = GetOption();
        var output = new TestWriter(option);
        var result = new InvokeResult();
        result.Add(GetPass());
        result.Add(GetFail());
        result.Add(GetFail("rid-003", SeverityLevel.Warning));
        result.Add(GetFail("rid-004", SeverityLevel.Information));
        var writer = new SarifOutputWriter(null, output, option, null);
        writer.Begin();
        writer.WriteObject(result, false);
        writer.End(new DefaultPipelineResult(null, Options.BreakLevel.None));

        var actual = JsonConvert.DeserializeObject<JObject>(output.Output.OfType<string>().FirstOrDefault());
        Assert.NotNull(actual);
        Assert.Equal("PSRule", actual["runs"][0]["tool"]["driver"]["name"].Value<string>());
        Assert.Equal("0.0.1", actual["runs"][0]["tool"]["driver"]["semanticVersion"].Value<string>().Split('+')[0]);

        // Fail with error
        Assert.Equal("rid-002", actual["runs"][0]["results"][0]["ruleId"].Value<string>());
        Assert.Equal("error", actual["runs"][0]["results"][0]["level"].Value<string>());

        // Fail with warning
        Assert.Equal("rid-003", actual["runs"][0]["results"][1]["ruleId"].Value<string>());
        Assert.Null(actual["runs"][0]["results"][1]["level"]);

        // Fail with note
        Assert.Equal("rid-004", actual["runs"][0]["results"][2]["ruleId"].Value<string>());
        Assert.Equal("note", actual["runs"][0]["results"][2]["level"].Value<string>());
    }

    [Fact]
    public void Yaml()
    {
        var option = GetOption();
        option.Repository.Url = "https://github.com/microsoft/PSRule.UnitTest";
        var output = new TestWriter(option);
        var result = new InvokeResult();
        result.Add(GetPass());
        result.Add(GetFail());
        result.Add(GetFail("rid-003", SeverityLevel.Warning));
        result.Add(GetFail("rid-004", SeverityLevel.Information));
        var writer = new YamlOutputWriter(output, option, null);
        writer.Begin();
        writer.WriteObject(result, false);
        writer.End(new DefaultPipelineResult(null, Options.BreakLevel.None));

        Assert.Equal(@"- detail:
    reason: []
  info:
    moduleName: TestModule
    recommendation: >-
      Recommendation for rule 001

      over two lines.
  level: Error
  outcome: Pass
  outcomeReason: Processed
  ruleName: rule-001
  runId: run-001
  source: []
  tag: {}
  targetName: TestObject1
  targetType: TestType
  time: 500
- detail:
    reason: []
  field:
    field-data: Custom field data
  info:
    annotations:
      annotation-data: Custom annotation
    moduleName: TestModule
    recommendation: Recommendation for rule 002
  level: Error
  outcome: Fail
  outcomeReason: Processed
  ref: rid-002
  ruleName: rule-002
  runId: run-001
  source: []
  tag: {}
  targetName: TestObject1
  targetType: TestType
  time: 1000
- detail:
    reason: []
  field:
    field-data: Custom field data
  info:
    annotations:
      annotation-data: Custom annotation
    moduleName: TestModule
    recommendation: Recommendation for rule 002
  level: Warning
  outcome: Fail
  outcomeReason: Processed
  ref: rid-003
  ruleName: rule-002
  runId: run-001
  source: []
  tag: {}
  targetName: TestObject1
  targetType: TestType
  time: 1000
- detail:
    reason: []
  field:
    field-data: Custom field data
  info:
    annotations:
      annotation-data: Custom annotation
    moduleName: TestModule
    recommendation: Recommendation for rule 002
  level: Information
  outcome: Fail
  outcomeReason: Processed
  ref: rid-004
  ruleName: rule-002
  runId: run-001
  source: []
  tag: {}
  targetName: TestObject1
  targetType: TestType
  time: 1000
", output.Output.OfType<string>().FirstOrDefault());
    }

    [Fact]
    public void Json()
    {
        var option = GetOption();
        option.Output.JsonIndent = 2;
        option.Repository.Url = "https://github.com/microsoft/PSRule.UnitTest";
        var output = new TestWriter(option);
        var result = new InvokeResult();
        result.Add(GetPass());
        result.Add(GetFail());
        result.Add(GetFail("rid-003", SeverityLevel.Warning));
        result.Add(GetFail("rid-004", SeverityLevel.Information));
        var writer = new JsonOutputWriter(output, option, null);
        writer.Begin();
        writer.WriteObject(result, false);
        writer.End(new DefaultPipelineResult(null, Options.BreakLevel.None));

        Assert.Equal(@"[
  {
    ""detail"": {},
    ""info"": {
      ""displayName"": ""Rule 001"",
      ""moduleName"": ""TestModule"",
      ""name"": ""rule-001"",
      ""recommendation"": ""Recommendation for rule 001\r\nover two lines."",
      ""synopsis"": ""This is rule 001.""
    },
    ""level"": ""Error"",
    ""outcome"": ""Pass"",
    ""outcomeReason"": ""Processed"",
    ""ruleName"": ""rule-001"",
    ""runId"": ""run-001"",
    ""source"": [],
    ""tag"": {},
    ""targetName"": ""TestObject1"",
    ""targetType"": ""TestType"",
    ""time"": 500
  },
  {
    ""detail"": {},
    ""field"": {
      ""field-data"": ""Custom field data""
    },
    ""info"": {
      ""annotations"": {
        ""annotation-data"": ""Custom annotation""
      },
      ""displayName"": ""Rule 002"",
      ""moduleName"": ""TestModule"",
      ""name"": ""rule-002"",
      ""recommendation"": ""Recommendation for rule 002"",
      ""synopsis"": ""This is rule 002.""
    },
    ""level"": ""Error"",
    ""outcome"": ""Fail"",
    ""outcomeReason"": ""Processed"",
    ""ref"": ""rid-002"",
    ""ruleName"": ""rule-002"",
    ""runId"": ""run-001"",
    ""source"": [],
    ""tag"": {},
    ""targetName"": ""TestObject1"",
    ""targetType"": ""TestType"",
    ""time"": 1000
  },
  {
    ""detail"": {},
    ""field"": {
      ""field-data"": ""Custom field data""
    },
    ""info"": {
      ""annotations"": {
        ""annotation-data"": ""Custom annotation""
      },
      ""displayName"": ""Rule 002"",
      ""moduleName"": ""TestModule"",
      ""name"": ""rule-002"",
      ""recommendation"": ""Recommendation for rule 002"",
      ""synopsis"": ""This is rule 002.""
    },
    ""level"": ""Warning"",
    ""outcome"": ""Fail"",
    ""outcomeReason"": ""Processed"",
    ""ref"": ""rid-003"",
    ""ruleName"": ""rule-002"",
    ""runId"": ""run-001"",
    ""source"": [],
    ""tag"": {},
    ""targetName"": ""TestObject1"",
    ""targetType"": ""TestType"",
    ""time"": 1000
  },
  {
    ""detail"": {},
    ""field"": {
      ""field-data"": ""Custom field data""
    },
    ""info"": {
      ""annotations"": {
        ""annotation-data"": ""Custom annotation""
      },
      ""displayName"": ""Rule 002"",
      ""moduleName"": ""TestModule"",
      ""name"": ""rule-002"",
      ""recommendation"": ""Recommendation for rule 002"",
      ""synopsis"": ""This is rule 002.""
    },
    ""level"": ""Information"",
    ""outcome"": ""Fail"",
    ""outcomeReason"": ""Processed"",
    ""ref"": ""rid-004"",
    ""ruleName"": ""rule-002"",
    ""runId"": ""run-001"",
    ""source"": [],
    ""tag"": {},
    ""targetName"": ""TestObject1"",
    ""targetType"": ""TestType"",
    ""time"": 1000
  }
]", output.Output.OfType<string>().FirstOrDefault());
    }

    [Fact]
    public void NUnit3()
    {
        var option = GetOption();
        option.Repository.Url = "https://github.com/microsoft/PSRule.UnitTest";
        var output = new TestWriter(option);
        var result = new InvokeResult();
        result.Add(GetPass());
        result.Add(GetFail());
        result.Add(GetFail("rid-003", SeverityLevel.Warning));
        result.Add(GetFail("rid-004", SeverityLevel.Information, "Synopsis \"with quotes\"."));
        var writer = new NUnit3OutputWriter(output, option, null);
        writer.Begin();
        writer.WriteObject(result, false);
        writer.End(new DefaultPipelineResult(null, Options.BreakLevel.None));

        var s = output.Output.OfType<string>().FirstOrDefault();
        var doc = new XmlDocument();
        doc.LoadXml(s);

        var declaration = doc.ChildNodes.Item(0) as XmlDeclaration;
        Assert.Equal("utf-8", declaration.Encoding);
        var xml = doc["test-results"]["test-suite"].OuterXml.Replace(System.Environment.NewLine, "\r\n");
        Assert.Equal("<test-suite type=\"TestFixture\" name=\"TestObject1\" executed=\"True\" result=\"Failure\" success=\"False\" time=\"3.5\" asserts=\"3\" description=\"\"><results><test-case description=\"This is rule 001.\" name=\"TestObject1 -- rule-001\" time=\"0.5\" asserts=\"0\" success=\"True\" result=\"Success\" executed=\"True\" /><test-case description=\"This is rule 002.\" name=\"TestObject1 -- rule-002\" time=\"1\" asserts=\"0\" success=\"False\" result=\"Failure\" executed=\"True\"><failure><message><![CDATA[Recommendation for rule 002\r\n]]></message><stack-trace><![CDATA[]]></stack-trace></failure></test-case><test-case description=\"This is rule 002.\" name=\"TestObject1 -- rule-002\" time=\"1\" asserts=\"0\" success=\"False\" result=\"Failure\" executed=\"True\"><failure><message><![CDATA[Recommendation for rule 002\r\n]]></message><stack-trace><![CDATA[]]></stack-trace></failure></test-case><test-case description=\"Synopsis &quot;with quotes&quot;.\" name=\"TestObject1 -- rule-002\" time=\"1\" asserts=\"0\" success=\"False\" result=\"Failure\" executed=\"True\"><failure><message><![CDATA[Recommendation for rule 002\r\n]]></message><stack-trace><![CDATA[]]></stack-trace></failure></test-case></results></test-suite>", xml);
    }

    [Fact]
    public void Csv()
    {
        var option = GetOption();
        option.Repository.Url = "https://github.com/microsoft/PSRule.UnitTest";
        var output = new TestWriter(option);
        var result = new InvokeResult();
        result.Add(GetPass());
        result.Add(GetFail());
        result.Add(GetFail("rid-003", SeverityLevel.Warning));
        result.Add(GetFail("rid-004", SeverityLevel.Information));
        var writer = new CsvOutputWriter(output, option, null);
        writer.Begin();
        writer.WriteObject(result, false);
        writer.End(new DefaultPipelineResult(null, Options.BreakLevel.None));

        var actual = output.Output.OfType<string>().FirstOrDefault();

        Assert.Equal(@"RuleName,TargetName,TargetType,Outcome,Synopsis,Recommendation
""rule-001"",""TestObject1"",""TestType"",""Pass"",""Processed"",""This is rule 001."",""Recommendation for rule 001 over two lines.""
""rule-002"",""TestObject1"",""TestType"",""Fail"",""Processed"",""This is rule 002."",""Recommendation for rule 002""
""rule-002"",""TestObject1"",""TestType"",""Fail"",""Processed"",""This is rule 002."",""Recommendation for rule 002""
""rule-002"",""TestObject1"",""TestType"",""Fail"",""Processed"",""This is rule 002."",""Recommendation for rule 002""
", actual);
    }

    [Fact]
    public void JobSummary()
    {
        using var stream = new MemoryStream();
        var option = GetOption();
        var output = new TestWriter(option);
        var result = new InvokeResult();
        var context = PipelineContext.New(GetOption(), null, null, null, null, null, new OptionContextBuilder(), null);
        result.Add(GetPass());
        result.Add(GetFail());
        result.Add(GetFail("rid-003", SeverityLevel.Warning, ruleId: "TestModule\\Rule-003"));
        var writer = new JobSummaryWriter(output, option, null, outputPath: "reports/summary.md", stream: stream);
        writer.Begin();
        writer.WriteObject(result, false);
        context.RunTime.Stop();
        writer.End(new DefaultPipelineResult(null, Options.BreakLevel.None));

        stream.Seek(0, SeekOrigin.Begin);
        using var reader = new StreamReader(stream);
        var s = reader.ReadToEnd().Replace(System.Environment.NewLine, "\r\n");
        Assert.Equal($"# PSRule result summary\r\n\r\n❌ PSRule completed with an overall result of 'Fail' with 3 rule(s) and 1 target(s) in {context.RunTime.Elapsed}.\r\n\r\n## Analysis\r\n\r\nThe following results were reported with fail or error results.\r\n\r\nName | Target name | Synopsis\r\n---- | ----------- | --------\r\nrule-002 | TestObject1 | This is rule 002.\r\nRule-003 | TestObject1 | This is rule 002.\r\n", s);
    }

    #region Helper methods

    private static RuleRecord GetPass()
    {
        return new RuleRecord
        (
            runId: "run-001",
            ruleId: ResourceId.Parse("TestModule\\rule-001"),
            @ref: null,
            targetObject: new TargetObject(new PSObject()),
            targetName: "TestObject1",
            targetType: "TestType",
            tag: new ResourceTags(),
            info: new RuleHelpInfo
            (
                "rule-001",
                "Rule 001",
                "TestModule",
                synopsis: new InfoString("This is rule 001."),
                recommendation: new InfoString("Recommendation for rule 001\r\nover two lines.")
            ),
            field: new Hashtable(),
            @default: new RuleProperties
            {
                Level = SeverityLevel.Error
            },
            extent: null,
            outcome: RuleOutcome.Pass,
            reason: RuleOutcomeReason.Processed
        )
        {
            Time = 500
        };
    }

    private static RuleRecord GetFail(string ruleRef = "rid-002", SeverityLevel level = SeverityLevel.Error, string synopsis = "This is rule 002.", string ruleId = "TestModule\\rule-002")
    {
        var info = new RuleHelpInfo(
            "rule-002",
            "Rule 002",
            "TestModule",
            synopsis: new InfoString(synopsis),
            recommendation: new InfoString("Recommendation for rule 002")
        );
        info.Annotations = new Hashtable
        {
            ["annotation-data"] = "Custom annotation"
        };
        return new RuleRecord(
            runId: "run-001",
            ruleId: ResourceId.Parse(ruleId),
            @ref: ruleRef,
            targetObject: new TargetObject(new PSObject()),
            targetName: "TestObject1",
            targetType: "TestType",
            tag: new ResourceTags(),
            info: info,
            field: new Hashtable
            {
                ["field-data"] = "Custom field data"
            },
            @default: new RuleProperties
            {
                Level = level
            },
            extent: null,
            outcome: RuleOutcome.Fail,
            reason: RuleOutcomeReason.Processed
        )
        {
            Time = 1000
        };
    }

    private static PSRuleOption GetOption()
    {
        return new PSRuleOption();
    }

    #endregion Helper methods
}
