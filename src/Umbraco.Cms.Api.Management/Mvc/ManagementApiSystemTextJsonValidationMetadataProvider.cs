using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Mvc.ModelBinding.Metadata;
using Umbraco.Cms.Api.Management.Extensions;

namespace Umbraco.Cms.Web.Common.Mvc;

/// <summary>
/// An implementation of <see cref="IDisplayMetadataProvider"/> and <see cref="IValidationMetadataProvider"/> for
/// the System.Text.Json.Serialization attribute classes that only applies to the management api controllers/models
/// </summary>
public sealed class ManagementApiSystemTextJsonValidationMetadataProvider : IValidationMetadataProvider
{
    private readonly JsonNamingPolicy _jsonNamingPolicy;

    /// <summary>
    /// Initializes a new instance of the <see cref="ManagementApiSystemTextJsonValidationMetadataProvider"/> class With the default <see cref="JsonNamingPolicy.CamelCase"/>
    /// </summary>
    public ManagementApiSystemTextJsonValidationMetadataProvider()
        : this(JsonNamingPolicy.CamelCase)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="ManagementApiSystemTextJsonValidationMetadataProvider"/> class With the default <see cref="JsonNamingPolicy.CamelCase"/> with an optional <see cref="JsonNamingPolicy"/>
    /// </summary>
    /// <param name="namingPolicy">The <see cref="JsonNamingPolicy"/> to be used to configure the metadata provider.</param>
    public ManagementApiSystemTextJsonValidationMetadataProvider(JsonNamingPolicy namingPolicy)
    {
        ArgumentNullException.ThrowIfNull(namingPolicy);

        _jsonNamingPolicy = namingPolicy;
    }

    /// <inheritdoc />
    public void CreateValidationMetadata(ValidationMetadataProviderContext context)
    {
        ArgumentNullException.ThrowIfNull(context);
        if (

            // the model being processed is the top model so no container => check if where it was declared was inside a management api controller
            context.Key.ParameterInfo?.Member.DeclaringType?.IsManagementApiController() is not true

            // check whether the properties parent is inside the management api. We can do this since we use separate viewModels for all output.
            && context.Key.ContainerType?.IsManagementApiViewModel() is not true)
        {
            return;
        }

        var propertyName = ReadPropertyNameFrom(context.Attributes);

        if (string.IsNullOrEmpty(propertyName))
        {
            propertyName = context.Key.Name is { } contextKeyName
                ? _jsonNamingPolicy.ConvertName(contextKeyName)
                : null;
        }

        context.ValidationMetadata.ValidationModelName = propertyName;
    }

    private static string? ReadPropertyNameFrom(IReadOnlyList<object> attributes)
        => attributes?.OfType<JsonPropertyNameAttribute>().FirstOrDefault()?.Name;
}
