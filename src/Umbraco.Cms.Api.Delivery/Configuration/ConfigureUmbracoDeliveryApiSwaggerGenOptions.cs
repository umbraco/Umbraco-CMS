using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;
using Umbraco.Cms.Api.Common.OpenApi;
using Umbraco.Cms.Api.Delivery.Filters;

namespace Umbraco.Cms.Api.Delivery.Configuration;

public class ConfigureUmbracoDeliveryApiSwaggerGenOptions: IConfigureOptions<SwaggerGenOptions>
{
    public void Configure(SwaggerGenOptions swaggerGenOptions)
    {
        swaggerGenOptions.SwaggerDoc(
            DeliveryApiConfiguration.ApiName,
            new OpenApiInfo
            {
                Title = DeliveryApiConfiguration.ApiTitle,
                Version = "Latest",
                Description = $"You can find out more about the {DeliveryApiConfiguration.ApiTitle} in [the documentation]({DeliveryApiConfiguration.ApiDocumentationContentArticleLink})."
            });

        swaggerGenOptions.DocumentFilter<MimeTypeDocumentFilter>(DeliveryApiConfiguration.ApiName);

        swaggerGenOptions.OperationFilter<SwaggerContentDocumentationFilter>();
        swaggerGenOptions.OperationFilter<SwaggerMediaDocumentationFilter>();
        swaggerGenOptions.ParameterFilter<SwaggerContentDocumentationFilter>();
        swaggerGenOptions.ParameterFilter<SwaggerMediaDocumentationFilter>();
    }
}
