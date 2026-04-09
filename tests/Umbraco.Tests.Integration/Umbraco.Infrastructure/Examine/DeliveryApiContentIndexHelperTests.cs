using Microsoft.Extensions.Options;
using Moq;
using NUnit.Framework;
using Umbraco.Cms.Core.Configuration.Models;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Infrastructure.Examine;
using Umbraco.Cms.Infrastructure.Persistence;
using Umbraco.Cms.Tests.Common.Builders;
using Umbraco.Cms.Tests.Common.Testing;
using Umbraco.Cms.Tests.Integration.Testing;

namespace Umbraco.Cms.Tests.Integration.Umbraco.Infrastructure.Examine;

[UmbracoTest(Database = UmbracoTestOptions.Database.NewSchemaPerTest)]
[TestFixture]
public class DeliveryApiContentIndexHelperTests : UmbracoIntegrationTestWithContent
{
    public override void CreateTestData()
    {
        base.CreateTestData();

        // Save an extra, published content item of a different type to those created via the base class,
        // that we'll use to test filtering out disallowed content types.
        var template = TemplateBuilder.CreateTextPageTemplate("textPage2");
        FileService.SaveTemplate(template);

        var contentType = ContentTypeBuilder.CreateSimpleContentType("umbTextpage2", "Textpage2", defaultTemplateId: template.Id);
        contentType.Key = Guid.NewGuid();
        ContentTypeService.Save(contentType);

        ContentType.AllowedContentTypes =
        [
            new ContentTypeSort(ContentType.Key, 0, "umbTextpage"),
            new ContentTypeSort(contentType.Key, 1, "umbTextpage2"),
        ];
        ContentTypeService.Save(ContentType);

        var subpage = ContentBuilder.CreateSimpleContent(contentType, "Alternate Text Page 4", Textpage.Id);
        ContentService.Save(subpage);

        // And then add some more of the first type, so the one we'll filter out in tests isn't in the last page.
        for (int i = 0; i < 5; i++)
        {
            subpage = ContentBuilder.CreateSimpleContent(ContentType, $"Text Page {5 + i}", Textpage.Id);
            ContentService.Save(subpage);
        }
    }

    [Test]
    public void Can_Enumerate_Descendants_For_Content_Index()
    {
        var sut = CreateDeliveryApiContentIndexHelper();

        var expectedNumberOfContentItems = GetExpectedNumberOfContentItems();

        var contentEnumerated = 0;
        Action<IContent[]> actionToPerform = content =>
        {
            contentEnumerated += content.Length;
        };

        const int pageSize = 3;
        sut.EnumerateApplicableDescendantsForContentIndex(
            Cms.Core.Constants.System.Root,
            actionToPerform,
            pageSize);

        Assert.AreEqual(expectedNumberOfContentItems, contentEnumerated);
    }

    [Test]
    public void Can_Enumerate_Descendants_For_Content_Index_With_Disallowed_Content_Type()
    {
        var sut = CreateDeliveryApiContentIndexHelper(["umbTextPage2"]);

        var expectedNumberOfContentItems = GetExpectedNumberOfContentItems();

        var contentEnumerated = 0;
        Action<IContent[]> actionToPerform = content =>
        {
            contentEnumerated += content.Length;
        };

        const int pageSize = 3;
        sut.EnumerateApplicableDescendantsForContentIndex(
            Cms.Core.Constants.System.Root,
            actionToPerform,
            pageSize);

        Assert.AreEqual(expectedNumberOfContentItems - 1, contentEnumerated);
    }

    private DeliveryApiContentIndexHelper CreateDeliveryApiContentIndexHelper(HashSet<string>? disallowedContentTypeAliases = null)
    {
        return new DeliveryApiContentIndexHelper(
            ContentService,
            GetRequiredService<IUmbracoDatabaseFactory>(),
            GetDeliveryApiSettings(disallowedContentTypeAliases ?? []));
    }

    private IOptionsMonitor<DeliveryApiSettings> GetDeliveryApiSettings(HashSet<string> disallowedContentTypeAliases)
    {
        var deliveryApiSettings = new DeliveryApiSettings
        {
            DisallowedContentTypeAliases = disallowedContentTypeAliases,
        };

        var optionsMonitorMock = new Mock<IOptionsMonitor<DeliveryApiSettings>>();
        optionsMonitorMock.Setup(o => o.CurrentValue).Returns(deliveryApiSettings);
        return optionsMonitorMock.Object;
    }

    private int GetExpectedNumberOfContentItems()
    {
        var result = ContentService.GetAllPublished().Count();
        Assert.AreEqual(10, result);
        return result;
    }
}
