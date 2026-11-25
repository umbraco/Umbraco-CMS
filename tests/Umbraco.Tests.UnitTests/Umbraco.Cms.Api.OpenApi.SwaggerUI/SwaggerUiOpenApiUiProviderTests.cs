using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Moq;
using NUnit.Framework;
using Swashbuckle.AspNetCore.SwaggerUI;
using Umbraco.Cms.Api.Common.DependencyInjection;
using Umbraco.Cms.Api.Common.OpenApi;
using Umbraco.Cms.Api.OpenApi.SwaggerUI;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.DependencyInjection;
using Umbraco.Extensions;

namespace Umbraco.Cms.Tests.UnitTests.Umbraco.Cms.Api.OpenApi.SwaggerUI;

[TestFixture]
public class SwaggerUiOpenApiUiProviderTests
{
    [Test]
    public void Swagger_UI_Lists_The_Ui_Documents_Sorted_By_Title_Under_The_Ui_Route_Prefix()
    {
        ServiceProvider provider = BuildServices(services =>
        {
            services.AddOpenApiDocumentToUi("zeta", "Zeta API");
            services.AddOpenApiDocumentToUi("alpha", "Alpha API");
        });

        SwaggerUIOptions options = provider.GetRequiredService<IOptions<SwaggerUIOptions>>().Value;

        Assert.That(options.RoutePrefix, Is.EqualTo("custom/openapi"));
        Assert.That(
            options.ConfigObject.Urls!.Select(x => (x.Name, x.Url)),
            Is.EqualTo(new[] { ("Alpha API", "/custom/openapi/alpha.json"), ("Zeta API", "/custom/openapi/zeta.json") }));
    }

    [Test]
    public void Swagger_UI_Authorizes_With_The_OpenApi_Ui_Client_Using_Pkce()
    {
        ServiceProvider provider = BuildServices();

        SwaggerUIOptions options = provider.GetRequiredService<IOptions<SwaggerUIOptions>>().Value;

        Assert.That(options.OAuthConfigObject.ClientId, Is.EqualTo(Constants.OAuthClientIds.OpenApiUi));
        Assert.That(options.OAuthConfigObject.UsePkceWithAuthorizationCodeGrant, Is.True);
    }

    [Test]
    public void OAuth_Redirect_Path_Is_Swagger_UI_Redirect_Page_Under_The_Ui_Route_Prefix()
    {
        ServiceProvider provider = BuildServices();

        IOpenApiUiProvider uiProvider = provider.GetRequiredService<IOpenApiUiProvider>();

        Assert.That(uiProvider, Is.InstanceOf<SwaggerUiOpenApiUiProvider>());
        Assert.That(uiProvider.GetOAuthRedirectPaths(), Is.EqualTo(new[] { "/custom/openapi/oauth2-redirect.html" }));
    }

    private static ServiceProvider BuildServices(Action<IServiceCollection>? configure = null)
    {
        var services = new ServiceCollection();
        services.Configure<UmbracoOpenApiOptions>(options =>
        {
            options.RouteTemplate = "custom/openapi/{documentName}.json";
            options.UiRoutePrefix = "custom/openapi";
        });
        Mock.Of<IUmbracoBuilder>(b => b.Services == services).AddSwaggerUiOpenApiUi();
        configure?.Invoke(services);
        return services.BuildServiceProvider();
    }
}
