using Microsoft.AspNetCore.OpenApi;
using Microsoft.OpenApi;
using Umbraco.Cms.Api.Common.Configuration;
using Umbraco.Cms.Api.Delivery.Filters.OpenApi;

namespace Umbraco.Cms.Api.Delivery.Configuration;

/// <summary>
/// Configures the OpenAPI options for the Umbraco Delivery API.
/// </summary>
public class ConfigureUmbracoDeliveryApiSwaggerGenOptions : ConfigureUmbracoOpenApiOptionsBase
{
    /// <inheritdoc />
    protected override string ApiName => DeliveryApiConfiguration.ApiName;

    /// <inheritdoc />
    protected override void ConfigureOpenApi(OpenApiOptions options)
    {
        options.AddDocumentTransformer((document, _, _) =>
        {
            document.Info = new OpenApiInfo
            {
                Title = DeliveryApiConfiguration.ApiTitle,
                Version = "Latest",
                Description = $"You can find out more about the {DeliveryApiConfiguration.ApiTitle} in [the documentation]({DeliveryApiConfiguration.ApiDocumentationContentArticleLink}).",
            };
            return Task.CompletedTask;
        });

        // Add API key security scheme and configure it for all operations
        options
            .AddDocumentTransformer<ApiKeyTransformer>()
            .AddOperationTransformer<ApiKeyTransformer>();

        options.AddOperationTransformer<ContentApiTransformer>();
        options.AddOperationTransformer<MediaApiTransformer>();
        options.AddDocumentTransformer<MimeTypesTransformer>();
    }
}
