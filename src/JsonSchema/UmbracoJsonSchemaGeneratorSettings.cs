using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using NJsonSchema.Generation;
using NJsonSchema.Generation.TypeMappers;
using Umbraco.Cms.Core;

namespace JsonSchema
{
    public class UmbracoJsonSchemaGeneratorSettings : JsonSchemaGeneratorSettings
    {
        public UmbracoJsonSchemaGeneratorSettings()
        {
            AlwaysAllowAdditionalObjectProperties = true;
            SerializerSettings = new JsonSerializerSettings();
            TypeNameGenerator = new UmbracoPrefixedTypeNameGenerator();
            DefaultReferenceTypeNullHandling = ReferenceTypeNullHandling.NotNull;

            SerializerSettings.Converters.Add(new StringEnumConverter());
        }
    }
}
