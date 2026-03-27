using System.Diagnostics;
using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Configuration.Models;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Tests.Common.Builders;
using Umbraco.Cms.Tests.Common.Testing;
using Umbraco.Cms.Tests.Integration.Attributes;
using Umbraco.Cms.Tests.Integration.Testing;

namespace Umbraco.Cms.Tests.Integration.Umbraco.Infrastructure.Services;

[TestFixture]
[UmbracoTest(Database = UmbracoTestOptions.Database.NewSchemaPerTest)]
internal class ContentVersionCleanupServiceTest : UmbracoIntegrationTest
{
    public ITemplateService TemplateService => GetRequiredService<ITemplateService>();

    public IContentTypeService ContentTypeService => GetRequiredService<IContentTypeService>();

    public IContentService ContentService => GetRequiredService<IContentService>();

    public IContentVersionService ContentVersionService => GetRequiredService<IContentVersionService>();

    /// <remarks>
    ///     This is covered by the unit tests, but nice to know it deletes on infra.
    /// </remarks>
    [Test]
    public async Task PerformContentVersionCleanup_WithNoKeepPeriods_DeletesEverythingExceptActive()
    {
        // For reference, Our currently has
        // 5000 Documents
        // With 200K Versions
        // With 11M Property data

        var template = TemplateBuilder.CreateTextPageTemplate();
        await TemplateService.CreateAsync(template, Constants.Security.SuperUserKey);

        var contentTypeA =
            ContentTypeBuilder.CreateSimpleContentType("contentTypeA", "contentTypeA", defaultTemplateId: template.Id);

        // Kill all historic
        contentTypeA.HistoryCleanup.PreventCleanup = false;
        contentTypeA.HistoryCleanup.KeepAllVersionsNewerThanDays = 0;
        contentTypeA.HistoryCleanup.KeepLatestVersionPerDayForDays = 0;

        await ContentTypeService.CreateAsync(contentTypeA, Constants.Security.SuperUserKey);

        var content = ContentBuilder.CreateSimpleContent(contentTypeA);
        ContentService.Save(content);
        ContentService.Publish(content, []);

        for (var i = 0; i < 10; i++)
        {
            ContentService.Publish(content, []);
        }

        var before = GetReport();

        Debug.Assert(before.ContentVersions == 12, "Expected 12 content versions (10 historic + current draft + current published).");
        Debug.Assert(before.PropertyData == 12 * 3, "Expected property data count to be 36 (12 versions * 3 props).");

        ContentVersionService.PerformContentVersionCleanup(DateTime.UtcNow.AddHours(1));

        var after = GetReport();

        Assert.Multiple(() =>
        {
            Assert.AreEqual(2, after.ContentVersions); // current draft, current published
            Assert.AreEqual(2, after.DocumentVersions);
            Assert.AreEqual(6, after.PropertyData); // CreateSimpleContentType = 3 props
        });
    }

    [Test]
    public async Task PerformContentVersionCleanup_WithKeepAllNewerThanDays_RetainsRecentVersions()
    {
        var template = TemplateBuilder.CreateTextPageTemplate();
        await TemplateService.CreateAsync(template, Constants.Security.SuperUserKey);

        var contentType =
            ContentTypeBuilder.CreateSimpleContentType("contentTypeA", "contentTypeA", defaultTemplateId: template.Id);

        contentType.HistoryCleanup.PreventCleanup = false;
        contentType.HistoryCleanup.KeepAllVersionsNewerThanDays = 7;
        contentType.HistoryCleanup.KeepLatestVersionPerDayForDays = 0;

        await ContentTypeService.CreateAsync(contentType, Constants.Security.SuperUserKey);

        var content = ContentBuilder.CreateSimpleContent(contentType);
        ContentService.Save(content);
        ContentService.Publish(content, []);

        for (var i = 0; i < 5; i++)
        {
            ContentService.Publish(content, []);
        }

        // Total: 7 versions (1 initial save + 6 publishes). Current draft + current published = 2 active.
        // 5 historic versions.
        var before = GetReport();
        Assert.AreEqual(7, before.ContentVersions);

        // Backdate 3 of the historic versions to 10 days ago so they exceed the 7-day keep window.
        using (var scope = ScopeProvider.CreateScope(autoComplete: true))
        {
            var db = ScopeAccessor.AmbientScope!.Database;
            var syntax = db.SqlContext.SqlSyntax;

            var umbracoContentVersion = syntax.GetQuotedTableName("umbracoContentVersion");
            var versionDate = syntax.GetQuotedColumnName("versionDate");
            db.Execute(
                $"UPDATE {umbracoContentVersion} SET {versionDate} = @0 WHERE id IN (1, 2, 3)",
                DateTime.UtcNow.AddDays(-10));
        }

        ContentVersionService.PerformContentVersionCleanup(DateTime.UtcNow);

        var after = GetReport();

        Assert.Multiple(() =>
        {
            // 3 backdated historic versions should be deleted. 2 active + 2 recent historic remain = 4.
            Assert.AreEqual(4, after.ContentVersions);
            Assert.AreEqual(4, after.DocumentVersions);
        });
    }

    [Test]
    public async Task PerformContentVersionCleanup_WithKeepLatestPerDay_RetainsOnePerDay()
    {
        var template = TemplateBuilder.CreateTextPageTemplate();
        await TemplateService.CreateAsync(template, Constants.Security.SuperUserKey);

        var contentType =
            ContentTypeBuilder.CreateSimpleContentType("contentTypeA", "contentTypeA", defaultTemplateId: template.Id);

        contentType.HistoryCleanup.PreventCleanup = false;
        contentType.HistoryCleanup.KeepAllVersionsNewerThanDays = 0;
        contentType.HistoryCleanup.KeepLatestVersionPerDayForDays = 90;

        await ContentTypeService.CreateAsync(contentType, Constants.Security.SuperUserKey);

        var content = ContentBuilder.CreateSimpleContent(contentType);
        ContentService.Save(content);
        ContentService.Publish(content, []);

        // Create 6 more publishes (total 8 versions = 1 save + 7 publishes)
        for (var i = 0; i < 6; i++)
        {
            ContentService.Publish(content, []);
        }

        var before = GetReport();
        Assert.AreEqual(8, before.ContentVersions);

        // Backdate versions into 3 "days":
        // Day 1 (10 days ago): versions 1, 2, 3 — should keep only version 3 (newest)
        // Day 2 (5 days ago): versions 4, 5 — should keep only version 5 (newest)
        // Day 3 (today): version 6 is historic, 7 & 8 are active (draft + published)
        //   Version 6 is the only historic version today — should be kept as latest for today
        using (var scope = ScopeProvider.CreateScope(autoComplete: true))
        {
            var db = ScopeAccessor.AmbientScope!.Database;
            var syntax = db.SqlContext.SqlSyntax;
            var umbracoContentVersion = syntax.GetQuotedTableName("umbracoContentVersion");
            var versionDate = syntax.GetQuotedColumnName("versionDate");
            var tenDaysAgo = DateTime.UtcNow.AddDays(-10);
            var fiveDaysAgo = DateTime.UtcNow.AddDays(-5);
            db.Execute($"UPDATE {umbracoContentVersion} SET {versionDate} = @0 WHERE id IN (1, 2, 3)", tenDaysAgo);
            db.Execute($"UPDATE {umbracoContentVersion} SET {versionDate} = @0 WHERE id IN (4, 5)", fiveDaysAgo);
        }

        ContentVersionService.PerformContentVersionCleanup(DateTime.UtcNow);

        var after = GetReport();

        Assert.Multiple(() =>
        {
            // Deleted: versions 1, 2 (day 1 non-latest) + version 4 (day 2 non-latest) = 3 deleted
            // Remaining: version 3 (day 1 latest) + version 5 (day 2 latest) + version 6 (today latest)
            //           + version 7 (current draft) + version 8 (current published) = 5
            Assert.AreEqual(5, after.ContentVersions);
            Assert.AreEqual(5, after.DocumentVersions);
        });
    }

    [Test]
    public async Task PerformContentVersionCleanup_WithContentTypeOverride_RespectsPreventCleanup()
    {
        var template = TemplateBuilder.CreateTextPageTemplate();
        await TemplateService.CreateAsync(template, Constants.Security.SuperUserKey);

        var contentTypeA =
            ContentTypeBuilder.CreateSimpleContentType("contentTypeA", "contentTypeA", defaultTemplateId: template.Id);
        contentTypeA.HistoryCleanup.PreventCleanup = false;
        contentTypeA.HistoryCleanup.KeepAllVersionsNewerThanDays = 0;
        contentTypeA.HistoryCleanup.KeepLatestVersionPerDayForDays = 0;
        await ContentTypeService.CreateAsync(contentTypeA, Constants.Security.SuperUserKey);

        var contentTypeB =
            ContentTypeBuilder.CreateSimpleContentType("contentTypeB", "contentTypeB", defaultTemplateId: template.Id);

        // Set preventCleanup directly on the content type — this creates the override policy row automatically.
        contentTypeB.HistoryCleanup.PreventCleanup = true;
        contentTypeB.HistoryCleanup.KeepAllVersionsNewerThanDays = 0;
        contentTypeB.HistoryCleanup.KeepLatestVersionPerDayForDays = 0;
        await ContentTypeService.CreateAsync(contentTypeB, Constants.Security.SuperUserKey);

        var contentA = ContentBuilder.CreateSimpleContent(contentTypeA);
        ContentService.Save(contentA);
        ContentService.Publish(contentA, []);
        ContentService.Publish(contentA, []);

        var contentB = ContentBuilder.CreateSimpleContent(contentTypeB);
        ContentService.Save(contentB);
        ContentService.Publish(contentB, []);
        ContentService.Publish(contentB, []);

        var before = GetReport();
        Assert.Greater(before.ContentVersions, 4);

        ContentVersionService.PerformContentVersionCleanup(DateTime.UtcNow.AddHours(1));

        var after = GetReport();

        // Verify contentTypeB versions were preserved by checking the DB directly
        using (var scope = ScopeProvider.CreateScope(autoComplete: true))
        {
            var db = ScopeAccessor.AmbientScope!.Database;
            var syntax = db.SqlContext.SqlSyntax;

            var umbracoContentVersion = syntax.GetQuotedTableName("umbracoContentVersion");
            var umbracoContent = syntax.GetQuotedTableName("umbracoContent");
            var nodeId = syntax.GetQuotedColumnName("nodeId");
            var contentTypeId = syntax.GetQuotedColumnName("contentTypeId");
            var contentAVersions = db.Single<int>(
                $"SELECT count(1) FROM {umbracoContentVersion} cv INNER JOIN {umbracoContent} c ON cv.{nodeId} = c.{nodeId} WHERE c.{contentTypeId} = @0",
                contentTypeA.Id);
            var contentBVersions = db.Single<int>(
                $"SELECT count(1) FROM {umbracoContentVersion} cv INNER JOIN {umbracoContent} c ON cv.{nodeId} = c.{nodeId} WHERE c.{contentTypeId} = @0",
                contentTypeB.Id);

            Assert.Multiple(() =>
            {
                // ContentTypeA: only active versions remain (current draft + current published = 2)
                Assert.AreEqual(2, contentAVersions);
                // ContentTypeB: all versions preserved (preventCleanup override)
                Assert.Greater(contentBVersions, 2);
            });
        }
    }

    [Test]
    public async Task PerformContentVersionCleanup_MultipleCalls_IsIdempotent()
    {
        var template = TemplateBuilder.CreateTextPageTemplate();
        await TemplateService.CreateAsync(template, Constants.Security.SuperUserKey);

        var contentType =
            ContentTypeBuilder.CreateSimpleContentType("contentTypeA", "contentTypeA", defaultTemplateId: template.Id);
        contentType.HistoryCleanup.PreventCleanup = false;
        contentType.HistoryCleanup.KeepAllVersionsNewerThanDays = 0;
        contentType.HistoryCleanup.KeepLatestVersionPerDayForDays = 0;
        await ContentTypeService.CreateAsync(contentType, Constants.Security.SuperUserKey);

        var content = ContentBuilder.CreateSimpleContent(contentType);
        ContentService.Save(content);
        ContentService.Publish(content, []);

        for (var i = 0; i < 5; i++)
        {
            ContentService.Publish(content, []);
        }

        // First cleanup run
        var firstResult = ContentVersionService.PerformContentVersionCleanup(DateTime.UtcNow.AddHours(1));
        Assert.Greater(firstResult.Count, 0, "First run should delete some versions");

        var afterFirst = GetReport();

        // Second cleanup run — should have nothing to do
        var secondResult = ContentVersionService.PerformContentVersionCleanup(DateTime.UtcNow.AddHours(1));
        Assert.AreEqual(0, secondResult.Count, "Second run should delete nothing");

        var afterSecond = GetReport();

        Assert.Multiple(() =>
        {
            Assert.AreEqual(afterFirst.ContentVersions, afterSecond.ContentVersions);
            Assert.AreEqual(afterFirst.DocumentVersions, afterSecond.DocumentVersions);
            Assert.AreEqual(afterFirst.PropertyData, afterSecond.PropertyData);
        });
    }

    public static void ConfigureLowMaxVersionsPerRun(IUmbracoBuilder builder)
    {
        builder.Services.Configure<ContentSettings>(settings =>
            settings.ContentVersionCleanupPolicy.MaxVersionsToDeletePerRun = 3);
    }

    [Test]
    [ConfigureBuilder(ActionName = nameof(ConfigureLowMaxVersionsPerRun))]
    public async Task PerformContentVersionCleanup_WithMaxVersionsPerRunCap_DeletesOnlyUpToCap()
    {
        var template = TemplateBuilder.CreateTextPageTemplate();
        await TemplateService.CreateAsync(template, Constants.Security.SuperUserKey);

        var contentType = ContentTypeBuilder.CreateSimpleContentType("contentTypeA", "contentTypeA", defaultTemplateId: template.Id);
        contentType.HistoryCleanup.PreventCleanup = false;
        contentType.HistoryCleanup.KeepAllVersionsNewerThanDays = 0;
        contentType.HistoryCleanup.KeepLatestVersionPerDayForDays = 0;
        await ContentTypeService.CreateAsync(contentType, Constants.Security.SuperUserKey);

        var content = ContentBuilder.CreateSimpleContent(contentType);
        ContentService.Save(content);
        ContentService.Publish(content, []);

        for (var i = 0; i < 8; i++)
        {
            ContentService.Publish(content, []);
        }

        // 10 versions total: 8 historic + current draft + current published.
        var before = GetReport();
        Assert.AreEqual(10, before.ContentVersions);

        // First run: MaxVersionsToDeletePerRun = 3, so only 3 historic versions should be deleted.
        var firstResult = ContentVersionService.PerformContentVersionCleanup(DateTime.UtcNow.AddHours(1));
        Assert.AreEqual(3, firstResult.Count, "First run should delete exactly 3 (the per-run cap)");

        var afterFirst = GetReport();
        Assert.AreEqual(7, afterFirst.ContentVersions);

        // Second run: another 3 deleted.
        var secondResult = ContentVersionService.PerformContentVersionCleanup(DateTime.UtcNow.AddHours(1));
        Assert.AreEqual(3, secondResult.Count, "Second run should delete another 3");

        var afterSecond = GetReport();
        Assert.AreEqual(4, afterSecond.ContentVersions);

        // Third run: only 2 historic remain, so fewer than cap deleted.
        var thirdResult = ContentVersionService.PerformContentVersionCleanup(DateTime.UtcNow.AddHours(1));
        Assert.AreEqual(2, thirdResult.Count, "Third run should delete remaining 2");

        var afterThird = GetReport();
        Assert.AreEqual(2, afterThird.ContentVersions); // Only current draft + current published remain.
    }

    private Report GetReport()
    {
        using var scope = ScopeProvider.CreateScope(autoComplete: true);

        var database = ScopeAccessor.AmbientScope.Database;
        var syntax = database.SqlContext.SqlSyntax;
        var contentVersions = database.Single<int>($"select count(1) from {syntax.GetQuotedTableName("umbracoContentVersion")}");
        var documentVersions = database.Single<int>($"select count(1) from {syntax.GetQuotedTableName("umbracoDocumentVersion")}");
        var propertyData = database.Single<int>($"select count(1) from {syntax.GetQuotedTableName("umbracoPropertyData")}");

        return new Report
        {
            ContentVersions = contentVersions,
            DocumentVersions = documentVersions,
            PropertyData = propertyData,
        };
    }

    private class Report
    {
        public int ContentVersions { get; set; }

        public int DocumentVersions { get; set; }

        public int PropertyData { get; set; }
    }
}
