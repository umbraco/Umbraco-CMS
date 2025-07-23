using Microsoft.AspNetCore.Http;
using NUnit.Framework;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Cache;
using Umbraco.Cms.Core.DeliveryApi;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.PublishedContent;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Services.Changes;
using Umbraco.Cms.Core.Web;
using Umbraco.Cms.Tests.Common.Builders;
using Umbraco.Cms.Tests.Common.Builders.Extensions;
using Umbraco.Cms.Tests.Common.Testing;
using Umbraco.Cms.Tests.Integration.Testing;

namespace Umbraco.Cms.Tests.Integration.Umbraco.Core.DeliveryApi.Request;

[TestFixture]
[UmbracoTest(Database = UmbracoTestOptions.Database.NewSchemaPerTest)]
public class ApiContentResponseBuilderTests : UmbracoIntegrationTest
{
    private IContentService ContentService => GetRequiredService<IContentService>();

    private IContentTypeService ContentTypeService => GetRequiredService<IContentTypeService>();

    private IApiContentResponseBuilder ApiContentResponseBuilder => GetRequiredService<IApiContentResponseBuilder>();

    protected IUmbracoContextAccessor UmbracoContextAccessor => GetRequiredService<IUmbracoContextAccessor>();

    protected IUmbracoContextFactory UmbracoContextFactory => GetRequiredService<IUmbracoContextFactory>();

    protected IVariationContextAccessor VariationContextAccessor => GetRequiredService<IVariationContextAccessor>();

    protected override void CustomTestSetup(IUmbracoBuilder builder)
    {
        builder.AddUmbracoHybridCache();
        builder.AddDeliveryApi();
    }

    [SetUp]
    public void SetUpTest()
    {
        var httpContextAccessor = GetRequiredService<IHttpContextAccessor>();

        httpContextAccessor.HttpContext = new DefaultHttpContext
        {
            Request =
            {
                Scheme = "https",
                Host = new HostString("localhost"),
                Path = "/",
                QueryString = new QueryString(string.Empty)
            },
            RequestServices = Services
        };
    }

    [Test]
    public async Task ContentBuilder_MapsContentDatesCorrectlyForCultureVariance()
    {
        await GetRequiredService<ILanguageService>().CreateAsync(new Language("da-DK", "Danish"), Constants.Security.SuperUserKey);

        var contentType = new ContentTypeBuilder()
            .WithAlias("theContentType")
            .WithContentVariation(ContentVariation.Culture)
            .Build();
        contentType.AllowedAsRoot = true;
        await ContentTypeService.CreateAsync(contentType, Constants.Security.SuperUserKey);

        var content = new ContentBuilder()
            .WithContentType(contentType)
            .WithCultureName("en-US", "Content EN")
            .WithCultureName("da-DK", "Content DA")
            .Build();
        ContentService.Save(content);
        ContentService.Publish(content, ["*"]);

        Thread.Sleep(200);
        content.SetCultureName("Content DA updated", "da-DK");
        ContentService.Save(content);
        ContentService.Publish(content, ["da-DK"]);

        RefreshContentCache();

        UmbracoContextAccessor.Clear();
        var umbracoContext = UmbracoContextFactory.EnsureUmbracoContext().UmbracoContext;
        var publishedContent = umbracoContext.Content.GetById(content.Key);
        Assert.IsNotNull(publishedContent);

        VariationContextAccessor.VariationContext = new VariationContext(culture: "en-US");
        var enResult = ApiContentResponseBuilder.Build(publishedContent);
        Assert.IsNotNull(enResult);

        VariationContextAccessor.VariationContext = new VariationContext(culture: "da-DK");
        var daResult = ApiContentResponseBuilder.Build(publishedContent);
        Assert.IsNotNull(daResult);

        Assert.GreaterOrEqual((daResult.UpdateDate - enResult.UpdateDate).TotalMilliseconds, 200);
    }

    private void RefreshContentCache()
        => GetRequiredService<ContentCacheRefresher>().Refresh([new ContentCacheRefresher.JsonPayload { ChangeTypes = TreeChangeTypes.RefreshAll }]);
}
