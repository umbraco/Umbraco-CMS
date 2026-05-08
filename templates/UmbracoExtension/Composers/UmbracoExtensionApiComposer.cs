using Umbraco.Cms.Api.Common.OpenApi;
using Umbraco.Cms.Api.Management.OpenApi;
using Umbraco.Cms.Core.Composing;
using Umbraco.Cms.Core.DependencyInjection;

namespace Umbraco.Extension.Composers;

public class UmbracoExtensionApiComposer : IComposer
{
    public void Compose(IUmbracoBuilder builder)
        => builder.AddBackOfficeOpenApiDocument(
            Constants.ApiName,
            document => document
                .WithTitle("Umbraco Extension Backoffice API")
                .WithBackOfficeAuthentication()
                .ConfigureOpenApiOptions(options =>
                {
                    // Related documentation:
                    // https://docs.umbraco.com/umbraco-cms/tutorials/creating-a-backoffice-api
                    // https://docs.umbraco.com/umbraco-cms/tutorials/creating-a-backoffice-api/adding-a-custom-openapi-document
                    // https://docs.umbraco.com/umbraco-cms/tutorials/creating-a-backoffice-api/versioning-your-api
                    // https://docs.umbraco.com/umbraco-cms/tutorials/creating-a-backoffice-api/access-policies
                    options.AddDocumentTransformer((doc, _, _) =>
                    {
                        doc.Info.Version = "1.0";
                        // doc.Info.Contact = new OpenApiContact
                        // {
                        //     Name = "Some Developer",
                        //     Email = "you@company.com",
                        //     Url = new Uri("https://company.com")
                        // };
                        return Task.CompletedTask;
                    });

                    // This is used to generate operation IDs in our OpenAPI JSON file so that the generated
                    // TypeScript client has nice method names which are not too verbose.
                    // https://docs.umbraco.com/umbraco-cms/tutorials/creating-a-backoffice-api/umbraco-schema-and-operation-ids#operation-ids
                    options.AddOperationTransformer((operation, context, _) =>
                    {
                        operation.OperationId = $"{context.Description.ActionDescriptor.RouteValues["action"]}";
                        return Task.CompletedTask;
                    });
                }));
}
