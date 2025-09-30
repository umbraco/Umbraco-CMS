using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;
using Umbraco.Cms.Api.Common.OpenApi;
using Umbraco.Cms.Api.Common.Serialization;
using Umbraco.Cms.Api.Management.DependencyInjection;
using Umbraco.Cms.Api.Management.OpenApi;

namespace Umbraco.Cms.Api.Management.Configuration;

public class ConfigureUmbracoManagementApiSwaggerGenOptions : IConfigureOptions<SwaggerGenOptions>
{
    private IUmbracoJsonTypeInfoResolver _umbracoJsonTypeInfoResolver;

    public ConfigureUmbracoManagementApiSwaggerGenOptions(IUmbracoJsonTypeInfoResolver umbracoJsonTypeInfoResolver)
    {
        _umbracoJsonTypeInfoResolver = umbracoJsonTypeInfoResolver;
    }

    public void Configure(SwaggerGenOptions swaggerGenOptions)
    {

        swaggerGenOptions.SwaggerDoc(
            ManagementApiConfiguration.ApiName,
            new OpenApiInfo
            {
                Title = ManagementApiConfiguration.ApiTitle,
                Version = "Latest",
                Description = "This shows all APIs available in this version of Umbraco - including all the legacy apis that are available for backward compatibility",
            });

        swaggerGenOptions.OperationFilter<ResponseHeaderOperationFilter>();
        swaggerGenOptions.UseOneOfForPolymorphism();

        // Ensure all types that implements the IOpenApiDiscriminator have a $type property in the OpenApi schema with the default value (The class name) that is expected by the server
        swaggerGenOptions.SelectDiscriminatorNameUsing(type => _umbracoJsonTypeInfoResolver.GetTypeDiscriminatorValue(type) is not null ? "$type" : null);
        swaggerGenOptions.SelectDiscriminatorValueUsing(_umbracoJsonTypeInfoResolver.GetTypeDiscriminatorValue);


        swaggerGenOptions.AddSecurityDefinition(
            ManagementApiConfiguration.ApiSecurityName,
            new OpenApiSecurityScheme
             {
                 In = ParameterLocation.Header,
                 Name = "Umbraco",
                 Type = SecuritySchemeType.OAuth2,
                 Description = "Umbraco Authentication",
                 Flows = new OpenApiOAuthFlows
                 {
                     AuthorizationCode = new OpenApiOAuthFlow
                     {
                         AuthorizationUrl =
                             new Uri(Common.Security.Paths.BackOfficeApi.AuthorizationEndpoint, UriKind.Relative),
                         TokenUrl = new Uri(Common.Security.Paths.BackOfficeApi.TokenEndpoint, UriKind.Relative)
                     }
                 }
             });

        // Sets Security requirement on backoffice apis
        swaggerGenOptions.OperationFilter<BackOfficeSecurityRequirementsOperationFilter>();
        swaggerGenOptions.OperationFilter<NotificationHeaderFilter>();
        swaggerGenOptions.SchemaFilter<RequireNonNullablePropertiesSchemaFilter>();
    }
}
