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
        /// Creates a new instance of <see cref="UmbracoJsonSchemaGeneratorSettings"/>.
        /// </summary>
        /// <param name="definitionPrefix">The prefix to use for definitions generated.</param>
        public UmbracoJsonSchemaGeneratorSettings()
        {
            AlwaysAllowAdditionalObjectProperties = true;
            SerializerSettings = new JsonSerializerSettings()
            {
                ContractResolver =  new WritablePropertiesOnlyResolver()
            };
            DefaultReferenceTypeNullHandling = ReferenceTypeNullHandling.NotNull;
            SchemaNameGenerator = new NamespacePrefixedSchemaNameGenerator();
            SerializerSettings.Converters.Add(new StringEnumConverter());

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
