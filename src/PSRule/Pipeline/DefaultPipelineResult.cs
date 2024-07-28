// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using PSRule.Definitions.Rules;
using PSRule.Options;

namespace PSRule.Pipeline;

internal sealed class DefaultPipelineResult(IPipelineWriter writer, BreakLevel breakLevel) : IPipelineResult
{
    private readonly IPipelineWriter _Writer = writer;
    private readonly BreakLevel _BreakLevel = breakLevel == BreakLevel.None ? ExecutionOption.Default.Break.Value : breakLevel;
    private bool _HadErrors;
    private bool _HadFailures;
    private SeverityLevel _WorstCase = SeverityLevel.None;

    /// <inheritdoc/>
    public bool HadErrors
    {
        get
        {
            return _HadErrors || (_Writer != null && _Writer.HadErrors);
        }
        set
        {
            _HadErrors = value;
        }
    }

    /// <inheritdoc/>
    public bool HadFailures
    {
        get
        {
            return _HadFailures || (_Writer != null && _Writer.HadFailures);
        }
        set
        {
            _HadFailures = value;
        }
    }

    /// <inheritdoc/>
    public bool ShouldBreakFromFailure { get; private set; }

    public void Fail(SeverityLevel level)
    {
        _WorstCase = _WorstCase.GetWorstCase(level);
        _HadFailures = true;

        if (ShouldBreakFromFailure || _BreakLevel == BreakLevel.Never || _WorstCase == SeverityLevel.None)
            return;

        if (_BreakLevel == BreakLevel.OnInformation && (_WorstCase == SeverityLevel.Information || _WorstCase == SeverityLevel.Warning || _WorstCase == SeverityLevel.Error))
        {
            ShouldBreakFromFailure = true;
        }
        else if (_BreakLevel == BreakLevel.OnWarning && (_WorstCase == SeverityLevel.Warning || _WorstCase == SeverityLevel.Error))
        {
            ShouldBreakFromFailure = true;
        }
        else if (_BreakLevel == BreakLevel.OnError && _WorstCase == SeverityLevel.Error)
        {
            ShouldBreakFromFailure = true;
        }
    }
}
