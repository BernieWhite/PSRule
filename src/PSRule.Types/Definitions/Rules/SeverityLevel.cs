// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Newtonsoft.Json.Converters;
using Newtonsoft.Json;

namespace PSRule.Definitions.Rules
{
    /// <summary>
    /// If the rule fails, how serious is the result.
    /// </summary>
    [JsonConverter(typeof(StringEnumConverter))]
    public enum SeverityLevel
    {
        /// <summary>
        /// Severity is unset.
        /// </summary>
        None = 0,

        /// <summary>
        /// A failure generates an error.
        /// </summary>
        Error = 1,

        /// <summary>
        /// A failure generates a warning.
        /// </summary>
        Warning = 2,

        /// <summary>
        /// A failure generate an informational message.
        /// </summary>
        Information = 3
    }
}
