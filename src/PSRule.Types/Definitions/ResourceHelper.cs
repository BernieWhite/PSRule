// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using PSRule.Definitions.Rules;

namespace PSRule.Definitions;

internal static class ResourceHelper
{
    private const string ANNOTATION_OBSOLETE = "obsolete";

    private const char SCOPE_SEPARATOR = '\\';

    internal const string STANDALONE_SCOPENAME = ".";

    internal static string GetIdString(string scope, string name)
    {
        return name.IndexOf(SCOPE_SEPARATOR) >= 0
            ? name
            : string.Concat(
            NormalizeScope(scope),
            SCOPE_SEPARATOR,
            name
        );
    }

    internal static void ParseIdString(string defaultScope, string id, out string? scope, out string? name)
    {
        ParseIdString(id, out scope, out name);
        scope ??= NormalizeScope(defaultScope);
    }

    internal static void ParseIdString(string id, out string? scope, out string? name)
    {
        scope = null;
        name = null;
        if (string.IsNullOrEmpty(id))
            return;

        var scopeSeparator = id.IndexOf(SCOPE_SEPARATOR);
        scope = scopeSeparator >= 0 ? id.Substring(0, scopeSeparator) : null;
        name = id.Substring(scopeSeparator + 1);
    }

    /// <summary>
    /// Checks each resource name and converts each into a full qualified <seealso cref="ResourceId"/>.
    /// </summary>
    /// <param name="defaultScope">The default scope to use if the resource name if not fully qualified.</param>
    /// <param name="name">An array of names. Qualified names (RuleIds) supplied are left intact.</param>
    /// <param name="kind">The <seealso cref="ResourceIdKind"/> of the <seealso cref="ResourceId"/>.</param>
    /// <returns>An array of RuleIds.</returns>
    internal static ResourceId[]? GetRuleId(string defaultScope, string[] name, ResourceIdKind kind)
    {
        if (name == null || name.Length == 0)
            return null;

        var result = new ResourceId[name.Length];
        for (var i = 0; i < name.Length; i++)
            result[i] = GetRuleId(defaultScope, name[i], kind);

        return (result.Length == 0) ? null : result;
    }

    internal static ResourceId GetRuleId(string? defaultScope, string name, ResourceIdKind kind)
    {
        defaultScope ??= STANDALONE_SCOPENAME;
        return name.IndexOf(SCOPE_SEPARATOR) > 0 ? ResourceId.Parse(name, kind) : new ResourceId(defaultScope, name, kind);
    }

    internal static ResourceId? GetIdNullable(string scope, string name, ResourceIdKind kind)
    {
        return string.IsNullOrEmpty(name) ? null : new ResourceId(scope, name, kind);
    }

    internal static bool IsObsolete(IResourceMetadata metadata)
    {
        return metadata != null &&
            metadata.Annotations != null &&
            metadata.Annotations.TryGetBool(ANNOTATION_OBSOLETE, out var obsolete)
            && obsolete.GetValueOrDefault(false);
    }

    internal static SeverityLevel GetLevel(SeverityLevel? level)
    {
        return !level.HasValue || level.Value == SeverityLevel.None ? SeverityLevel.Error : level.Value;
    }

    internal static string NormalizeScope(string? scope)
    {
        return scope == null || string.IsNullOrEmpty(scope) ? STANDALONE_SCOPENAME : scope;
    }
}
