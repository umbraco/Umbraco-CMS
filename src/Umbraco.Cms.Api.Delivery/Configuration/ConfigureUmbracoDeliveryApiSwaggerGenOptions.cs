using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

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
            });
    }
}
