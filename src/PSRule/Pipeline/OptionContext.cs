// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using PSRule.Configuration;
using PSRule.Definitions;
using PSRule.Options;

namespace PSRule.Pipeline;

internal sealed class OptionContext
{
    private ConventionOption _Convention;
    private List<string> _ConventionOrder;

    public OptionContext()
    {

    }

    public Options.BaselineOption Baseline { get; set; }

    public BindingOption Binding { get; set; }

    public ConfigurationOption Configuration { get; set; }

    public ConventionOption Convention
    {
        get
        {
            return _Convention;
        }
        set
        {
            _Convention = value;
            _ConventionOrder = null;
        }
    }

    public ExecutionOption Execution { get; set; }

    public IncludeOption Include { get; set; }

    public InputOption Input { get; set; }

    public LoggingOption Logging { get; set; }

    public OutputOption Output { get; set; }

    public OverrideOption Override { get; set; }

    public RepositoryOption Repository { get; set; }

    public RequiresOption Requires { get; set; }

    public RuleOption Rule { get; set; }

    public SuppressionOption Suppression { get; set; }

    public IResourceFilter ConventionFilter { get; set; }

    public IResourceFilter RuleFilter { get; set; }

    internal int GetConventionOrder(IConvention convention)
    {
        if (Convention?.Include == null || Convention.Include.Length == 0)
            return -1;

        _ConventionOrder ??= new List<string>(Convention.Include);
        var index = _ConventionOrder.IndexOf(convention.Id.Value);
        if (index == -1)
            index = _ConventionOrder.IndexOf(convention.Name);

        return index > -1 ? index : int.MaxValue;
    }
}
