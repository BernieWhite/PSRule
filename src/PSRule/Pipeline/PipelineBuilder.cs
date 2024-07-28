// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Collections;
using System.Globalization;
using PSRule.Configuration;
using PSRule.Data;
using PSRule.Definitions;
using PSRule.Definitions.Baselines;
using PSRule.Options;
using PSRule.Pipeline.Output;
using PSRule.Resources;

namespace PSRule.Pipeline;

/// <summary>
/// A helper to create a PowerShell-based pipeline for running PSRule.
/// </summary>
public static class PipelineBuilder
{
    /// <summary>
    /// Create a builder for an Assert pipeline.
    /// Used by Assert-PSRule.
    /// </summary>
    /// <remarks>
    /// Assert pipelines process objects with rules and produce text-based output suitable for output to a CI pipeline.
    /// </remarks>
    /// <param name="source">An array of sources.</param>
    /// <param name="option">Options that configure PSRule.</param>
    /// <param name="hostContext">An implementation of a host context that will receive output and results.</param>
    /// <returns>A builder object to configure the pipeline.</returns>
    public static IInvokePipelineBuilder Assert(Source[] source, PSRuleOption option, IHostContext hostContext)
    {
        var pipeline = new AssertPipelineBuilder(source, hostContext);
        pipeline.Configure(option);
        return pipeline;
    }

    /// <summary>
    /// Create a builder for an Invoke pipeline.
    /// Used by Invoke-PSRule.
    /// </summary>
    /// <remarks>
    /// Invoke pipelines process objects and produce records indicating the outcome of each rule.
    /// </remarks>
    /// <param name="source">An array of sources.</param>
    /// <param name="option">Options that configure PSRule.</param>
    /// <param name="hostContext">An implementation of a host context that will receive output and results.</param>
    /// <returns>A builder object to configure the pipeline.</returns>
    public static IInvokePipelineBuilder Invoke(Source[] source, PSRuleOption option, IHostContext hostContext)
    {
        var pipeline = new InvokeRulePipelineBuilder(source, hostContext);
        pipeline.Configure(option);
        return pipeline;
    }

    /// <summary>
    /// Create a builder for a Test pipeline.
    /// Used by Test-PSRule.
    /// </summary>
    /// <remarks>
    /// Test pipelines process objects and true or false the outcome of each rule.
    /// </remarks>
    /// <param name="source">An array of sources.</param>
    /// <param name="option">Options that configure PSRule.</param>
    /// <param name="hostContext">An implementation of a host context that will receive output and results.</param>
    /// <returns>A builder object to configure the pipeline.</returns>
    public static IInvokePipelineBuilder Test(Source[] source, PSRuleOption option, IHostContext hostContext)
    {
        var pipeline = new TestPipelineBuilder(source, hostContext);
        pipeline.Configure(option);
        return pipeline;
    }

    /// <summary>
    /// Create a builder for a Get pipeline.
    /// Used by Get-PSRule.
    /// </summary>
    /// <remarks>
    /// Get pipelines list rules that are discovered by PSRule either in modules or as standalone rules.
    /// </remarks>
    /// <param name="source">An array of sources.</param>
    /// <param name="option">Options that configure PSRule.</param>
    /// <param name="hostContext">An implementation of a host context that will receive output and results.</param>
    /// <returns>A builder object to configure the pipeline.</returns>
    public static IGetPipelineBuilder Get(Source[] source, PSRuleOption option, IHostContext hostContext)
    {
        var pipeline = new GetRulePipelineBuilder(source, hostContext);
        pipeline.Configure(option);
        return pipeline;
    }

    /// <summary>
    /// Create a builder for a help pipeline.
    /// Used by Get-PSRuleHelp.
    /// </summary>
    /// <remarks>
    /// Gets command lines help content for all or specific rules.
    /// </remarks>
    /// <param name="source">An array of sources.</param>
    /// <param name="option">Options that configure PSRule.</param>
    /// <param name="hostContext">An implementation of a host context that will receive output and results.</param>
    /// <returns>A builder object to configure the pipeline.</returns>
    public static IHelpPipelineBuilder GetHelp(Source[] source, PSRuleOption option, IHostContext hostContext)
    {
        var pipeline = new GetRuleHelpPipelineBuilder(source, hostContext);
        pipeline.Configure(option);
        return pipeline;
    }

    /// <summary>
    /// Create a builder to define a list of rule sources.
    /// </summary>
    /// <param name="option">>Options that configure PSRule.</param>
    /// <param name="hostContext">>An implementation of a host context that will receive output and results.</param>
    /// <returns>A builder object to configure the source pipeline.</returns>
    public static ISourcePipelineBuilder RuleSource(PSRuleOption option, IHostContext hostContext)
    {
        var pipeline = new SourcePipelineBuilder(hostContext, option);
        return pipeline;
    }

    /// <summary>
    /// Create a builder for a get baseline pipeline.
    /// Used by Get-PSRuleBaseline.
    /// </summary>
    /// <param name="source">An array of sources.</param>
    /// <param name="option">Options that configure PSRule.</param>
    /// <param name="hostContext">An implementation of a host context that will receive output and results.</param>
    /// <returns>A builder object to configure the pipeline.</returns>
    public static IPipelineBuilder GetBaseline(Source[] source, PSRuleOption option, IHostContext hostContext)
    {
        var pipeline = new GetBaselinePipelineBuilder(source, hostContext);
        pipeline.Configure(option);
        return pipeline;
    }

    /// <summary>
    /// Create a builder for an export baseline pipeline.
    /// Used by Export-PSRuleBaseline.
    /// </summary>
    /// <param name="source">An array of sources.</param>
    /// <param name="option">Options that configure PSRule.</param>
    /// <param name="hostContext">An implementation of a host context that will receive output and results.</param>
    /// <returns>A builder object to configure the pipeline.</returns>
    public static IPipelineBuilder ExportBaseline(Source[] source, PSRuleOption option, IHostContext hostContext)
    {
        var pipeline = new ExportBaselinePipelineBuilder(source, hostContext);
        pipeline.Configure(option);
        return pipeline;
    }

    /// <summary>
    /// Create a builder for a target pipeline.
    /// Used by Get-PSRuleTarget.
    /// </summary>
    /// <param name="option">Options that configure PSRule.</param>
    /// <param name="hostContext">An implementation of a host context that will receive output and results.</param>
    /// <returns>A builder object to configure the pipeline.</returns>
    public static IGetTargetPipelineBuilder GetTarget(PSRuleOption option, IHostContext hostContext)
    {
        var pipeline = new GetTargetPipelineBuilder(null, hostContext);
        pipeline.Configure(option);
        return pipeline;
    }
}

/// <summary>
/// A helper to build a PSRule pipeline.
/// </summary>
public interface IPipelineBuilder
{
    /// <summary>
    /// Configure the pipeline with options.
    /// </summary>
    IPipelineBuilder Configure(PSRuleOption option);

    /// <summary>
    /// Configure the pipeline to use a specific baseline.
    /// </summary>
    /// <param name="baseline">A baseline option or the name of a baseline.</param>
    void Baseline(Configuration.BaselineOption baseline);

    /// <summary>
    /// Build the pipeline.
    /// </summary>
    /// <param name="writer">Optionally specify a custom writer which will handle output processing.</param>
    IPipeline Build(IPipelineWriter writer = null);
}

internal abstract class PipelineBuilderBase : IPipelineBuilder
{
    private const string ENGINE_MODULE_NAME = "PSRule";

    protected readonly PSRuleOption Option;
    protected readonly Source[] Source;
    protected readonly IHostContext HostContext;
    protected BindTargetMethod BindTargetNameHook;
    protected BindTargetMethod BindTargetTypeHook;
    protected BindTargetMethod BindFieldHook;
    protected VisitTargetObject VisitTargetObject;

    private string[] _Include;
    private Hashtable _Tag;
    private Configuration.BaselineOption _Baseline;
    private string[] _Convention;
    private PathFilter _InputFilter;
    private PipelineWriter _Writer;

    private readonly HostPipelineWriter _Output;

    private const int MIN_JSON_INDENT = 0;
    private const int MAX_JSON_INDENT = 4;

    protected PipelineBuilderBase(Source[] source, IHostContext hostContext)
    {
        Option = new PSRuleOption();
        Source = source;
        _Output = new HostPipelineWriter(hostContext, Option, ShouldProcess);
        HostContext = hostContext;
        BindTargetNameHook = PipelineHookActions.BindTargetName;
        BindTargetTypeHook = PipelineHookActions.BindTargetType;
        BindFieldHook = PipelineHookActions.BindField;
        VisitTargetObject = PipelineReceiverActions.PassThru;
    }

    /// <summary>
    /// Determines if the pipeline is executing in a remote PowerShell session.
    /// </summary>
    public bool InSession => HostContext != null && HostContext.InSession;

    /// <inheritdoc/>
    public void Name(string[] name)
    {
        if (name == null || name.Length == 0)
            return;

        _Include = name;
    }

    /// <inheritdoc/>
    public void Tag(Hashtable tag)
    {
        if (tag == null || tag.Count == 0)
            return;

        _Tag = tag;
    }

    /// <inheritdoc/>
    public void Convention(string[] convention)
    {
        if (convention == null || convention.Length == 0)
            return;

        _Convention = convention;
    }

    /// <inheritdoc/>
    public virtual IPipelineBuilder Configure(PSRuleOption option)
    {
        if (option == null)
            return this;

        Option.Baseline = new Options.BaselineOption(option.Baseline);
        Option.Binding = new BindingOption(option.Binding);
        Option.Convention = new ConventionOption(option.Convention);
        Option.Execution = GetExecutionOption(option.Execution);
        Option.Input = new InputOption(option.Input);
        Option.Input.Format ??= InputOption.Default.Format;
        Option.Output = new OutputOption(option.Output);
        Option.Output.Outcome ??= OutputOption.Default.Outcome;
        Option.Output.Banner ??= OutputOption.Default.Banner;
        Option.Output.Style = GetStyle(option.Output.Style ?? OutputOption.Default.Style.Value);
        Option.Override = new OverrideOption(option.Override);
        Option.Repository = GetRepository(option.Repository);
        return this;
    }

    /// <inheritdoc/>
    public abstract IPipeline Build(IPipelineWriter writer = null);

    /// <inheritdoc/>
    public void Baseline(Configuration.BaselineOption baseline)
    {
        if (baseline == null)
            return;

        _Baseline = baseline;
    }

    /// <summary>
    /// Require correct module versions for pipeline execution.
    /// </summary>
    protected bool RequireModules()
    {
        var result = true;
        if (Option.Requires.TryGetValue(ENGINE_MODULE_NAME, out var requiredVersion))
        {
            var engineVersion = Engine.GetVersion();
            if (GuardModuleVersion(ENGINE_MODULE_NAME, engineVersion, requiredVersion))
                result = false;
        }
        for (var i = 0; Source != null && i < Source.Length; i++)
        {
            if (Source[i].Module != null && Option.Requires.TryGetValue(Source[i].Module.Name, out requiredVersion))
            {
                if (GuardModuleVersion(Source[i].Module.Name, Source[i].Module.Version, requiredVersion))
                    result = false;
            }
        }
        return result;
    }

    /// <summary>
    /// Require sources for pipeline execution.
    /// </summary>
    protected bool RequireSources()
    {
        if (Source == null || Source.Length == 0)
        {
            PrepareWriter().WarnRulePathNotFound();
            return false;
        }
        return true;
    }

    private bool GuardModuleVersion(string moduleName, string moduleVersion, string requiredVersion)
    {
        if (!TryModuleVersion(moduleVersion, requiredVersion))
        {
            var writer = PrepareWriter();
            writer.ErrorRequiredVersionMismatch(moduleName, moduleVersion, requiredVersion);
            writer.End(new DefaultPipelineResult(null, BreakLevel.None) { HadErrors = true });
            return true;
        }
        return false;
    }

    private static bool TryModuleVersion(string moduleVersion, string requiredVersion)
    {
        return SemanticVersion.TryParseVersion(moduleVersion, out var version) &&
            SemanticVersion.TryParseConstraint(requiredVersion, out var constraint) &&
            constraint.Equals(version);
    }

    protected PipelineContext PrepareContext(BindTargetMethod bindTargetName, BindTargetMethod bindTargetType, BindTargetMethod bindField)
    {
        var unresolved = new List<ResourceRef>();
        if (_Baseline is Configuration.BaselineOption.BaselineRef baselineRef)
            unresolved.Add(new BaselineRef(ResolveBaselineGroup(baselineRef.Name), ScopeType.Explicit));

        return PipelineContext.New(
            option: Option,
            hostContext: HostContext,
            reader: PrepareReader(),
            bindTargetName: bindTargetName,
            bindTargetType: bindTargetType,
            bindField: bindField,
            optionBuilder: GetOptionBuilder(),
            unresolved: unresolved
        );
    }

    protected string[] ResolveBaselineGroup(string[] name)
    {
        for (var i = 0; name != null && i < name.Length; i++)
            name[i] = ResolveBaselineGroup(name[i]);

        return name;
    }

    protected string ResolveBaselineGroup(string name)
    {
        if (name == null || name.Length < 2 || !name.StartsWith("@") ||
            Option == null || Option.Baseline == null || Option.Baseline.Group == null ||
            Option.Baseline.Group.Count == 0)
            return name;

        var key = name.Substring(1);
        if (!Option.Baseline.Group.TryGetValue(key, out var baselines) || baselines.Length == 0)
            throw new PipelineConfigurationException("Baseline.Group", string.Format(Thread.CurrentThread.CurrentCulture, PSRuleResources.PSR0003, key));

        var writer = PrepareWriter();
        writer.WriteVerbose($"Using baseline group '{key}': {baselines[0]}");
        return baselines[0];
    }

    protected virtual PipelineInputStream PrepareReader()
    {
        return new PipelineInputStream(null, null, GetInputObjectSourceFilter(), Option);
    }

    protected virtual PipelineWriter PrepareWriter()
    {
        if (_Writer != null)
            return _Writer;

        var output = GetOutput();
        _Writer = Option.Output.Format switch
        {
            OutputFormat.Csv => new CsvOutputWriter(output, Option, ShouldProcess),
            OutputFormat.Json => new JsonOutputWriter(output, Option, ShouldProcess),
            OutputFormat.NUnit3 => new NUnit3OutputWriter(output, Option, ShouldProcess),
            OutputFormat.Yaml => new YamlOutputWriter(output, Option, ShouldProcess),
            OutputFormat.Markdown => new MarkdownOutputWriter(output, Option, ShouldProcess),
            OutputFormat.Wide => new WideOutputWriter(output, Option, ShouldProcess),
            OutputFormat.Sarif => new SarifOutputWriter(Source, output, Option, ShouldProcess),
            _ => output,
        };
        return _Writer;
    }

    protected virtual PipelineWriter GetOutput(bool writeHost = false)
    {
        // Redirect to file instead
        return !string.IsNullOrEmpty(Option.Output.Path)
            ? new FileOutputWriter(
                inner: _Output,
                option: Option,
                encoding: Option.Output.GetEncoding(),
                path: Option.Output.Path,
                shouldProcess: HostContext.ShouldProcess,
                writeHost: writeHost
            )
            : _Output;
    }

    protected static string[] GetCulture(string[] culture)
    {
        var result = new List<string>();
        var parent = new List<string>();
        var set = new HashSet<string>();
        for (var i = 0; culture != null && i < culture.Length; i++)
        {
            var c = CultureInfo.CreateSpecificCulture(culture[i]);
            if (!set.Contains(c.Name))
            {
                result.Add(c.Name);
                set.Add(c.Name);
            }
            for (var p = c.Parent; !string.IsNullOrEmpty(p.Name); p = p.Parent)
            {
                if (!set.Contains(p.Name))
                {
                    parent.Add(p.Name);
                    set.Add(p.Name);
                }
            }
        }
        if (parent.Count > 0)
            result.AddRange(parent);

        return result.Count == 0 ? null : result.ToArray();
    }

    protected static RepositoryOption GetRepository(RepositoryOption option)
    {
        var result = new RepositoryOption(option);
        if (string.IsNullOrEmpty(result.Url) && GitHelper.TryRepository(out var url))
            result.Url = url;

        if (string.IsNullOrEmpty(result.BaseRef) && GitHelper.TryBaseRef(out var baseRef))
            result.BaseRef = baseRef;

        return result;
    }

    /// <summary>
    /// Coalesce execution options with defaults.
    /// </summary>
    protected static ExecutionOption GetExecutionOption(ExecutionOption option)
    {
        var result = ExecutionOption.Combine(option, ExecutionOption.Default);

        // Handle when preference is set to none. The default should be used.
        result.AliasReference = result.AliasReference == ExecutionActionPreference.None ? ExecutionOption.Default.AliasReference.Value : result.AliasReference;
        result.DuplicateResourceId = result.DuplicateResourceId == ExecutionActionPreference.None ? ExecutionOption.Default.DuplicateResourceId.Value : result.DuplicateResourceId;
        result.InvariantCulture = result.InvariantCulture == ExecutionActionPreference.None ? ExecutionOption.Default.InvariantCulture.Value : result.InvariantCulture;
        result.RuleExcluded = result.RuleExcluded == ExecutionActionPreference.None ? ExecutionOption.Default.RuleExcluded.Value : result.RuleExcluded;
        result.RuleInconclusive = result.RuleInconclusive == ExecutionActionPreference.None ? ExecutionOption.Default.RuleInconclusive.Value : result.RuleInconclusive;
        result.RuleSuppressed = result.RuleSuppressed == ExecutionActionPreference.None ? ExecutionOption.Default.RuleSuppressed.Value : result.RuleSuppressed;
        result.SuppressionGroupExpired = result.SuppressionGroupExpired == ExecutionActionPreference.None ? ExecutionOption.Default.SuppressionGroupExpired.Value : result.SuppressionGroupExpired;
        result.UnprocessedObject = result.UnprocessedObject == ExecutionActionPreference.None ? ExecutionOption.Default.UnprocessedObject.Value : result.UnprocessedObject;
        return result;
    }

    protected PathFilter GetInputObjectSourceFilter()
    {
        return Option.Input.IgnoreObjectSource.GetValueOrDefault(InputOption.Default.IgnoreObjectSource.Value) ? GetInputFilter() : null;
    }

    protected PathFilter GetInputFilter()
    {
        if (_InputFilter == null)
        {
            var basePath = Environment.GetWorkingPath();
            var ignoreGitPath = Option.Input.IgnoreGitPath ?? InputOption.Default.IgnoreGitPath.Value;
            var ignoreRepositoryCommon = Option.Input.IgnoreRepositoryCommon ?? InputOption.Default.IgnoreRepositoryCommon.Value;
            var builder = PathFilterBuilder.Create(basePath, Option.Input.PathIgnore, ignoreGitPath, ignoreRepositoryCommon);
            builder.UseGitIgnore();

            _InputFilter = builder.Build();
        }
        return _InputFilter;
    }

    private OptionContextBuilder GetOptionBuilder()
    {
        return new OptionContextBuilder(Option, _Include, _Tag, _Convention);
    }

    protected void ConfigureBinding(PSRuleOption option)
    {
        if (option.Pipeline.BindTargetName != null && option.Pipeline.BindTargetName.Count > 0)
        {
            // Do not allow custom binding functions to be used with constrained language mode
            if (Option.Execution.LanguageMode == LanguageMode.ConstrainedLanguage)
                throw new PipelineConfigurationException(optionName: "BindTargetName", message: PSRuleResources.ConstrainedTargetBinding);

            foreach (var action in option.Pipeline.BindTargetName)
                BindTargetNameHook = AddBindTargetAction(action, BindTargetNameHook);
        }

        if (option.Pipeline.BindTargetType != null && option.Pipeline.BindTargetType.Count > 0)
        {
            // Do not allow custom binding functions to be used with constrained language mode
            if (Option.Execution.LanguageMode == LanguageMode.ConstrainedLanguage)
                throw new PipelineConfigurationException(optionName: "BindTargetType", message: PSRuleResources.ConstrainedTargetBinding);

            foreach (var action in option.Pipeline.BindTargetType)
                BindTargetTypeHook = AddBindTargetAction(action, BindTargetTypeHook);
        }
    }

    private static BindTargetMethod AddBindTargetAction(BindTargetFunc action, BindTargetMethod previous)
    {
        // Nest the previous write action in the new supplied action
        // Execution chain will be: action -> previous -> previous..n
        return (string[] propertyNames, bool caseSensitive, bool preferTargetInfo, object targetObject, out string path) =>
        {
            return action(propertyNames, caseSensitive, preferTargetInfo, targetObject, previous, out path);
        };
    }

    private static BindTargetMethod AddBindTargetAction(BindTargetName action, BindTargetMethod previous)
    {
        return AddBindTargetAction((string[] propertyNames, bool caseSensitive, bool preferTargetInfo, object targetObject, BindTargetMethod next, out string path) =>
        {
            path = null;
            var targetType = action(targetObject);
            return string.IsNullOrEmpty(targetType) ? next(propertyNames, caseSensitive, preferTargetInfo, targetObject, out path) : targetType;
        }, previous);
    }

    protected void AddVisitTargetObjectAction(VisitTargetObjectAction action)
    {
        // Nest the previous write action in the new supplied action
        // Execution chain will be: action -> previous -> previous..n
        var previous = VisitTargetObject;
        VisitTargetObject = (targetObject) => action(targetObject, previous);
    }

    /// <summary>
    /// Normalizes JSON indent range between minimum 0 and maximum 4.
    /// </summary>
    /// <param name="jsonIndent"></param>
    /// <returns>The number of characters to indent.</returns>
    protected static int NormalizeJsonIndentRange(int? jsonIndent)
    {
        if (jsonIndent.HasValue)
        {
            if (jsonIndent < MIN_JSON_INDENT)
                return MIN_JSON_INDENT;

            else if (jsonIndent > MAX_JSON_INDENT)
                return MAX_JSON_INDENT;

            return jsonIndent.Value;
        }
        return MIN_JSON_INDENT;
    }

    protected bool TryChangedFiles(out string[] files)
    {
        files = null;
        if (!Option.Input.IgnoreUnchangedPath.GetValueOrDefault(InputOption.Default.IgnoreUnchangedPath.Value) ||
            !GitHelper.TryGetChangedFiles(Option.Repository.BaseRef, "d", null, out files))
            return false;

        for (var i = 0; i < files.Length; i++)
            HostContext.Verbose(string.Format(Thread.CurrentThread.CurrentCulture, PSRuleResources.UsingChangedFile, files[i]));

        return true;
    }

    protected bool ShouldProcess(string target, string action)
    {
        return HostContext == null || HostContext.ShouldProcess(target, action);
    }

    protected static OutputStyle GetStyle(OutputStyle style)
    {
        if (style != OutputStyle.Detect)
            return style;

        if (Environment.IsAzurePipelines())
            return OutputStyle.AzurePipelines;

        if (Environment.IsGitHubActions())
            return OutputStyle.GitHubActions;

        return Environment.IsVisualStudioCode() ?
            OutputStyle.VisualStudioCode :
            OutputStyle.Client;
    }
}
