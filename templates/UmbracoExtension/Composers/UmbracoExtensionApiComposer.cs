using Microsoft.Extensions.DependencyInjection;
using Microsoft.OpenApi;
using Umbraco.Cms.Api.Common.DependencyInjection;
using Umbraco.Cms.Api.Management.OpenApi;
using Umbraco.Cms.Core.Composing;
using Umbraco.Cms.Core.DependencyInjection;

namespace Umbraco.Extension.Composers;

public class UmbracoExtensionApiComposer : IComposer
{
    public void Compose(IUmbracoBuilder builder)
    {
        builder.Services.AddOpenApi(
            Constants.ApiName,
            options =>
            {
                // Related documentation:
                // https://docs.umbraco.com/umbraco-cms/tutorials/creating-a-backoffice-api
                // https://docs.umbraco.com/umbraco-cms/tutorials/creating-a-backoffice-api/adding-a-custom-openapi-document
                // https://docs.umbraco.com/umbraco-cms/tutorials/creating-a-backoffice-api/versioning-your-api
                // https://docs.umbraco.com/umbraco-cms/tutorials/creating-a-backoffice-api/access-policies

                // Configure the Swagger generation options
                // Add in a new Swagger API document solely for our own package that can be browsed via Swagger UI
                // Along with having a generated swagger JSON file that we can use to auto generate a TypeScript client
                options.AddDocumentTransformer((document, _, _) =>
                {
                    document.Info = new OpenApiInfo
                    {
                        Title = "Umbraco ExtensionBackoffice API",
                        Version = "1.0",
                        // Contact = new OpenApiContact
                        // {
                        //     Name = "Some Developer",
                        //     Email = "you@company.com",
                        //     Url = new Uri("https://company.com")
                        // }
                    };
                    return Task.CompletedTask;
                });

                // Enable Umbraco authentication for the "Example" Swagger document
                options.AddBackofficeSecurityRequirements();

                // This is used to generate nice operation IDs in our swagger json file
                // So that the generated TypeScript client has nice method names and not too verbose
                // https://docs.umbraco.com/umbraco-cms/tutorials/creating-a-backoffice-api/umbraco-schema-and-operation-ids#operation-ids
                options.AddOperationTransformer((operation, context, _) =>
                {
                    operation.OperationId = $"{context.Description.ActionDescriptor.RouteValues["action"]}";
                    return Task.CompletedTask;
                });
            });

        builder.Services.AddOpenApiDocumentToUi(Constants.ApiName, "Umbraco Extension Backoffice API");
    }
}
