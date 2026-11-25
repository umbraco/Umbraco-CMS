using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Moq;
using NUnit.Framework;
using Scalar.AspNetCore;
using Umbraco.Cms.Api.Common.OpenApi;
using Umbraco.Cms.Api.OpenApi.Scalar;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.DependencyInjection;
using Umbraco.Extensions;

namespace Umbraco.Cms.Tests.UnitTests.Umbraco.Cms.Api.OpenApi.Scalar;

[TestFixture]
public class ScalarOpenApiUiProviderTests
{
    private const string SecurityScheme = "Test-Scheme";

    [Test]
    public void Scalar_Authorizes_The_Listed_Security_Schemes_With_The_OpenApi_Ui_Client_Using_Pkce()
    {
        ServiceProvider provider = BuildServices();

        ScalarOptions options = provider.GetRequiredService<IOptions<ScalarOptions>>().Value;

        var scheme = options.Authentication!.SecuritySchemes![SecurityScheme] as ScalarOAuth2SecurityScheme;
        Assert.That(scheme, Is.Not.Null);
        Assert.That(scheme!.Flows!.AuthorizationCode!.ClientId, Is.EqualTo(Constants.OAuthClientIds.OpenApiUi));
        Assert.That(scheme.Flows.AuthorizationCode.Pkce, Is.EqualTo(Pkce.Sha256));
    }

    [Test]
    public void Scalar_Resolves_Documents_From_The_Umbraco_Route_Template()
    {
        ServiceProvider provider = BuildServices();

        ScalarOptions options = provider.GetRequiredService<IOptions<ScalarOptions>>().Value;

        Assert.That(options.OpenApiRoutePattern, Is.EqualTo("custom/openapi/{documentName}.json"));
    }

    [Test]
    public void Scalar_Does_Not_Send_Data_To_Hosted_Services_By_Default()
    {
        ServiceProvider provider = BuildServices();

        ScalarOptions options = provider.GetRequiredService<IOptions<ScalarOptions>>().Value;

        Assert.That(options.Telemetry, Is.False);
        Assert.That(options.DefaultFonts, Is.False);
    }

    [Test]
    public void OAuth_Redirect_Path_Is_The_Root_Of_The_Ui_Route_Prefix()
    {
        ServiceProvider provider = BuildServices();

        IOpenApiUiProvider uiProvider = provider.GetRequiredService<IOpenApiUiProvider>();

        Assert.That(uiProvider, Is.InstanceOf<ScalarOpenApiUiProvider>());
        Assert.That(uiProvider.GetOAuthRedirectPaths(), Is.EqualTo(new[] { "/custom/openapi/" }));
    }

    private static ServiceProvider BuildServices()
    {
        var services = new ServiceCollection();
        services.Configure<UmbracoOpenApiOptions>(options =>
        {
            options.RouteTemplate = "custom/openapi/{documentName}.json";
            options.UiRoutePrefix = "custom/openapi";
        });
        services.Configure<UmbracoOpenApiUiOptions>(options => options.OAuthSecuritySchemes.Add(SecurityScheme));
        Mock.Of<IUmbracoBuilder>(b => b.Services == services).AddScalarOpenApiUi();
        return services.BuildServiceProvider();
    }
}
