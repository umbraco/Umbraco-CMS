using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Microsoft.OpenApi;
using Swashbuckle.AspNetCore.SwaggerGen;
using Umbraco.Cms.Api.Common.Security;
using Umbraco.Cms.Api.Common.Serialization;
using Umbraco.Cms.Api.Management.DependencyInjection;
using Umbraco.Cms.Api.Management.OpenApi;

namespace Umbraco.Cms.Api.Management.Configuration;

/// <summary>
/// Provides configuration for Swagger generation options specific to the Umbraco Management API.
/// This class is used to customize the Swagger documentation for the API endpoints.
/// </summary>
public class ConfigureUmbracoManagementApiSwaggerGenOptions : IConfigureOptions<SwaggerGenOptions>
{
    private readonly IUmbracoJsonTypeInfoResolver _umbracoJsonTypeInfoResolver;

    /// <summary>
    /// Initializes a new instance of the <see cref="ConfigureUmbracoManagementApiSwaggerGenOptions"/> class.
    /// </summary>
    /// <param name="umbracoJsonTypeInfoResolver">An instance of <see cref="IUmbracoJsonTypeInfoResolver"/> used to resolve JSON type information for Umbraco.</param>
    public ConfigureUmbracoManagementApiSwaggerGenOptions(IUmbracoJsonTypeInfoResolver umbracoJsonTypeInfoResolver)
    {
        _umbracoJsonTypeInfoResolver = umbracoJsonTypeInfoResolver;
    }

    /// <summary>
    /// Configures the <see cref="SwaggerGenOptions"/> for the Umbraco Management API.
    /// Sets up the Swagger documentation, including API metadata, security definitions for OAuth2 authentication,
    /// operation filters for response headers and security requirements, and schema filters for non-nullable properties.
    /// Also configures polymorphism handling and discriminator properties for OpenAPI schemas.
    /// </summary>
    /// <param name="swaggerGenOptions">The <see cref="SwaggerGenOptions"/> instance to configure for the Management API.</param>
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
                             new Uri(Paths.BackOfficeApi.AuthorizationEndpoint, UriKind.Relative),
                         TokenUrl = new Uri(Paths.BackOfficeApi.TokenEndpoint, UriKind.Relative),
                     },
                 },
             });

        // Sets Security requirement on backoffice apis
        swaggerGenOptions.OperationFilter<BackOfficeSecurityRequirementsOperationFilter>();
        swaggerGenOptions.OperationFilter<NotificationHeaderFilter>();
        swaggerGenOptions.SchemaFilter<RequireNonNullablePropertiesSchemaFilter>();
    }
}
