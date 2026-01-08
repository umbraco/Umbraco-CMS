using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Umbraco.Cms.Api.Common.Serialization;
using Umbraco.Cms.Infrastructure.Serialization;

namespace Umbraco.Cms.Api.Management.Serialization;

public class ConfigureUmbracoBackofficeJsonOptions : IConfigureNamedOptions<JsonOptions>
{
    private readonly IUmbracoJsonTypeInfoResolver _umbracoJsonTypeInfoResolver;

    public ConfigureUmbracoBackofficeJsonOptions(IUmbracoJsonTypeInfoResolver umbracoJsonTypeInfoResolver)
    {
        _umbracoJsonTypeInfoResolver = umbracoJsonTypeInfoResolver;
    }

    public void Configure(string? name, JsonOptions options)
    {
        if (name != Core.Constants.JsonOptionsNames.BackOffice)
        {
            return;
        }

        Configure(options);
    }

    public void Configure(JsonOptions options)
    {
        // all back-office specific JSON options go here
        options.JsonSerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
        options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
        options.JsonSerializerOptions.Converters.Add(new JsonUdiConverter());
        options.JsonSerializerOptions.Converters.Add(new JsonUdiRangeConverter());
        options.JsonSerializerOptions.Converters.Add(new JsonObjectConverter());

        options.JsonSerializerOptions.TypeInfoResolver = _umbracoJsonTypeInfoResolver;

        options.JsonSerializerOptions.MaxDepth = 64; // Ensures the maximum possible value is used, in particular to support handling as best we can levels of nested blocks.
    }
}
