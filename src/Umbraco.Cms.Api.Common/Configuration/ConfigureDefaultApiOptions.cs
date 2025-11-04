using Microsoft.AspNetCore.OpenApi;
using Microsoft.OpenApi;

namespace Umbraco.Cms.Api.Common.Configuration;

/// <summary>
/// Configures the OpenAPI options for the Default API.
/// </summary>
public class ConfigureDefaultApiOptions : ConfigureUmbracoOpenApiOptionsBase
{
    protected override string ApiName => DefaultApiConfiguration.ApiName;

    protected override void ConfigureOpenApi(OpenApiOptions options)
    {
        options.AddDocumentTransformer((document, _, _) =>
        {
            document.Info = new OpenApiInfo
            {
                Title = "Default API",
                Version = "Latest",
                Description = "All endpoints not defined under specific APIs",
            };
            return Task.CompletedTask;
        });
    }
}
