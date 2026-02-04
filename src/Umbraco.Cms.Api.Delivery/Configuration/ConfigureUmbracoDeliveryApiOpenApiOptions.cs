using Microsoft.AspNetCore.OpenApi;
using Umbraco.Cms.Api.Common.Configuration;
using Umbraco.Cms.Api.Delivery.OpenApi.Transformers;

namespace Umbraco.Cms.Api.Delivery.Configuration;

/// <summary>
/// Configures the OpenAPI options for the Umbraco Delivery API.
/// </summary>
public class ConfigureUmbracoDeliveryApiOpenApiOptions : ConfigureUmbracoOpenApiOptionsBase
{
    /// <inheritdoc />
    protected override string ApiName => DeliveryApiConfiguration.ApiName;

    /// <inheritdoc />
    protected override string ApiTitle => DeliveryApiConfiguration.ApiTitle;

    /// <inheritdoc />
    protected override string ApiVersion => "Latest";

    /// <inheritdoc />
    protected override string ApiDescription =>
        $"You can find out more about the {DeliveryApiConfiguration.ApiTitle} in [the documentation]({DeliveryApiConfiguration.ApiDocumentationContentArticleLink}).";

    /// <inheritdoc />
    protected override void ConfigureOpenApi(OpenApiOptions options)
    {
        base.ConfigureOpenApi(options);

        // Add API key security scheme and configure it for all operations
        options
            .AddDocumentTransformer<ApiKeyTransformer>()
            .AddOperationTransformer<ApiKeyTransformer>();

        options.AddOperationTransformer<ContentApiTransformer>();
        options.AddOperationTransformer<MediaApiTransformer>();
        options.AddDocumentTransformer<MimeTypesTransformer>();
        options
            .AddSchemaTransformer<ContentTypeSchemaTransformer>()
            .AddDocumentTransformer<ContentTypeSchemaTransformer>();
    }
}
