// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Collections;
using System.Management.Automation;
using System.Reflection;
using PSRule.Annotations;
using PSRule.Configuration;
using PSRule.Converters;
using PSRule.Data;
using PSRule.Definitions;
using PSRule.Definitions.Expressions;
using PSRule.Host;
using PSRule.Pipeline;
using PSRule.Pipeline.Emitters;
using PSRule.Runtime;
using YamlDotNet.Core;
using YamlDotNet.Core.Events;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;
using YamlDotNet.Serialization.TypeInspectors;
using YamlDotNet.Serialization.TypeResolvers;

namespace PSRule;

#nullable enable

/// <summary>
/// A YAML converter that allows short and full notation of suppression rules.
/// </summary>
internal sealed class SuppressionRuleYamlTypeConverter : IYamlTypeConverter
{
    public bool Accepts(Type type)
    {
        return type == typeof(SuppressionRule);
    }

    public object? ReadYaml(IParser parser, Type type)
    {
        var result = new SuppressionRule();
        if (parser.TryConsume<SequenceStart>(out _))
        {
            var targetNames = new List<string>();
            while (parser.TryConsume<Scalar>(out var scalar) && scalar != null)
                targetNames.Add(scalar.Value);

            result.TargetName = [.. targetNames];
            parser.MoveNext();
        }
        else if (parser.TryConsume<MappingStart>(out _))
        {
            while (parser.TryConsume<Scalar>(out var scalar) && scalar != null)
            {
                var name = scalar.Value;
                if (name == "targetName" && parser.TryConsume<SequenceStart>(out _))
                {
                    var targetNames = new List<string>();
                    while (parser.TryConsume<Scalar>(out var item) && item != null)
                        targetNames.Add(item.Value);

                    result.TargetName = [.. targetNames];
                    parser.MoveNext();
                }
            }
            parser.MoveNext();
        }
        return result;
    }

    public void WriteYaml(IEmitter emitter, object? value, Type type)
    {
        throw new NotImplementedException();
    }
}

/// <summary>
/// A YAML converter for de/serializing a field map.
/// </summary>
internal sealed class FieldMapYamlTypeConverter : IYamlTypeConverter
{
    public bool Accepts(Type type)
    {
        return type == typeof(FieldMap);
    }

    public object? ReadYaml(IParser parser, Type type)
    {
        var result = new FieldMap();
        if (parser.TryConsume<MappingStart>(out _))
        {
            while (parser.TryConsume<Scalar>(out var scalar) && scalar != null)
            {
                var fieldName = scalar.Value;
                if (parser.TryConsume<SequenceStart>(out _))
                {
                    var fields = new List<string>();
                    while (!parser.Accept<SequenceEnd>(out _))
                    {
#pragma warning disable IDE0001
                        if (parser.TryConsume<Scalar>(out scalar) && scalar != null)
                            fields.Add(scalar.Value);
#pragma warning restore
                    }
                    result.Set(fieldName, [.. fields]);
                    parser.Require<SequenceEnd>();
                    parser.MoveNext();
                }
            }
            parser.Require<MappingEnd>();
            parser.MoveNext();
        }
        return result;
    }

    public void WriteYaml(IEmitter emitter, object? value, Type type)
    {
        if (type == typeof(FieldMap) && value == null)
        {
            emitter.Emit(new MappingStart());
            emitter.Emit(new MappingEnd());
        }
        if (value is not FieldMap map)
            return;

        emitter.Emit(new MappingStart());
        foreach (var field in map)
        {
            emitter.Emit(new Scalar(field.Key));
            emitter.Emit(new SequenceStart(null, null, false, SequenceStyle.Block));
            for (var i = 0; i < field.Value.Length; i++)
            {
                emitter.Emit(new Scalar(field.Value[i]));
            }
            emitter.Emit(new SequenceEnd());
        }
        emitter.Emit(new MappingEnd());
    }
}

/// <summary>
/// A YAML converter to deserialize a map/ object as a PSObject.
/// </summary>
internal sealed class PSObjectYamlTypeConverter : MappingTypeConverter, IYamlTypeConverter
{
    public bool Accepts(Type type)
    {
        return type == typeof(PSObject);
    }

    public object? ReadYaml(IParser parser, Type type)
    {
        // Handle empty objects
        if (parser.TryConsume<Scalar>(out var scalar) && scalar != null)
        {
            return PSObject.AsPSObject(scalar.Value);
        }

        var result = new PSObject();
        if (parser.TryConsume<MappingStart>(out _))
        {
#pragma warning disable IDE0001
            while (parser.TryConsume<Scalar>(out scalar) && scalar != null)
            {
                var name = scalar.Value;
                var property = ReadNoteProperty(parser, name) ?? throw new NotImplementedException();
                result.Properties.Add(property);
            }
#pragma warning restore
            parser.Require<MappingEnd>();
            parser.MoveNext();
        }
        return result;
    }

    public void WriteYaml(IEmitter emitter, object? value, Type type)
    {
        Map(emitter, value);
    }

    private PSNoteProperty? ReadNoteProperty(IParser parser, string name)
    {
        if (parser.TryConsume<SequenceStart>(out _))
        {
            var values = new List<PSObject>();
            while (parser.Current is not SequenceEnd)
            {
                if (parser.Current is MappingStart)
                {
                    values.Add(PSObject.AsPSObject(ReadYaml(parser, typeof(PSObject))));
                }
                else if (parser.TryConsume<Scalar>(out var scalar))
                {
                    values.Add(PSObject.AsPSObject(scalar.Value));
                }
            }
            parser.Require<SequenceEnd>();
            parser.MoveNext();
            return new PSNoteProperty(name, values.ToArray());
        }
        else if (parser.Current is MappingStart)
        {
            return new PSNoteProperty(name, ReadYaml(parser, typeof(PSObject)));
        }
        else if (parser.TryConsume<Scalar>(out var scalar))
        {
            return new PSNoteProperty(name, scalar.Value);
        }
        return null;
    }
}

internal abstract class MappingTypeConverter
{
    protected void Map(IEmitter emitter, object? value)
    {
        if (value is null) return;

        emitter.Emit(new MappingStart());
        foreach (var kv in GetKV(value))
        {
            emitter.Emit(new Scalar(kv.Key));
            Primitive(emitter, kv.Value);
        }
        emitter.Emit(new MappingEnd());
    }

    protected void Primitive(IEmitter emitter, object value)
    {
        if (value == null)
            return;

        value = ExpressionHelpers.GetBaseObject(value);
        if (value is string s)
        {
            emitter.Emit(new Scalar(s));
            return;
        }

        if (value is int || value is long || value is bool)
        {
            emitter.Emit(new Scalar(null, null, value.ToString(), ScalarStyle.Plain, false, false));
            return;
        }

        if (value is IEnumerable enumerable)
        {
            emitter.Emit(new SequenceStart(null, null, false, SequenceStyle.Block));
            foreach (var item in enumerable)
                Primitive(emitter, item);

            emitter.Emit(new SequenceEnd());
            return;
        }

        if (value is PSObject || value is IDictionary)
        {
            Map(emitter, value);
            return;
        }

        emitter.Emit(new Scalar(value.ToString()));
    }

    private static IEnumerable<KeyValuePair<string, object>> GetKV(object value)
    {
        var o = ExpressionHelpers.GetBaseObject(value);
        if (o is IDictionary d)
            foreach (DictionaryEntry kv in d)
                yield return new KeyValuePair<string, object>(kv.Key.ToString(), kv.Value);

        if (o is PSObject psObject)
            foreach (var p in psObject.Properties)
                yield return new KeyValuePair<string, object>(p.Name, p.Value);
    }
}

/// <summary>
/// A YAML resolver to convert any dictionary types to PSObjects instead.
/// </summary>
internal sealed class PSObjectYamlTypeResolver : INodeTypeResolver
{
    public bool Resolve(NodeEvent? nodeEvent, ref Type currentType)
    {
        if (nodeEvent is SequenceStart)
        {
            currentType = typeof(PSObject[]);
            return true;
        }
        else if (currentType == typeof(Dictionary<object, object>) || nodeEvent is MappingStart)
        {
            currentType = typeof(PSObject);
            return true;
        }
        return false;
    }
}

internal sealed class PSOptionYamlTypeResolver : INodeTypeResolver
{
    public bool Resolve(NodeEvent? nodeEvent, ref Type currentType)
    {
        if (currentType == typeof(object) && nodeEvent is SequenceStart)
        {
            currentType = typeof(PSObject[]);
            return true;
        }
        return false;
    }
}

/// <summary>
/// A YAML type inspector to order properties alphabetically
/// </summary>
internal sealed class OrderedPropertiesTypeInspector(ITypeInspector innerTypeDescriptor) : TypeInspectorSkeleton
{
    public override IEnumerable<IPropertyDescriptor> GetProperties(Type type, object? container)
    {
        return innerTypeDescriptor
            .GetProperties(type, container)
            .OrderBy(prop => prop.Name);
    }
}

/// <summary>
/// A YAML type inspector to read fields and properties from a type for serialization.
/// </summary>
internal sealed class FieldYamlTypeInspector : TypeInspectorSkeleton
{
    private readonly ITypeResolver _TypeResolver;
    private readonly INamingConvention _NamingConvention;

    public FieldYamlTypeInspector()
    {
        _TypeResolver = new StaticTypeResolver();
        _NamingConvention = CamelCaseNamingConvention.Instance;
    }

    public override IEnumerable<IPropertyDescriptor> GetProperties(Type type, object? container)
    {
        return GetPropertyDescriptor(type: type);
    }

    private IEnumerable<IPropertyDescriptor> GetPropertyDescriptor(Type type)
    {
        foreach (var f in SelectField(type: type))
        {
            yield return f;
        }

        foreach (var p in SelectProperty(type: type))
        {
            yield return p;
        }
    }

    private IEnumerable<Field> SelectField(Type type)
    {
        return type
            .GetRuntimeFields()
            .Where(f => !f.IsStatic && f.IsPublic && !f.IsDefined(typeof(YamlIgnoreAttribute), true))
            .Select(p => new Field(p, _TypeResolver, _NamingConvention));
    }

    private IEnumerable<Property> SelectProperty(Type type)
    {
        return type
            .GetProperties(bindingAttr: BindingFlags.Public | BindingFlags.GetProperty | BindingFlags.Instance)
            .Where(p => p.CanRead && IsAllowedProperty(p.Name) && !p.IsDefined(typeof(YamlIgnoreAttribute), true))
            .Select(p => new Property(p, _TypeResolver, _NamingConvention));
    }

    private static bool IsAllowedProperty(string name)
    {
        return !(name == "TargetObject" || name == "Exception");
    }

    private sealed class Field : IPropertyDescriptor
    {
        private readonly FieldInfo _FieldInfo;
        private readonly ITypeResolver _TypeResolver;
        private readonly INamingConvention _NamingConvention;

        public Field(FieldInfo fieldInfo, ITypeResolver typeResolver, INamingConvention namingConvention)
        {
            _FieldInfo = fieldInfo;
            _TypeResolver = typeResolver;
            _NamingConvention = namingConvention;
            ScalarStyle = ScalarStyle.Any;
        }

        public string Name => _NamingConvention.Apply(_FieldInfo.Name);

        public Type Type => _FieldInfo.FieldType;

        public Type? TypeOverride { get; set; }

        public int Order { get; set; }

        public bool CanWrite => false;

        public ScalarStyle ScalarStyle { get; set; }

        public void Write(object target, object? value)
        {
            throw new NotImplementedException();
        }

        public T? GetCustomAttribute<T>() where T : Attribute
        {
            return _FieldInfo.GetCustomAttributes(typeof(T), true).OfType<T>().FirstOrDefault();
        }

        public IObjectDescriptor Read(object target)
        {
            var propertyValue = _FieldInfo.GetValue(target);
            var actualType = TypeOverride ?? _TypeResolver.Resolve(Type, propertyValue);
            return new ObjectDescriptor(propertyValue, actualType, Type, ScalarStyle);
        }
    }

    private sealed class Property : IPropertyDescriptor
    {
        private readonly PropertyInfo _PropertyInfo;
        private readonly ITypeResolver _TypeResolver;
        private readonly INamingConvention _NamingConvention;

        public Property(PropertyInfo propertyInfo, ITypeResolver typeResolver, INamingConvention namingConvention)
        {
            _PropertyInfo = propertyInfo;
            _TypeResolver = typeResolver;
            _NamingConvention = namingConvention;
            ScalarStyle = ScalarStyle.Any;
        }

        public string Name => _NamingConvention.Apply(_PropertyInfo.Name);

        public Type Type => _PropertyInfo.PropertyType;

        public Type? TypeOverride { get; set; }

        public int Order { get; set; }

        public bool CanWrite => false;

        public ScalarStyle ScalarStyle { get; set; }

        public T? GetCustomAttribute<T>() where T : Attribute
        {
            return _PropertyInfo.GetCustomAttributes(typeof(T), true).OfType<T>().FirstOrDefault();
        }

        public void Write(object target, object? value)
        {
            throw new NotImplementedException();
        }

        public IObjectDescriptor Read(object target)
        {
            var propertyValue = _PropertyInfo.GetValue(target);
            var actualType = TypeOverride ?? _TypeResolver.Resolve(Type, propertyValue);
            return new ObjectDescriptor(propertyValue, actualType, Type, ScalarStyle);
        }
    }
}

/// <summary>
/// A custom deserializer to convert YAML into a <see cref="ResourceObject"/>.
/// </summary>
internal sealed class ResourceNodeDeserializer : INodeDeserializer
{
    private const string FIELD_APIVERSION = "apiVersion";
    private const string FIELD_KIND = "kind";
    private const string FIELD_METADATA = "metadata";
    private const string FIELD_SPEC = "spec";

    private readonly INodeDeserializer _Next;
    private readonly SpecFactory _Factory;

    public ResourceNodeDeserializer(INodeDeserializer next)
    {
        _Next = next;
        _Factory = new SpecFactory();
    }

    bool INodeDeserializer.Deserialize(IParser reader, Type expectedType, Func<IParser, Type, object?> nestedObjectDeserializer, out object? value)
    {
        if (typeof(ResourceObject).IsAssignableFrom(expectedType))
        {
            var comment = reader.Current == null || RunspaceContext.CurrentThread == null ? null : HostHelper.GetCommentMeta(RunspaceContext.CurrentThread.Source?.File, reader.Current.Start.Line - 2, reader.Current.Start.Column);
            var resource = MapResource(reader, nestedObjectDeserializer, comment);
            value = new ResourceObject(resource);
            return true;
        }
        else
        {
            return _Next.Deserialize(reader, expectedType, nestedObjectDeserializer, out value);
        }
    }

    private IResource? MapResource(IParser reader, Func<IParser, Type, object?> nestedObjectDeserializer, CommentMetadata? comment)
    {
        IResource? result = null;
        string? apiVersion = null;
        string? kind = null;
        ResourceMetadata? metadata = null;
        if (reader.TryConsume<MappingStart>(out var mappingStart) && mappingStart != null)
        {
            var extent = new SourceExtent(RunspaceContext.CurrentThread!.Source!.File, mappingStart.Start.Line, mappingStart.Start.Column);
            while (reader.TryConsume<Scalar>(out var scalar) && scalar != null)
            {
                // Read apiVersion
                if (TryApiVersion(reader, scalar, out var apiVersionValue))
                {
                    apiVersion = apiVersionValue;
                }
                // Read kind
                else if (TryKind(reader, scalar, out var kindValue))
                {
                    kind = kindValue;
                }
                // Read metadata
                else if (TryMetadata(reader, scalar, nestedObjectDeserializer, out var metadataValue))
                {
                    metadata = metadataValue;
                }
                // Read spec
                else if (kind != null && apiVersion != null && TrySpec(reader, scalar, apiVersion, kind, nestedObjectDeserializer, metadata, comment, extent, out var resource))
                {
                    result = resource;
                }
                else
                {
                    reader.SkipThisAndNestedEvents();
                }
            }
            reader.Require<MappingEnd>();
            reader.MoveNext();
        }
        return result;
    }

    private static bool TryApiVersion(IParser reader, Scalar scalar, out string? apiVersion)
    {
        apiVersion = null;
        if (scalar.Value == FIELD_APIVERSION)
        {
            apiVersion = reader.Consume<Scalar>().Value;
            return true;
        }
        return false;
    }

    private static bool TryKind(IParser reader, Scalar scalar, out string? kind)
    {
        kind = null;
        if (scalar.Value == FIELD_KIND)
        {
            kind = reader.Consume<Scalar>().Value;
            return true;
        }
        return false;
    }

    private bool TryMetadata(IParser reader, Scalar scalar, Func<IParser, Type, object?> nestedObjectDeserializer, out ResourceMetadata? metadata)
    {
        metadata = null;
        if (scalar.Value != FIELD_METADATA)
            return false;

        if (reader.Current is MappingStart)
        {
            if (!_Next.Deserialize(reader, typeof(ResourceMetadata), nestedObjectDeserializer, out var value) || value is not ResourceMetadata metadata_value)
                return false;

            metadata = metadata_value;
            return true;
        }
        return false;
    }

    private bool TrySpec(IParser reader, Scalar scalar, string apiVersion, string kind, Func<IParser, Type, object?> nestedObjectDeserializer, ResourceMetadata? metadata, CommentMetadata? comment, ISourceExtent extent, out IResource? spec)
    {
        spec = null;
        return scalar.Value == FIELD_SPEC && TryResource(reader, apiVersion, kind, nestedObjectDeserializer, metadata, comment, extent, out spec);
    }

    private bool TryResource(IParser reader, string apiVersion, string kind, Func<IParser, Type, object?> nestedObjectDeserializer, ResourceMetadata? metadata, CommentMetadata? comment, ISourceExtent extent, out IResource? spec)
    {
        spec = null;
        if (_Factory.TryDescriptor(apiVersion, kind, out var descriptor) && reader.Current is MappingStart)
        {
            if (!_Next.Deserialize(reader, descriptor.SpecType, nestedObjectDeserializer, out var value))
                return false;

            spec = descriptor.CreateInstance(extent.File, metadata, comment, extent, value);
            return true;
        }
        return false;
    }
}

/// <summary>
/// A custom deserializer to convert YAML into a language expression.
/// </summary>
internal sealed class LanguageExpressionDeserializer : INodeDeserializer
{
    private const string OPERATOR_IF = "if";

    private readonly INodeDeserializer _Next;
    private readonly LanguageExpressionFactory _Factory;
    private readonly FunctionBuilder _FunctionBuilder;

    public LanguageExpressionDeserializer(INodeDeserializer next)
    {
        _Next = next;
        _Factory = new LanguageExpressionFactory();
        _FunctionBuilder = new FunctionBuilder();
    }

    bool INodeDeserializer.Deserialize(IParser reader, Type expectedType, Func<IParser, Type, object?> nestedObjectDeserializer, out object? value)
    {
        if (typeof(LanguageExpression).IsAssignableFrom(expectedType))
        {
            var resource = MapOperator(OPERATOR_IF, null, null, reader, nestedObjectDeserializer);
            value = new LanguageIf(resource);
            return true;
        }
        else
        {
            return _Next.Deserialize(reader, expectedType, nestedObjectDeserializer, out value);
        }
    }

    /// <summary>
    /// Map an operator.
    /// </summary>
    private LanguageOperator? MapOperator(string type, LanguageExpression.PropertyBag? properties, LanguageExpression? subselector, IParser reader, Func<IParser, Type, object?> nestedObjectDeserializer)
    {
        if (TryExpression(reader, type, properties, nestedObjectDeserializer, out LanguageOperator? result) && result != null)
        {
            // If and Not
            if (reader.TryConsume<MappingStart>(out _))
            {
                result.Add(MapExpression(reader, nestedObjectDeserializer));
                reader.Require<MappingEnd>();
                reader.MoveNext();
            }
            // AllOf and AnyOf
            else if (reader.TryConsume<SequenceStart>(out _))
            {
                while (reader.TryConsume<MappingStart>(out _))
                {
                    result.Add(MapExpression(reader, nestedObjectDeserializer));
                    reader.Require<MappingEnd>();
                    reader.MoveNext();
                }
                reader.Require<SequenceEnd>();
                reader.MoveNext();
            }
            result.Subselector = subselector;
        }
        return result;
    }

    private LanguageCondition? MapCondition(string type, LanguageExpression.PropertyBag properties, IParser reader, Func<IParser, Type, object?> nestedObjectDeserializer)
    {
        if (TryExpression(reader, type, null, nestedObjectDeserializer, out LanguageCondition? result) && result != null)
        {
            while (!reader.Accept<MappingEnd>(out var end) && end != null)
            {
                MapProperty(properties, reader, nestedObjectDeserializer, out _, out _);
            }
            result.Add(properties);
        }
        return result;
    }

    private LanguageExpression? MapExpression(IParser reader, Func<IParser, Type, object?> nestedObjectDeserializer)
    {
        LanguageExpression? result = null;
        var properties = new LanguageExpression.PropertyBag();
        MapProperty(properties, reader, nestedObjectDeserializer, out var key, out var subselector);
        if (key == null)
            return null;

        if (TryCondition(key))
        {
            result = MapCondition(key, properties, reader, nestedObjectDeserializer);
        }
        else if (TryOperator(key) && reader.Accept<MappingStart>(out _))
        {
            var op = MapOperator(key, properties, subselector, reader, nestedObjectDeserializer);
            MapProperty(properties, reader, nestedObjectDeserializer, out _, out _);
            result = op;
        }
        else if (TryOperator(key) && reader.Accept<SequenceStart>(out _))
        {
            var op = MapOperator(key, properties, subselector, reader, nestedObjectDeserializer);
            MapProperty(properties, reader, nestedObjectDeserializer, out _, out subselector);
            if (op != null && subselector != null)
                op.Subselector = subselector;

            result = op;
        }
        return result;
    }

    private ExpressionFnOuter MapFunction(string type, IParser reader, Func<IParser, Type, object?> nestedObjectDeserializer)
    {
        _FunctionBuilder.Push();
        string? name = null;
        while (!(reader.Accept<MappingEnd>(out _) || reader.Accept<SequenceEnd>(out _)))
        {
            if (reader.TryConsume<Scalar>(out var s) && s != null)
            {
                if (name != null)
                {
                    _FunctionBuilder.Add(name, s.Value);
                    name = null;
                }
                else
                {
                    name = s.Value;
                }
            }
            else if (reader.TryConsume<MappingStart>(out _) && name != null)
            {
                var child = MapFunction(name, reader, nestedObjectDeserializer);
                _FunctionBuilder.Add(name, child);
                name = null;
                reader.Consume<MappingEnd>();
            }
            else if (reader.TryConsume<SequenceStart>(out _) && name != null)
            {
                var sequence = MapSequence(name, reader, nestedObjectDeserializer);
                _FunctionBuilder.Add(name, sequence);
                name = null;
                reader.Consume<SequenceEnd>();
            }
        }
        var result = _FunctionBuilder.Pop();
        return result;
    }

    private object[] MapSequence(string name, IParser reader, Func<IParser, Type, object?> nestedObjectDeserializer)
    {
        var result = new List<object>();
        while (!reader.Accept<SequenceEnd>(out _))
        {
            if (reader.TryConsume<Scalar>(out var s))
                result.Add(s.Value);

            else if (reader.TryConsume<MappingStart>(out _))
            {
                var child = MapFunction(name, reader, nestedObjectDeserializer);
                result.Add(child);
                reader.Consume<MappingEnd>();
            }
        }
        return [.. result];
    }

    private void MapProperty(LanguageExpression.PropertyBag properties, IParser reader, Func<IParser, Type, object?> nestedObjectDeserializer, out string? name, out LanguageExpression? subselector)
    {
        name = null;
        subselector = null;
        while (reader.TryConsume<Scalar>(out var scalar) && scalar != null)
        {
            var key = scalar.Value;
            if (TryCondition(key) || TryOperator(key))
                name = key;

#pragma warning disable IDE0001
            if (reader.TryConsume<Scalar>(out scalar) && scalar != null)
            {
                properties[key] = scalar.Value;
            }
#pragma warning restore IDE0001
            // value:
            else if (TryValue(key, reader, nestedObjectDeserializer, out var value))
            {
                properties[key] = value;
            }
            else if (TryCondition(key) && reader.TryConsume<MappingStart>(out _))
            {
                if (TryFunction(reader, nestedObjectDeserializer, out var fn))
                    properties[key] = fn;
            }
            else if (TryCondition(key) && reader.TryConsume<SequenceStart>(out _))
            {
                var objects = new List<string>();
                while (!reader.TryConsume<SequenceEnd>(out _))
                {
#pragma warning disable IDE0001
                    if (reader.TryConsume<Scalar>(out scalar) && scalar != null)
                    {
                        objects.Add(scalar.Value);
                    }
#pragma warning restore IDE0001
                }
                properties[key] = objects.ToArray();
            }
            // where:
            else if (TrySubSelector(key) && reader.TryConsume<MappingStart>(out _))
            {
                subselector = MapExpression(reader, nestedObjectDeserializer);
                reader.Consume<MappingEnd>();
            }
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

    private bool TryValue(string key, IParser reader, Func<IParser, Type, object?> nestedObjectDeserializer, out object? value)
    {
        value = null;
        if (key != "value")
            return false;

        if (reader.TryConsume<MappingStart>(out _) && TryFunction(reader, nestedObjectDeserializer, out var fn))
        {
            value = fn;
            return true;
        }
        reader.SkipThisAndNestedEvents();
        return false;
    }

    private bool TryFunction(IParser reader, Func<IParser, Type, object?> nestedObjectDeserializer, out ExpressionFnOuter? fn)
    {
        fn = null;
        if (!IsFunction(reader))
            return false;

        reader.Consume<Scalar>();
        reader.Consume<MappingStart>();
        fn = MapFunction("$", reader, nestedObjectDeserializer);
        reader.Consume<MappingEnd>();
        reader.Consume<MappingEnd>();
        return true;
    }

    private static bool IsFunction(IParser reader)
    {
        return reader.Accept<Scalar>(out var scalar) || scalar?.Value == "$";
    }

    private bool TryExpression<T>(IParser reader, string type, LanguageExpression.PropertyBag? properties, Func<IParser, Type, object?> nestedObjectDeserializer, out T? expression) where T : LanguageExpression
    {
        expression = null;
        if (_Factory.TryDescriptor(type, out var descriptor))
        {
            expression = (T)descriptor.CreateInstance(RunspaceContext.CurrentThread!.Source!.File, properties);
            return expression != null;
        }
        return false;
    }
}

internal sealed class PSObjectYamlDeserializer : INodeDeserializer
{
    private readonly INodeDeserializer _Next;
    private readonly PSObjectYamlTypeConverter _Converter;
    private readonly IFileInfo? _SourceInfo;

    public PSObjectYamlDeserializer(INodeDeserializer next, IFileInfo sourceInfo)
    {
        _Next = next;
        _Converter = new PSObjectYamlTypeConverter();
        _SourceInfo = sourceInfo;
    }

    public PSObjectYamlDeserializer(INodeDeserializer next)
    {
        _Next = next;
        _Converter = new PSObjectYamlTypeConverter();
    }

    bool INodeDeserializer.Deserialize(IParser reader, Type expectedType, Func<IParser, Type, object?> nestedObjectDeserializer, out object? value)
    {
        if (expectedType == typeof(PSObject[]) && reader.Current is MappingStart)
        {
            var parser = reader as YamlEmitterParser;
            var fileInfo = parser?.Info ?? _SourceInfo;

            var lineNumber = reader.Current.Start.Line;
            var linePosition = reader.Current.Start.Column;
            value = _Converter.ReadYaml(reader, typeof(PSObject));
            if (value is PSObject pso)
            {
                pso.UseTargetInfo(out var info);
                info.SetSource(fileInfo?.Path, lineNumber, linePosition);
                value = new PSObject[] { pso };
                return true;
            }
            return false;
        }
        else
        {
            return _Next.Deserialize(reader, expectedType, nestedObjectDeserializer, out value);
        }
    }
}

internal sealed class TargetObjectYamlDeserializer : INodeDeserializer
{
    private readonly INodeDeserializer _Next;
    private readonly PSObjectYamlTypeConverter _Converter;

    public TargetObjectYamlDeserializer(INodeDeserializer next)
    {
        _Next = next;
        _Converter = new PSObjectYamlTypeConverter();
    }

    bool INodeDeserializer.Deserialize(IParser reader, Type expectedType, Func<IParser, Type, object?> nestedObjectDeserializer, out object? value)
    {
        value = null;
        if (expectedType == typeof(TargetObject[]) && reader.Current is MappingStart)
        {
            if (TryGetTargetObject(reader, out var targetObject) && targetObject != null)
            {
                value = new TargetObject[] { targetObject };
                return true;
            }
            return false;
        }
        else if (expectedType == typeof(TargetObject[]) && reader.TryConsume<SequenceStart>(out _))
        {
            var result = new List<TargetObject>();
            while (reader.Current is MappingStart)
            {
                if (TryGetTargetObject(reader, out var targetObject) && targetObject != null)
                {
                    result.Add(targetObject);
                }
            }
            value = result.ToArray();
            return reader.TryConsume<SequenceEnd>(out _);
        }
        else
        {
            return _Next.Deserialize(reader, expectedType, nestedObjectDeserializer, out value);
        }
    }

    private bool TryGetTargetObject(IParser reader, out TargetObject? value)
    {
        value = null;
        var parser = reader as YamlEmitterParser;
        var fileInfo = parser?.Info;
        var lineNumber = reader.Current?.Start.Line;
        var linePosition = reader.Current?.Start.Column;

        if (_Converter.ReadYaml(reader, typeof(PSObject)) is PSObject o)
        {
            o.UseTargetInfo(out var info);
            info.SetSource(fileInfo?.Path, lineNumber, linePosition);

            var targetObject = new TargetObject(o);

            value = targetObject;
            return true;
        }
        return false;
    }
}

internal sealed class InfoStringYamlTypeConverter : IYamlTypeConverter
{
    public bool Accepts(Type type)
    {
        return type == typeof(InfoString);
    }

    public object? ReadYaml(IParser parser, Type type)
    {
        return parser.TryConsume<Scalar>(out var scalar) &&
            !string.IsNullOrEmpty(scalar.Value) ? new InfoString(scalar.Value) : new InfoString();
    }

    public void WriteYaml(IEmitter emitter, object? value, Type type)
    {
        if (value is InfoString info && info.HasValue && info.Text != null)
            emitter.Emit(new Scalar(info.Text));
    }
}

/// <summary>
/// A converter for converting <see cref="EnumMap{T}"/> to/ from YAML.
/// </summary>
internal sealed class EnumMapYamlTypeConverter<T> : IYamlTypeConverter where T : struct, Enum
{
    public bool Accepts(Type type)
    {
        return typeof(EnumMap<T>).IsAssignableFrom(type);
    }

    public object ReadYaml(IParser parser, Type type)
    {
        var map = new EnumMap<T>();
        if (parser.TryConsume<MappingStart>(out _))
        {
            while (parser.TryConsume(out Scalar scalar))
            {
                var propertyName = scalar.Value;
                if (parser.TryConsume(out scalar) && TypeConverter.TryEnum<T>(scalar.Value, convert: true, out var value) && value != null)
                    map.Add(propertyName, value.Value);
            }
            parser.Require<MappingEnd>();
            parser.MoveNext();
        }
        return map;
    }

    public void WriteYaml(IEmitter emitter, object value, Type type)
    {
        if (type == typeof(EnumMap<T>) && value == null)
        {
            emitter.Emit(new MappingStart());
            emitter.Emit(new MappingEnd());
        }
        if (value is not EnumMap<T> map)
            return;

        emitter.Emit(new MappingStart());
        foreach (var kv in map)
        {
            emitter.Emit(new Scalar(kv.Key));
            emitter.Emit(new Scalar(kv.Value.ToString()));
        }
        emitter.Emit(new MappingEnd());
    }
}

#nullable restore
