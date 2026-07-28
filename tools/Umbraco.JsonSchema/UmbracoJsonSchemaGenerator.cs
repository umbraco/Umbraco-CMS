using System.Text.Json;
using System.Text.Json.Serialization;
using Namotion.Reflection;
using NJsonSchema;
using NJsonSchema.Generation;

namespace Umbraco.JsonSchema;

/// <inheritdoc />
internal sealed class UmbracoJsonSchemaGenerator : JsonSchemaGenerator
{
    /// <summary>
    /// Initializes a new instance of the <see cref="UmbracoJsonSchemaGenerator" /> class.
    /// </summary>
    public UmbracoJsonSchemaGenerator()
        : base(new SystemTextJsonSchemaGeneratorSettings()
        {
            AlwaysAllowAdditionalObjectProperties = true,
            FlattenInheritanceHierarchy = true,
            IgnoreObsoleteProperties = true,
            ReflectionService = new UmbracoSystemTextJsonReflectionService(),
            SerializerOptions = new JsonSerializerOptions()
            {
                Converters = { new JsonStringEnumConverter() },
                IgnoreReadOnlyProperties = true,
            },
        })
    { }

    /// <inheritdoc />
    private sealed class UmbracoSystemTextJsonReflectionService : SystemTextJsonReflectionService
    {
        /// <inheritdoc />
        public override void GenerateProperties(NJsonSchema.JsonSchema schema, ContextualType contextualType, SystemTextJsonSchemaGeneratorSettings settings, JsonSchemaGenerator schemaGenerator, JsonSchemaResolver schemaResolver)
        {
            // Populate schema properties
            base.GenerateProperties(schema, contextualType, settings, schemaGenerator, schemaResolver);

            if (settings.SerializerOptions.IgnoreReadOnlyProperties)
            {
                // Remove read-only properties (because this is not implemented by the base class)
                foreach (ContextualPropertyInfo property in contextualType.Properties)
                {
                    if (property.CanWrite is false)
                    {
                        string propertyName = GetPropertyName(property, settings);

                        schema.Properties.Remove(propertyName);
                    }
                }
            }

            // TimeSpan values are bound using TimeSpan.Parse, which doesn't accept the ISO 8601
            // durations asserted by the format the base class infers, so remove it
            // (https://github.com/umbraco/Umbraco-CMS/issues/23482).
            foreach (ContextualPropertyInfo property in contextualType.Properties)
            {
                if (property.PropertyInfo.PropertyType != typeof(TimeSpan) &&
                    property.PropertyInfo.PropertyType != typeof(TimeSpan?))
                {
                    continue;
                }

                string propertyName = GetPropertyName(property, settings);

                if (schema.Properties.TryGetValue(propertyName, out JsonSchemaProperty? propertySchema))
                {
                    propertySchema.Format = null;
                }
            }
        }
    }
}
