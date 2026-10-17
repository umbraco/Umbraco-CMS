using Microsoft.Extensions.DependencyInjection;
using Microsoft.OpenApi;
using Swashbuckle.AspNetCore.SwaggerGen;
using Umbraco.Cms.Core.DependencyInjection;

namespace Umbraco.Cms.Api.Management.OpenApi;

/// <summary>
/// A schema filter for OpenAPI that enforces non-nullable properties to be required in the generated schema.
/// </summary>
public class RequireNonNullablePropertiesSchemaFilter : ISchemaFilter
{
    // Resolved on first use, as the resolver depends on the schema generator options, which construct this filter.
    private readonly Lazy<ISerializerDataContractResolver> _dataContractResolver;

    /// <summary>
    /// Initializes a new instance of the <see cref="RequireNonNullablePropertiesSchemaFilter"/> class.
    /// </summary>
    [Obsolete("Please use the constructor with all parameters. Scheduled for removal in Umbraco 19.")]
    public RequireNonNullablePropertiesSchemaFilter()
        : this(StaticServiceProvider.Instance)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="RequireNonNullablePropertiesSchemaFilter"/> class.
    /// </summary>
    /// <param name="serviceProvider">The service provider used to resolve the <see cref="ISerializerDataContractResolver"/>.</param>
    [ActivatorUtilitiesConstructor]
    public RequireNonNullablePropertiesSchemaFilter(IServiceProvider serviceProvider)
        => _dataContractResolver = new Lazy<ISerializerDataContractResolver>(serviceProvider.GetRequiredService<ISerializerDataContractResolver>);

    /// <summary>
    /// Add to model.Required all properties that are not nullable.
    /// </summary>
    public void Apply(IOpenApiSchema model, SchemaFilterContext context)
    {
        if (model is not OpenApiSchema schema || schema.Properties is null)
        {
            return;
        }

        Dictionary<string, DataProperty>? dataProperties = null;

        schema.Required ??= new SortedSet<string>();
        foreach ((var propKey, IOpenApiSchema propertySchema) in schema.Properties)
        {
            if (schema.Required.Contains(propKey) || IsNullable(propertySchema, propKey, context.Type, ref dataProperties))
            {
                continue;
            }

            schema.Required.Add(propKey);
        }
    }

    private bool IsNullable(IOpenApiSchema propertySchema, string propertyName, Type type, ref Dictionary<string, DataProperty>? dataProperties)
    {
        if (propertySchema.Type?.HasFlag(JsonSchemaType.Null) is true
            || ContainsNullType(propertySchema.OneOf)
            || ContainsNullType(propertySchema.AnyOf))
        {
            return true;
        }

        // A schema without a type (e.g. for object) carries no nullability, so use the declared nullability of the member.
        if (propertySchema.Type is null
            && propertySchema is not OpenApiSchemaReference
            && propertySchema.OneOf is not { Count: > 0 }
            && propertySchema.AnyOf is not { Count: > 0 }
            && propertySchema.AllOf is not { Count: > 0 })
        {
            dataProperties ??= GetDataProperties(type);
            return dataProperties.TryGetValue(propertyName, out DataProperty? dataProperty) && dataProperty.IsNullable;
        }

        return false;
    }

    private static bool ContainsNullType(IList<IOpenApiSchema>? schemas)
        => schemas?.Any(s => s.Type == JsonSchemaType.Null) is true;

    private Dictionary<string, DataProperty> GetDataProperties(Type type)
        => _dataContractResolver.Value.GetDataContractForType(type).ObjectProperties?.ToDictionary(p => p.Name) ?? [];
}
