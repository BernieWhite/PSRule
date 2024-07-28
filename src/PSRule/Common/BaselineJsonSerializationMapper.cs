// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Management.Automation;
using Newtonsoft.Json;
using PSRule.Configuration;
using PSRule.Definitions;
using PSRule.Definitions.Baselines;
using PSRule.Options;

namespace PSRule;

internal static class BaselineJsonSerializationMapper
{
    private const string SYNOPSIS_COMMENT = "Synopsis: ";

    internal static void MapBaseline(JsonWriter writer, JsonSerializer serializer, Baseline baseline)
    {
        writer.WriteStartObject();
        writer.WriteComment(string.Concat(SYNOPSIS_COMMENT, baseline.Synopsis));
        if (baseline != null)
        {
            MapProperty(writer, nameof(baseline.ApiVersion), baseline.ApiVersion);
            MapProperty(writer, nameof(baseline.Kind), baseline.Kind);
            MapResourceMetadata(writer, serializer, nameof(baseline.Metadata), baseline.Metadata);
            MapBaselineSpec(writer, serializer, nameof(baseline.Spec), baseline.Spec);
        }
        writer.WriteEndObject();
    }

    private static void MapResourceMetadata(JsonWriter writer, JsonSerializer serializer, string propertyName, ResourceMetadata resourceMetadata)
    {
        if (resourceMetadata == null)
            return;

        MapPropertyName(writer, propertyName);
        writer.WriteStartObject();
        MapProperty(writer, serializer, nameof(resourceMetadata.Annotations), resourceMetadata.Annotations);
        MapProperty(writer, nameof(resourceMetadata.Name), resourceMetadata.Name);
        MapProperty(writer, serializer, nameof(resourceMetadata.Tags), resourceMetadata.Tags);
        writer.WriteEndObject();
    }

    private static void MapBaselineSpec(JsonWriter writer, JsonSerializer serializer, string propertyName, BaselineSpec baselineSpec)
    {
        if (baselineSpec == null)
            return;

        MapPropertyName(writer, propertyName);
        writer.WriteStartObject();
        MapProperty(writer, serializer, nameof(baselineSpec.Binding), baselineSpec.Binding);
        MapProperty(writer, serializer, nameof(baselineSpec.Configuration), baselineSpec.Configuration);
        //MapProperty(writer, nameof(baselineSpec.Convention), baselineSpec.Convention);
        MapProperty(writer, serializer, nameof(baselineSpec.Override), baselineSpec.Override);
        MapProperty(writer, serializer, nameof(baselineSpec.Rule), baselineSpec.Rule);
        writer.WriteEndObject();
    }

    private static void MapPropertyName(JsonWriter writer, string propertyName)
    {
        writer.WritePropertyName(propertyName.ToCamelCase());
    }

    /// <summary>
    /// Map a dictionary property.
    /// </summary>
    private static void MapProperty<T>(JsonWriter writer, JsonSerializer serializer, string propertyName, IDictionary<string, T> value)
    {
        if (value.NullOrEmpty())
            return;

        MapPropertyName(writer, propertyName);
        MapDictionary(writer, serializer, value);
    }

    /// <summary>
    /// Map a nullable boolean property.
    /// </summary>
    private static void MapProperty(JsonWriter writer, JsonSerializer serializer, string propertyName, bool? value)
    {
        if (!value.HasValue)
            return;

        MapPropertyName(writer, propertyName);
        serializer.Serialize(writer, value);
    }

    /// <summary>
    /// Map an enum property.
    /// </summary>
    private static void MapProperty<T>(JsonWriter writer, string propertyName, T value) where T : Enum
    {
        if (value == null)
            return;

        MapPropertyName(writer, propertyName);
        writer.WriteValue(Enum.GetName(typeof(T), value));
    }

    /// <summary>
    /// Map a string array property.
    /// </summary>
    private static void MapProperty(JsonWriter writer, string propertyName, string[] value)
    {
        if (value == null)
            return;

        MapPropertyName(writer, propertyName);
        MapStringArraySequence(writer, value);
    }

    /// <summary>
    /// Map a string property.
    /// </summary>
    private static void MapProperty(JsonWriter writer, string propertyName, string value)
    {
        if (value == null)
            return;

        MapPropertyName(writer, propertyName);
        writer.WriteValue(value);
    }

    /// <summary>
    /// Map a BindingOption property.
    /// </summary>
    private static void MapProperty(JsonWriter writer, JsonSerializer serializer, string propertyName, BindingOption value)
    {
        if (value == null)
            return;

        MapPropertyName(writer, propertyName);
        writer.WriteStartObject();
        MapProperty(writer, serializer, nameof(value.Field), value.Field?.GetFieldMap);
        MapProperty(writer, serializer, nameof(value.IgnoreCase), value.IgnoreCase);
        MapProperty(writer, nameof(value.NameSeparator), value.NameSeparator);
        MapProperty(writer, serializer, nameof(value.PreferTargetInfo), value.PreferTargetInfo);
        MapProperty(writer, nameof(value.TargetName), value.TargetName);
        MapProperty(writer, nameof(value.TargetType), value.TargetType);
        MapProperty(writer, serializer, nameof(value.UseQualifiedName), value.UseQualifiedName);
        writer.WriteEndObject();
    }

    /// <summary>
    /// Map a ConventionOption property.
    /// </summary>
    private static void MapProperty(JsonWriter writer, string propertyName, ConventionOption value)
    {
        if (value == null)
            return;

        MapPropertyName(writer, propertyName);
        writer.WriteStartObject();
        MapProperty(writer, nameof(value.Include), value.Include);
        writer.WriteEndObject();
    }

    /// <summary>
    /// Map a OverrideOption property.
    /// </summary>
    private static void MapProperty(JsonWriter writer, JsonSerializer serializer, string propertyName, OverrideOption value)
    {
        if (value == null)
            return;

        MapPropertyName(writer, propertyName);
        writer.WriteStartObject();
        MapProperty(writer, serializer, nameof(value.Level), value.Level?.ToDictionary());
        writer.WriteEndObject();
    }

    /// <summary>
    /// Map a RuleOption property.
    /// </summary>
    private static void MapProperty(JsonWriter writer, JsonSerializer serializer, string propertyName, RuleOption value)
    {
        if (value == null)
            return;

        MapPropertyName(writer, propertyName);
        writer.WriteStartObject();
        MapProperty(writer, nameof(value.Exclude), value.Exclude);
        MapProperty(writer, nameof(value.Include), value.Include);
        MapProperty(writer, serializer, nameof(value.IncludeLocal), value.IncludeLocal);
        MapProperty(writer, serializer, nameof(value.Tag), value.Tag?.ToDictionary());
        writer.WriteEndObject();
    }

    private static void MapDictionary<T>(JsonWriter writer, JsonSerializer serializer, IDictionary<string, T> dictionary)
    {
        writer.WriteStartObject();
        foreach (var kvp in dictionary.ToSortedDictionary())
        {
            MapPropertyName(writer, kvp.Key);
            if (kvp.Value is string stringValue)
                writer.WriteValue(stringValue);
            else if (kvp.Value is string[] stringValues)
                MapStringArraySequence(writer, stringValues);
            else if (kvp.Value is PSObject[] psObjects)
                MapPSObjectArraySequence(writer, serializer, psObjects);
            else
                serializer.Serialize(writer, kvp.Value);
        }
        writer.WriteEndObject();
    }

    private static void MapStringArraySequence(JsonWriter writer, string[] sequence)
    {
        writer.WriteStartArray();
        var sortedSequence = sequence.OrderBy(item => item);
        foreach (var item in sortedSequence)
            writer.WriteValue(item);

        writer.WriteEndArray();
    }

    private static void MapPSObjectArraySequence(JsonWriter writer, JsonSerializer serializer, PSObject[] sequence)
    {
        writer.WriteStartArray();
        foreach (var obj in sequence)
        {
            if (obj.BaseObject == null || obj.HasNoteProperty())
            {
                writer.WriteStartObject();
                var sortedProperties = obj.Properties.OrderBy(prop => prop.Name);
                foreach (var propertyInfo in sortedProperties)
                {
                    MapPropertyName(writer, propertyInfo.Name);
                    serializer.Serialize(writer, propertyInfo.Value);
                }
                writer.WriteEndObject();
            }
            else
                serializer.Serialize(writer, obj.BaseObject);
        }
        writer.WriteEndArray();
    }
}
