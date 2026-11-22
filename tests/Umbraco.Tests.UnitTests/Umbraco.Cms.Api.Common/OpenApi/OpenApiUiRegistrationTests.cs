using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Moq;
using NUnit.Framework;
using Umbraco.Cms.Api.Common.DependencyInjection;
using Umbraco.Cms.Api.Common.OpenApi;
using Umbraco.Cms.Core.DependencyInjection;

namespace Umbraco.Cms.Tests.UnitTests.Umbraco.Cms.Api.Common.OpenApi;

[TestFixture]
public class OpenApiUiRegistrationTests
{
    [Test]
    public void AddOpenApiDocumentToUi_Resolves_Document_Url_From_Route_Template()
    {
        var services = CreateServices();
        services.AddOpenApiDocumentToUi("my-api", "My API");

        UmbracoOpenApiUiOptions uiOptions = Resolve(services);

        Assert.That(
            uiOptions.Documents,
            Is.EqualTo(new[] { new OpenApiUiDocument("my-api", "My API", "/custom/openapi/my-api.json") }));
    }

    [Test]
    public void AddOpenApiDocumentToUi_Same_Document_Twice_Lists_It_Once_With_The_Last_Title()
    {
        var services = CreateServices();
        services.AddOpenApiDocumentToUi("my-api", "First");
        services.AddOpenApiDocumentToUi("other-api");
        services.AddOpenApiDocumentToUi("my-api", "Second");

        UmbracoOpenApiUiOptions uiOptions = Resolve(services);

        Assert.That(uiOptions.Documents.Select(x => (x.Name, x.Title)), Is.EqualTo(new[] { ("my-api", "Second"), ("other-api", "other-api") }));
    }

    [TestCase(true)]
    [TestCase(false)]
    public void AddOpenApiUi_Takes_Precedence_Over_TryAddOpenApiUi_In_Either_Order(bool tryAddFirst)
    {
        var services = new ServiceCollection();
        IUmbracoBuilder builder = Mock.Of<IUmbracoBuilder>(b => b.Services == services);

        if (tryAddFirst)
        {
            builder.TryAddOpenApiUi<DefaultUiProvider>();
            builder.AddOpenApiUi<ChosenUiProvider>();
        }
        else
        {
            builder.AddOpenApiUi<ChosenUiProvider>();
            builder.TryAddOpenApiUi<DefaultUiProvider>();
        }

        IOpenApiUiProvider[] providers = services.BuildServiceProvider().GetServices<IOpenApiUiProvider>().ToArray();

        Assert.That(providers, Has.Length.EqualTo(1));
        Assert.That(providers[0], Is.InstanceOf<ChosenUiProvider>());
    }

    [Test]
    public void AddOpenApiUi_Replaces_A_Provider_Added_Before()
    {
        var services = new ServiceCollection();
        IUmbracoBuilder builder = Mock.Of<IUmbracoBuilder>(b => b.Services == services);

        builder.AddOpenApiUi<DefaultUiProvider>();
        builder.AddOpenApiUi<ChosenUiProvider>();

        Assert.That(services.BuildServiceProvider().GetServices<IOpenApiUiProvider>().Single(), Is.InstanceOf<ChosenUiProvider>());
    }

    private static ServiceCollection CreateServices()
    {
        var services = new ServiceCollection();
        services.Configure<UmbracoOpenApiOptions>(options => options.RouteTemplate = "custom/openapi/{documentName}.json");
        return services;
    }

    private static UmbracoOpenApiUiOptions Resolve(IServiceCollection services)
        => services.BuildServiceProvider().GetRequiredService<IOptions<UmbracoOpenApiUiOptions>>().Value;

    private sealed class DefaultUiProvider : IOpenApiUiProvider
    {
        public IEnumerable<string> GetOAuthRedirectPaths() => [];
    }

    private sealed class ChosenUiProvider : IOpenApiUiProvider
    {
        public IEnumerable<string> GetOAuthRedirectPaths() => [];
    }
}
