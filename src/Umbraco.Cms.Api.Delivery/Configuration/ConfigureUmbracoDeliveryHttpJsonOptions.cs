using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Http.Json;
using Microsoft.Extensions.Options;
using Umbraco.Cms.Core;

namespace Umbraco.Cms.Api.Delivery.Configuration;

/// <summary>
/// Configures the Http JSON options for the Umbraco Delivery API.
/// </summary>
internal class ConfigureUmbracoDeliveryHttpJsonOptions : IConfigureNamedOptions<JsonOptions>
{
    private readonly IOptionsMonitor<Microsoft.AspNetCore.Mvc.JsonOptions> _mvcJsonOptions;

    /// <summary>
    /// Initializes a new instance of the <see cref="ConfigureUmbracoDeliveryHttpJsonOptions"/> class.
    /// </summary>
    /// <param name="mvcJsonOptions">The configured MVC json options.</param>
    public ConfigureUmbracoDeliveryHttpJsonOptions(IOptionsMonitor<Microsoft.AspNetCore.Mvc.JsonOptions> mvcJsonOptions)
        => _mvcJsonOptions = mvcJsonOptions;

    /// <inheritdoc />
    public void Configure(JsonOptions options) => Configure(Options.DefaultName, options);

    /// <inheritdoc />
    public void Configure(string? name, JsonOptions options)
    {
        if (name != Constants.JsonOptionsNames.DeliveryApi)
        {
            return;
        }

        // Copy all converters from the Delivery API MVC JSON options
        Microsoft.AspNetCore.Mvc.JsonOptions backofficeMvcJsonOptions = _mvcJsonOptions.Get(Constants.JsonOptionsNames.DeliveryApi);
        foreach (JsonConverter jsonConverter in backofficeMvcJsonOptions.JsonSerializerOptions.Converters)
        {
            options.SerializerOptions.Converters.Add(jsonConverter);
        }

        options.SerializerOptions.PropertyNamingPolicy = backofficeMvcJsonOptions.JsonSerializerOptions.PropertyNamingPolicy;
        options.SerializerOptions.TypeInfoResolver = backofficeMvcJsonOptions.JsonSerializerOptions.TypeInfoResolver;
        options.SerializerOptions.MaxDepth = backofficeMvcJsonOptions.JsonSerializerOptions.MaxDepth;

        // Open API specific settings
        options.SerializerOptions.NumberHandling = JsonNumberHandling.Strict;
    }
}
