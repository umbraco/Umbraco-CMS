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

namespace Umbraco.Cms.Tests.Integration.Umbraco.Core.PublishedContent;

[TestFixture]
[UmbracoTest(Database = UmbracoTestOptions.Database.NewSchemaPerTest)]
public class PublishedContentFallbackTests : UmbracoIntegrationTest
{
    private IContentService ContentService => GetRequiredService<IContentService>();

    private IContentTypeService ContentTypeService => GetRequiredService<IContentTypeService>();

    private IUmbracoContextAccessor UmbracoContextAccessor => GetRequiredService<IUmbracoContextAccessor>();

    private IUmbracoContextFactory UmbracoContextFactory => GetRequiredService<IUmbracoContextFactory>();

    private IVariationContextAccessor VariationContextAccessor => GetRequiredService<IVariationContextAccessor>();

    private IPublishedValueFallback PublishedValueFallback => GetRequiredService<IPublishedValueFallback>();

    private ContentCacheRefresher ContentCacheRefresher => GetRequiredService<ContentCacheRefresher>();

    private IApiContentBuilder ApiContentBuilder => GetRequiredService<IApiContentBuilder>();

    protected override void CustomTestSetup(IUmbracoBuilder builder)
        => builder
            .AddUmbracoHybridCache()
            .AddDeliveryApi();

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

    [TestCase("Invariant title", "Segmented title", "Segmented title")]
    [TestCase(null, "Segmented title", "Segmented title")]
    [TestCase("Invariant title", null, "Invariant title")]
    [TestCase(null, null, null)]
    public async Task Property_Value_Performs_Fallback_To_Default_Segment_For_Templated_Rendering(string? invariantTitle, string? segmentedTitle, string? expectedResult)
    {
        var publishedContent = await SetupSegmentedContentAsync(invariantTitle, segmentedTitle);

        // NOTE: the TextStringValueConverter.ConvertIntermediateToObject() explicitly converts a null source value to an empty string

        var segmentedResult = publishedContent.Value<string>(PublishedValueFallback, "title", segment: "s1");
        Assert.AreEqual(expectedResult ?? string.Empty, segmentedResult);

        var invariantResult = publishedContent.Value<string>(PublishedValueFallback, "title", segment: string.Empty);
        Assert.AreEqual(invariantTitle ?? string.Empty, invariantResult);
    }

    [TestCase("Invariant title", "Segmented title", "Segmented title")]
    [TestCase(null, "Segmented title", "Segmented title")]
    [TestCase("Invariant title", null, "Invariant title")]
    [TestCase(null, null, null)]
    public async Task Property_Value_Performs_Fallback_To_Default_Segment_For_Delivery_Api_Output(string? invariantTitle, string? segmentedTitle, string? expectedResult)
    {
        UmbracoContextFactory.EnsureUmbracoContext();

        var publishedContent = await SetupSegmentedContentAsync(invariantTitle, segmentedTitle);

        VariationContextAccessor.VariationContext = new VariationContext(culture: null, segment: "s1");
        var apiContent = ApiContentBuilder.Build(publishedContent);
        Assert.IsNotNull(apiContent);
        Assert.IsTrue(apiContent.Properties.TryGetValue("title", out var segmentedValue));
        Assert.AreEqual(expectedResult, segmentedValue);

        VariationContextAccessor.VariationContext = new VariationContext(culture: null, segment: null);
        apiContent = ApiContentBuilder.Build(publishedContent);
        Assert.IsNotNull(apiContent);
        Assert.IsTrue(apiContent.Properties.TryGetValue("title", out var invariantValue));
        Assert.AreEqual(invariantTitle, invariantValue);
    }

    private async Task<IPublishedContent> SetupSegmentedContentAsync(string? invariantTitle, string? segmentedTitle)
    {
        var contentType = new ContentTypeBuilder()
            .WithAlias("theContentType")
            .WithContentVariation(ContentVariation.Segment)
            .AddPropertyType()
            .WithAlias("title")
            .WithName("Title")
            .WithDataTypeId(Constants.DataTypes.Textbox)
            .WithPropertyEditorAlias(Constants.PropertyEditors.Aliases.TextBox)
            .WithValueStorageType(ValueStorageType.Nvarchar)
            .WithVariations(ContentVariation.Segment)
            .Done()
            .WithAllowAsRoot(true)
            .Build();
        await ContentTypeService.CreateAsync(contentType, Constants.Security.SuperUserKey);

        var content = new ContentBuilder()
            .WithContentType(contentType)
            .WithName("Content")
            .Build();
        content.SetValue("title", invariantTitle);
        content.SetValue("title", segmentedTitle, segment: "s1");
        ContentService.Save(content);
        ContentService.Publish(content, ["*"]);

        ContentCacheRefresher.Refresh([new ContentCacheRefresher.JsonPayload { ChangeTypes = TreeChangeTypes.RefreshAll }]);

        UmbracoContextAccessor.Clear();
        var umbracoContext = UmbracoContextFactory.EnsureUmbracoContext().UmbracoContext;
        var publishedContent = umbracoContext.Content.GetById(content.Key);
        Assert.IsNotNull(publishedContent);

        return publishedContent;
    }
}
