// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Collections;
using System.Collections.ObjectModel;
using System.Management.Automation;
using Newtonsoft.Json;
using PSRule.Annotations;
using PSRule.Converters.Yaml;
using PSRule.Definitions;
using PSRule.Definitions.Baselines;
using PSRule.Definitions.Conventions;
using PSRule.Definitions.ModuleConfigs;
using PSRule.Definitions.Rules;
using PSRule.Definitions.Selectors;
using PSRule.Definitions.SuppressionGroups;
using PSRule.Help;
using PSRule.Pipeline;
using PSRule.Rules;
using PSRule.Runtime;
using YamlDotNet.Core;
using YamlDotNet.Core.Events;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;
using YamlDotNet.Serialization.NodeDeserializers;
using Rule = PSRule.Rules.Rule;

namespace PSRule.Host;

internal static class HostHelper
{
    private const string Markdown_Extension = ".md";

    internal static IRuleV1[] GetRule(Source[] source, RunspaceContext context, bool includeDependencies)
    {
        var rules = ToRuleV1(GetLanguageBlock(context, source), context);
        var builder = new DependencyGraphBuilder<IRuleV1>(context, includeDependencies, includeDisabled: true);
        builder.Include(rules, filter: (b) => Match(context, b));
        return builder.GetItems();
    }

    internal static RuleHelpInfo[] GetRuleHelp(Source[] source, RunspaceContext context)
    {
        return ToRuleHelp(ToRuleBlockV1(GetLanguageBlock(context, source), context, skipDuplicateName: true).GetAll(), context);
    }

    internal static DependencyGraph<RuleBlock> GetRuleBlockGraph(Source[] source, RunspaceContext context)
    {
        var blocks = GetLanguageBlock(context, source);
        var rules = ToRuleBlockV1(blocks, context, skipDuplicateName: false);
        Import(GetConventions(blocks, context), context);
        var builder = new DependencyGraphBuilder<RuleBlock>(context, includeDependencies: true, includeDisabled: false);
        builder.Include(rules, filter: (b) => Match(context, b));
        return builder.Build();
    }

    private static IEnumerable<ILanguageBlock> GetYamlJsonLanguageBlocks(Source[] source, RunspaceContext context)
    {
        var results = new List<ILanguageBlock>();
        results.AddRange(GetYamlLanguageBlocks(source, context));
        results.AddRange(GetJsonLanguageBlocks(source, context));
        return results;
    }

    /// <summary>
    /// Read YAML/JSON objects and return baselines.
    /// </summary>
    internal static IEnumerable<Baseline> GetBaseline(Source[] source, RunspaceContext context)
    {
        return ToBaselineV1(GetYamlJsonLanguageBlocks(source, context), context);
    }

    /// <summary>
    /// Read YAML/JSON objects and return module configurations.
    /// </summary>
    internal static IEnumerable<ModuleConfigV1> GetModuleConfigForTests(Source[] source, RunspaceContext context)
    {
        return ToModuleConfigV1(GetYamlJsonLanguageBlocks(source, context), context);
    }

    /// <summary>
    /// Read YAML/JSON objects and return selectors.
    /// </summary>
    internal static IEnumerable<SelectorV1> GetSelectorForTests(Source[] source, RunspaceContext context)
    {
        return ToSelectorV1(GetYamlJsonLanguageBlocks(source, context), context);
    }

    /// <summary>
    /// Read YAML/JSON objects and return suppression groups.
    /// </summary>
    internal static IEnumerable<SuppressionGroupV1> GetSuppressionGroupForTests(Source[] source, RunspaceContext context)
    {
        return ToSuppressionGroupV1(GetYamlJsonLanguageBlocks(source, context), context);
    }

    internal static IEnumerable<ILanguageBlock> ImportResource(Source[] source, RunspaceContext context)
    {
        return source == null || source.Length == 0 ? Array.Empty<ILanguageBlock>() : GetYamlJsonLanguageBlocks(source, context);
    }

    /// <summary>
    /// Called from PowerShell to get additional metdata from a language block, such as comment help.
    /// </summary>
    internal static CommentMetadata GetCommentMeta(ISourceFile file, int lineNumber, int offset)
    {
        var context = RunspaceContext.CurrentThread;
        if (lineNumber < 0 || RunspaceContext.CurrentThread.IsScope(RunspaceScope.None) || context.Source.SourceContentCache == null)
            return new CommentMetadata();

        var lines = context.Source.SourceContentCache;
        var i = lineNumber;
        var comments = new List<string>();

        // Back track lines with comments immediately before block
        for (; i >= 0 && lines[i].Contains("#"); i--)
            comments.Insert(0, lines[i]);

        // Check if any comments were found
        var metadata = new CommentMetadata();
        if (comments.Count > 0)
        {
            foreach (var comment in comments)
            {
                if (comment.StartsWith("# Description: ", StringComparison.OrdinalIgnoreCase))
                    metadata.Synopsis = comment.Substring(15);

                if (comment.StartsWith("# Synopsis: ", StringComparison.OrdinalIgnoreCase))
                    metadata.Synopsis = comment.Substring(12);
            }
        }
        return metadata;
    }

    internal static void UnblockFile(IPipelineWriter writer, string[] publisher, string[] path)
    {
        var ps = PowerShell.Create();
        try
        {
            ps.Runspace.SessionStateProxy.SetVariable("trustedPublisher", publisher);
            ps.Runspace.SessionStateProxy.SetVariable("trustedPath", path);
            ps.AddScript("$trustedPath | ForEach-Object { Get-AuthenticodeSignature -FilePath $_ } | Where-Object { $_.Status -eq 'Valid' -and $_.SignerCertificate.Subject -in $trustedPublisher } | ForEach-Object { Unblock-File -Path $_.Path -Confirm:$False; }");
            ps.Invoke();
            if (ps.HadErrors)
            {
                foreach (var error in ps.Streams.Error)
                    writer.WriteError(error);
            }
        }
        finally
        {
            ps.Runspace = null;
            ps.Dispose();
        }
    }

    private static ILanguageBlock[] GetLanguageBlock(RunspaceContext context, Source[] sources)
    {
        var results = new List<ILanguageBlock>();
        results.AddRange(GetPSLanguageBlocks(context, sources));
        results.AddRange(GetYamlJsonLanguageBlocks(sources, context));
        return results.ToArray();
    }

    /// <summary>
    /// Execute PowerShell script files to get language blocks.
    /// </summary>
    private static ILanguageBlock[] GetPSLanguageBlocks(RunspaceContext context, Source[] sources)
    {
        if (context.Pipeline.Option.Execution.RestrictScriptSource == Options.RestrictScriptSource.DisablePowerShell)
            return Array.Empty<ILanguageBlock>();

        var results = new List<ILanguageBlock>();
        var ps = context.GetPowerShell();

        try
        {
            context.Writer.EnterScope("[Discovery.Rule]");
            context.PushScope(RunspaceScope.Source);

            // Process scripts
            foreach (var source in sources)
            {
                foreach (var file in source.File)
                {
                    if (file.Type != SourceType.Script)
                        continue;

                    ps.Commands.Clear();
                    context.VerboseRuleDiscovery(path: file.Path);
                    context.EnterLanguageScope(file);
                    try
                    {
                        var scriptAst = System.Management.Automation.Language.Parser.ParseFile(file.Path, out var tokens, out var errors);
                        var visitor = new RuleLanguageAst();
                        scriptAst.Visit(visitor);

                        if (visitor.Errors != null && visitor.Errors.Count > 0)
                        {
                            foreach (var record in visitor.Errors)
                                context.WriteError(record);

                            continue;
                        }
                        if (errors != null && errors.Length > 0)
                        {
                            foreach (var error in errors)
                                context.WriteError(error);

                            continue;
                        }

                        // Invoke script
                        ps.AddScript(string.Concat("& '", file.Path, "'"), true);
                        var invokeResults = ps.Invoke();

                        // Discovery has errors so skip this file
                        if (ps.HadErrors)
                            continue;

                        foreach (var ir in invokeResults)
                        {
                            if (ir.BaseObject is ILanguageBlock block)
                                results.Add(block);
                        }
                    }
                    finally
                    {
                        context.ExitLanguageScope(file);
                    }
                }
            }
        }
        finally
        {
            context.Writer.ExitScope();
            context.PopScope(RunspaceScope.Source);
            ps.Runspace = null;
            ps.Dispose();
        }
        return results.ToArray();
    }

    /// <summary>
    /// Get language blocks from YAML source files.
    /// </summary>
    private static ILanguageBlock[] GetYamlLanguageBlocks(Source[] sources, RunspaceContext context)
    {
        var result = new Collection<ILanguageBlock>();
        var visitor = new ResourceValidator(context);
        var d = new DeserializerBuilder()
            .IgnoreUnmatchedProperties()
            .WithNamingConvention(CamelCaseNamingConvention.Instance)
            .WithTypeMapping<IResourceAnnotations, ResourceAnnotations>()
            .WithTypeMapping<IResourceTags, ResourceTags>()
            .WithTypeMapping<IResourceLabels, ResourceLabels>()
            .WithTypeConverter(new FieldMapYamlTypeConverter())
            .WithTypeConverter(new StringArrayMapConverter())
            .WithTypeConverter(new StringArrayConverter())
            .WithTypeConverter(new PSObjectYamlTypeConverter())
            .WithTypeConverter(new EnumMapYamlTypeConverter<SeverityLevel>())
            .WithNodeTypeResolver(new PSOptionYamlTypeResolver())
            .WithNodeDeserializer(
                inner => new ResourceNodeDeserializer(new LanguageExpressionDeserializer(inner)),
                s => s.InsteadOf<ObjectNodeDeserializer>())
            .Build();

        try
        {
            context.Writer?.EnterScope("[Discovery.Resource]");
            context.PushScope(RunspaceScope.Resource);
            foreach (var source in sources)
            {
                foreach (var file in source.File)
                {
                    if (file.Type != SourceType.Yaml)
                        continue;

                    context.VerboseRuleDiscovery(path: file.Path);
                    context.EnterLanguageScope(file);
                    try
                    {
                        using var reader = new StreamReader(file.Path);
                        var parser = new Parser(reader);
                        parser.TryConsume<StreamStart>(out _);
                        while (parser.Current is DocumentStart)
                        {
                            var item = d.Deserialize<ResourceObject>(parser);
                            if (item == null || item.Block == null)
                                continue;

                            if (item.Visit(visitor))
                                result.Add(item.Block);
                        }
                    }
                    finally
                    {
                        context.ExitLanguageScope(file);
                    }
                }
            }
        }
        finally
        {
            context.Writer?.ExitScope();
            context.PopScope(RunspaceScope.Resource);
        }
        return result.Count == 0 ? Array.Empty<ILanguageBlock>() : result.ToArray();
    }

    /// <summary>
    /// Get language blocks from JSON source files.
    /// </summary>
    private static ILanguageBlock[] GetJsonLanguageBlocks(Source[] sources, RunspaceContext context)
    {
        var result = new Collection<ILanguageBlock>();
        var visitor = new ResourceValidator(context);
        var deserializer = new JsonSerializer
        {
            DateTimeZoneHandling = DateTimeZoneHandling.Utc
        };
        deserializer.Converters.Add(new ResourceObjectJsonConverter());
        deserializer.Converters.Add(new FieldMapJsonConverter());
        deserializer.Converters.Add(new StringArrayJsonConverter());
        deserializer.Converters.Add(new LanguageExpressionJsonConverter());
        deserializer.Converters.Add(new EnumMapJsonConverter<SeverityLevel>());

        try
        {
            context.Writer?.EnterScope("[Discovery.Resource]");
            context.PushScope(RunspaceScope.Resource);

            foreach (var source in sources)
            {
                foreach (var file in source.File)
                {
                    if (file.Type != SourceType.Json)
                        continue;

                    context.VerboseRuleDiscovery(file.Path);
                    context.EnterLanguageScope(file);
                    try
                    {
                        using var reader = new JsonTextReader(new StreamReader(file.Path));

                        // Consume lines until start of array
                        reader.SkipComments(out _);
                        if (reader.TryConsume(JsonToken.StartArray))
                        {
                            reader.SkipComments(out _);
                            while (reader.TokenType != JsonToken.EndArray)
                            {
                                var value = deserializer.Deserialize<ResourceObject>(reader);
                                if (value?.Block != null && value.Visit(visitor))
                                    result.Add(value.Block);

                                // Consume all end objects at the end of each resource
                                while (reader.TryConsume(JsonToken.EndObject)) { }
                            }
                        }
                    }
                    finally
                    {
                        context.ExitLanguageScope(file);
                    }
                }
            }
        }
        finally
        {
            context.Writer?.ExitScope();
            context.PopScope(RunspaceScope.Resource);
        }
        return result.Count == 0 ? Array.Empty<ILanguageBlock>() : result.ToArray();
    }

    public static void InvokeRuleBlock(RunspaceContext context, RuleBlock ruleBlock, RuleRecord ruleRecord)
    {
        RunspaceContext.CurrentThread = context;
        var condition = ruleBlock.Condition;
        context.VerboseObjectStart();

        try
        {
            context.EnterLanguageScope(ruleBlock.Source);
            var invokeResult = condition.If();
            if (invokeResult == null)
            {
                ruleRecord.OutcomeReason = RuleOutcomeReason.PreconditionFail;
                return;
            }
            else if (invokeResult.HadErrors || context.HadErrors)
            {
                ruleRecord.OutcomeReason = RuleOutcomeReason.None;
                ruleRecord.Outcome = RuleOutcome.Error;
            }
            else if (invokeResult.Count == 0)
            {
                ruleRecord.OutcomeReason = RuleOutcomeReason.Inconclusive;
                ruleRecord.Outcome = RuleOutcome.Fail;
                context.WarnRuleInconclusive(ruleRecord.RuleId);
            }
            else
            {
                ruleRecord.OutcomeReason = RuleOutcomeReason.Processed;
                ruleRecord.Outcome = invokeResult.AllOf() ? RuleOutcome.Pass : RuleOutcome.Fail;
            }
            context.VerboseConditionResult(pass: invokeResult.Pass, count: invokeResult.Count, outcome: ruleRecord.Outcome);
        }
        catch (CmdletInvocationException)
        {
            throw;
        }
        catch (Exception ex)
        {
            context.Error(ex);
        }
        // TODO: Exit scope
        //finally
        //{
        //    context.ExitSourceScope();
        //}
    }

    /// <summary>
    /// Convert matching langauge blocks to rules.
    /// </summary>
    private static DependencyTargetCollection<IRuleV1> ToRuleV1(ILanguageBlock[] blocks, RunspaceContext context)
    {
        // Index rules by RuleId
        var results = new DependencyTargetCollection<IRuleV1>();

        // Keep track of rule names and ids that have been added
        var knownRuleNames = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        var knownRuleIds = new HashSet<ResourceId>(ResourceIdEqualityComparer.Default);


        foreach (var block in blocks.OfType<RuleBlock>())
        {
            if (knownRuleIds.ContainsIds(block.Id, block.Ref, block.Alias, out var duplicateId))
            {
                context.DuplicateResourceId(block.Id, duplicateId.Value);
                continue;
            }

            if (knownRuleNames.ContainsNames(block.Id, block.Ref, block.Alias, out var duplicateName))
                context.WarnDuplicateRuleName(duplicateName);

            results.TryAdd(new Rule
            {
                Id = block.Id,
                Ref = block.Ref,
                Alias = block.Alias,
                Source = block.Source,
                Tag = block.Tag,
                Level = block.Level,
                Info = block.Info,
                DependsOn = block.DependsOn,
                Flags = block.Flags,
                Extent = block.Extent,
                Labels = block.Labels,
            });
            knownRuleNames.AddNames(block.Id, block.Ref, block.Alias);
            knownRuleIds.AddIds(block.Id, block.Ref, block.Alias);
        }

        foreach (var block in blocks.OfType<RuleV1>())
        {
            if (knownRuleIds.ContainsIds(block.Id, block.Ref, block.Alias, out var duplicateId))
            {
                context.DuplicateResourceId(block.Id, duplicateId.Value);
                continue;
            }

            if (knownRuleNames.ContainsNames(block.Id, block.Ref, block.Alias, out var duplicateName))
                context.WarnDuplicateRuleName(duplicateName);

            context.EnterLanguageScope(block.Source);
            try
            {
                var info = GetRuleHelpInfo(context, block);
                results.TryAdd(new Rule
                {
                    Id = block.Id,
                    Ref = block.Ref,
                    Alias = block.Alias,
                    Source = block.Source,
                    Tag = block.Metadata.Tags,
                    Level = block.Level,
                    Info = info,
                    DependsOn = null, // TODO: No support for DependsOn yet
                    Flags = block.Flags,
                    Extent = block.Extent,
                    Labels = block.Metadata.Labels,
                });
                knownRuleNames.AddNames(block.Id, block.Ref, block.Alias);
                knownRuleIds.AddIds(block.Id, block.Ref, block.Alias);
            }
            finally
            {
                context.ExitLanguageScope(block.Source);
            }
        }
        return results;
    }

    private static DependencyTargetCollection<RuleBlock> ToRuleBlockV1(ILanguageBlock[] blocks, RunspaceContext context, bool skipDuplicateName)
    {
        // Index rules by RuleId
        var results = new DependencyTargetCollection<RuleBlock>();

        // Keep track of rule names and ids that have been added
        var knownRuleNames = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        var knownRuleIds = new HashSet<ResourceId>(ResourceIdEqualityComparer.Default);

        // Process from PowerShell
        foreach (var block in blocks.OfType<RuleBlock>())
        {
            if (knownRuleIds.ContainsIds(block.Id, block.Ref, block.Alias, out var duplicateId))
            {
                context.DuplicateResourceId(block.Id, duplicateId.Value);
                continue;
            }
            if (knownRuleNames.ContainsNames(block.Id, block.Ref, block.Alias, out var duplicateName))
            {
                context.WarnDuplicateRuleName(duplicateName);
                if (skipDuplicateName)
                    continue;
            }

            results.TryAdd(block);
            knownRuleNames.AddNames(block.Id, block.Ref, block.Alias);
            knownRuleIds.AddIds(block.Id, block.Ref, block.Alias);
        }

        // Process from YAML/ JSON
        foreach (var block in blocks.OfType<RuleV1>())
        {
            var ruleName = block.Name;
            if (knownRuleIds.ContainsIds(block.Id, block.Ref, block.Alias, out var duplicateId))
            {
                context.DuplicateResourceId(block.Id, duplicateId.Value);
                continue;
            }
            if (knownRuleNames.ContainsNames(block.Id, block.Ref, block.Alias, out var duplicateName))
            {
                context.WarnDuplicateRuleName(duplicateName);
                if (skipDuplicateName)
                    continue;
            }

            context.EnterLanguageScope(block.Source);
            context.LanguageScope.TryGetOverride(block.Id, out var propertyOverride);
            try
            {
                var info = GetRuleHelpInfo(context, block) ?? new RuleHelpInfo(
                    ruleName,
                    ruleName,
                    block.Source.Module,
                    synopsis: new InfoString(block.Synopsis)
                );
                MergeAnnotations(info, block.Metadata);

                results.TryAdd(new RuleBlock
                (
                    source: block.Source,
                    id: block.Id,
                    @ref: block.Ref,
                    @default: new RuleProperties
                    {
                        Level = block.Level
                    },
                    @override: propertyOverride,
                    info: info,
                    condition: new RuleVisitor(context, block.Id, block.Source, block.Spec),
                    alias: block.Alias,
                    tag: block.Metadata.Tags,
                    dependsOn: null,  // No support for DependsOn yet
                    configuration: null, // No support for rule configuration use module or workspace config
                    extent: null,
                    flags: block.Flags,
                    labels: block.Metadata.Labels
                ));
                knownRuleNames.AddNames(block.Id, block.Ref, block.Alias);
                knownRuleIds.AddIds(block.Id, block.Ref, block.Alias);
            }
            finally
            {
                context.ExitLanguageScope(block.Source);
            }
        }
        return results;
    }

    private static void MergeAnnotations(RuleHelpInfo info, ResourceMetadata metadata)
    {
        if (info == null || metadata == null || metadata.Annotations == null || metadata.Annotations.Count == 0)
            return;

        info.Annotations ??= new Hashtable(StringComparer.OrdinalIgnoreCase);
        foreach (var kv in metadata.Annotations)
        {
            if (!info.Annotations.ContainsKey(kv.Key))
                info.Annotations[kv.Key] = kv.Value;
        }
        if (!info.HasOnlineHelp())
            info.SetOnlineHelpUrl(metadata.Link);
    }

    private static RuleHelpInfo[] ToRuleHelp(IEnumerable<ILanguageBlock> blocks, RunspaceContext context)
    {
        // Index rules by RuleId
        var results = new Dictionary<string, RuleHelpInfo>(StringComparer.OrdinalIgnoreCase);

        foreach (var block in blocks.OfType<RuleBlock>())
        {
            context.EnterLanguageScope(block.Source);
            try
            {
                // Ignore rule blocks that don't match
                if (!Match(context, block))
                    continue;

                if (!results.ContainsKey(block.Id.Value))
                    results[block.Id.Value] = block.Info;
            }
            finally
            {
                context.ExitLanguageScope(block.Source);
            }

        }
        return results.Values.ToArray();
    }

    private static Baseline[] ToBaselineV1(IEnumerable<ILanguageBlock> blocks, RunspaceContext context)
    {
        if (blocks == null)
            return Array.Empty<Baseline>();

        // Index baselines by BaselineId
        var results = new Dictionary<string, Baseline>(StringComparer.OrdinalIgnoreCase);

        foreach (var block in blocks.OfType<Baseline>().ToArray())
        {
            context.EnterLanguageScope(block.Source);
            try
            {
                // Ignore baselines that don't match
                if (!Match(context, block))
                    continue;

                if (!results.ContainsKey(block.BaselineId))
                    results[block.BaselineId] = block;

            }
            finally
            {
                context.ExitLanguageScope(block.Source);
            }
        }
        return results.Values.ToArray();
    }

    private static SuppressionGroupV1[] ToSuppressionGroupV1(IEnumerable<ILanguageBlock> blocks, RunspaceContext context)
    {
        if (blocks == null)
            return Array.Empty<SuppressionGroupV1>();

        // Index suppression groups by Id.
        var results = new Dictionary<string, SuppressionGroupV1>(StringComparer.OrdinalIgnoreCase);

        foreach (var block in blocks.OfType<SuppressionGroupV1>().ToArray())
        {
            context.EnterLanguageScope(block.Source);
            try
            {
                // Ignore suppression groups that don't match.
                if (!Match(context, block))
                    continue;

                UpdateHelpInfo(context, block);
                if (!results.ContainsKey(block.Id.Value))
                    results[block.Id.Value] = block;
            }
            finally
            {
                context.ExitLanguageScope(block.Source);
            }
        }
        return results.Values.ToArray();
    }

    private static ModuleConfigV1[] ToModuleConfigV1(IEnumerable<ILanguageBlock> blocks, RunspaceContext context)
    {
        if (blocks == null)
            return Array.Empty<ModuleConfigV1>();

        // Index configurations by Name.
        var results = new Dictionary<string, ModuleConfigV1>(StringComparer.OrdinalIgnoreCase);
        foreach (var block in blocks.OfType<ModuleConfigV1>().ToArray())
        {
            if (!results.ContainsKey(block.Name))
                results[block.Name] = block;
        }
        return results.Values.ToArray();
    }

    /// <summary>
    /// Get conventions.
    /// </summary>
    private static IConvention[] GetConventions(ILanguageBlock[] blocks, RunspaceContext context)
    {
        // Index by Id.
        var index = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        var results = new List<IConvention>(blocks.Length);

        foreach (var block in blocks.OfType<ScriptBlockConvention>().ToArray())
        {
            context.EnterLanguageScope(block.Source);
            try
            {
                // Ignore blocks that don't match.
                if (!Match(context, block))
                    continue;

                if (!index.Contains(block.Id.Value))
                    results.Add(block);

            }
            finally
            {
                context.ExitLanguageScope(block.Source);
            }
        }
        return Sort(context, results.ToArray());
    }

    private static SelectorV1[] ToSelectorV1(IEnumerable<ILanguageBlock> blocks, RunspaceContext context)
    {
        if (blocks == null)
            return Array.Empty<SelectorV1>();

        // Index selectors by Id.
        var results = new Dictionary<string, SelectorV1>(StringComparer.OrdinalIgnoreCase);

        foreach (var block in blocks.OfType<SelectorV1>().ToArray())
        {
            context.EnterLanguageScope(block.Source);
            try
            {
                // Ignore selectors that don't match.
                if (!Match(context, block))
                    continue;

                if (!results.ContainsKey(block.Id.Value))
                    results[block.Id.Value] = block;
            }
            finally
            {
                context.ExitLanguageScope(block.Source);
            }
        }
        return results.Values.ToArray();
    }

    private static void Import(IConvention[] blocks, RunspaceContext context)
    {
        foreach (var resource in blocks)
            context.Import(resource);
    }

    private static bool Match(RunspaceContext context, RuleBlock resource)
    {
        var filter = context.LanguageScope.GetFilter(ResourceKind.Rule);
        return filter == null || filter.Match(resource);
    }

    private static bool Match(RunspaceContext context, IRuleV1 resource)
    {
        context.EnterLanguageScope(resource.Source);
        var filter = context.LanguageScope.GetFilter(ResourceKind.Rule);
        return filter == null || filter.Match(resource);
    }

    private static bool Match(RunspaceContext context, Baseline resource)
    {
        var filter = context.LanguageScope.GetFilter(ResourceKind.Baseline);
        return filter == null || filter.Match(resource);
    }

    private static bool Match(RunspaceContext context, ScriptBlockConvention block)
    {
        var filter = context.LanguageScope.GetFilter(ResourceKind.Convention);
        return filter == null || filter.Match(block);
    }

    private static bool Match(RunspaceContext context, SelectorV1 resource)
    {
        var filter = context.LanguageScope.GetFilter(ResourceKind.Selector);
        return filter == null || filter.Match(resource);
    }

    private static bool Match(RunspaceContext context, SuppressionGroupV1 suppressionGroup)
    {
        var filter = context.LanguageScope.GetFilter(ResourceKind.SuppressionGroup);
        return filter == null || filter.Match(suppressionGroup);
    }

    private static IConvention[] Sort(RunspaceContext context, IConvention[] conventions)
    {
        Array.Sort(conventions, new ConventionComparer(context));
        return conventions;
    }

    internal static RuleHelpInfo GetRuleHelpInfo(RunspaceContext context, string name, string defaultSynopsis, string defaultDisplayName, InfoString defaultDescription, InfoString defaultRecommendation)
    {
        return !TryHelpPath(context, name, out var path, out var culture) || !TryDocument(path, culture, out var document)
            ? new RuleHelpInfo(
                name: name,
                displayName: defaultDisplayName ?? name,
                moduleName: context.Source.File.Module,
                synopsis: InfoString.Create(defaultSynopsis),
                description: defaultDescription,
                recommendation: defaultRecommendation
            )
            : new RuleHelpInfo(
                name: name,
                displayName: document.Name ?? defaultDisplayName ?? name,
                moduleName: context.Source.File.Module,
                synopsis: document.Synopsis ?? new InfoString(defaultSynopsis),
                description: document.Description ?? defaultDescription,
                recommendation: document.Recommendation ?? defaultRecommendation ?? document.Synopsis ?? InfoString.Create(defaultSynopsis)
            )
            {
                Notes = document.Notes?.Text,
                Links = GetLinks(document.Links),
                Annotations = document.Annotations?.ToHashtable()
            };
    }

    private static RuleHelpInfo GetRuleHelpInfo(RunspaceContext context, IRuleV1 rule)
    {
        return GetRuleHelpInfo(context, rule.Name, rule.Synopsis, rule.Info.DisplayName, rule.Info.Description, rule.Recommendation);
    }

    internal static void UpdateHelpInfo(RunspaceContext context, IResource resource)
    {
        if (context == null || resource == null || !TryHelpPath(context, resource.Name, out var path, out var culture) || !TryHelpInfo(path, culture, out var info))
            return;

        resource.Info.Update(info);
    }

    private static bool TryHelpPath(RunspaceContext context, string name, out string path, out string culture)
    {
        path = null;
        culture = null;
        if (string.IsNullOrEmpty(context.Source.File.HelpPath))
            return false;

        var helpFileName = string.Concat(name, Markdown_Extension);
        path = context.GetLocalizedPath(helpFileName, out culture);
        return path != null;
    }

    private static bool TryDocument(string path, string culture, out RuleDocument document)
    {
        document = null;
        var markdown = File.ReadAllText(path);
        if (string.IsNullOrEmpty(markdown))
            return false;

        var reader = new MarkdownReader(yamlHeaderOnly: false);
        var stream = reader.Read(markdown, path);
        var lexer = new RuleHelpLexer(culture);
        document = lexer.Process(stream);
        return document != null;
    }

    private static bool TryHelpInfo(string path, string culture, out IResourceHelpInfo info)
    {
        info = null;
        var markdown = File.ReadAllText(path);
        if (string.IsNullOrEmpty(markdown))
            return false;

        var reader = new MarkdownReader(yamlHeaderOnly: false);
        var stream = reader.Read(markdown, path);
        var lexer = new ResourceHelpLexer(culture);
        info = lexer.Process(stream).ToInfo();
        return info != null;
    }

    private static Rules.Link[] GetLinks(Help.Link[] links)
    {
        if (links == null || links.Length == 0)
            return null;

        var result = new Rules.Link[links.Length];
        for (var i = 0; i < links.Length; i++)
            result[i] = new Rules.Link(links[i].Name, links[i].Uri);

        return result;
    }
}
