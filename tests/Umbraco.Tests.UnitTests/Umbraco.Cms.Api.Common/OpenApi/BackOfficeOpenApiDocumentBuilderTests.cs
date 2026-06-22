using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Microsoft.AspNetCore.OpenApi;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Moq;
using NUnit.Framework;
using Swashbuckle.AspNetCore.SwaggerUI;
using Umbraco.Cms.Api.Common.Attributes;
using Umbraco.Cms.Api.Common.OpenApi;
using Umbraco.Cms.Core.DependencyInjection;

namespace Umbraco.Cms.Tests.UnitTests.Umbraco.Cms.Api.Common.OpenApi;

[TestFixture]
public class BackOfficeOpenApiDocumentBuilderTests
{
    private const string DocumentName = "test-doc";

    [Test]
    public void Defaults_Set_CreateSchemaReferenceId_To_UmbracoSchemaIdGenerator()
    {
        OpenApiOptions options = BuildAndResolveOptions();

        Assert.AreEqual(
            (Func<System.Text.Json.Serialization.Metadata.JsonTypeInfo, string?>)UmbracoSchemaIdGenerator
                .CreateSchemaReferenceId,
            options.CreateSchemaReferenceId);
    }

    [Test]
    public void Defaults_ShouldInclude_Returns_True_For_Endpoint_With_Matching_MapToApi()
    {
        OpenApiOptions options = BuildAndResolveOptions();
        ApiDescription description = CreateApiDescription(new MapToApiAttribute(DocumentName));

        Assert.IsTrue(options.ShouldInclude!(description));
    }

    [Test]
    public void Defaults_ShouldInclude_Returns_False_For_Endpoint_With_Different_MapToApi()
    {
        OpenApiOptions options = BuildAndResolveOptions();
        ApiDescription description = CreateApiDescription(new MapToApiAttribute("other-doc"));

        Assert.IsFalse(options.ShouldInclude!(description));
    }

    [Test]
    public void Defaults_ShouldInclude_Returns_False_For_Endpoint_Without_MapToApi()
    {
        OpenApiOptions options = BuildAndResolveOptions();
        ApiDescription description = CreateApiDescription();

        Assert.IsFalse(options.ShouldInclude!(description));
    }

    [Test]
    public void ConfigureOpenApiOptions_Composes_Multiple_Callbacks_In_Order()
    {
        var calls = new List<int>();

        BuildAndResolveOptions(b => b
            .ConfigureOpenApiOptions(_ => calls.Add(1))
            .ConfigureOpenApiOptions(_ => calls.Add(2))
            .ConfigureOpenApiOptions(_ => calls.Add(3)));

        Assert.That(calls, Is.EqualTo(new[] { 1, 2, 3 }));
    }

    [Test]
    public void ConfigureOpenApiOptions_Runs_After_Defaults_So_Callbacks_Can_Override()
    {
        Func<System.Text.Json.Serialization.Metadata.JsonTypeInfo, string?> customGenerator = _ => "custom";

        OpenApiOptions options = BuildAndResolveOptions(b => b
            .ConfigureOpenApiOptions(o => o.CreateSchemaReferenceId = customGenerator));

        Assert.AreEqual(customGenerator, options.CreateSchemaReferenceId);
    }

    [Test]
    public void WithTitle_Without_WithUiTitle_Uses_Title_As_Swagger_Dropdown_Label()
    {
        SwaggerUIOptions swaggerOptions = BuildAndResolveSwaggerOptions(b => b.WithTitle("My API"));

        Assert.IsTrue(swaggerOptions.ConfigObject.Urls!.Any(url => url.Name == "My API"));
    }

    [Test]
    public void WithUiTitle_Overrides_Swagger_Dropdown_Label_Only()
    {
        SwaggerUIOptions swaggerOptions = BuildAndResolveSwaggerOptions(b => b
            .WithTitle("Info Title")
            .WithUiTitle("Short Label"));

        Assert.IsTrue(swaggerOptions.ConfigObject.Urls!.Any(url => url.Name == "Short Label"));
        Assert.IsFalse(swaggerOptions.ConfigObject.Urls!.Any(url => url.Name == "Info Title"));
    }

    [Test]
    public void Default_Without_Title_Uses_DocumentName_As_Swagger_Dropdown_Label()
    {
        SwaggerUIOptions swaggerOptions = BuildAndResolveSwaggerOptions();

        Assert.IsTrue(swaggerOptions.ConfigObject.Urls!.Any(url => url.Name == DocumentName));
    }

    [Test]
    public void ExcludeFromUi_Skips_Swagger_Dropdown_Registration()
    {
        SwaggerUIOptions swaggerOptions = BuildAndResolveSwaggerOptions(b => b
            .WithTitle("My API")
            .ExcludeFromUi());

        IEnumerable<UrlDescriptor>? urls = swaggerOptions.ConfigObject.Urls;
        Assert.IsTrue(urls is null || urls.All(url => url.Name != "My API" && url.Name != DocumentName));
    }

    private static OpenApiOptions BuildAndResolveOptions(Action<BackOfficeOpenApiDocumentBuilder>? configure = null)
    {
        ServiceCollection services = Build(configure);
        ServiceProvider provider = services.BuildServiceProvider();
        return provider.GetRequiredService<IOptionsMonitor<OpenApiOptions>>().Get(DocumentName);
    }

    private static SwaggerUIOptions BuildAndResolveSwaggerOptions(Action<BackOfficeOpenApiDocumentBuilder>? configure = null)
    {
        ServiceCollection services = Build(configure);
        ServiceProvider provider = services.BuildServiceProvider();
        return provider.GetRequiredService<IOptionsMonitor<SwaggerUIOptions>>().CurrentValue;
    }

    private static ServiceCollection Build(Action<BackOfficeOpenApiDocumentBuilder>? configure = null)
    {
        var services = new ServiceCollection();
        IUmbracoBuilder umbracoBuilder = Mock.Of<IUmbracoBuilder>(b => b.Services == services);
        var documentBuilder = new BackOfficeOpenApiDocumentBuilder(DocumentName);
        configure?.Invoke(documentBuilder);
        documentBuilder.Build(umbracoBuilder);

        return services;
    }

    private static ApiDescription CreateApiDescription(params object[] endpointMetadata) =>
        new()
        {
            ActionDescriptor = new ActionDescriptor
            {
                EndpointMetadata = endpointMetadata.ToList(),
            },
        };
}
