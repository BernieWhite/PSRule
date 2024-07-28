// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Diagnostics;
using System.Globalization;
using System.Net;
using System.Security;
using System.Text;
using PSRule.Data;

namespace PSRule;

/// <summary>
/// A helper for accessing environment and runtime variables.
/// </summary>
public static class Environment
{
    private static readonly char[] STRINGARRAYMAP_ITEMSEPARATOR = new char[] { ',' };
    private static readonly char[] STRINGARRAY_SEPARATOR = new char[] { ';' };
    private static readonly char[] LINUX_PATH_ENV_SEPARATOR = new char[] { ':' };
    private static readonly char[] WINDOWS_PATH_ENV_SEPARATOR = new char[] { ';' };

    private const char BACKSLASH = '\\';
    private const char SLASH = '/';

    private const char STRINGARRYAMAP_PAIRSEPARATOR = '=';
    private const string PATH_ENV = "PATH";
    private const string DEFAULT_CREDENTIAL_USERNAME = "na";
    private const string TF_BUILD = "TF_BUILD";
    private const string GITHUB_ACTIONS = "GITHUB_ACTIONS";

    /// <summary>
    /// A callback that is overridden by PowerShell so that the current working path can be retrieved.
    /// </summary>
    private static WorkingPathResolver _GetWorkingPath = () => Directory.GetCurrentDirectory();

    /// <summary>
    /// Sets the current culture to use when processing rules unless otherwise specified.
    /// </summary>
    private static CultureInfo _CurrentCulture = Thread.CurrentThread.CurrentCulture;

    /// <summary>
    /// A delegate to allow callback get current working path.
    /// </summary>
    public delegate string WorkingPathResolver();

    /// <summary>
    /// Configures PSRule to use the culture of the current thread at runtime.
    /// </summary>
    [DebuggerStepThrough]
    public static void UseCurrentCulture()
    {
        UseCurrentCulture(Thread.CurrentThread.CurrentCulture);
    }

    /// <summary>
    /// Configures PSRule to use the specified culture at runtime.
    /// </summary>
    /// <param name="culture">A valid culture.</param>
    [DebuggerStepThrough]
    public static void UseCurrentCulture(string culture)
    {
        UseCurrentCulture(CultureInfo.CreateSpecificCulture(culture));
    }

    /// <summary>
    /// Configures PSRule to use the specified culture at runtime. 
    /// </summary>
    /// <param name="culture">A valid culture.</param>
    public static void UseCurrentCulture(CultureInfo culture)
    {
        _CurrentCulture = culture;
    }

    /// <summary>
    /// Configures PSRule to use the specified resolver to determine the current working path.
    /// </summary>
    /// <param name="resolver">A method that can be used to resolve the current working path.</param>
    internal static void UseWorkingPathResolver(WorkingPathResolver resolver)
    {
        _GetWorkingPath = resolver;
    }

    /// <summary>
    /// Gets the current working path being used by PSRule.
    /// </summary>
    /// <returns>The current working path.</returns>
    public static string GetWorkingPath()
    {
        return _GetWorkingPath();
    }

    /// <summary>
    /// Get the current culture being used by PSRule.
    /// </summary>
    /// <returns>The current culture.</returns>
    public static CultureInfo GetCurrentCulture()
    {
        return _CurrentCulture;
    }

    /// <summary>
    /// Get a full path instead of a relative path that may be passed from PowerShell.
    /// </summary>
    /// <param name="path">A full or relative path.</param>
    /// <param name="normalize">When set to <c>true</c> the returned path uses forward slashes instead of backslashes.</param>
    /// <param name="basePath">The base path to use. When <c>null</c> of unspecified, the current working path will be used.</param>
    /// <returns>A absolute path.</returns>
    internal static string GetRootedPath(string? path, bool normalize = false, string? basePath = null)
    {
        basePath ??= GetWorkingPath();
        if (string.IsNullOrEmpty(path))
            path = normalize ? string.Empty : basePath;

        var rootedPath = Path.IsPathRooted(path) ? Path.GetFullPath(path) : Path.GetFullPath(Path.Combine(basePath, path));
        return normalize ? rootedPath.Replace(BACKSLASH, SLASH) : rootedPath;
    }

    /// <summary>
    /// Get a full base path instead of a relative path that may be passed from PowerShell.
    /// </summary>
    /// <param name="path">A full or relative path.</param>
    /// <param name="normalize">When set to <c>true</c> the returned path uses forward slashes instead of backslashes.</param>
    /// <param name="basePath">A base path to use if the <c>path</c> is relative.</param>
    /// <returns>A absolute base path.</returns>
    /// <remarks>
    /// A base path always includes a trailing <c>/</c>.
    /// </remarks>
    internal static string GetRootedBasePath(string path, bool normalize = false, string? basePath = null)
    {
        if (string.IsNullOrEmpty(path))
            path = string.Empty;

        var rootedPath = GetRootedPath(path, basePath: basePath);
        var result = rootedPath.Length > 0 && IsPathSeparator(rootedPath[rootedPath.Length - 1])
            ? rootedPath
            : string.Concat(rootedPath, Path.DirectorySeparatorChar);
        return normalize ? result.Replace(BACKSLASH, SLASH) : result;
    }

    /// <summary>
    /// Determine if the environment is running within Azure Pipelines.
    /// </summary>
    public static bool IsAzurePipelines()
    {
        return TryBool(TF_BUILD, out var azp) && azp.HasValue && azp.Value;
    }

    /// <summary>
    /// Determines if the environment is running within GitHub Actions.
    /// </summary>
    public static bool IsGitHubActions()
    {
        return TryBool(GITHUB_ACTIONS, out var gh) && gh.HasValue && gh.Value;
    }

    /// <summary>
    /// Determine if the environment is running within Visual Studio Code.
    /// </summary>
    public static bool IsVisualStudioCode()
    {
        return TryString("TERM_PROGRAM", out var term) && term == "vscode";
    }

    /// <summary>
    /// Get the run identifier for the current environment.
    /// </summary>
    public static string? GetRunId()
    {
        if (TryString("PSRULE_RUN_ID", out var runId) && runId != null)
            return runId;

        return TryString("BUILD_REPOSITORY_NAME", out var prefix) && TryString("BUILD_BUILDID", out var suffix) ||
            TryString("GITHUB_REPOSITORY", out prefix) && TryString("GITHUB_RUN_ID", out suffix)
            ? string.Concat(prefix, "/", suffix)
            : null;
    }

    /// <summary>
    /// Try to get the environment variable as a <see cref="string"/>.
    /// </summary>
    public static bool TryString(string key, out string? value)
    {
        return TryVariable(key, out value) && !string.IsNullOrEmpty(value);
    }

    /// <summary>
    /// Try to get the environment variable as a <see cref="SecureString"/>.
    /// </summary>
    public static bool TrySecureString(string key, out SecureString? value)
    {
        value = null;
        if (!TryString(key, out var variable))
            return false;

        value = new NetworkCredential(DEFAULT_CREDENTIAL_USERNAME, variable).SecurePassword;
        return true;
    }

    /// <summary>
    /// Try to get the environment variable as an <see cref="int"/>.
    /// </summary>
    public static bool TryInt(string key, out int value)
    {
        value = default;
        return TryVariable(key, out var variable) && int.TryParse(variable, out value);
    }

    /// <summary>
    /// Try to get the environment variable as a <see cref="bool"/>.
    /// </summary>
    public static bool TryBool(string key, out bool? value)
    {
        value = default;
        return TryVariable(key, out var variable) && TryParseBool(variable, out value);
    }

    /// <summary>
    /// Try to get the environment variable as a enum of type <typeparamref name="TEnum"/>.
    /// </summary>
    public static bool TryEnum<TEnum>(string key, out TEnum value) where TEnum : struct
    {
        value = default;
        return TryVariable(key, out var variable) && Enum.TryParse(variable, ignoreCase: true, out value);
    }

    /// <summary>
    /// Try to get the environment variable as an array of strings.
    /// </summary>
    public static bool TryStringArray(string key, out string[]? value)
    {
        value = default;
        if (!TryVariable(key, out var variable) || variable == null)
            return false;

        value = variable.Split(STRINGARRAY_SEPARATOR, options: StringSplitOptions.RemoveEmptyEntries);
        return value != null;
    }

    /// <summary>
    /// Try to get the environment variable as a <see cref="StringArrayMap"/>.
    /// </summary>
    public static bool TryStringArrayMap(string key, out StringArrayMap? value)
    {
        value = default;
        if (!TryVariable(key, out var variable) || variable == null)
            return false;

        var pairs = variable.Split(STRINGARRAY_SEPARATOR, options: StringSplitOptions.RemoveEmptyEntries);
        if (pairs == null)
            return false;

        var map = new StringArrayMap();
        for (var i = 0; i < pairs.Length; i++)
        {
            var index = pairs[i].IndexOf(STRINGARRYAMAP_PAIRSEPARATOR);
            if (index < 1 || index + 1 >= pairs[i].Length) continue;

            var left = pairs[i].Substring(0, index);
            var right = pairs[i].Substring(index + 1);
            var pair = right.Split(STRINGARRAYMAP_ITEMSEPARATOR, StringSplitOptions.RemoveEmptyEntries);
            map[left] = pair;
        }
        value = map;
        return true;
    }

    /// <summary>
    /// Try to get the PATH environment variable.
    /// </summary>
    public static bool TryPathEnvironmentVariable(out string[]? value)
    {
        return TryPathEnvironmentVariable(PATH_ENV, out value);
    }

    /// <summary>
    /// Try to get a PATH environment variable with a specific name.
    /// </summary>
    public static bool TryPathEnvironmentVariable(string key, out string[]? value)
    {
        value = default;
        if (!TryVariable(key, out var variable) || variable == null)
            return false;

        var separator = System.Environment.OSVersion.Platform == PlatformID.Win32NT ? WINDOWS_PATH_ENV_SEPARATOR : LINUX_PATH_ENV_SEPARATOR;
        value = variable.Split(separator, options: StringSplitOptions.RemoveEmptyEntries);
        return value != null;
    }

    /// <summary>
    /// Combines one or more environment variables into a single string.
    /// </summary>
    public static string CombineEnvironmentVariable(params string[] args)
    {
        var sb = new StringBuilder();
        var separator = System.Environment.OSVersion.Platform == PlatformID.Win32NT ? WINDOWS_PATH_ENV_SEPARATOR : LINUX_PATH_ENV_SEPARATOR;

        for (var i = 0; args != null && i < args.Length; i++)
        {
            if (string.IsNullOrEmpty(args[i]))
                continue;

            if (sb.Length > 0)
            {
                sb.Append(separator);
            }
            sb.Append(args[i]);
        }
        return sb.Length == 0 ? string.Empty : sb.ToString();
    }

    /// <summary>
    /// Try to get any environment variable with a specific prefix.
    /// </summary>
    public static IEnumerable<KeyValuePair<string, object>> GetByPrefix(string? prefix)
    {
        var env = System.Environment.GetEnvironmentVariables();
        var enumerator = env.GetEnumerator();
        while (enumerator.MoveNext())
        {
            var key = enumerator.Key.ToString();
            if (prefix == null || key.StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
                yield return new KeyValuePair<string, object>(key, enumerator.Value);
        }
    }

    private static bool TryVariable(string key, out string? variable)
    {
        variable = System.Environment.GetEnvironmentVariable(key);
        return variable != null;
    }

    private static bool TryParseBool(string? variable, out bool? value)
    {
        value = default;
        if (variable == null)
            return false;

        if (bool.TryParse(variable, out var b))
        {
            value = b;
            return true;
        }
        if (int.TryParse(variable, out var i))
        {
            value = i > 0;
            return true;
        }
        return false;
    }

    /// <summary>
    /// Determine if the <seealso cref="char"/> is a path separator character.
    /// </summary>
    /// <param name="c">The character to check.</param>
    /// <returns>Returns <c>true</c> if the character is a path separator. Otherwise <c>false</c> is returned.</returns>
    [DebuggerStepThrough]
    private static bool IsPathSeparator(char c)
    {
        return c == Path.DirectorySeparatorChar || c == Path.AltDirectorySeparatorChar || c == SLASH || c == BACKSLASH;
    }
}
