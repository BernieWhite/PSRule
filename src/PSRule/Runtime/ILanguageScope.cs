// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using PSRule.Configuration;
using PSRule.Definitions;
using PSRule.Definitions.Rules;
using PSRule.Pipeline;

namespace PSRule.Runtime;

/// <summary>
/// A named scope for language elements.
/// </summary>
internal interface ILanguageScope : IDisposable
{
    /// <summary>
    /// The name of the scope.
    /// </summary>
    string Name { get; }

    BindingOption Binding { get; }

    /// <summary>
    /// Get an ordered culture preference list which will be tries for finding help.
    /// </summary>
    string[] Culture { get; }

    void Configure(OptionContext context);

    /// <summary>
    /// Try to get a specific configuration value by name.
    /// </summary>
    bool TryConfigurationValue(string key, out object value);

    void WithFilter(IResourceFilter resourceFilter);

    /// <summary>
    /// Get a filter for a specific resource kind.
    /// </summary>
    IResourceFilter GetFilter(ResourceKind kind);

    /// <summary>
    /// Add a service to the scope.
    /// </summary>
    void AddService(string name, object service);

    /// <summary>
    /// Get a previously added service.
    /// </summary>
    object GetService(string name);

    bool TryGetType(object o, out string type, out string path);

    bool TryGetName(object o, out string name, out string path);

    bool TryGetScope(object o, out string[] scope);

    bool TryGetOverride(ResourceId id, out RuleOverride propertyOverride);
}
