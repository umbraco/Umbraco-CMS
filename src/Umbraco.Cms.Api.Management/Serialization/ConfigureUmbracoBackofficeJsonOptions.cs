using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Umbraco.Cms.Api.Common.Serialization;
using Umbraco.Cms.Infrastructure.Serialization;

namespace Umbraco.Cms.Api.Management.Serialization;

/// <summary>
/// Provides configuration for JSON serialization options used by the Umbraco backoffice API.
/// </summary>
public class ConfigureUmbracoBackofficeJsonOptions : IConfigureNamedOptions<JsonOptions>
{
    private readonly IUmbracoJsonTypeInfoResolver _umbracoJsonTypeInfoResolver;

    /// <summary>
    /// Initializes a new instance of the <see cref="ConfigureUmbracoBackofficeJsonOptions"/> class with the specified JSON type info resolver for Umbraco.
    /// </summary>
    /// <param name="umbracoJsonTypeInfoResolver">The JSON type info resolver used to configure Umbraco backoffice JSON options.</param>
    public ConfigureUmbracoBackofficeJsonOptions(IUmbracoJsonTypeInfoResolver umbracoJsonTypeInfoResolver)
    {
        _umbracoJsonTypeInfoResolver = umbracoJsonTypeInfoResolver;
    }

    /// <summary>
    /// Configures the JSON options for the Umbraco backoffice if the specified name matches the backoffice options name.
    /// </summary>
    /// <param name="name">The name of the JSON options instance to configure. Configuration is applied only if this matches the backoffice options name.</param>
    /// <param name="options">The <see cref="JsonOptions"/> instance to be configured.</param>
    public void Configure(string? name, JsonOptions options)
    {
        if (name != Core.Constants.JsonOptionsNames.BackOffice)
        {
            return;
        }

        Configure(options);
    }

    /// <summary>
    /// Applies Umbraco backoffice-specific configuration to the provided JSON options.
    /// </summary>
    /// <param name="options">The <see cref="JsonOptions"/> instance to configure for the Umbraco backoffice.</param>
    public void Configure(JsonOptions options)
    {
        // all back-office specific JSON options go here
        options.JsonSerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
        options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
        options.JsonSerializerOptions.Converters.Add(new JsonUdiConverter());
        options.JsonSerializerOptions.Converters.Add(new JsonUdiRangeConverter());
        options.JsonSerializerOptions.Converters.Add(new ValidationProblemDetailsConverter());
        options.JsonSerializerOptions.Converters.Add(new JsonObjectConverter());

        options.JsonSerializerOptions.TypeInfoResolver = _umbracoJsonTypeInfoResolver;

        options.JsonSerializerOptions.MaxDepth = 64; // Ensures the maximum possible value is used, in particular to support handling as best we can levels of nested blocks.
    }
}
