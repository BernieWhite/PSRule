// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Collections;
using PSRule.Configuration;
using PSRule.Definitions;
using PSRule.Definitions.Baselines;
using PSRule.Definitions.Conventions;
using PSRule.Definitions.ModuleConfigs;
using PSRule.Definitions.Rules;
using PSRule.Options;
using PSRule.Runtime;

namespace PSRule.Pipeline;

/// <summary>
/// A helper to create an <see cref="OptionContext"/>.
/// </summary>
internal sealed class OptionContextBuilder
{
    private readonly Dictionary<string, bool> _ModuleBaselineScope;
    private readonly List<OptionScope> _Scopes;
    private readonly OptionScopeComparer _Comparer;
    private readonly string[] _DefaultCulture;
    private readonly List<string> _ConventionOrder;

    internal OptionContextBuilder(string[] include = null, Hashtable tag = null, string[] convention = null)
    {
        _ModuleBaselineScope = new Dictionary<string, bool>();
        _Scopes = new List<OptionScope>();
        _Comparer = new OptionScopeComparer();
        _DefaultCulture = GetDefaultCulture();
        _ConventionOrder = new List<string>();
        Parameter(include, tag, convention);
    }

    /// <summary>
    /// Create a builder with parameter and workspace options set.
    /// </summary>
    /// <param name="option">The workspace options.</param>
    /// <param name="include">A list of rule identifiers to include set by parameters. If not set all rules that meet filters are included.</param>
    /// <param name="tag">A tag filter to determine which rules are included by parameters.</param>
    /// <param name="convention">A list of conventions to include by parameters.</param>
    internal OptionContextBuilder(PSRuleOption option, string[] include = null, Hashtable tag = null, string[] convention = null)
        : this(include, tag, convention)
    {
        Workspace(option);
    }

    /// <summary>
    /// Build an <see cref="OptionContext"/>.
    /// </summary>
    internal OptionContext Build(string languageScope)
    {
        languageScope = ResourceHelper.NormalizeScope(languageScope);
        var context = new OptionContext();

        _Scopes.Sort(_Comparer);

        for (var i = 0; i < _Scopes.Count; i++)
        {
            if (_Scopes[i] != null && ShouldCombine(languageScope, _Scopes[i]))
                Combine(context, _Scopes[i]);
        }
        //Combine(PSRuleOption.FromDefault());
        context.Output.Culture ??= _DefaultCulture;
        context.Rule.IncludeLocal = GetIncludeLocal(_Scopes) ?? context.Rule.IncludeLocal ?? true;

        context.RuleFilter = GetRuleFilter(context.Rule);
        context.ConventionFilter = GetConventionFilter(languageScope, _Scopes);
        context.Convention = new ConventionOption
        {
            Include = GetConventions(_Scopes)
        };
        return context;
    }

    public bool ContainsBaseline(string baselineId)
    {
        return _ModuleBaselineScope.ContainsKey(baselineId);
    }

    /// <summary>
    /// Check for any obsolete resources and log warnings.
    /// </summary>
    internal void CheckObsolete(ILogger logger)
    {
        if (logger == null)
            return;

        foreach (var kv in _ModuleBaselineScope)
        {
            if (kv.Value)
                logger.WarnResourceObsolete(ResourceKind.Baseline, kv.Key);
        }
    }

    private void Parameter(string[] ruleInclude, Hashtable ruleTag, string[] conventionInclude)
    {
        _Scopes.Add(OptionScope.FromParameters(ruleInclude, ruleTag, conventionInclude));
    }

    internal void Workspace(PSRuleOption option)
    {
        _Scopes.Add(OptionScope.FromWorkspace(option));
    }

    internal void Baseline(ScopeType type, string baselineId, string module, BaselineSpec spec, bool obsolete)
    {
        baselineId = ResourceHelper.GetIdString(module, baselineId);
        _ModuleBaselineScope.Add(baselineId, obsolete);
        _Scopes.Add(OptionScope.FromBaseline(type, baselineId, module, spec, obsolete));
    }

    internal void ModuleConfig(string module, ModuleConfigV1Spec spec)
    {
        _Scopes.Add(OptionScope.FromModuleConfig(module, spec));
    }

    private static bool ShouldCombine(string languageScope, OptionScope optionScope)
    {
        return optionScope.LanguageScope == ResourceHelper.STANDALONE_SCOPENAME || optionScope.LanguageScope == languageScope || optionScope.Type == ScopeType.Explicit;
    }

    /// <summary>
    /// Combine the specified <see cref="OptionScope"/> with an existing <see cref="OptionContext"/>.
    /// </summary>
    private static void Combine(OptionContext context, OptionScope optionScope)
    {
        context.Baseline = Options.BaselineOption.Combine(context.Baseline, optionScope.Baseline);
        context.Binding = BindingOption.Combine(context.Binding, optionScope.Binding);
        context.Configuration = ConfigurationOption.Combine(context.Configuration, optionScope.Configuration);
        //context.Convention = ConventionOption.Combine(context.Convention, optionScope.Convention);
        context.Execution = ExecutionOption.Combine(context.Execution, optionScope.Execution);
        context.Include = IncludeOption.Combine(context.Include, optionScope.Include);
        context.Input = InputOption.Combine(context.Input, optionScope.Input);
        context.Logging = LoggingOption.Combine(context.Logging, optionScope.Logging);
        context.Output = OutputOption.Combine(context.Output, optionScope.Output);
        context.Override = OverrideOption.Combine(context.Override, optionScope.Override);
        context.Repository = RepositoryOption.Combine(context.Repository, optionScope.Repository);
        context.Requires = RequiresOption.Combine(context.Requires, optionScope.Requires);
        context.Rule = RuleOption.Combine(context.Rule, optionScope.Rule);
        context.Suppression = SuppressionOption.Combine(context.Suppression, optionScope.Suppression);
    }

    private static IResourceFilter GetRuleFilter(RuleOption option)
    {
        //var include = _Parameter?.Include ?? _Explicit?.Include ?? _WorkspaceBaseline?.Include ?? _ModuleBaseline?.Include;
        //var exclude = _Explicit?.Exclude ?? _WorkspaceBaseline?.Exclude ?? _ModuleBaseline?.Exclude;
        //var tag = _Parameter?.Tag ?? _Explicit?.Tag ?? _WorkspaceBaseline?.Tag ?? _ModuleBaseline?.Tag;
        //var labels = _Parameter?.Labels ?? _Explicit?.Labels ?? _WorkspaceBaseline?.Labels ?? _ModuleBaseline?.Labels;
        //var includeLocal = _Explicit == null &&
        //    _Parameter?.Include == null &&
        //    _Parameter?.Tag == null &&
        //    _Parameter?.Labels == null &&
        //    (_WorkspaceBaseline == null || !_WorkspaceBaseline.IncludeLocal.HasValue) ? true : _WorkspaceBaseline?.IncludeLocal;
        return new RuleFilter(option.Include, option.Tag, option.Exclude, option.IncludeLocal, option.Labels);
    }

    private static IResourceFilter GetConventionFilter(string languageScope, List<OptionScope> scopes)
    {
        //var include = new List<string>();
        //for (var i = 0; _Parameter?.Convention?.Include != null && i < _Parameter.Convention.Include.Length; i++)
        //    include.Add(_Parameter.Convention.Include[i]);

        //for (var i = 0; _WorkspaceConfig?.Convention?.Include != null && i < _WorkspaceConfig.Convention.Include.Length; i++)
        //    include.Add(_WorkspaceConfig.Convention.Include[i]);

        //for (var i = 0; _ModuleConfig?.Convention?.Include != null && i < _ModuleConfig.Convention.Include.Length; i++)
        //    include.Add(ResourceHelper.GetIdString(_ModuleConfig.LanguageScope, _ModuleConfig.Convention.Include[i]));

        return new ConventionFilter(GetConventions(scopes));
    }

    private static string[] GetConventions(List<OptionScope> scopes)
    {
        var include = new List<string>();
        for (var i = 0; i < scopes.Count; i++)
        {
            var add = scopes[i].Convention?.Include;
            if (add == null || add.Length == 0)
                continue;

            for (var j = 0; j < add.Length; j++)
            {
                if (scopes[i].Type == ScopeType.Module)
                    add[j] = ResourceHelper.GetIdString(scopes[i].LanguageScope, add[j]);
            }
            include.AddUnique(add);
        }
        return include.ToArray();
    }

    private bool? GetIncludeLocal(List<OptionScope> scopes)
    {
        for (var i = 0; i < scopes.Count; i++)
        {
            if (scopes[i].Type == ScopeType.Workspace && scopes[i].Rule != null && scopes[i].Rule.IncludeLocal.HasValue)
                return scopes[i].Rule.IncludeLocal.Value;
        }
        return null;
    }

    private static string[] GetDefaultCulture()
    {
        var result = new List<string>();
        var set = new HashSet<string>();

        // Fallback to current culture
        var current = Environment.GetCurrentCulture();
        if (!set.Contains(current.Name) && !string.IsNullOrEmpty(current.Name))
        {
            result.Add(current.Name);
            set.Add(current.Name);
        }
        for (var p = current.Parent; !string.IsNullOrEmpty(p.Name); p = p.Parent)
        {
            if (!result.Contains(p.Name))
                result.Add(p.Name);
        }
        return result.ToArray();
    }
}
