using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
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
            SerializerSettings = new JsonSerializerSettings();
            DefaultReferenceTypeNullHandling = ReferenceTypeNullHandling.NotNull;
            SchemaNameGenerator = new NamespacePrefixedSchemaNameGenerator();
            SerializerSettings.Converters.Add(new StringEnumConverter());
        }
    }
}
