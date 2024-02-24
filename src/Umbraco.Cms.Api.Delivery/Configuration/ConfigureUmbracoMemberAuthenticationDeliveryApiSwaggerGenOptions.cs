using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;
using Umbraco.Cms.Api.Common.Security;
using Umbraco.Cms.Api.Delivery.Controllers.Content;
using Umbraco.Cms.Api.Delivery.Filters;

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
    private const string AuthSchemeName = "Umbraco Member";

    public void Configure(SwaggerGenOptions options)
    {
        options.AddSecurityDefinition(
            AuthSchemeName,
            new OpenApiSecurityScheme
            {
                In = ParameterLocation.Header,
                Name = AuthSchemeName,
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

        // add security requirements for content API operations
        options.OperationFilter<DeliveryApiSecurityFilter>();
    }

    private class DeliveryApiSecurityFilter : SwaggerFilterBase<ContentApiControllerBase>, IOperationFilter
    {
        public void Apply(OpenApiOperation operation, OperationFilterContext context)
        {
            if (CanApply(context) is false)
            {
                return;
            }

            operation.Security = new List<OpenApiSecurityRequirement>
            {
                new OpenApiSecurityRequirement
                {
                    {
                        new OpenApiSecurityScheme
                        {
                            Reference = new OpenApiReference
                            {
                                Type = ReferenceType.SecurityScheme,
                                Id = AuthSchemeName,
                            }
                        },
                        new string[] { }
                    }
                }
            };
        }
    }
}
