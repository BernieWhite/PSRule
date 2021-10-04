﻿// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Newtonsoft.Json;
using PSRule.Configuration;

namespace PSRule.Pipeline.Output
{
    internal sealed class JsonOutputWriter : SerializationOutputWriter<object>
    {
        internal JsonOutputWriter(PipelineWriter inner, PSRuleOption option)
            : base(inner, option) { }

        protected override string Serialize(object[] o)
        {
            var settings = new JsonSerializerSettings
            {
                NullValueHandling = NullValueHandling.Ignore,
                Formatting = Formatting.Indented
            };
            settings.Converters.Add(new ErrorCategoryJsonConverter());
            settings.ContractResolver = new SortedPropertyContractResolver();
            return JsonConvert.SerializeObject(o, settings: settings);
        }
    }
}
