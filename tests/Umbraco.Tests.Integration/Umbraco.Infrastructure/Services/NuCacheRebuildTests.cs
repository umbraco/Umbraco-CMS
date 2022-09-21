using System;
using NUnit.Framework;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.PublishedCache;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Strings;
using Umbraco.Cms.Tests.Common.Builders;
using Umbraco.Cms.Tests.Common.Testing;
using Umbraco.Cms.Tests.Integration.Testing;

namespace Umbraco.Cms.Tests.Integration.Umbraco.Infrastructure.Services;

[TestFixture]
[UmbracoTest(Database = UmbracoTestOptions.Database.NewSchemaPerTest, PublishedRepositoryEvents = true,
    WithApplication = true)]
public class NuCacheRebuildTests : UmbracoIntegrationTest
{
    private IFileService FileService => GetRequiredService<IFileService>();

    private IContentService ContentService => GetRequiredService<IContentService>();

    private IContentTypeService ContentTypeService => GetRequiredService<IContentTypeService>();

    private IPublishedSnapshotService PublishedSnapshotService => GetRequiredService<IPublishedSnapshotService>();

    [Test]
    public void UnpublishedNameChanges()
    {
        var urlSegmentProvider = new DefaultUrlSegmentProvider(ShortStringHelper);

        var template = TemplateBuilder.CreateTextPageTemplate();
        FileService.SaveTemplate(template);

        var contentType = ContentTypeBuilder.CreateTextPageContentType(defaultTemplateId: template.Id);
        ContentTypeService.Save(contentType);

        var content = ContentBuilder.CreateTextpageContent(contentType, "hello", Constants.System.Root);

        ContentService.SaveAndPublish(content);
        var cachedContent = ContentService.GetById(content.Id);
        var segment = urlSegmentProvider.GetUrlSegment(cachedContent);

        // Does a new node work?

        Assert.AreEqual("hello", segment);

        content.Name = "goodbye";
        cachedContent = ContentService.GetById(content.Id);
        segment = urlSegmentProvider.GetUrlSegment(cachedContent);

        // We didn't save anything, so all should still be the same

        Assert.AreEqual("hello", segment);

        ContentService.Save(content);
        cachedContent = ContentService.GetById(content.Id);
        segment = urlSegmentProvider.GetUrlSegment(cachedContent);

        // At this point we have saved the new name, but not published. The url should still be the previous name

        Assert.AreEqual("hello", segment);

        PublishedSnapshotService.RebuildAll();

        cachedContent = ContentService.GetById(content.Id);
        segment = urlSegmentProvider.GetUrlSegment(cachedContent);

        // After a rebuild, the unpublished name should still not be the url.
        // This was previously incorrect, per #11074

        Assert.AreEqual("hello", segment);

        ContentService.SaveAndPublish(content);
        cachedContent = ContentService.GetById(content.Id);
        segment = urlSegmentProvider.GetUrlSegment(cachedContent);

        // The page has now been published, so we should see the new url segment
        Assert.AreEqual("goodbye", segment);

        PublishedSnapshotService.RebuildAll();
        cachedContent = ContentService.GetById(content.Id);
        segment = urlSegmentProvider.GetUrlSegment(cachedContent);

        // Just double checking that things remain after a rebuild
        Assert.AreEqual("goodbye", segment);
    }
}
