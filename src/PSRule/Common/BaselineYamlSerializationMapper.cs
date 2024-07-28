// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Management.Automation;
using PSRule.Configuration;
using PSRule.Definitions;
using PSRule.Definitions.Baselines;
using PSRule.Options;
using YamlDotNet.Core;
using YamlDotNet.Core.Events;

namespace PSRule;

internal static class BaselineYamlSerializationMapper
{
    private const string SYNOPSIS_COMMENT = "Synopsis: ";

    internal static void MapBaseline(IEmitter emitter, Baseline baseline)
    {
        emitter.Emit(new MappingStart());
        emitter.Emit(new Comment(string.Concat(SYNOPSIS_COMMENT, baseline.Synopsis), isInline: false));

        if (baseline != null)
        {
            MapProperty(emitter, nameof(baseline.ApiVersion), baseline.ApiVersion);
            MapProperty(emitter, nameof(baseline.Kind), baseline.Kind);
            MapResourceMetadata(emitter, nameof(baseline.Metadata), baseline.Metadata);
            MapBaselineSpec(emitter, nameof(baseline.Spec), baseline.Spec);
        }

        emitter.Emit(new MappingEnd());
    }

    private static void MapResourceMetadata(IEmitter emitter, string propertyName, ResourceMetadata resourceMetadata)
    {
        if (resourceMetadata == null)
            return;

        MapPropertyName(emitter, propertyName);
        emitter.Emit(new MappingStart());
        MapProperty(emitter, nameof(resourceMetadata.Annotations), resourceMetadata.Annotations);
        MapProperty(emitter, nameof(resourceMetadata.Name), resourceMetadata.Name);
        MapProperty(emitter, nameof(resourceMetadata.Tags), resourceMetadata.Tags);
        emitter.Emit(new MappingEnd());
    }

    private static void MapBaselineSpec(IEmitter emitter, string propertyName, BaselineSpec baselineSpec)
    {
        if (baselineSpec == null)
            return;

        MapPropertyName(emitter, propertyName);
        emitter.Emit(new MappingStart());
        MapProperty(emitter, nameof(baselineSpec.Binding), baselineSpec.Binding);
        MapProperty(emitter, nameof(baselineSpec.Configuration), baselineSpec.Configuration);
        //MapProperty(emitter, nameof(baselineSpec.Convention), baselineSpec.Convention);
        MapProperty(emitter, nameof(baselineSpec.Override), baselineSpec.Override);
        MapProperty(emitter, nameof(baselineSpec.Rule), baselineSpec.Rule);
        emitter.Emit(new MappingEnd());
    }

    private static void MapPropertyName(IEmitter emitter, string propertyName)
    {
        emitter.Emit(new Scalar(propertyName.ToCamelCase()));
    }

    /// <summary>
    /// Map a dictionary property.
    /// </summary>
    private static void MapProperty<T>(IEmitter emitter, string propertyName, IDictionary<string, T> value)
    {
        if (value.NullOrEmpty())
            return;

        MapPropertyName(emitter, propertyName);
        MapDictionary(emitter, value);
    }

    /// <summary>
    /// Map a nullable boolean property.
    /// </summary>
    private static void MapProperty(IEmitter emitter, string propertyName, bool? value)
    {
        if (!value.HasValue)
            return;

        MapPropertyName(emitter, propertyName);
        emitter.Emit(new Scalar(value.ToString()));
    }

    /// <summary>
    /// Map an enum property.
    /// </summary>
    private static void MapProperty<T>(IEmitter emitter, string propertyName, T value) where T : Enum
    {
        if (value == null)
            return;

        MapPropertyName(emitter, propertyName);
        emitter.Emit(new Scalar(Enum.GetName(typeof(T), value)));
    }

    /// <summary>
    /// Map a string array property.
    /// </summary>
    private static void MapProperty(IEmitter emitter, string propertyName, string[] value)
    {
        if (value == null)
            return;

        MapPropertyName(emitter, propertyName);
        MapStringArraySequence(emitter, value);
    }

    /// <summary>
    /// Map a string property.
    /// </summary>
    private static void MapProperty(IEmitter emitter, string propertyName, string value)
    {
        if (value == null)
            return;

        MapPropertyName(emitter, propertyName);
        emitter.Emit(new Scalar(value));
    }

    /// <summary>
    /// Map a BindingOption property.
    /// </summary>
    private static void MapProperty(IEmitter emitter, string propertyName, BindingOption value)
    {
        if (value == null)
            return;

        MapPropertyName(emitter, propertyName);
        emitter.Emit(new MappingStart());
        MapProperty(emitter, nameof(value.Field), value.Field?.GetFieldMap);
        MapProperty(emitter, nameof(value.IgnoreCase), value.IgnoreCase);
        MapProperty(emitter, nameof(value.NameSeparator), value.NameSeparator);
        MapProperty(emitter, nameof(value.PreferTargetInfo), value.PreferTargetInfo);
        MapProperty(emitter, nameof(value.TargetName), value.TargetName);
        MapProperty(emitter, nameof(value.TargetType), value.TargetType);
        MapProperty(emitter, nameof(value.UseQualifiedName), value.UseQualifiedName);
        emitter.Emit(new MappingEnd());
    }

    /// <summary>
    /// Map a ConventionOption property.
    /// </summary>
    private static void MapProperty(IEmitter emitter, string propertyName, ConventionOption value)
    {
        if (value == null)
            return;

        MapPropertyName(emitter, propertyName);
        emitter.Emit(new MappingStart());
        MapProperty(emitter, propertyName, value.Include);
        emitter.Emit(new MappingEnd());
    }

    /// <summary>
    /// Map a OverrideOption property.
    /// </summary>
    private static void MapProperty(IEmitter emitter, string propertyName, OverrideOption value)
    {
        if (value == null)
            return;

        MapPropertyName(emitter, propertyName);
        emitter.Emit(new MappingStart());
        MapProperty(emitter, nameof(value.Level), value.Level?.ToDictionary());
        emitter.Emit(new MappingEnd());
    }

    /// <summary>
    /// Map a RuleOption property.
    /// </summary>
    private static void MapProperty(IEmitter emitter, string propertyName, RuleOption value)
    {
        if (value == null)
            return;

        MapPropertyName(emitter, propertyName);
        emitter.Emit(new MappingStart());
        MapProperty(emitter, nameof(value.Exclude), value.Exclude);
        MapProperty(emitter, nameof(value.Include), value.Include);
        MapProperty(emitter, nameof(value.IncludeLocal), value.IncludeLocal);
        MapProperty(emitter, nameof(value.Tag), value.Tag?.ToDictionary());
        emitter.Emit(new MappingEnd());
    }

    private static void MapDictionary<T>(IEmitter emitter, IDictionary<string, T> dictionary)
    {
        emitter.Emit(new MappingStart());
        foreach (var kvp in dictionary.ToSortedDictionary())
        {
            MapPropertyName(emitter, kvp.Key);
            if (kvp.Value is string stringValue)
                emitter.Emit(new Scalar(stringValue));
            else if (kvp.Value is string[] stringValues)
                MapStringArraySequence(emitter, stringValues);
            else if (kvp.Value is PSObject[] psObjects)
                MapPSObjectArraySequence(emitter, psObjects);
            else
                emitter.Emit(new Scalar(kvp.Value.ToString()));
        }
        emitter.Emit(new MappingEnd());
    }

    private static void MapStringArraySequence(IEmitter emitter, string[] sequence)
    {
        emitter.Emit(new SequenceStart(anchor: null, tag: null, isImplicit: false, style: SequenceStyle.Block));
        var sortedSequence = sequence.OrderBy(item => item);
        foreach (var item in sortedSequence)
            emitter.Emit(new Scalar(item));

        emitter.Emit(new SequenceEnd());
    }

    private static void MapPSObjectArraySequence(IEmitter emitter, PSObject[] sequence)
    {
        emitter.Emit(new SequenceStart(anchor: null, tag: null, isImplicit: false, style: SequenceStyle.Block));
        foreach (var obj in sequence)
        {
            if (obj.BaseObject == null || obj.HasNoteProperty())
            {
                emitter.Emit(new MappingStart());
                var sortedProperties = obj.Properties.OrderBy(prop => prop.Name);
                foreach (var propertyInfo in sortedProperties)
                {
                    MapPropertyName(emitter, propertyInfo.Name);
                    emitter.Emit(new Scalar(propertyInfo.Value.ToString()));
                }
                emitter.Emit(new MappingEnd());
            }
            else
                emitter.Emit(new Scalar(obj.BaseObject.ToString()));
        }
        emitter.Emit(new SequenceEnd());
    }
}
