// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Management.Automation;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using PSRule.Annotations;
using PSRule.Configuration;
using PSRule.Converters;
using PSRule.Data;
using PSRule.Definitions;
using PSRule.Definitions.Baselines;
using PSRule.Definitions.Expressions;
using PSRule.Pipeline;
using PSRule.Pipeline.Emitters;
using PSRule.Resources;
using PSRule.Runtime;

namespace PSRule;

/// <summary>
/// A base <seealso cref="PSObject"/> converter.
/// </summary>
internal abstract class PSObjectBaseConverter : JsonConverter
{
    /// <summary>
    /// Skip JSON comments.
    /// </summary>
    protected static bool SkipComments(JsonReader reader)
    {
        var hasComments = false;
        while (reader.TokenType == JsonToken.Comment && reader.Read())
            hasComments = true;

        return hasComments;
    }

    protected static void ReadObject(PSObject value, JsonReader reader, bool bindTargetInfo, IFileInfo sourceInfo)
    {
        SkipComments(reader);
        var path = reader.Path;
        if (reader.TokenType != JsonToken.StartObject || !reader.Read())
            throw new PipelineSerializationException(PSRuleResources.ReadJsonFailedExpectedToken, Enum.GetName(typeof(JsonToken), reader.TokenType), reader.Path);

        string name = null;
        var lineNumber = 0;
        var linePosition = 0;

        if (bindTargetInfo && reader is IJsonLineInfo lineInfo && lineInfo.HasLineInfo())
        {
            lineNumber = lineInfo.LineNumber;
            linePosition = lineInfo.LinePosition;
        }

        // Read each token
        while (reader.TokenType != JsonToken.EndObject)
        {
            switch (reader.TokenType)
            {
                case JsonToken.PropertyName:
                    name = reader.Value.ToString();
                    if (string.IsNullOrEmpty(name))
                    {
                        reader.Skip();
                    }
                    else if (name == PSRuleTargetInfo.PropertyName)
                    {
                        var targetInfo = ReadInfo(reader);
                        if (targetInfo != null)
                            value.SetTargetInfo(targetInfo);
                    }
                    break;

                case JsonToken.StartObject:
                    var item = new PSObject();
                    ReadObject(item, reader, bindTargetInfo: false, sourceInfo: null);
                    value.Properties.Add(new PSNoteProperty(name, value: item));
                    break;

                case JsonToken.StartArray:
                    var items = ReadArray(reader: reader);
                    value.Properties.Add(new PSNoteProperty(name, value: items));
                    break;

                case JsonToken.Comment:
                    break;

                default:
                    value.Properties.Add(new PSNoteProperty(name, value: reader.Value));
                    break;
            }
            if (!reader.Read() || reader.TokenType == JsonToken.None)
                throw new PipelineSerializationException(PSRuleResources.ReadJsonFailedExpectedToken, Enum.GetName(typeof(JsonToken), reader.TokenType), reader.Path);
        }
        if (bindTargetInfo)
        {
            value.UseTargetInfo(out var info);
            info.SetSource(sourceInfo?.Path, lineNumber, linePosition);
            if (string.IsNullOrEmpty(info.Path))
                info.Path = path;
        }
    }

    protected static PSRuleTargetInfo ReadInfo(JsonReader reader)
    {
        if (!reader.Read() || reader.TokenType == JsonToken.None || reader.TokenType != JsonToken.StartObject)
            return null;

        var s = JsonSerializer.Create();
        return s.Deserialize<PSRuleTargetInfo>(reader);
    }

    protected static PSObject[] ReadArray(JsonReader reader)
    {
        SkipComments(reader);
        if (reader.TokenType != JsonToken.StartArray || !reader.Read())
            throw new PipelineSerializationException(PSRuleResources.ReadJsonFailedExpectedToken, Enum.GetName(typeof(JsonToken), reader.TokenType), reader.Path);

        var result = new List<PSObject>();

        // Read until the end of the array
        while (reader.TokenType != JsonToken.EndArray)
        {
            switch (reader.TokenType)
            {
                case JsonToken.StartObject:
                    var item = new PSObject();
                    ReadObject(item, reader, bindTargetInfo: false, sourceInfo: null);
                    result.Add(item);
                    break;

                case JsonToken.StartArray:
                    result.Add(PSObject.AsPSObject(ReadArray(reader)));
                    break;

                case JsonToken.Null:
                    result.Add(null);
                    break;

                case JsonToken.Comment:
                    break;

                default:
                    result.Add(PSObject.AsPSObject(reader.Value));
                    break;
            }
            if (!reader.Read() || reader.TokenType == JsonToken.None)
                throw new PipelineSerializationException(PSRuleResources.ReadJsonFailedExpectedToken, Enum.GetName(typeof(JsonToken), reader.TokenType), reader.Path);
        }
        return result.ToArray();
    }
}

/// <summary>
/// A custom serializer to correctly convert PSObject properties to JSON instead of CLIXML.
/// </summary>
internal sealed class PSObjectJsonConverter : PSObjectBaseConverter
{
    public override bool CanConvert(Type objectType)
    {
        return objectType == typeof(PSObject);
    }

    public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
    {
        if (value is not PSObject obj)
            throw new ArgumentException(message: PSRuleResources.SerializeNullPSObject, paramName: nameof(value));

        if (WriteFileSystemInfo(writer, value, serializer) || WriteBaseObject(writer, obj, serializer))
            return;

        writer.WriteStartObject();
        foreach (var property in obj.Properties)
        {
            // Ignore properties that are not readable or can cause race condition
            if (!property.IsGettable || property.Value is PSDriveInfo || property.Value is ProviderInfo || property.Value is DirectoryInfo)
                continue;

            writer.WritePropertyName(property.Name);
            serializer.Serialize(writer, property.Value);
        }
        writer.WriteEndObject();
    }

    public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
    {
        // Create target object based on JObject
        var result = existingValue as PSObject ?? new PSObject();

        // Read tokens
        ReadObject(result, reader, bindTargetInfo: true, sourceInfo: null);
        return result;
    }

    /// <summary>
    /// Serialize a file system info object.
    /// </summary>
    private static bool WriteFileSystemInfo(JsonWriter writer, object value, JsonSerializer serializer)
    {
        if (value is not FileSystemInfo fileSystemInfo)
            return false;

        serializer.Serialize(writer, fileSystemInfo.FullName);
        return true;
    }

    /// <summary>
    /// Serialize the base object.
    /// </summary>
    private static bool WriteBaseObject(JsonWriter writer, PSObject value, JsonSerializer serializer)
    {
        if (value.BaseObject == null || value.HasNoteProperty())
            return false;

        serializer.Serialize(writer, value.BaseObject);
        return true;
    }
}

/// <summary>
/// A custom serializer to convert PSObjects that may or may not be in a JSON array to an a PSObject array.
/// </summary>
internal sealed class PSObjectArrayJsonConverter : PSObjectBaseConverter
{
    private readonly IFileInfo _SourceInfo;

    public PSObjectArrayJsonConverter(IFileInfo sourceInfo)
    {
        _SourceInfo = sourceInfo;
    }

    public override bool CanConvert(Type objectType)
    {
        return objectType == typeof(PSObject[]);
    }

    public override bool CanWrite => false;

    public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
    {
        throw new NotImplementedException();
    }

    public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
    {
        SkipComments(reader);
        if (reader.TokenType == JsonToken.Comment && !reader.Read())
            return Array.Empty<PSObject>();

        if (reader.TokenType != JsonToken.StartObject && reader.TokenType != JsonToken.StartArray)
            throw new PipelineSerializationException(PSRuleResources.ReadJsonFailedExpectedToken, Enum.GetName(typeof(JsonToken), reader.TokenType), reader.Path);

        var parser = reader as JsonEmitterParser;
        var fileInfo = parser?.Info ?? _SourceInfo;

        var result = new List<PSObject>();
        var isArray = reader.TokenType == JsonToken.StartArray;

        if (isArray)
            reader.Read();

        while (reader.TokenType != JsonToken.None && (!isArray || (isArray && reader.TokenType != JsonToken.EndArray)))
        {
            if (SkipComments(reader))
                continue;

            var value = new PSObject();
            ReadObject(value, reader, bindTargetInfo: true, sourceInfo: fileInfo);
            result.Add(value);

            // Consume the EndObject token.
            reader.Read();
        }
        return result.ToArray();
    }
}

/// <summary>
/// A custom serializer to convert ErrorCategory to a string.
/// </summary>
internal sealed class ErrorCategoryJsonConverter : JsonConverter
{
    public override bool CanConvert(Type objectType)
    {
        return objectType == typeof(ErrorCategory);
    }

    public override bool CanWrite => true;

    public override bool CanRead => false;

    public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
    {
        writer.WriteValue(Enum.GetName(typeof(ErrorCategory), value));
    }

    public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
    {
        throw new NotImplementedException();
    }
}

/// <summary>
/// A custom serializer to convert Baseline object to JSON
/// </summary>
internal sealed class BaselineJsonConverter : JsonConverter
{
    public override bool CanConvert(Type objectType)
    {
        return objectType == typeof(Baseline);
    }

    public override bool CanWrite => true;

    public override bool CanRead => false;

    public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
    {
        BaselineJsonSerializationMapper.MapBaseline(writer, serializer, value as Baseline);
    }

    public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
    {
        throw new NotImplementedException();
    }
}

/// <summary>
/// A contract resolver to order properties alphabetically
/// </summary>
internal sealed class OrderedPropertiesContractResolver : DefaultContractResolver
{
    public OrderedPropertiesContractResolver() : base()
    {
        NamingStrategy = new CamelCaseNamingStrategy
        {
            ProcessDictionaryKeys = true,
            OverrideSpecifiedNames = true
        };
    }

    protected override IList<JsonProperty> CreateProperties(Type type, MemberSerialization memberSerialization)
    {
        return base
            .CreateProperties(type, memberSerialization)
            .OrderBy(prop => prop.PropertyName)
            .OrderBy(prop => prop.Order)
            .ToList();
    }
}

/// <summary>
/// A custom deserializer to convert JSON into a <see cref="ResourceObject"/>.
/// </summary>
internal sealed class ResourceObjectJsonConverter : JsonConverter
{
    private const string FIELD_APIVERSION = "apiVersion";
    private const string FIELD_KIND = "kind";
    private const string FIELD_METADATA = "metadata";
    private const string FIELD_SPEC = "spec";
    private const string FIELD_SYNOPSIS = "Synopsis: ";

    public override bool CanRead => true;

    public override bool CanWrite => false;

    private readonly SpecFactory _Factory;

    public ResourceObjectJsonConverter()
    {
        _Factory = new SpecFactory();
    }

    public override bool CanConvert(Type objectType)
    {
        return objectType == typeof(ResourceObject);
    }

    public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
    {
        var resource = MapResource(reader, serializer);
        return new ResourceObject(resource);
    }

    public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
    {
        throw new NotImplementedException();
    }

    private IResource MapResource(JsonReader reader, JsonSerializer serializer)
    {
        reader.GetSourceExtent(RunspaceContext.CurrentThread.Source.File, out var extent);
        reader.SkipComments(out _);
        if (reader.TokenType != JsonToken.StartObject || !reader.Read())
            throw new PipelineSerializationException(PSRuleResources.ReadJsonFailed);

        IResource result = null;
        string apiVersion = null;
        string kind = null;
        ResourceMetadata metadata = null;
        CommentMetadata comment = null;

        while (reader.TokenType != JsonToken.EndObject)
        {
            if (reader.TokenType == JsonToken.PropertyName)
            {
                var propertyName = reader.Value.ToString();

                // Read apiVersion
                if (TryApiVersion(reader, propertyName, apiVersion: out var apiVersionValue))
                {
                    apiVersion = apiVersionValue;
                }

                // Read kind
                else if (TryKind(reader, propertyName, kind: out var kindValue))
                {
                    kind = kindValue;
                }

                // Read metadata
                else if (TryMetadata(
                    reader: reader,
                    serializer: serializer,
                    propertyName: propertyName,
                    metadata: out var metadataValue))
                {
                    metadata = metadataValue;
                }

                // Try Spec
                else if (kind != null && TrySpec(
                    reader: reader,
                    serializer: serializer,
                    propertyName: propertyName,
                    apiVersion: apiVersion,
                    kind: kind,
                    metadata: metadata,
                    comment: comment,
                    extent: extent,
                    spec: out var specValue))
                {
                    result = specValue;

                    // Break out of loop if result is populated
                    // Needed so we don't read more than we have to
                    if (result != null && reader.TokenType == JsonToken.EndObject)
                        break;
                }
                else
                {
                    reader.Skip();
                }
            }

            else if (reader.TokenType == JsonToken.Comment)
            {
                var commentLine = reader.Value.ToString().TrimStart();
                if (commentLine.Length > FIELD_SYNOPSIS.Length && commentLine.StartsWith(FIELD_SYNOPSIS))
                {
                    comment = new CommentMetadata
                    {
                        Synopsis = commentLine.Substring(FIELD_SYNOPSIS.Length)
                    };
                }
            }
            reader.Read();
        }
        return result;
    }

    private static bool TryApiVersion(JsonReader reader, string propertyName, out string apiVersion)
    {
        apiVersion = null;
        if (propertyName == FIELD_APIVERSION)
        {
            apiVersion = reader.ReadAsString();
            return true;
        }
        return false;
    }

    private static bool TryKind(JsonReader reader, string propertyName, out string kind)
    {
        kind = null;
        if (propertyName == FIELD_KIND)
        {
            kind = reader.ReadAsString();
            return true;
        }
        return false;
    }

    private static bool TryMetadata(JsonReader reader, JsonSerializer serializer, string propertyName, out ResourceMetadata metadata)
    {
        metadata = null;
        if (propertyName == FIELD_METADATA)
        {
            if (reader.Read() && reader.TokenType == JsonToken.StartObject)
            {
                metadata = serializer.Deserialize<ResourceMetadata>(reader);
                return true;
            }
        }
        return false;
    }

    private bool TrySpec(
        JsonReader reader,
        JsonSerializer serializer,
        string propertyName,
        string apiVersion,
        string kind,
        ResourceMetadata metadata,
        CommentMetadata comment,
        ISourceExtent extent,
        out IResource spec)
    {
        spec = null;
        if (propertyName == FIELD_SPEC && _Factory.TryDescriptor(apiVersion: apiVersion, name: kind, descriptor: out var descriptor))
        {
            if (reader.Read() && reader.TokenType == JsonToken.StartObject)
            {
                reader.SkipComments(out _);
                var deserializedSpec = serializer.Deserialize(reader, objectType: descriptor.SpecType);
                spec = descriptor.CreateInstance(
                    source: RunspaceContext.CurrentThread.Source.File,
                    metadata: metadata,
                    comment: comment,
                    extent: extent,
                    spec: deserializedSpec
                );
                return true;
            }
        }
        return false;
    }
}

/// <summary>
/// A JSON converter for de/serializing a field map.
/// </summary>
internal sealed class FieldMapJsonConverter : JsonConverter
{
    public override bool CanRead => true;

    public override bool CanWrite => true;

    public override bool CanConvert(Type objectType)
    {
        return objectType == typeof(FieldMap);
    }

    public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
    {
        var fieldMap = existingValue as FieldMap ?? new FieldMap();
        ReadFieldMap(fieldMap, reader);
        return fieldMap;
    }

    public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
    {
        if (value is not FieldMap map) return;

        writer.WriteStartObject();
        foreach (var field in map)
        {
            writer.WritePropertyName(field.Key);
            serializer.Serialize(writer, field.Value);
        }
        writer.WriteEndObject();
    }

    private static void ReadFieldMap(FieldMap map, JsonReader reader)
    {
        if (reader.TokenType != JsonToken.StartObject || !reader.Read())
            throw new PipelineSerializationException(PSRuleResources.ReadJsonFailed);

        string propertyName = null;
        while (reader.TokenType != JsonToken.EndObject)
        {
            if (reader.TokenType == JsonToken.PropertyName)
            {
                propertyName = reader.Value.ToString();
            }

            else if (reader.TokenType == JsonToken.StartArray)
            {
                var items = new List<string>();

                while (reader.TokenType != JsonToken.EndArray)
                {
                    if (SkipComments(reader))
                        continue;

                    var item = reader.ReadAsString();
                    if (!string.IsNullOrEmpty(item))
                    {
                        items.Add(item);
                    }
                }

                map.Set(propertyName, items.ToArray());
            }

            reader.Read();
        }
    }

    /// <summary>
    /// Skip JSON comments.
    /// </summary>
    private static bool SkipComments(JsonReader reader)
    {
        var hasComments = false;
        while (reader.TokenType == JsonToken.Comment && reader.Read())
            hasComments = true;

        return hasComments;
    }
}

/// <summary>
/// A JSON converter that handles string to string array.
/// </summary>
internal sealed class StringArrayJsonConverter : JsonConverter
{
    public override bool CanRead => true;
    public override bool CanWrite => false;

    public override bool CanConvert(Type objectType)
    {
        return typeof(string[]).IsAssignableFrom(objectType);
    }

    public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
    {
        if (reader.TryConsume(JsonToken.StartArray))
        {
            reader.SkipComments(out _);
            var result = new List<string>();
            while (reader.TryConsume(JsonToken.String, out var s_object) && s_object is string s)
            {
                result.Add(s);
                reader.SkipComments(out _);
            }
            return result.ToArray();
        }
        else if (reader.TokenType == JsonToken.String && reader.Value is string s)
        {
            return new string[] { s };
        }
        return null;
    }

    public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
    {
        throw new NotImplementedException();
    }
}

/// <summary>
/// A custom converter for deserializing JSON into a language expression.
/// </summary>
internal sealed class LanguageExpressionJsonConverter : JsonConverter
{
    private const string OPERATOR_IF = "if";

    private readonly LanguageExpressionFactory _Factory;
    private readonly FunctionBuilder _FunctionBuilder;

    public LanguageExpressionJsonConverter()
    {
        _Factory = new LanguageExpressionFactory();
        _FunctionBuilder = new FunctionBuilder();
    }

    public override bool CanRead => true;
    public override bool CanWrite => false;

    public override bool CanConvert(Type objectType)
    {
        return typeof(LanguageExpression).IsAssignableFrom(objectType);
    }

    public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
    {
        var expression = MapOperator(OPERATOR_IF, null, null, reader);
        return new LanguageIf(expression);
    }

    public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Map an operator.
    /// </summary>
    private LanguageOperator MapOperator(string type, LanguageExpression.PropertyBag properties, LanguageExpression subselector, JsonReader reader)
    {
        if (TryExpression(type, properties, out LanguageOperator result))
        {
            reader.SkipComments(out _);

            // If and Not
            if (reader.TryConsume(JsonToken.StartObject))
            {
                result.Add(MapExpression(reader));
                if (type != "if")
                    reader.Consume(JsonToken.EndObject);
            }
            // AllOf and AnyOf
            else if (reader.TryConsume(JsonToken.StartArray))
            {
                while (reader.TokenType != JsonToken.EndArray)
                {
                    if (reader.SkipComments(out var hasComments) && hasComments)
                        continue;

                    if (reader.TryConsume(JsonToken.StartObject))
                    {
                        result.Add(MapExpression(reader));
                        reader.Consume(JsonToken.EndObject);
                    }
                    if (reader.TokenType == JsonToken.EndObject)
                        throw new PipelineSerializationException(PSRuleResources.ReadJsonFailedExpectedToken, Enum.GetName(typeof(JsonToken), reader.TokenType), reader.Path);
                }
                reader.Consume(JsonToken.EndArray);
                reader.SkipComments(out _);
            }
            result.Subselector = subselector;
        }
        return result;
    }

    private LanguageExpression MapCondition(string type, LanguageExpression.PropertyBag properties, JsonReader reader)
    {
        if (TryExpression(type, null, out LanguageCondition result))
        {
            while (reader.TokenType != JsonToken.EndObject)
            {
                MapProperty(properties, reader, out _, out _);
            }
            result.Add(properties);
        }
        return result;
    }

    private LanguageExpression MapExpression(JsonReader reader)
    {
        LanguageExpression result = null;
        var properties = new LanguageExpression.PropertyBag();
        reader.SkipComments(out _);
        MapProperty(properties, reader, out var key, out var subselector);
        if (key != null && TryCondition(key))
        {
            result = MapCondition(key, properties, reader);
        }
        else if ((reader.TokenType == JsonToken.StartObject || reader.TokenType == JsonToken.StartArray) &&
            TryOperator(key))
        {
            var op = MapOperator(key, properties, subselector, reader);
            MapProperty(properties, reader, out _, out subselector);
            if (subselector != null)
                op.Subselector = subselector;

            result = op;
        }
        return result;
    }

    private ExpressionFnOuter MapFunction(string type, JsonReader reader)
    {
        _FunctionBuilder.Push();
        while (reader.TokenType != JsonToken.EndObject)
        {
            if (reader.Value is string name)
            {
                reader.Consume(JsonToken.PropertyName);
                if (reader.TryConsume(JsonToken.StartObject))
                {
                    var child = MapFunction(name, reader);
                    _FunctionBuilder.Add(name, child);
                    reader.Consume(JsonToken.EndObject);
                }
                else if (reader.TryConsume(JsonToken.StartArray))
                {
                    var sequence = MapSequence(name, reader);
                    _FunctionBuilder.Add(name, sequence);
                    reader.Consume(JsonToken.EndArray);
                }
                else
                {
                    _FunctionBuilder.Add(name, reader.Value);
                    reader.Read();
                }
            }
        }
        var result = _FunctionBuilder.Pop();
        return result;
    }

    private object MapSequence(string name, JsonReader reader)
    {
        var result = new List<object>();
        while (reader.TokenType != JsonToken.EndArray)
        {
            if (reader.TryConsume(JsonToken.StartObject))
            {
                var child = MapFunction(name, reader);
                result.Add(child);
                reader.Consume(JsonToken.EndObject);
            }
            else
            {
                result.Add(reader.Value);
                reader.Read();
            }
        }
        return result.ToArray();
    }

    private void MapProperty(LanguageExpression.PropertyBag properties, JsonReader reader, out string name, out LanguageExpression subselector)
    {
        //if (reader.TokenType != JsonToken.StartObject || !reader.Read())
        //    throw new PipelineSerializationException(PSRuleResources.ReadJsonFailedExpectedToken, Enum.GetName(typeof(JsonToken), reader.TokenType));

        name = null;
        subselector = null;
        reader.SkipComments(out _);
        while (reader.TokenType == JsonToken.PropertyName)
        {
            var key = reader.Value.ToString();
            if (TryCondition(key) || TryOperator(key))
                name = key;

            if (reader.Read())
            {
                // value:
                if (TryValue(key, reader, out var value))
                {
                    properties[key] = value;
                    reader.Read();
                }
                else if (TryCondition(key) && reader.TryConsume(JsonToken.StartObject))
                {
                    if (TryFunction(reader, key, out var fn))
                        properties.Add(key, fn);

                    reader.Consume(JsonToken.EndObject);
                }
                // where:
                else if (TrySubSelector(key) && reader.TryConsume(JsonToken.StartObject))
                {
                    subselector = MapExpression(reader);
                    reader.Consume(JsonToken.EndObject);
                }
                else if (reader.TokenType == JsonToken.StartObject)
                {
                    break;
                }
                else if (reader.TokenType == JsonToken.StartArray)
                {
                    if (!TryCondition(key))
                        break;

                    var objects = new List<string>();
                    while (reader.TokenType != JsonToken.EndArray)
                    {
                        if (reader.SkipComments(out var hasComments) && hasComments)
                            continue;

                        var item = reader.ReadAsString();
                        if (!string.IsNullOrEmpty(item))
                            objects.Add(item);
                    }
                    properties.Add(key, objects.ToArray());
                    reader.Consume(JsonToken.EndArray);
                }
                else
                {
                    properties.Add(key, reader.Value);
                    reader.Read();
                }
            }
            reader.SkipComments(out _);
        }
    }

    private bool TrySubSelector(string key)
    {
        return _Factory.IsSubselector(key);
    }

    private bool TryOperator(string key)
    {
        return _Factory.IsOperator(key);
    }

    private bool TryCondition(string key)
    {
        return _Factory.IsCondition(key);
    }

    private bool TryValue(string key, JsonReader reader, out object value)
    {
        value = null;
        if (key != "value")
            return false;

        if (reader.TryConsume(JsonToken.StartObject) &&
            TryFunction(reader, reader.Value as string, out var fn))
        {
            value = fn;
            return true;
        }
        return false;
    }

    private bool TryFunction(JsonReader reader, string key, out ExpressionFnOuter fn)
    {
        fn = null;
        if (!IsFunction(reader))
            return false;

        reader.Consume(JsonToken.PropertyName);
        reader.Consume(JsonToken.StartObject);
        fn = MapFunction("$", reader);
        if (fn == null)
            throw new Exception();

        reader.Consume(JsonToken.EndObject);
        return true;
    }

    private static bool IsFunction(JsonReader reader)
    {
        return reader.TokenType == JsonToken.PropertyName &&
            reader.Value is string s &&
            s == "$";
    }

    private bool TryExpression<T>(string type, LanguageExpression.PropertyBag properties, out T expression) where T : LanguageExpression
    {
        expression = null;
        if (_Factory.TryDescriptor(type, out var descriptor))
        {
            expression = (T)descriptor.CreateInstance(
                source: RunspaceContext.CurrentThread.Source.File,
                properties: properties
            );
            return expression != null;
        }
        return false;
    }
}

/// <summary>
/// A converter for converting <see cref="ResourceId"/> to/ from JSON.
/// </summary>
internal sealed class ResourceIdConverter : JsonConverter<ResourceId>
{
    public override ResourceId ReadJson(JsonReader reader, Type objectType, ResourceId existingValue, bool hasExistingValue, JsonSerializer serializer)
    {
        var value = reader.ReadAsString();
        return value != null ? ResourceId.Parse(value) : default;
    }

    public override void WriteJson(JsonWriter writer, ResourceId value, JsonSerializer serializer)
    {
        writer.WriteValue(value.Value);
    }
}

/// <summary>
/// A converter for converting <see cref="EnumMap{T}"/> to/ from JSON.
/// </summary>
internal sealed class EnumMapJsonConverter<T> : JsonConverter where T : struct, Enum
{
    public override bool CanRead => true;

    public override bool CanWrite => false;

    public override bool CanConvert(Type objectType)
    {
        return typeof(EnumMap<T>).IsAssignableFrom(objectType);
    }

    public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
    {
        var map = existingValue as EnumMap<T> ?? new EnumMap<T>();
        ReadMap(map, reader);
        return map;
    }

    public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
    {
        throw new NotImplementedException();
    }

    private static void ReadMap(EnumMap<T> map, JsonReader reader)
    {
        if (reader.TokenType != JsonToken.StartObject || !reader.Read())
            throw new PipelineSerializationException(PSRuleResources.ReadJsonFailed);

        string propertyName = null;
        while (reader.TokenType != JsonToken.EndObject)
        {
            if (reader.TokenType == JsonToken.PropertyName)
            {
                propertyName = reader.Value.ToString();
            }
            else if (reader.TokenType == JsonToken.String && TypeConverter.TryEnum<T>(reader.Value, convert: true, out var value) && value != null)
            {
                map.Add(propertyName, value.Value);
            }
            reader.Read();
        }
    }
}

/// <summary>
///  A converter for converting <see cref="SemanticVersion.Version"/> to/ from JSON.
/// </summary>
internal sealed class SemanticVersionConverter : JsonConverter<SemanticVersion.Version>
{
    public override SemanticVersion.Version ReadJson(JsonReader reader, Type objectType, SemanticVersion.Version existingValue, bool hasExistingValue, JsonSerializer serializer)
    {
        return reader.TokenType == JsonToken.String && SemanticVersion.TryParseVersion(reader.Value as string, out var version) ? version : default;
    }

    public override void WriteJson(JsonWriter writer, SemanticVersion.Version value, JsonSerializer serializer)
    {
        writer.WriteValue(value?.ToString());
    }
}

internal sealed class CaseInsensitiveDictionaryConverter<TValue> : JsonConverter
{
    public override bool CanConvert(Type objectType)
    {
        return objectType == typeof(Dictionary<string, TValue>) || objectType == typeof(IDictionary<string, TValue>);
    }

    public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
    {
        existingValue ??= new Dictionary<string, TValue>(StringComparer.OrdinalIgnoreCase);
        serializer.Deserialize<Dictionary<string, TValue>>(reader);
        return existingValue;
    }

    public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
    {
        serializer.Serialize(writer, value);
    }
}
