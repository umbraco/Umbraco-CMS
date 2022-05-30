using System;
using System.Diagnostics;
using NUnit.Framework;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Infrastructure.Persistence.Dtos;
using Umbraco.Cms.Tests.Common.Builders;
using Umbraco.Cms.Tests.Common.Testing;
using Umbraco.Cms.Tests.Integration.Testing;

namespace Umbraco.Cms.Tests.Integration.Umbraco.Infrastructure.Services;

[TestFixture]
[UmbracoTest(Database = UmbracoTestOptions.Database.NewSchemaPerTest)]
internal class ContentVersionCleanupServiceTest : UmbracoIntegrationTest
{
    public IFileService FileService => GetRequiredService<IFileService>();

    public IContentTypeService ContentTypeService => GetRequiredService<IContentTypeService>();

    public IContentService ContentService => GetRequiredService<IContentService>();

    public IContentVersionService ContentVersionService => GetRequiredService<IContentVersionService>();

    /// <remarks>
    ///     This is covered by the unit tests, but nice to know it deletes on infra.
    ///     And proves implementation is compatible with SQL CE.
    /// </remarks>
    [Test]
    public void PerformContentVersionCleanup_WithNoKeepPeriods_DeletesEverythingExceptActive()
    {
        // For reference, Our currently has
        // 5000 Documents
        // With 200K Versions
        // With 11M Property data

        var template = TemplateBuilder.CreateTextPageTemplate();
        FileService.SaveTemplate(template);

        var contentTypeA =
            ContentTypeBuilder.CreateSimpleContentType("contentTypeA", "contentTypeA", defaultTemplateId: template.Id);

        // Kill all historic
        contentTypeA.HistoryCleanup.PreventCleanup = false;
        contentTypeA.HistoryCleanup.KeepAllVersionsNewerThanDays = 0;
        contentTypeA.HistoryCleanup.KeepLatestVersionPerDayForDays = 0;

        ContentTypeService.Save(contentTypeA);

        var content = ContentBuilder.CreateSimpleContent(contentTypeA);
        ContentService.SaveAndPublish(content);

        for (var i = 0; i < 10; i++)
        {
            ContentService.SaveAndPublish(content);
        }

        var before = GetReport();

        Debug.Assert(before.ContentVersions == 12); // 10 historic + current draft + current published
        Debug.Assert(before.PropertyData == 12 * 3); // CreateSimpleContentType = 3 props

        ContentVersionService.PerformContentVersionCleanup(DateTime.Now.AddHours(1));

        var after = GetReport();

        Assert.Multiple(() =>
        {
            Assert.AreEqual(2, after.ContentVersions); // current draft, current published
            Assert.AreEqual(2, after.DocumentVersions);
            Assert.AreEqual(6, after.PropertyData); // CreateSimpleContentType = 3 props
        });
    }

    private Report GetReport()
    {
        using (var scope = ScopeProvider.CreateScope(autoComplete: true))
        {
            // SQL CE is fun!
            var contentVersions =
                ScopeAccessor.AmbientScope.Database.Single<int>(@"select count(1) from umbracoContentVersion");
            var documentVersions =
                ScopeAccessor.AmbientScope.Database.Single<int>(@"select count(1) from umbracoDocumentVersion");
            var propertyData =
                ScopeAccessor.AmbientScope.Database.Single<int>(@"select count(1) from umbracoPropertyData");

            return new Report
            {
                ContentVersions = contentVersions,
                DocumentVersions = documentVersions,
                PropertyData = propertyData
            };
        }
    }

    private void InsertCleanupPolicy(IContentType contentType, int daysToKeepAll, int daysToRollupAll,
        bool preventCleanup = false)
    {
        using (var scope = ScopeProvider.CreateScope(autoComplete: true))
        {
            var entity = new ContentVersionCleanupPolicyDto
            {
                ContentTypeId = contentType.Id,
                KeepAllVersionsNewerThanDays = daysToKeepAll,
                KeepLatestVersionPerDayForDays = daysToRollupAll,
                PreventCleanup = preventCleanup,
                Updated = DateTime.Today
            };

            ScopeAccessor.AmbientScope.Database.Insert(entity);
        }
    }

    private class Report
    {
        public int ContentVersions { get; set; }

        public int DocumentVersions { get; set; }

        public int PropertyData { get; set; }
    }
}
