// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Collections;
using System.Management.Automation;
using PSRule.Definitions;
using PSRule.Definitions.Rules;
using PSRule.Pipeline;
using PSRule.Resources;
using PSRule.Rules;
using PSRule.Runtime;

namespace PSRule.Commands;

/// <summary>
/// A Rule language block.
/// </summary>
[Cmdlet(VerbsCommon.New, RuleLanguageNouns.RuleDefinition)]
internal sealed class NewRuleDefinitionCommand : LanguageBlock
{
    private const string CmdletName = "Invoke-RuleBlock";
    private const string Cmdlet_TypeParameter = "Type";
    private const string Cmdlet_IfParameter = "If";
    private const string Cmdlet_WithParameter = "With";
    private const string Cmdlet_BodyParameter = "Body";
    private const string Cmdlet_SourceParameter = "Source";

    /// <summary>
    /// The name of the rule.
    /// </summary>
    [Parameter(Mandatory = true, Position = 0)]
    [ValidateNotNullOrEmpty()]
    [ValidateLength(3, 128)]
    public string Name { get; set; }

    /// <summary>
    /// If the rule fails, how serious is the result.
    /// </summary>
    [Parameter(Mandatory = false)]
    public SeverityLevel? Level { get; set; }

    /// <summary>
    /// The definition of the deployment.
    /// </summary>
    [Parameter(Mandatory = false, Position = 1)]
    public ScriptBlock Body { get; set; }

    /// <summary>
    /// A set of tags with additional metadata for the rule.
    /// </summary>
    [Parameter(Mandatory = false)]
    public Hashtable Tag { get; set; }

    /// <summary>
    /// An optional script precondition before the rule is evaluated.
    /// </summary>
    [Parameter(Mandatory = false)]
    public ScriptBlock If { get; set; }

    /// <summary>
    /// An optional type precondition before the rule is evaluated.
    /// </summary>
    [Parameter(Mandatory = false)]
    public string[] Type { get; set; }

    /// <summary>
    /// An optional selector precondition before the rule is evaluated.
    /// </summary>
    [Parameter(Mandatory = false)]
    public string[] With { get; set; }

    /// <summary>
    /// Deployments that this deployment depends on.
    /// </summary>
    [Parameter(Mandatory = false)]
    [ValidateNotNullOrEmpty()]
    public string[] DependsOn { get; set; }

    /// <summary>
    /// A set of default configuration values.
    /// </summary>
    [Parameter(Mandatory = false)]
    public Hashtable Configure { get; set; }

    /// <summary>
    /// Any aliases for the rule.
    /// </summary>
    [Parameter(Mandatory = false)]
    public string[] Alias { get; set; }

    /// <summary>
    /// An optional reference identifer for the resource.
    /// </summary>
    [Parameter(Mandatory = false)]
    [ValidateLength(3, 128)]
    public string Ref { get; set; }

    /// <summary>
    /// Any taxonomy references.
    /// </summary>
    [Parameter(Mandatory = false)]
    public Hashtable Labels { get; set; }

    protected override void ProcessRecord()
    {
        if (!IsSourceScope())
            throw new RuleException(string.Format(Thread.CurrentThread.CurrentCulture, PSRuleResources.KeywordSourceScope, LanguageKeywords.Rule));

        var context = RunspaceContext.CurrentThread;
        var source = context.Source.File;
        var errorPreference = GetErrorActionPreference();
        var metadata = GetCommentMetadata(source, MyInvocation.ScriptLineNumber, MyInvocation.OffsetInLine);
        var level = ResourceHelper.GetLevel(Level);
        var tag = GetTag(Tag);
        var extent = new SourceExtent(
            file: source,
            line: MyInvocation.ScriptLineNumber,
            position: MyInvocation.OffsetInLine
        );
        var flags = ResourceFlags.None;
        var id = new ResourceId(source.Module, Name, ResourceIdKind.Id);
        var labels = ResourceLabels.FromHashtable(Labels);

        context.VerboseFoundResource(name: Name, moduleName: source.Module, scriptName: MyInvocation.ScriptName);

        CheckDependsOn();
        var ps = GetCondition(context, id, source, errorPreference);
        var info = PSRule.Host.HostHelper.GetRuleHelpInfo(context, Name, metadata.Synopsis, null, null, null) ?? new RuleHelpInfo(
            name: Name,
            displayName: Name,
            moduleName: source.Module
        );
        context.LanguageScope.TryGetOverride(id, out var propertyOverride);

#pragma warning disable CA2000 // Dispose objects before losing scope, needs to be passed to pipeline
        var block = new RuleBlock(
            source: source,
            id: id,
            @ref: ResourceHelper.GetIdNullable(source.Module, Ref, ResourceIdKind.Ref),
            @default: new RuleProperties
            {
                Level = level
            },
            @override: propertyOverride,
            info: info,
            condition: ps,
            tag: tag,
            alias: ResourceHelper.GetRuleId(source.Module, Alias, ResourceIdKind.Alias),
            dependsOn: ResourceHelper.GetRuleId(source.Module, DependsOn, ResourceIdKind.Unknown),
            configuration: Configure,
            extent: extent,
            flags: flags,
            labels: labels
        );
#pragma warning restore CA2000 // Dispose objects before losing scope, needs to be passed to pipeline
        WriteObject(block);
    }

    private PowerShellCondition GetCondition(RunspaceContext context, ResourceId id, ISourceFile source, ActionPreference errorAction)
    {
        var result = context.GetPowerShell();
        result.AddCommand(new CmdletInfo(CmdletName, typeof(InvokeRuleBlockCommand)));
        result.AddParameter(Cmdlet_TypeParameter, Type);
        result.AddParameter(Cmdlet_WithParameter, GetScopedSelectors(source));
        result.AddParameter(Cmdlet_IfParameter, If);
        result.AddParameter(Cmdlet_BodyParameter, Body);
        result.AddParameter(Cmdlet_SourceParameter, source);
        return new PowerShellCondition(id, source, result, errorAction);
    }

    private void CheckDependsOn()
    {
        if (MyInvocation.BoundParameters.ContainsKey(nameof(DependsOn)) && (DependsOn == null || DependsOn.Length == 0))
        {
            WriteError(new ErrorRecord(
                exception: new ArgumentNullException(paramName: nameof(DependsOn)),
                errorId: "PSRule.Runtime.ArgumentNull",
                errorCategory: ErrorCategory.InvalidArgument,
                targetObject: null
            ));
        }
    }

    private ResourceId[] GetScopedSelectors(ISourceFile source)
    {
        return ResourceHelper.GetRuleId(source.Module, With, ResourceIdKind.Unknown);
    }
}
