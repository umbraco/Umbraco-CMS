using Microsoft.AspNetCore.OpenApi;
using Microsoft.Extensions.Options;
using Microsoft.OpenApi;
using Umbraco.Extensions;

namespace Umbraco.Cms.Api.Delivery.Configuration;

public class ConfigureUmbracoDeliveryApiSwaggerGenOptions : IConfigureNamedOptions<OpenApiOptions>
{
    /// <inheritdoc />
    public void Configure(OpenApiOptions options)
    {
        // No default configuration
    }

    /// <inheritdoc />
    public void Configure(string? name, OpenApiOptions options)
    {
        if (name != DeliveryApiConfiguration.ApiName)
        {
            return;
        }

        options.ConfigureUmbracoDefaultApiOptions(name);
        options.AddDocumentTransformer((document, context, cancellationToken) =>
        {
            document.Info = new OpenApiInfo
            {
                Title = DeliveryApiConfiguration.ApiTitle,
                Version = "Latest",
                Description = $"You can find out more about the {DeliveryApiConfiguration.ApiTitle} in [the documentation]({DeliveryApiConfiguration.ApiDocumentationContentArticleLink}).",
            };
            return Task.CompletedTask;
        });

        // swaggerGenOptions.DocumentFilter<MimeTypeDocumentFilter>(DeliveryApiConfiguration.ApiName);
        // swaggerGenOptions.DocumentFilter<RemoveSecuritySchemesDocumentFilter>(DeliveryApiConfiguration.ApiName);
        //
        // swaggerGenOptions.OperationFilter<SwaggerContentDocumentationFilter>();
        // swaggerGenOptions.OperationFilter<SwaggerMediaDocumentationFilter>();
        // swaggerGenOptions.ParameterFilter<SwaggerContentDocumentationFilter>();
        // swaggerGenOptions.ParameterFilter<SwaggerMediaDocumentationFilter>();
    }
}
