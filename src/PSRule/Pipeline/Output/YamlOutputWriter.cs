// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using PSRule.Configuration;
using PSRule.Definitions.Baselines;
using PSRule.Definitions.Rules;
using YamlDotNet.Core;
using YamlDotNet.Core.Events;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace PSRule.Pipeline.Output;

internal sealed class YamlOutputWriter : SerializationOutputWriter<object>
{
    internal YamlOutputWriter(PipelineWriter inner, PSRuleOption option, ShouldProcess shouldProcess)
        : base(inner, option, shouldProcess) { }

    protected override string Serialize(object[] o)
    {
        return o[0] is IEnumerable<Baseline> baselines ? ToBaselineYaml(baselines) : ToYaml(o);
    }

    internal static string ToYaml(object[] o)
    {
        var s = new SerializerBuilder()
            .DisableAliases()
            .WithTypeInspector(f => new FieldYamlTypeInspector())
            .WithTypeInspector(inspector => new OrderedPropertiesTypeInspector(inspector))
            .WithTypeConverter(new PSObjectYamlTypeConverter())
            .WithTypeConverter(new FieldMapYamlTypeConverter())
            .WithTypeConverter(new InfoStringYamlTypeConverter())
            .WithTypeConverter(new EnumMapYamlTypeConverter<SeverityLevel>())
            .WithNamingConvention(CamelCaseNamingConvention.Instance)
            .ConfigureDefaultValuesHandling(DefaultValuesHandling.OmitDefaults)
            .Build();

        return s.Serialize(o);
    }

    internal static string ToBaselineYaml(IEnumerable<Baseline> baselines)
    {
        using var output = new StringWriter();
        var emitter = new Emitter(output, bestIndent: 2, bestWidth: int.MaxValue, isCanonical: false);

        emitter.Emit(new StreamStart());

        foreach (var baseline in baselines)
        {
            emitter.Emit(new DocumentStart());
            BaselineYamlSerializationMapper.MapBaseline(emitter, baseline);
            emitter.Emit(new DocumentEnd(isImplicit: true));
        }

        emitter.Emit(new StreamEnd());

        return output.ToString();
    }
}
