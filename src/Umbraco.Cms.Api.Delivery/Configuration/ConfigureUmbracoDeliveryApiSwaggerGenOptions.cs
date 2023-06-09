using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;
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
                Description = $"You can find out more about the Content Delivery API on [our documentation platform]({DeliveryApiConfiguration.ApiDocumentationArticleLink})."
            });

        swaggerGenOptions.OperationFilter<AddCompleteSwaggerDocumentationFilter>();
        swaggerGenOptions.ParameterFilter<AddCompleteSwaggerDocumentationFilter>();
    }
}
