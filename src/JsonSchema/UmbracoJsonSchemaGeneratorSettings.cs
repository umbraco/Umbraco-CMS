// Copyright (c) Umbraco.
// See LICENSE for more details.

using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Serialization;
using NJsonSchema.Generation;

namespace JsonSchema
{
    /// <inheritdoc />
    public class UmbracoJsonSchemaGeneratorSettings : JsonSchemaGeneratorSettings
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="UmbracoJsonSchemaGeneratorSettings"/> class.
        /// </summary>
        public UmbracoJsonSchemaGeneratorSettings()
        {
            AlwaysAllowAdditionalObjectProperties = true;
            SerializerSettings = new JsonSerializerSettings()
            {
                ContractResolver = new WritablePropertiesOnlyResolver()
            };
            DefaultReferenceTypeNullHandling = ReferenceTypeNullHandling.NotNull;
            SchemaNameGenerator = new NamespacePrefixedSchemaNameGenerator();
            SerializerSettings.Converters.Add(new StringEnumConverter());
            IgnoreObsoleteProperties = true;
            GenerateExamples = true;
        }

        private class WritablePropertiesOnlyResolver : DefaultContractResolver
        {
            protected override IList<JsonProperty> CreateProperties(Type type, MemberSerialization memberSerialization)
            {
                IList<JsonProperty> props = base.CreateProperties(type, memberSerialization);
                return props.Where(p => p.Writable).ToList();
            }
        }
    }
}
