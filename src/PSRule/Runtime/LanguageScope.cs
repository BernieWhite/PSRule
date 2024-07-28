// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Diagnostics;
using PSRule.Configuration;
using PSRule.Data;
using PSRule.Definitions;
using PSRule.Definitions.Rules;
using PSRule.Options;
using PSRule.Pipeline;

namespace PSRule.Runtime;

[DebuggerDisplay("{Name}")]
internal sealed class LanguageScope : ILanguageScope
{
    private readonly RunspaceContext _Context;
    private IDictionary<string, object> _Configuration;
    private WildcardMap<RuleOverride> _Override;
    private readonly Dictionary<string, object> _Service;
    private readonly Dictionary<ResourceKind, IResourceFilter> _Filter;

    private bool _Disposed;

    public LanguageScope(RunspaceContext context, string name)
    {
        _Context = context;
        Name = ResourceHelper.NormalizeScope(name);
        //_Configuration = new Dictionary<string, object>();
        _Filter = new Dictionary<ResourceKind, IResourceFilter>();
        _Service = new Dictionary<string, object>();
    }

    /// <inheritdoc/>
    public string Name { [DebuggerStepThrough] get; }

    /// <inheritdoc/>
    public BindingOption Binding { [DebuggerStepThrough] get; [DebuggerStepThrough] private set; }

    /// <inheritdoc/>
    public string[] Culture { [DebuggerStepThrough] get; [DebuggerStepThrough] private set; }

    /// <inheritdoc/>
    public void Configure(OptionContext context)
    {
        if (context == null)
            throw new ArgumentNullException(nameof(context));

        _Configuration = context.Configuration;
        WithFilter(context.RuleFilter);
        WithFilter(context.ConventionFilter);
        Binding = context.Binding;
        Culture = context.Output.Culture;
        _Override = WithOverride(context.Override);
    }

    private static WildcardMap<RuleOverride> WithOverride(OverrideOption option)
    {
        if (option == null || option.Level == null)
            return null;

        var overrides = option.Level
            .Where(l => l.Value != SeverityLevel.None)
            .Select(l => new KeyValuePair<string, RuleOverride>(l.Key, new RuleOverride { Level = l.Value }));
        return new WildcardMap<RuleOverride>(overrides);
    }

    /// <inheritdoc/>
    public bool TryConfigurationValue(string key, out object value)
    {
        value = null;
        return !string.IsNullOrEmpty(key) && _Configuration.TryGetValue(key, out value);
    }

    /// <inheritdoc/>
    public bool TryGetOverride(ResourceId id, out RuleOverride value)
    {
        value = null;
        if (_Override == null)
            return false;

        return _Override.TryGetValue(id.Value, out value) ||
            _Override.TryGetValue(id.Name, out value);
    }

    /// <inheritdoc/>
    public void WithFilter(IResourceFilter resourceFilter)
    {
        _Filter[resourceFilter.Kind] = resourceFilter;
    }

    /// <inheritdoc/>
    public IResourceFilter GetFilter(ResourceKind kind)
    {
        return _Filter.TryGetValue(kind, out var filter) ? filter : null;
    }

    /// <inheritdoc/>
    public void AddService(string name, object service)
    {
        if (_Service.ContainsKey(name))
            return;

        _Service.Add(name, service);
    }

    /// <inheritdoc/>
    public object GetService(string name)
    {
        return _Service.TryGetValue(name, out var service) ? service : null;
    }

    public bool TryGetType(object o, out string type, out string path)
    {
        if (_Context != null && _Context.TargetObject.Value == o)
        {
            var binding = _Context.TargetBinder.Result(Name);
            type = binding.TargetType;
            path = binding.TargetTypePath;
            return true;
        }
        else if (_Context != null)
        {
            var binding = _Context.TargetBinder.Using(Name).Bind(o);
            type = binding.TargetType;
            path = binding.TargetTypePath;
            return true;
        }
        type = null;
        path = null;
        return false;
    }

    public bool TryGetName(object o, out string name, out string path)
    {
        if (_Context != null && _Context.TargetObject.Value == o)
        {
            var binding = _Context.TargetBinder.Result(Name);
            name = binding.TargetName;
            path = binding.TargetNamePath;
            return true;
        }
        else if (_Context != null)
        {
            var binding = _Context.TargetBinder.Using(Name).Bind(o);
            name = binding.TargetName;
            path = binding.TargetNamePath;
            return true;
        }
        name = null;
        path = null;
        return false;
    }

    public bool TryGetScope(object o, out string[] scope)
    {
        if (_Context != null && _Context.TargetObject.Value == o)
        {
            scope = _Context.TargetObject.Scope;
            return true;
        }
        scope = null;
        return false;
    }

    private void Dispose(bool disposing)
    {
        if (!_Disposed)
        {
            if (disposing)
            {
                // Release and dispose services
                if (_Service != null && _Service.Count > 0)
                {
                    foreach (var kv in _Service)
                    {
                        if (kv.Value is IDisposable d)
                            d.Dispose();
                    }
                    _Service.Clear();
                }
            }
            _Disposed = true;
        }
    }

    public void Dispose()
    {
        // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }
}
