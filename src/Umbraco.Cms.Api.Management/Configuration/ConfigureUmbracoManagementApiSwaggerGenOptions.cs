using Microsoft.AspNetCore.OpenApi;
using Microsoft.OpenApi;
using Umbraco.Cms.Api.Common.Configuration;
using Umbraco.Cms.Api.Common.Serialization;
using Umbraco.Cms.Api.Management.DependencyInjection;
using Umbraco.Cms.Api.Management.OpenApi;

namespace Umbraco.Cms.Api.Management.Configuration;

public class ConfigureUmbracoManagementApiSwaggerGenOptions : ConfigureUmbracoOpenApiOptionsBase
{
    private IUmbracoJsonTypeInfoResolver _umbracoJsonTypeInfoResolver;

    public ConfigureUmbracoManagementApiSwaggerGenOptions(IUmbracoJsonTypeInfoResolver umbracoJsonTypeInfoResolver) =>
        _umbracoJsonTypeInfoResolver = umbracoJsonTypeInfoResolver;

    protected override string ApiName => ManagementApiConfiguration.ApiName;

    protected override void ConfigureOpenApi(OpenApiOptions options)
    {
        options.AddDocumentTransformer((document, _, _) =>
        {
            document.Info = new OpenApiInfo
            {
                Title = ManagementApiConfiguration.ApiTitle,
                Version = "Latest",
                Description =
                    "This shows all APIs available in this version of Umbraco - including all the legacy apis that are available for backward compatibility"
            };
            return Task.CompletedTask;
        });

        options.AddOperationTransformer<ResponseHeaderTransformer>();
        options.AddOperationTransformer<NotificationHeaderTransformer>();

        // Sets Security requirement on backoffice apis
        options
            .AddOperationTransformer<BackOfficeSecurityRequirementsTransformer>()
            .AddDocumentTransformer<BackOfficeSecurityRequirementsTransformer>();

        // swaggerGenOptions.UseOneOfForPolymorphism();
        //
        // // Ensure all types that implements the IOpenApiDiscriminator have a $type property in the OpenApi schema with the default value (The class name) that is expected by the server
        // swaggerGenOptions.SelectDiscriminatorNameUsing(type => _umbracoJsonTypeInfoResolver.GetTypeDiscriminatorValue(type) is not null ? "$type" : null);
        // swaggerGenOptions.SelectDiscriminatorValueUsing(_umbracoJsonTypeInfoResolver.GetTypeDiscriminatorValue);
    }
}
