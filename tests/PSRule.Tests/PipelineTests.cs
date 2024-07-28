// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Management.Automation;
using System.Threading;
using Newtonsoft.Json.Linq;
using PSRule.Configuration;
using PSRule.Definitions.Rules;
using PSRule.Options;
using PSRule.Pipeline;
using PSRule.Resources;
using PSRule.Rules;

namespace PSRule;

public sealed class PipelineTests
{
    internal class TestObject
    {
        public string Name { get; set; }
    }

    [Fact]
    public void BuildInvokePipeline()
    {
        var option = GetOption();
        var builder = PipelineBuilder.Invoke(GetSource(), option, null);
        Assert.NotNull(builder.Build());
    }

    [Fact]
    public void InvokePipeline()
    {
        var testObject1 = new TestObject { Name = "TestObject1" };
        var option = GetOption();
        option.Rule.Include = ["FromFile1"];
        var builder = PipelineBuilder.Invoke(GetSource(), option, null);
        var pipeline = builder.Build();

        Assert.NotNull(pipeline);
        pipeline.Begin();
        for (var i = 0; i < 100; i++)
            pipeline.Process(PSObject.AsPSObject(testObject1));

        pipeline.End();
    }

    [Fact]
    public void InvokePipelineWithJObject()
    {
        var parent = new JObject
        {
            ["resources"] = new JArray(new object[] {
                new JObject
                {
                    ["Name"] = "TestValue"
                },
                new JObject
                {
                    ["Name"] = "TestValue2"
                }
            })
        };

        var option = GetOption();
        option.Rule.Include = ["ScriptReasonTest"];
        //option.Input.Format = InputFormat.File;
        var builder = PipelineBuilder.Invoke(GetSource(), option, null);
        var writer = new TestWriter(option);
        var pipeline = builder.Build(writer);

        Assert.NotNull(pipeline);
        pipeline.Begin();
        pipeline.Process(PSObject.AsPSObject(parent["resources"][0]));
        pipeline.Process(PSObject.AsPSObject(parent["resources"][1]));
        pipeline.End();

        var actual = (writer.Output[0] as InvokeResult).AsRecord().FirstOrDefault();
        Assert.Equal(RuleOutcome.Pass, actual.Outcome);
        Assert.Equal(SeverityLevel.Error, actual.Default.Level);
        Assert.Equal(SeverityLevel.Warning, actual.Override.Level);
        Assert.Equal(SeverityLevel.Warning, actual.Level);

        actual = (writer.Output[1] as InvokeResult).AsRecord().FirstOrDefault();
        Assert.Equal(RuleOutcome.Fail, actual.Outcome);
        Assert.Equal(SeverityLevel.Error, actual.Default.Level);
        Assert.Equal(SeverityLevel.Warning, actual.Override.Level);
        Assert.Equal(SeverityLevel.Warning, actual.Level);
        Assert.Equal("Name", actual.Detail.Reason.First().Path);
        Assert.Equal("resources[1].Name", actual.Detail.Reason.First().FullPath);
    }

    [Fact]
    public void InvokePipelineWithPathPrefix()
    {
        var parent = new JObject
        {
            ["resources"] = new JArray(new object[] {
                new JObject
                {
                    ["Name"] = "TestValue"
                },
                new JObject
                {
                    ["Name"] = "TestValue2"
                }
            })
        };

        var option = GetOption();
        option.Rule.Include = ["WithPathPrefix"];
        var builder = PipelineBuilder.Invoke(GetSource(), option, null);
        var writer = new TestWriter(option);
        var pipeline = builder.Build(writer);

        Assert.NotNull(pipeline);
        pipeline.Begin();
        pipeline.Process(PSObject.AsPSObject(parent["resources"][0]));
        pipeline.Process(PSObject.AsPSObject(parent["resources"][1]));
        pipeline.End();

        var actual = (writer.Output[0] as InvokeResult).AsRecord().FirstOrDefault();
        Assert.Equal(RuleOutcome.Pass, actual.Outcome);

        actual = (writer.Output[1] as InvokeResult).AsRecord().FirstOrDefault();
        Assert.Equal(RuleOutcome.Fail, actual.Outcome);
        Assert.Equal("item.Name", actual.Detail.Reason.First().Path);
        Assert.Equal("resources[1].item.Name", actual.Detail.Reason.First().FullPath);
    }

    [Fact]
    public void InvokePipelineWithExclude()
    {
        var option = GetOption(ruleExcludedAction: ExecutionActionPreference.Warn);
        option.Rule.Include = ["FromFile1"];
        option.Rule.Exclude = ["FromFile2"];
        var builder = PipelineBuilder.Invoke(GetSource(), option, null);
        var writer = new TestWriter(GetOption());
        var pipeline = builder.Build(writer);

        Assert.NotNull(pipeline);
        pipeline.Begin();
        pipeline.End();

        Assert.Contains(writer.Warnings, (string s) => { return s == string.Format(Thread.CurrentThread.CurrentCulture, PSRuleResources.RuleExcluded, ".\\FromFile2"); });
    }

    [Fact]
    public void BuildGetPipeline()
    {
        var builder = PipelineBuilder.Get(GetSource(), GetOption(), null);
        Assert.NotNull(builder.Build());
    }

    [Fact]
    public void GetRuleWithBaseline()
    {
        var option = PSRuleOption.FromDefault();
        option.Baseline.Group = new Data.StringArrayMap();
        option.Baseline.Group["test"] = [".\\TestBaseline1"];
        option.Execution.InvariantCulture = ExecutionActionPreference.Ignore;
        Assert.Single(option.Baseline.Group);

        var builder = PipelineBuilder.Get(GetSource(
        [
            "Baseline.Rule.yaml",
            "FromFileBaseline.Rule.ps1"
        ]), option, null);
        builder.Baseline(Configuration.BaselineOption.FromString("@test"));
        var writer = new TestWriter(option);
        var pipeline = builder.Build(writer);

        pipeline.Begin();
        pipeline.Process(null);
        pipeline.End();

        Assert.Single(writer.Output);
    }

    [Fact]
    public void PipelineWithInvariantCulture()
    {
        Environment.UseCurrentCulture(CultureInfo.InvariantCulture);
        var context = PipelineContext.New(GetOption(), null, null, null, null, null, new OptionContextBuilder(), null);
        var writer = new TestWriter(GetOption());
        var pipeline = new GetRulePipeline(context, GetSource(), new PipelineInputStream(null, null, null, null), writer, false);
        try
        {
            pipeline.Begin();
            pipeline.Process(null);
            pipeline.End();
            Assert.Contains(writer.Warnings, (string s) => { return s == PSRuleResources.UsingInvariantCulture; });
        }
        finally
        {
            Environment.UseCurrentCulture();
        }
    }

    [Fact]
    public void PipelineWithInvariantCultureDisabled()
    {
        Environment.UseCurrentCulture(CultureInfo.InvariantCulture);
        var option = new PSRuleOption();
        option.Execution.InvariantCulture = ExecutionActionPreference.Ignore;
        var context = PipelineContext.New(option, null, null, null, null, null, new OptionContextBuilder(), null);
        var writer = new TestWriter(option);
        var pipeline = new GetRulePipeline(context, GetSource(), new PipelineInputStream(null, null, null, null), writer, false);
        try
        {
            pipeline.Begin();
            pipeline.Process(null);
            pipeline.End();
            Assert.DoesNotContain(writer.Warnings, (string s) => { return s == PSRuleResources.UsingInvariantCulture; });
        }
        finally
        {
            Environment.UseCurrentCulture();
        }
    }

    [Fact]
    public void PipelineWithOptions()
    {
        var option = GetOption(GetSourcePath("PSRule.Tests.yml"));
        var builder = PipelineBuilder.Get(GetSource(), option, null);
        Assert.NotNull(builder.Build());
    }

    [Fact]
    public void PipelineWithRequires()
    {
        var option = GetOption(GetSourcePath("PSRule.Tests6.yml"));
        var builder = PipelineBuilder.Get(GetSource(), option, null);
        Assert.Null(builder.Build());
    }

    /// <summary>
    /// An Invoke pipeline reading from an input file.
    /// </summary>
    [Fact]
    public void PipelineWithSource()
    {
        var option = GetOption();
        option.Rule.Include = ["FromFile1"];
        option.Input.PathIgnore =
        [
            "**/ObjectFromFile*.json",
            "!**/ObjectFromFile.json"
        ];

        // Default
        var writer = new TestWriter(option);
        var builder = PipelineBuilder.Invoke(GetSource(), option, null);
        builder.InputPath(["./**/ObjectFromFile*.json"]);
        var pipeline = builder.Build(writer);
        Assert.NotNull(pipeline);
        pipeline.Begin();
        pipeline.Process(GetTestObject());
        pipeline.Process(GetFileObject());
        pipeline.End();

        var items = writer.Output.OfType<InvokeResult>().SelectMany(i => i.AsRecord()).ToArray();
        Assert.Equal(4, items.Length);
        Assert.True(items[0].HasSource());
        Assert.True(items[1].HasSource());
        Assert.True(items[2].HasSource());
        Assert.True(items[3].HasSource());

        // With reason full path
        option.Rule.Include = ["ScriptReasonTest"];
        writer = new TestWriter(option);
        builder = PipelineBuilder.Invoke(GetSource(), option, null);
        PipelineBuilder.Invoke(GetSource(), option, null);
        builder.InputPath(["./**/ObjectFromFile*.json"]);
        pipeline = builder.Build(writer);
        Assert.NotNull(pipeline);
        pipeline.Begin();
        pipeline.Process(GetTestObject());
        pipeline.Process(GetFileObject());
        pipeline.End();

        items = writer.Output.OfType<InvokeResult>().SelectMany(i => i.AsRecord()).ToArray();
        Assert.Equal(4, items.Length);
        //Assert.Equal("master.items[0].Name", items[1].Detail.Reason.First().FullPath);
        //Assert.Equal("Name", items[1].Detail.Reason.First().Path);
        Assert.Equal("[1].Name", items[1].Detail.Reason.First().FullPath);
        Assert.Equal("Name", items[1].Detail.Reason.First().Path);
        Assert.Equal("resources[0].Name", items[2].Detail.Reason.First().FullPath);
        Assert.Equal("Name", items[2].Detail.Reason.First().Path);
        Assert.Equal("Name", items[3].Detail.Reason.First().FullPath);
        Assert.Equal("Name", items[3].Detail.Reason.First().Path);

        // With IgnoreObjectSource
        option.Rule.Include = ["FromFile1"];
        option.Input.IgnoreObjectSource = true;
        writer = new TestWriter(option);
        builder = PipelineBuilder.Invoke(GetSource(), option, null);
        PipelineBuilder.Invoke(GetSource(), option, null);
        builder.InputPath(["./**/ObjectFromFile*.json"]);
        pipeline = builder.Build(writer);
        Assert.NotNull(pipeline);
        pipeline.Begin();
        pipeline.Process(GetTestObject());
        pipeline.Process(GetFileObject());
        pipeline.End();

        items = writer.Output.OfType<InvokeResult>().SelectMany(i => i.AsRecord()).ToArray();
        Assert.Equal(2, items.Length);
        Assert.True(items[0].HasSource());
        Assert.True(items[1].HasSource());
        //Assert.True(items[2].HasSource());
    }

    ///// <summary>
    ///// An Invoke pipeline reading from an input file with File format.
    ///// </summary>
    //[Fact]
    //public void PipelineWithFileFormat()
    //{
    //    var option = GetOption();
    //    option.Input.Format = InputFormat.File;
    //    option.Rule.Include = new string[] { "FromFile1" };
    //    var builder = PipelineBuilder.Invoke(GetSource(), option, null);
    //    builder.InputPath(new string[] { "./**/ObjectFromFile.json" });
    //    var writer = new TestWriter(option);
    //    var pipeline = builder.Build(writer);
    //    Assert.NotNull(pipeline);
    //    pipeline.Begin();
    //    pipeline.Process(null);
    //    pipeline.End();

    //    var items = writer.Output.OfType<InvokeResult>().SelectMany(i => i.AsRecord()).ToArray();
    //    Assert.Single(items);
    //    Assert.True(items[0].HasSource());
    //}

    #region Helper methods

    private static Source[] GetSource(string[] files = null)
    {
        var builder = new SourcePipelineBuilder(null, null);
        builder.Directory(GetSourcePath("FromFile.Rule.ps1"));
        for (var i = 0; files != null && i < files.Length; i++)
            builder.Directory(GetSourcePath(files[i]));

        return builder.Build();
    }

    private static PSRuleOption GetOption(string path = null, ExecutionActionPreference ruleExcludedAction = ExecutionActionPreference.None)
    {
        var option = path == null ? new PSRuleOption() : PSRuleOption.FromFile(path);
        option.Rule.IncludeLocal = false;
        option.Execution.RuleExcluded = ruleExcludedAction;
        option.Override.Level ??= new Data.EnumMap<SeverityLevel>();
        option.Override.Level.Add("ScriptReasonTest", SeverityLevel.Warning);
        return option;
    }

    private static string GetSourcePath(string fileName)
    {
        return Path.Combine(AppDomain.CurrentDomain.BaseDirectory, fileName);
    }

    private static PSObject GetTestObject()
    {
        var info = new PSObject();
        info.Properties.Add(new PSNoteProperty("path", "resources[0]"));
        var source = new PSObject();
        source.Properties.Add(new PSNoteProperty("file", Environment.GetRootedPath("./ObjectFromFileNotFile.json")));
        source.Properties.Add(new PSNoteProperty("type", "example"));
        info.Properties.Add(new PSNoteProperty("source", new PSObject[] { source }));
        var o = new PSObject();
        o.Properties.Add(new PSNoteProperty("name", "FromObjectTest"));
        o.Properties.Add(new PSNoteProperty("_PSRule", info));
        return o;
    }

    private static PSObject GetFileObject()
    {
        var info = new FileInfo(Environment.GetRootedPath("./ObjectFromFileSingle.json"));
        return new PSObject(info);
    }

    #endregion Helper methods
}
