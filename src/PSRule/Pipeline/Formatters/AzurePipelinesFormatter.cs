// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using PSRule.Configuration;
using PSRule.Rules;

namespace PSRule.Pipeline.Formatters;

/// <summary>
/// Formatter for Azure Pipelines.
/// </summary>
internal sealed class AzurePipelinesFormatter : AssertFormatterBase, IAssertFormatter
{
    // Available commands are defined here: https://docs.microsoft.com/en-us/azure/devops/pipelines/scripts/logging-commands
    private const string MESSAGE_PREFIX_ERROR = "##vso[task.logissue type=error]";
    private const string MESSAGE_PREFIX_WARNING = "##vso[task.logissue type=warning]";

    private readonly TerminalSupport _TerminalSupport;

    internal AzurePipelinesFormatter(Source[] source, IPipelineWriter logger, PSRuleOption option)
        : base(source, logger, option)
    {
        _TerminalSupport = new TerminalSupport(4)
        {
            MessageIdent = string.Empty,
            ErrorStatus = MESSAGE_PREFIX_ERROR,
            WarningStatus = MESSAGE_PREFIX_WARNING,
        };
    }

    protected override TerminalSupport GetTerminalSupport()
    {
        return _TerminalSupport;
    }

    protected override void ErrorDetail(RuleRecord record)
    {
        if (record.Error == null)
            return;

        LineBreak();
        Error(GetErrorMessage(record));
        LineBreak();
        WriteLine(record.Error.PositionMessage);
        LineBreak();
        WriteLine(record.Error.ScriptStackTrace);
    }

    protected override void FailDetail(RuleRecord record)
    {
        base.FailDetail(record);
        var message = GetFailMessage(record);
        if (record.Level == Definitions.Rules.SeverityLevel.Error)
            Error(message);

        if (record.Level == Definitions.Rules.SeverityLevel.Warning)
            Warning(message);

        if (record.Level != Definitions.Rules.SeverityLevel.Information)
            LineBreak();
    }
}
