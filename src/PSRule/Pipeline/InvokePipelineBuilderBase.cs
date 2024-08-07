// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using PSRule.Configuration;
using PSRule.Definitions;
using PSRule.Host;
using PSRule.Options;

namespace PSRule.Pipeline;

internal abstract class InvokePipelineBuilderBase : PipelineBuilderBase, IInvokePipelineBuilder
{
    protected InputPathBuilder _InputPath;
    protected string _ResultVariableName;

    private List<string> _TrustedPublishers;

    protected InvokePipelineBuilderBase(Source[] source, IHostContext hostContext)
        : base(source, hostContext)
    {
        _InputPath = null;
    }

    public void InputPath(string[] path)
    {
        if (path == null || path.Length == 0)
            return;

        PathFilter required = null;
        if (TryChangedFiles(out var files))
        {
            required = PathFilter.Create(Environment.GetWorkingPath(), path);
            path = files;
        }

        var builder = new InputPathBuilder(PrepareWriter(), Environment.GetWorkingPath(), "*", GetInputFilter(), required);
        builder.Add(path);
        _InputPath = builder;
    }

    public void ResultVariable(string variableName)
    {
        _ResultVariableName = variableName;
    }

    public void UnblockPublisher(string publisher)
    {
        if (System.Environment.OSVersion.Platform != PlatformID.Win32NT)
            return;

        _TrustedPublishers ??= [];
        _TrustedPublishers.Add(publisher);
    }

    public override IPipelineBuilder Configure(PSRuleOption option)
    {
        if (option == null)
            return this;

        base.Configure(option);

        Option.Logging.RuleFail = option.Logging.RuleFail ?? LoggingOption.Default.RuleFail;
        Option.Logging.RulePass = option.Logging.RulePass ?? LoggingOption.Default.RulePass;
        Option.Logging.LimitVerbose = option.Logging.LimitVerbose;
        Option.Logging.LimitDebug = option.Logging.LimitDebug;

        Option.Output.As = option.Output.As ?? OutputOption.Default.As;
        Option.Output.Culture = GetCulture(option.Output.Culture);
        Option.Output.Encoding = option.Output.Encoding ?? OutputOption.Default.Encoding;
        Option.Output.Format = option.Output.Format ?? OutputOption.Default.Format;
        Option.Output.Path = option.Output.Path ?? OutputOption.Default.Path;
        Option.Output.JsonIndent = NormalizeJsonIndentRange(option.Output.JsonIndent);

        if (option.Rule != null)
            Option.Rule = new RuleOption(option.Rule);

        if (option.Configuration != null)
            Option.Configuration = new ConfigurationOption(option.Configuration);

        ConfigureBinding(option);
        Option.Requires = new RequiresOption(option.Requires);
        if (option.Suppression.Count > 0)
            Option.Suppression = new SuppressionOption(option.Suppression);

        return this;
    }

    public override IPipeline Build(IPipelineWriter writer = null)
    {
        writer ??= PrepareWriter();
        Unblock(writer);
        return !RequireModules() || !RequireSources()
            ? null
            : (IPipeline)new InvokeRulePipeline(PrepareContext(BindTargetNameHook, BindTargetTypeHook, BindFieldHook), Source, writer, Option.Output.Outcome.Value);
    }

    protected void Unblock(IPipelineWriter writer)
    {
        if (Source == null || Source.Length == 0 ||
            _TrustedPublishers == null || _TrustedPublishers.Count == 0 ||
            System.Environment.OSVersion.Platform != PlatformID.Win32NT)
            return;

        var files = new List<string>();
        for (var i = 0; i < Source.Length; i++)
        {
            for (var j = 0; j < Source[i].File.Length; j++)
            {
                if (Source[i].File[j].Type == SourceType.Script && IsBlocked(Source[i].File[j].Path))
                    files.Add(Source[i].File[j].Path);
            }
        }
        if (files.Count > 0 && Option.Execution.RestrictScriptSource != Options.RestrictScriptSource.DisablePowerShell)
        {
            HostHelper.UnblockFile(writer, _TrustedPublishers.ToArray(), files.ToArray());
        }
    }

    private static bool IsBlocked(string path)
    {
        try
        {
            var zone = File.ReadLines(string.Concat(path, ":Zone.Identifier")).FirstOrDefault(s => s.StartsWith("ZoneId="));
            return zone != null;
        }
        catch
        {
            return false;
        }
    }

    protected override PipelineInputStream PrepareReader()
    {
        if (!string.IsNullOrEmpty(Option.Input.ObjectPath))
        {
            AddVisitTargetObjectAction((sourceObject, next) =>
            {
                return PipelineReceiverActions.ReadObjectPath(sourceObject, next, Option.Input.ObjectPath, true);
            });
        }

        if (Option.Input.Format == InputFormat.Yaml)
        {
            AddVisitTargetObjectAction((sourceObject, next) =>
            {
                return PipelineReceiverActions.ConvertFromYaml(sourceObject, next);
            });
        }
        else if (Option.Input.Format == InputFormat.Json)
        {
            AddVisitTargetObjectAction((sourceObject, next) =>
            {
                return PipelineReceiverActions.ConvertFromJson(sourceObject, next);
            });
        }
        else if (Option.Input.Format == InputFormat.Markdown)
        {
            AddVisitTargetObjectAction((sourceObject, next) =>
            {
                return PipelineReceiverActions.ConvertFromMarkdown(sourceObject, next);
            });
        }
        else if (Option.Input.Format == InputFormat.PowerShellData)
        {
            AddVisitTargetObjectAction((sourceObject, next) =>
            {
                return PipelineReceiverActions.ConvertFromPowerShellData(sourceObject, next);
            });
        }
        return new PipelineInputStream(VisitTargetObject, _InputPath, GetInputObjectSourceFilter(), Option);
    }
}
