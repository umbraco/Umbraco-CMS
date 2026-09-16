using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Microsoft.AspNetCore.OpenApi;
using Microsoft.Extensions.Options;
using NUnit.Framework;
using Umbraco.Cms.Search.Core;
using Umbraco.Cms.Search.Core.DependencyInjection;
using Umbraco.Cms.Tests.Common.Testing;
using Umbraco.Cms.Tests.Integration.Testing;

namespace Umbraco.Cms.Tests.Integration.Umbraco.Search.Core;

[TestFixture]
[UmbracoTest(Database = UmbracoTestOptions.Database.None)]
public class UmbracoBuilderExtensionsTests : UmbracoIntegrationTest
{
    protected override void CustomTestSetup(IUmbracoBuilder builder)
    {
        base.CustomTestSetup(builder);

        // Call AddSearchCore() twice to ensure it's idempotent.
        builder.AddSearchCore();
        builder.AddSearchCore();
    }

    [Test]
    public void Registers_Search_OpenApi_Document()
    {
        // AddSearchCore registers a dedicated OpenAPI document for the Search API. The builder configures
        // the named OpenApiOptions to only include endpoints decorated with [MapToApi(Constants.Api.Name)],
        // so an endpoint without that attribute must be excluded - proving the document was registered.
        OpenApiOptions openApiOptions = GetRequiredService<IOptionsMonitor<OpenApiOptions>>().Get(Constants.Api.Name);

        var unmappedEndpoint = new ApiDescription { ActionDescriptor = new ActionDescriptor() };
        Assert.That(openApiOptions.ShouldInclude(unmappedEndpoint), Is.False);
    }
}
