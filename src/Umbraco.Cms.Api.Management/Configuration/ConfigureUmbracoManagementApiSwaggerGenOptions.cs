using Microsoft.AspNetCore.OpenApi;
using Microsoft.Extensions.Options;
using Microsoft.OpenApi;
using Umbraco.Cms.Api.Common.Serialization;
using Umbraco.Cms.Api.Management.DependencyInjection;
using Umbraco.Extensions;

namespace Umbraco.Cms.Api.Management.Configuration;

public class ConfigureUmbracoManagementApiSwaggerGenOptions : IConfigureNamedOptions<OpenApiOptions>
{
    private IUmbracoJsonTypeInfoResolver _umbracoJsonTypeInfoResolver;

    public ConfigureUmbracoManagementApiSwaggerGenOptions(IUmbracoJsonTypeInfoResolver umbracoJsonTypeInfoResolver)
    {
        _umbracoJsonTypeInfoResolver = umbracoJsonTypeInfoResolver;
    }

    /// <inheritdoc />
    public void Configure(OpenApiOptions options)
    {
        // No default configuration
    }

    /// <inheritdoc />
    public void Configure(string? name, OpenApiOptions options)
    {
        if (name != ManagementApiConfiguration.ApiName)
        {
            return;
        }

        options.ConfigureUmbracoDefaultApiOptions(name);
        options.AddDocumentTransformer((document, context, cancellationToken) =>
        {
            document.Info = new OpenApiInfo
            {
                Title = ManagementApiConfiguration.ApiTitle,
                Version = "Latest",
                Description = "This shows all APIs available in this version of Umbraco - including all the legacy apis that are available for backward compatibility",
            };
            return Task.CompletedTask;
        });

        // swaggerGenOptions.OperationFilter<ResponseHeaderOperationFilter>();
        // swaggerGenOptions.UseOneOfForPolymorphism();
        //
        // // Ensure all types that implements the IOpenApiDiscriminator have a $type property in the OpenApi schema with the default value (The class name) that is expected by the server
        // swaggerGenOptions.SelectDiscriminatorNameUsing(type => _umbracoJsonTypeInfoResolver.GetTypeDiscriminatorValue(type) is not null ? "$type" : null);
        // swaggerGenOptions.SelectDiscriminatorValueUsing(_umbracoJsonTypeInfoResolver.GetTypeDiscriminatorValue);
        //
        //
        // swaggerGenOptions.AddSecurityDefinition(
        //     ManagementApiConfiguration.ApiSecurityName,
        //     new OpenApiSecurityScheme
        //      {
        //          In = ParameterLocation.Header,
        //          Name = "Umbraco",
        //          Type = SecuritySchemeType.OAuth2,
        //          Description = "Umbraco Authentication",
        //          Flows = new OpenApiOAuthFlows
        //          {
        //              AuthorizationCode = new OpenApiOAuthFlow
        //              {
        //                  AuthorizationUrl =
        //                      new Uri(Common.Security.Paths.BackOfficeApi.AuthorizationEndpoint, UriKind.Relative),
        //                  TokenUrl = new Uri(Common.Security.Paths.BackOfficeApi.TokenEndpoint, UriKind.Relative)
        //              }
        //          }
        //      });
        //
        // // Sets Security requirement on backoffice apis
        // swaggerGenOptions.OperationFilter<BackOfficeSecurityRequirementsOperationFilter>();
        // swaggerGenOptions.OperationFilter<NotificationHeaderFilter>();
        // swaggerGenOptions.SchemaFilter<RequireNonNullablePropertiesSchemaFilter>();
    }
}
