﻿// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Net;
using System.Security;

namespace PSRule
{
    internal static class EnvironmentHelperExtensions
    {
        public static bool IsAzurePipelines(this EnvironmentHelper helper)
        {
            return helper.TryBool("TF_BUILD", out bool azp) && azp;
        }

        public static bool IsGitHubActions(this EnvironmentHelper helper)
        {
            return helper.TryBool("GITHUB_ACTIONS", out bool gh) && gh;
        }

        public static bool IsVisualStudioCode(this EnvironmentHelper helper)
        {
            return helper.TryString("TERM_PROGRAM", out string term) && term == "vscode";
        }

        public static string GetRunId(this EnvironmentHelper helper)
        {
            if (helper.TryString("PSRULE_RUN_ID", out string runId))
                return runId;

            if (helper.TryString("BUILD_REPOSITORY_NAME", out string prefix) && helper.TryString("BUILD_BUILDID", out string suffix))
                return string.Concat(prefix, "/", suffix);

            if (helper.TryString("GITHUB_REPOSITORY", out prefix) && helper.TryString("GITHUB_RUN_ID", out suffix))
                return string.Concat(prefix, "/", suffix);

            return null;
        }
    }

    internal sealed class EnvironmentHelper
    {
        private readonly static char[] STRINGARRAY_SEPARATOR = new char[] { ';' };

        public static readonly EnvironmentHelper Default = new EnvironmentHelper();

        internal bool TryString(string key, out string value)
        {
            return TryVariable(key, out value) && !string.IsNullOrEmpty(value);
        }

        internal bool TrySecureString(string key, out SecureString value)
        {
            value = null;
            if (!TryString(key, out string variable))
                return false;

            value = new NetworkCredential("na", variable).SecurePassword;
            return true;
        }

        internal bool TryInt(string key, out int value)
        {
            value = default;
            return TryVariable(key, out string variable) && int.TryParse(variable, out value);
        }

        internal bool TryBool(string key, out bool value)
        {
            value = default;
            return TryVariable(key, out string variable) && TryParseBool(variable, out value);
        }

        internal bool TryEnum<TEnum>(string key, out TEnum value) where TEnum : struct
        {
            value = default;
            if (!TryVariable(key, out string variable))
                return false;

            return Enum.TryParse(variable, ignoreCase: true, out value);
        }

        internal bool TryStringArray(string key, out string[] value)
        {
            value = default;
            if (!TryVariable(key, out string variable))
                return false;

            value = variable.Split(STRINGARRAY_SEPARATOR, options: StringSplitOptions.RemoveEmptyEntries);
            return value != null;
        }

        private bool TryVariable(string key, out string variable)
        {
            variable = Environment.GetEnvironmentVariable(key);
            return variable != null;
        }

        private static bool TryParseBool(string variable, out bool value)
        {
            if (bool.TryParse(variable, out value))
                return true;

            if (int.TryParse(variable, out int ivalue))
            {
                value = ivalue > 0;
                return true;
            }
            return false;
        }

        internal IEnumerable<KeyValuePair<string, object>> WithPrefix(string prefix)
        {
            var env = Environment.GetEnvironmentVariables();
            var enumerator = env.GetEnumerator();
            while (enumerator.MoveNext())
            {
                var key = enumerator.Key.ToString();
                if (key.StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
                {
                    yield return new KeyValuePair<string, object>(key, enumerator.Value);
                }
            }
        }
    }
}