// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using PSRule.Configuration;
using PSRule.Rules;

namespace PSRule.Pipeline.Formatters;

/// <summary>
/// Formatter for GitHub Actions.
/// </summary>
internal sealed class GitHubActionsFormatter : AssertFormatterBase, IAssertFormatter
{
    // Available commands are defined here: https://docs.github.com/en/actions/learn-github-actions/workflow-commands-for-github-actions
    private const string ERROR_COMMAND_NO_SOURCE = "::error::";
    private const string WARNING_COMMAND_NO_SOURCE = "::warning::";
    private const string NOTICE_COMMAND_NO_SOURCE = "::notice::";

    private readonly TerminalSupport _TerminalSupport;

    internal GitHubActionsFormatter(Source[] source, IPipelineWriter logger, PSRuleOption option)
        : base(source, logger, option)
    {
        _TerminalSupport = new TerminalSupport(4)
        {
            MessageIdent = string.Empty,
            ErrorStatus = ERROR_COMMAND_NO_SOURCE,
            WarningStatus = WARNING_COMMAND_NO_SOURCE,
            InformationStatus = NOTICE_COMMAND_NO_SOURCE,
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

        if (record.Level == Definitions.Rules.SeverityLevel.Information)
            Information(message);

        LineBreak();
    }
}
