using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;
using Umbraco.Cms.Api.Common.Security;

namespace Umbraco.Cms.Api.Delivery.Configuration;

/// <summary>
/// This configures member authentication for the Delivery API in Swagger. Consult the docs for
/// member authentication within the Delivery API for instructions on how to use this.
/// </summary>
/// <remarks>
/// This class is not used by the core CMS due to the required installation dependencies (local login page among other things).
/// </remarks>
public class ConfigureUmbracoMemberAuthenticationDeliveryApiSwaggerGenOptions : IConfigureOptions<SwaggerGenOptions>
{
    public void Configure(SwaggerGenOptions options)
        => options.AddSecurityDefinition(
            "Umbraco Member",
            new OpenApiSecurityScheme
            {
                In = ParameterLocation.Header,
                Name = "Umbraco Member",
                Type = SecuritySchemeType.OAuth2,
                Description = "Umbraco Member Authentication",
                Flows = new OpenApiOAuthFlows
                {
                    AuthorizationCode = new OpenApiOAuthFlow
                    {
                        AuthorizationUrl = new Uri(Paths.MemberApi.AuthorizationEndpoint, UriKind.Relative),
                        TokenUrl = new Uri(Paths.MemberApi.TokenEndpoint, UriKind.Relative)
                    }
                }
            });
}
