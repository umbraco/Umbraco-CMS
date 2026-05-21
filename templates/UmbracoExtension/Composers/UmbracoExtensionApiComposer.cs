using Umbraco.Cms.Api.Common.OpenApi;
using Umbraco.Cms.Api.Management.OpenApi;
using Umbraco.Cms.Core.Composing;
using Umbraco.Cms.Core.DependencyInjection;

namespace Umbraco.Extension.Composers;

public class UmbracoExtensionApiComposer : IComposer
{
    public void Compose(IUmbracoBuilder builder) =>

        // See https://docs.umbraco.com/umbraco-cms/tutorials/creating-a-backoffice-api (and its sub-pages) for
        // guidance on customizing this document.
        builder.AddBackOfficeOpenApiDocument(
            Constants.ApiName,
            document => document
                .WithTitle("Umbraco Extension Backoffice API")
                .WithBackOfficeAuthentication()
                .ConfigureOpenApiOptions(options =>
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
                    })));
}
