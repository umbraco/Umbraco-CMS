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
    public async Task PerformElementVersionCleanup_WithNoKeepPeriods_DeletesEverythingExceptActive()
    {
        var elementType = ContentTypeBuilder.CreateSimpleElementType();

        // Kill all historic
        elementType.HistoryCleanup.PreventCleanup = false;
        elementType.HistoryCleanup.KeepAllVersionsNewerThanDays = 0;
        elementType.HistoryCleanup.KeepLatestVersionPerDayForDays = 0;

        await ContentTypeService.CreateAsync(elementType, Constants.Security.SuperUserKey);

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
            Assert.That(before.ContentVersions, Is.EqualTo(12)); // 10 historic + current draft + current published
            Assert.That(before.ElementVersions, Is.EqualTo(12));
            Assert.That(before.PropertyData, Is.EqualTo(12 * 3)); // CreateSimpleContentType = 3 props
        });

        ElementVersionService.PerformContentVersionCleanup(DateTime.UtcNow.AddHours(1));

        var after = GetReport();

        Assert.Multiple(() =>
        {
            Assert.That(after.ContentVersions, Is.EqualTo(2)); // current draft, current published
            Assert.That(after.ElementVersions, Is.EqualTo(2));
            Assert.That(after.PropertyData, Is.EqualTo(6)); // CreateSimpleContentType = 3 props
        });
    }

    [Test]
    public async Task PerformElementVersionCleanup_WithPreventCleanup_DeletesNothing()
    {
        var elementType = ContentTypeBuilder.CreateSimpleElementType();

        // Retain all historic
        elementType.HistoryCleanup.PreventCleanup = true;

        await ContentTypeService.CreateAsync(elementType, Constants.Security.SuperUserKey);

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
            Assert.That(before.ContentVersions, Is.EqualTo(12)); // 10 historic + current draft + current published
            Assert.That(before.ElementVersions, Is.EqualTo(12));
            Assert.That(before.PropertyData, Is.EqualTo(12 * 3)); // CreateSimpleContentType = 3 props
        });

        ElementVersionService.PerformContentVersionCleanup(DateTime.UtcNow.AddHours(1));

        var after = GetReport();

        // no changes
        Assert.Multiple(() =>
        {
            Assert.That(after.ContentVersions, Is.EqualTo(before.ContentVersions));
            Assert.That(after.ElementVersions, Is.EqualTo(before.ElementVersions));
            Assert.That(after.PropertyData, Is.EqualTo(before.PropertyData));
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

        await ContentTypeService.CreateAsync(elementType, Constants.Security.SuperUserKey);

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
            Assert.That(before.ContentVersions, Is.EqualTo(12)); // 10 historic + current draft + current published
            Assert.That(before.ElementVersions, Is.EqualTo(12));
            Assert.That(before.PropertyData, Is.EqualTo(12 * 3)); // CreateSimpleContentType = 3 props
        });

        ElementVersionService.PerformContentVersionCleanup(DateTime.UtcNow.AddHours(1));

        var after = GetReport();

        Assert.Multiple(() =>
        {
            Assert.That(after.ContentVersions, Is.EqualTo(7)); // current draft, current published + 5 retained versions
            Assert.That(after.ElementVersions, Is.EqualTo(7));
            Assert.That(after.PropertyData, Is.EqualTo(7 * 3)); // CreateSimpleContentType = 3 props
        });

        var allVersions = await ElementVersionService.GetPagedContentVersionsAsync(element.Key, null, 0, 1000);
        Assert.That(allVersions.Success, Is.True);
        Assert.That(allVersions.Result.Total, Is.EqualTo(7));

        var allVersionIds = allVersions.Result.Items.Select(item => item.VersionId).ToArray();
        Assert.That(element.PublishedVersionId, Is.Not.EqualTo(element.VersionId));
        Assert.That(allVersionIds, Does.Contain(element.VersionId));
        Assert.That(allVersionIds, Does.Contain(element.PublishedVersionId));
        foreach (var retainedVersionId in retainedVersionIds)
        {
            Assert.That(allVersionIds, Does.Contain(retainedVersionId));
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
