using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using NJsonSchema.Generation;

namespace Umbraco.JsonSchema.Core;

/// <inheritdoc />
public class UmbracoJsonSchemaGenerator : JsonSchemaGenerator
{
    /// <summary>
    /// Initializes a new instance of the <see cref="UmbracoJsonSchemaGenerator" /> class.
    /// </summary>
    public UmbracoJsonSchemaGenerator()
        : base(new JsonSchemaGeneratorSettings()
        {
            AlwaysAllowAdditionalObjectProperties = true,
            DefaultReferenceTypeNullHandling = ReferenceTypeNullHandling.NotNull,
            GenerateExamples = true,
            IgnoreObsoleteProperties = true,
            SchemaNameGenerator = new NamespacePrefixedSchemaNameGenerator(),
            SerializerSettings = new JsonSerializerSettings()
            {
                ContractResolver = new WritablePropertiesOnlyResolver()
            }
        })
    {
        Settings.SerializerSettings.Converters.Add(new StringEnumConverter());
    }
}
