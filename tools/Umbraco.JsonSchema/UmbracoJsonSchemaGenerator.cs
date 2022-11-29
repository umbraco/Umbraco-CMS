using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Serialization;
using NJsonSchema.Generation;

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
            FlattenInheritanceHierarchy = true,
            IgnoreObsoleteProperties = true,
            SerializerSettings = new JsonSerializerSettings()
            {
                ContractResolver = new WritablePropertiesOnlyResolver()
            }
        })
    {
        Settings.SerializerSettings.Converters.Add(new StringEnumConverter());
    }

    /// <inheritdoc />
    private class WritablePropertiesOnlyResolver : DefaultContractResolver
    {
        /// <inheritdoc />
        protected override IList<JsonProperty> CreateProperties(Type type, MemberSerialization memberSerialization)
            => base.CreateProperties(type, memberSerialization).Where(p => p.Writable).ToList();
    }
}
