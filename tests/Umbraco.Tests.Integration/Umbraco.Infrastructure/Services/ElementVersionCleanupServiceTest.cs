using NUnit.Framework;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Tests.Common.Builders;
using Umbraco.Cms.Tests.Common.Testing;
using Umbraco.Cms.Tests.Integration.Testing;

namespace Umbraco.Cms.Tests.Integration.Umbraco.Infrastructure.Services;

[TestFixture]
[UmbracoTest(Database = UmbracoTestOptions.Database.NewSchemaPerTest)]
internal class ElementVersionCleanupServiceTest : UmbracoIntegrationTest
{
    public IContentTypeService ContentTypeService => GetRequiredService<IContentTypeService>();

    public IElementService ElementService => GetRequiredService<IElementService>();

    public IElementVersionService ElementVersionService => GetRequiredService<IElementVersionService>();

    [Test]
    public void PerformElementVersionCleanup_WithNoKeepPeriods_DeletesEverythingExceptActive()
    {
        var elementType = ContentTypeBuilder.CreateSimpleElementType();

        // Kill all historic
        elementType.HistoryCleanup.PreventCleanup = false;
        elementType.HistoryCleanup.KeepAllVersionsNewerThanDays = 0;
        elementType.HistoryCleanup.KeepLatestVersionPerDayForDays = 0;

        ContentTypeService.Save(elementType);

        var element = ElementBuilder.CreateSimpleElement(elementType);
        ElementService.Save(element);
        ElementService.Publish(element, Array.Empty<string>());

        for (var i = 0; i < 10; i++)
        {
            ElementService.Publish(element, Array.Empty<string>());
        }

        var before = GetReport();

        Assert.Multiple(() =>
        {
            Assert.AreEqual(12, before.ContentVersions); // 10 historic + current draft + current published
            Assert.AreEqual(12, before.ElementVersions);
            Assert.AreEqual(12 * 3, before.PropertyData); // CreateSimpleContentType = 3 props
        });

        ElementVersionService.PerformContentVersionCleanup(DateTime.UtcNow.AddHours(1));

        var after = GetReport();

        Assert.Multiple(() =>
        {
            Assert.AreEqual(2, after.ContentVersions); // current draft, current published
            Assert.AreEqual(2, after.ElementVersions);
            Assert.AreEqual(6, after.PropertyData); // CreateSimpleContentType = 3 props
        });
    }

    [Test]
    public void PerformElementVersionCleanup_WithPreventCleanup_DeletesNothing()
    {
        var elementType = ContentTypeBuilder.CreateSimpleElementType();

        // Retain all historic
        elementType.HistoryCleanup.PreventCleanup = true;

        ContentTypeService.Save(elementType);

        var element = ElementBuilder.CreateSimpleElement(elementType);
        ElementService.Save(element);
        ElementService.Publish(element, Array.Empty<string>());

        for (var i = 0; i < 10; i++)
        {
            ElementService.Publish(element, Array.Empty<string>());
        }

        var before = GetReport();

        Assert.Multiple(() =>
        {
            Assert.AreEqual(12, before.ContentVersions); // 10 historic + current draft + current published
            Assert.AreEqual(12, before.ElementVersions);
            Assert.AreEqual(12 * 3, before.PropertyData); // CreateSimpleContentType = 3 props
        });

        ElementVersionService.PerformContentVersionCleanup(DateTime.UtcNow.AddHours(1));

        var after = GetReport();

        // no changes
        Assert.Multiple(() =>
        {
            Assert.AreEqual(before.ContentVersions, after.ContentVersions);
            Assert.AreEqual(before.ElementVersions, after.ElementVersions);
            Assert.AreEqual(before.PropertyData, after.PropertyData);
        });
    }

    [Test]
    public async Task PerformElementVersionCleanup_CanPreventCleanupOfSpecificVersions()
    {
        var elementType = ContentTypeBuilder.CreateSimpleElementType();

        // Kill all historic
        elementType.HistoryCleanup.PreventCleanup = false;
        elementType.HistoryCleanup.KeepAllVersionsNewerThanDays = 0;
        elementType.HistoryCleanup.KeepLatestVersionPerDayForDays = 0;

        ContentTypeService.Save(elementType);

        var element = ElementBuilder.CreateSimpleElement(elementType);
        ElementService.Save(element);
        ElementService.Publish(element, Array.Empty<string>());

        var retainedVersionIds = new List<int>();
        for (var i = 0; i < 10; i++)
        {
            var result = ElementService.Publish(element, Array.Empty<string>());
            if (i < 5)
            {
                retainedVersionIds.Add(result.Content.VersionId);
                await ElementVersionService.SetPreventCleanupAsync(retainedVersionIds.Last().ToGuid(), true, Constants.Security.SuperUserKey);
            }
        }

        var before = GetReport();

        Assert.Multiple(() =>
        {
            Assert.AreEqual(12, before.ContentVersions); // 10 historic + current draft + current published
            Assert.AreEqual(12, before.ElementVersions);
            Assert.AreEqual(12 * 3, before.PropertyData); // CreateSimpleContentType = 3 props
        });

        ElementVersionService.PerformContentVersionCleanup(DateTime.UtcNow.AddHours(1));

        var after = GetReport();

        Assert.Multiple(() =>
        {
            Assert.AreEqual(7, after.ContentVersions); // current draft, current published + 5 retained versions
            Assert.AreEqual(7, after.ElementVersions);
            Assert.AreEqual(7 * 3, after.PropertyData); // CreateSimpleContentType = 3 props
        });

        var allVersions = await ElementVersionService.GetPagedContentVersionsAsync(element.Key, null, 0, 1000);
        Assert.IsTrue(allVersions.Success);
        Assert.AreEqual(7, allVersions.Result.Total);

        var allVersionIds = allVersions.Result.Items.Select(item => item.VersionId).ToArray();
        Assert.AreNotEqual(element.VersionId, element.PublishedVersionId);
        Assert.Contains(element.VersionId, allVersionIds);
        Assert.Contains(element.PublishedVersionId, allVersionIds);
        foreach (var retainedVersionId in retainedVersionIds)
        {
            Assert.Contains(retainedVersionId, allVersionIds);
        }
    }

    private Report GetReport()
    {
        using (var scope = ScopeProvider.CreateScope(autoComplete: true))
        {
            // SQL CE is fun!
            var contentVersions =
                ScopeAccessor.AmbientScope.Database.Single<int>(@"select count(1) from umbracoContentVersion");
            var elementVersions =
                ScopeAccessor.AmbientScope.Database.Single<int>(@"select count(1) from umbracoElementVersion");
            var propertyData =
                ScopeAccessor.AmbientScope.Database.Single<int>(@"select count(1) from umbracoPropertyData");

            return new Report
            {
                ContentVersions = contentVersions,
                ElementVersions = elementVersions,
                PropertyData = propertyData
            };
        }
    }

    private class Report
    {
        public int ContentVersions { get; set; }

        public int ElementVersions { get; set; }

        public int PropertyData { get; set; }
    }
}
