// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using PSRule.Configuration;
using PSRule.Definitions;
using PSRule.Definitions.Rules;
using PSRule.Pipeline;
using PSRule.Runtime;

namespace PSRule;

internal sealed class MockLanguageScope : ILanguageScope
{
    internal RuleFilter RuleFilter;

    public MockLanguageScope(string name)
    {
        Name = name;
    }

    public string Name { get; }

    public BindingOption Binding => throw new System.NotImplementedException();

    public string[] Culture => throw new System.NotImplementedException();

    public void AddService(string name, object service)
    {

    }

    public void Configure(OptionContext context)
    {
        throw new NotImplementedException();
    }

    public void Dispose()
    {

    }

    public IResourceFilter GetFilter(ResourceKind kind)
    {
        throw new System.NotImplementedException();
    }

    public object GetService(string name)
    {
        throw new System.NotImplementedException();
    }

    public bool TryConfigurationValue(string key, out object value)
    {
        throw new System.NotImplementedException();
    }

    public bool TryGetName(object o, out string name, out string path)
    {
        throw new System.NotImplementedException();
    }

    public bool TryGetOverride(ResourceId id, out RuleOverride propertyOverride)
    {
        throw new NotImplementedException();
    }

    public bool TryGetScope(object o, out string[] scope)
    {
        throw new System.NotImplementedException();
    }

    public bool TryGetType(object o, out string type, out string path)
    {
        throw new System.NotImplementedException();
    }

    public void WithFilter(IResourceFilter resourceFilter)
    {
        if (resourceFilter is RuleFilter ruleFilter)
            RuleFilter = ruleFilter;
    }
}
