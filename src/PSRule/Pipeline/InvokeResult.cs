// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using PSRule.Definitions.Rules;
using PSRule.Rules;

namespace PSRule.Pipeline;

/// <summary>
/// A result for a target object.
/// </summary>
public sealed class InvokeResult
{
    private readonly List<RuleRecord> _Record;
    private RuleOutcome _Outcome;
    private SeverityLevel _Level;
    private long _Time;
    private int _Total;
    private int _Error;
    private int _Pass;
    private int _Fail;

    internal InvokeResult()
    {
        _Record = new List<RuleRecord>();
        _Time = 0;
        _Total = 0;
        _Error = 0;
        _Pass = 0;
        _Fail = 0;
    }

    /// <summary>
    /// The execution time of all rules in milliseconds.
    /// </summary>
    internal long Time => _Time;

    /// <summary>
    /// The total number of rule records.
    /// </summary>
    internal int Total => _Total;

    /// <summary>
    /// The number of rule records with a error result.
    /// </summary>
    internal int Error => _Error;

    /// <summary>
    /// The number of rule records with a fail result.
    /// </summary>
    internal int Fail => _Fail;

    /// <summary>
    /// The number of rules records with a pass result.
    /// </summary>
    internal int Pass => _Pass;

    /// <summary>
    /// The worst outcome of all rule records.
    /// </summary>
    public RuleOutcome Outcome => _Outcome;

    /// <summary>
    /// The highest severity level of all rule records.
    /// </summary>
    public SeverityLevel Level => _Level;

    internal string TargetName
    {
        get
        {
            return IsEmptyRecord() ? null : _Record[0].TargetName;
        }
    }

    internal string TargetType
    {
        get
        {
            return IsEmptyRecord() ? null : _Record[0].TargetType;
        }
    }

    private bool IsEmptyRecord()
    {
        return _Record == null || _Record.Count == 0;
    }

    /// <summary>
    /// Get the individual records for the target object.
    /// </summary>
    /// <returns>Returns an enumeration of RuleRecords.</returns>
    public RuleRecord[] AsRecord()
    {
        return _Record.ToArray();
    }

    /// <summary>
    /// Get an overall pass or fail for the target object.
    /// </summary>
    /// <returns>Returns true if object passed and false if object failed.</returns>
    public bool IsSuccess()
    {
        return _Outcome == RuleOutcome.Pass || _Outcome == RuleOutcome.None;
    }

    /// <summary>
    /// Determines of the target object was processed.
    /// </summary>
    public bool IsProcessed()
    {
        return _Outcome == RuleOutcome.Pass || _Outcome == RuleOutcome.Fail || _Outcome == RuleOutcome.Error;
    }

    /// <summary>
    /// Add a record to the result.
    /// </summary>
    /// <param name="ruleRecord">The record after processing a rule.</param>
    internal void Add(RuleRecord ruleRecord)
    {
        _Outcome = ruleRecord.Outcome.GetWorstCase(_Outcome);
        _Time += ruleRecord.Time;
        _Total++;

        if (ruleRecord.Outcome == RuleOutcome.Pass)
            _Pass++;

        if (ruleRecord.Outcome == RuleOutcome.Error)
            _Error++;

        if (ruleRecord.Outcome == RuleOutcome.Fail)
        {
            _Fail++;
            _Level = _Level.GetWorstCase(ruleRecord.Level);
        }
        _Record.Add(ruleRecord);
    }
}
