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
        public UmbracoJsonSchemaGeneratorSettings(string definitionPrefix)
        {
            AlwaysAllowAdditionalObjectProperties = true;
            SerializerSettings = new JsonSerializerSettings();
            TypeNameGenerator = new PrefixedTypeNameGenerator(definitionPrefix);
            DefaultReferenceTypeNullHandling = ReferenceTypeNullHandling.NotNull;

            SerializerSettings.Converters.Add(new StringEnumConverter());
        }
    }
}
