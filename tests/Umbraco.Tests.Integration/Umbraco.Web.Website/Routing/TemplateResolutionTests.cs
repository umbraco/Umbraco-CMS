// Copyright (c) Umbraco.
// See LICENSE for more details.

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewEngines;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.Extensions.Options;
using Moq;
using NUnit.Framework;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Cache;
using Umbraco.Cms.Core.Configuration.Models;
using Umbraco.Cms.Core.DependencyInjection;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.PublishedContent;
using Umbraco.Cms.Core.Notifications;
using Umbraco.Cms.Core.Routing;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Sync;
using Umbraco.Cms.Core.Web;
using Umbraco.Cms.Tests.Common;
using Umbraco.Cms.Tests.Common.Builders;
using Umbraco.Cms.Tests.Common.Testing;
using Umbraco.Cms.Tests.Integration.Testing;
using Umbraco.Cms.Tests.Integration.Umbraco.Infrastructure.Scoping;
using Umbraco.Cms.Web.Common.Templates;
using Umbraco.Extensions;

namespace Umbraco.Cms.Tests.Integration.Umbraco.Web.Website.Routing;

/// <summary>
///     Verifies that front-end routing and rendering resolve a document's template from the template id held by the
///     published content.
/// </summary>
[TestFixture]
[UmbracoTest(Database = UmbracoTestOptions.Database.NewSchemaPerTest, WithApplication = true)]
internal sealed class TemplateResolutionTests : UmbracoIntegrationTest
{
    private readonly TestVariationContextAccessor _variationContextAccessor = new();

    private ITemplate _pageTemplate = null!;
    private ITemplate _alternativeTemplate = null!;
    private IContent _page = null!;

    private ITemplateService TemplateService => GetRequiredService<ITemplateService>();

    private IContentService ContentService => GetRequiredService<IContentService>();

    protected override void CustomTestSetup(IUmbracoBuilder builder)
    {
        builder.Services.AddUnique<IVariationContextAccessor>(_variationContextAccessor);
        builder.AddUmbracoHybridCache();

        // Ensure cache refreshers run, so the published cache reflects what is published.
        builder.Services.AddUnique<IServerMessenger, ScopedRepositoryTests.LocalServerMessenger>();
        builder.AddNotificationHandler<ContentTreeChangeNotification, ContentTreeChangeDistributedCacheNotificationHandler>();
    }

    // This SetUp must remain synchronous. EnsureUmbracoContext() writes to an AsyncLocal, and AsyncLocal mutations
    // made inside an awaited Task do not flow back to the test method's execution context.
    [SetUp]
    public void SetUp()
    {
        DeleteAllTemplateViewFiles();

        _pageTemplate = CreateTemplate("Routed Page", "routedPage");
        _alternativeTemplate = CreateTemplate("Alternative Page", "alternativePage");

        ContentType contentType = ContentTypeBuilder.CreateSimpleContentType("routedPageType", "Routed Page Type");
        contentType.AllowedTemplates = [_pageTemplate, _alternativeTemplate];
        contentType.SetDefaultTemplate(_pageTemplate);
        GetRequiredService<IContentTypeService>().CreateAsync(contentType, Constants.Security.SuperUserKey).GetAwaiter().GetResult();

        _page = ContentBuilder.CreateSimpleContent(contentType, "Routed Page");
        _page.TemplateId = _pageTemplate.Id;
        ContentService.SaveAsync(_page, Constants.Security.SuperUserKey, null, CancellationToken.None).GetAwaiter().GetResult();
        ContentService.PublishAsync(_page, ["*"], Constants.Security.SuperUserKey, CancellationToken.None).GetAwaiter().GetResult();

        GetRequiredService<IHttpContextAccessor>().HttpContext = new DefaultHttpContext
        {
            Request =
            {
                Scheme = "https",
                Host = new HostString("localhost"),
                Path = "/",
                QueryString = new QueryString(string.Empty),
            },
        };

        _variationContextAccessor.VariationContext = new VariationContext(
            GetRequiredService<ILanguageService>().GetDefaultIsoCodeAsync().GetAwaiter().GetResult());
        GetRequiredService<IUmbracoContextFactory>().EnsureUmbracoContext();
    }

    [TearDown]
    public void TearDownTemplateFiles() => DeleteAllTemplateViewFiles();

    [Test]
    public async Task Routing_Resolves_The_Template_Of_The_Published_Content()
    {
        IPublishedRequest request = await RouteAsync(string.Empty);

        Assert.That(request.PublishedContent?.Key, Is.EqualTo(_page.Key));
        Assert.That(request.Template, Is.Not.Null);
        Assert.That(request.Template!.Key, Is.EqualTo(_pageTemplate.Key));
    }

    [Test]
    public async Task Routing_Resolves_An_Alternative_Template_By_Alias()
    {
        IPublishedRequest request = await RouteAsync("?altTemplate=alternativePage");

        Assert.That(request.PublishedContent?.Key, Is.EqualTo(_page.Key));
        Assert.That(request.Template?.Key, Is.EqualTo(_alternativeTemplate.Key));
    }

    [Test]
    public async Task Renderer_Renders_The_Template_Of_The_Published_Content()
    {
        var renderedViews = new List<string>();
        TemplateRenderer renderer = CreateTemplateRenderer(renderedViews);

        var writer = new StringWriter();
        await renderer.RenderAsync(_page.Id, null, writer);

        Assert.That(renderedViews, Is.EqualTo(new[] { "~/Views/routedPage.cshtml" }));
        Assert.That(writer.ToString(), Is.EqualTo("rendered:~/Views/routedPage.cshtml"));
    }

    [Test]
    public async Task Renderer_Renders_An_Alternative_Template_By_Id()
    {
        var renderedViews = new List<string>();
        TemplateRenderer renderer = CreateTemplateRenderer(renderedViews);

        var writer = new StringWriter();
        await renderer.RenderAsync(_page.Id, _alternativeTemplate.Id, writer);

        Assert.That(renderedViews, Is.EqualTo(new[] { "~/Views/alternativePage.cshtml" }));
    }

    [Test]
    public async Task Renderer_Reports_An_Alternative_Template_That_Does_Not_Exist()
    {
        var renderedViews = new List<string>();
        TemplateRenderer renderer = CreateTemplateRenderer(renderedViews);

        var writer = new StringWriter();
        await renderer.RenderAsync(_page.Id, int.MaxValue, writer);

        Assert.That(renderedViews, Is.Empty);
        Assert.That(writer.ToString(), Does.Contain("the altTemplate was not found"));
    }

    private ITemplate CreateTemplate(string name, string alias)
    {
        var result = TemplateService
            .CreateAsync(name, alias, "@{ Layout = null; }", null, Constants.Security.SuperUserKey, CancellationToken.None)
            .GetAwaiter()
            .GetResult();
        Assert.That(result.Success, Is.True, $"Creating template {alias} failed with {result.Status}.");
        return result.Result;
    }

    private async Task<IPublishedRequest> RouteAsync(string queryString)
    {
        HttpContext httpContext = GetRequiredService<IHttpContextAccessor>().HttpContext!;
        httpContext.Request.QueryString = new QueryString(queryString);

        var router = GetRequiredService<IPublishedRouter>();
        IPublishedRequestBuilder builder = await router.CreateRequestAsync(new Uri("https://localhost/" + queryString));
        return await router.RouteRequestAsync(builder, new RouteRequestOptions(RouteDirection.Inbound));
    }

    /// <summary>
    ///     Creates a renderer whose view engine records the requested view and renders a marker, so the test asserts
    ///     which template was resolved without compiling Razor views.
    /// </summary>
    private TemplateRenderer CreateTemplateRenderer(List<string> renderedViews)
    {
        var viewEngine = new Mock<ICompositeViewEngine>();
        viewEngine
            .Setup(x => x.GetView(It.IsAny<string?>(), It.IsAny<string>(), It.IsAny<bool>()))
            .Returns((string? _, string viewPath, bool _) =>
            {
                renderedViews.Add(viewPath);
                var view = new Mock<IView>();
                view
                    .Setup(x => x.RenderAsync(It.IsAny<ViewContext>()))
                    .Returns((ViewContext context) => context.Writer.WriteAsync("rendered:" + viewPath));
                return ViewEngineResult.Found(viewPath, view.Object);
            });

        var tempDataFactory = new Mock<ITempDataDictionaryFactory>();
        tempDataFactory
            .Setup(x => x.GetTempData(It.IsAny<HttpContext>()))
            .Returns(Mock.Of<ITempDataDictionary>());

        return new TemplateRenderer(
            GetRequiredService<IUmbracoContextAccessor>(),
            GetRequiredService<IPublishedRouter>(),
            TemplateService,
            GetRequiredService<IOptionsMonitor<WebRoutingSettings>>(),
            GetRequiredService<IHttpContextAccessor>(),
            viewEngine.Object,
            new EmptyModelMetadataProvider(),
            tempDataFactory.Object,
            GetRequiredService<ILanguageService>(),
            IdKeyMap);
    }
}
