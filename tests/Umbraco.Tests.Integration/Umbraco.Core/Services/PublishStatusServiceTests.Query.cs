using NUnit.Framework;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Services.Navigation;
using Umbraco.Cms.Tests.Common.Builders;

namespace Umbraco.Cms.Tests.Integration.Umbraco.Core.Services;

internal sealed partial class PublishStatusServiceTests
{
    private IPublishStatusQueryService PublishStatusQueryService => GetRequiredService<IPublishStatusQueryService>();

    [Test]
    public void When_Nothing_Is_Publised_All_Documents_Have_Unpublished_Status()
    {
        Assert.Multiple(() =>
        {
            Assert.IsFalse(PublishStatusQueryService.IsDocumentPublished(Textpage.Key, DefaultCulture));
            Assert.IsFalse(PublishStatusQueryService.IsDocumentPublished(Subpage.Key, DefaultCulture));
            Assert.IsFalse(PublishStatusQueryService.IsDocumentPublished(Subpage2.Key, DefaultCulture));
            Assert.IsFalse(PublishStatusQueryService.IsDocumentPublished(Subpage3.Key, DefaultCulture));

            Assert.IsFalse(PublishStatusQueryService.IsDocumentPublished(Trashed.Key, DefaultCulture));

            Assert.IsFalse(PublishStatusQueryService.IsDocumentPublished(Textpage.Key, UnusedCulture));
            Assert.IsFalse(PublishStatusQueryService.IsDocumentPublished(Subpage.Key, UnusedCulture));
            Assert.IsFalse(PublishStatusQueryService.IsDocumentPublished(Subpage2.Key, UnusedCulture));
            Assert.IsFalse(PublishStatusQueryService.IsDocumentPublished(Subpage3.Key, UnusedCulture));

            Assert.IsFalse(PublishStatusQueryService.IsDocumentPublished(Trashed.Key, UnusedCulture));

            Assert.IsFalse(PublishStatusQueryService.IsDocumentPublishedInAnyCulture(Textpage.Key));
            Assert.IsFalse(PublishStatusQueryService.IsDocumentPublishedInAnyCulture(Subpage.Key));
            Assert.IsFalse(PublishStatusQueryService.IsDocumentPublishedInAnyCulture(Subpage2.Key));
            Assert.IsFalse(PublishStatusQueryService.IsDocumentPublishedInAnyCulture(Subpage3.Key));

        });
    }

    [Test]
    public void Unpublish_Updates_Document_Path_Published_Status()
    {
        var grandchild = ContentBuilder.CreateSimpleContent(ContentType, "Grandchild", Subpage2.Id);

        var contentSchedule = ContentScheduleCollection.CreateWithEntry(DateTime.UtcNow.AddMinutes(-5), null);
        ContentService.Save(grandchild, -1, contentSchedule);

        var publishResults = ContentService.PublishBranch(Textpage, PublishBranchFilter.IncludeUnpublished, ["*"]);

        var subPage2FromDB = ContentService.GetById(Subpage2.Key);
        var publishResult = ContentService.Unpublish(subPage2FromDB);
        Assert.Multiple(() =>
        {
            Assert.IsTrue(publishResults.All(x => x.Result == PublishResultType.SuccessPublish));
            Assert.IsTrue(publishResult.Success);
            Assert.IsTrue(PublishStatusQueryService.IsDocumentPublished(Textpage.Key, DefaultCulture));
            Assert.IsFalse(PublishStatusQueryService.IsDocumentPublished(Subpage2.Key, DefaultCulture));
            Assert.IsTrue(PublishStatusQueryService.IsDocumentPublished(grandchild.Key, DefaultCulture)); // grandchild is still published, but it will not be routable

            Assert.IsFalse(PublishStatusQueryService.IsDocumentPublished(Textpage.Key, UnusedCulture));
            Assert.IsFalse(PublishStatusQueryService.IsDocumentPublished(Subpage2.Key, UnusedCulture));
            Assert.IsFalse(PublishStatusQueryService.IsDocumentPublished(grandchild.Key, UnusedCulture));

            Assert.IsTrue(PublishStatusQueryService.IsDocumentPublishedInAnyCulture(Textpage.Key));
            Assert.IsFalse(PublishStatusQueryService.IsDocumentPublishedInAnyCulture(Subpage2.Key));
            Assert.IsTrue(PublishStatusQueryService.IsDocumentPublishedInAnyCulture(grandchild.Key));
        });
    }

    [Test]
    public void Publish_Branch_Updates_Document_Path_Published_Status()
    {
        var publishResults = ContentService.PublishBranch(Textpage, PublishBranchFilter.IncludeUnpublished, ["*"]);
        Assert.Multiple(() =>
        {
            Assert.IsTrue(publishResults.All(x => x.Result == PublishResultType.SuccessPublish));
            Assert.IsTrue(PublishStatusQueryService.IsDocumentPublished(Textpage.Key, DefaultCulture));
            Assert.IsTrue(PublishStatusQueryService.IsDocumentPublished(Subpage.Key, DefaultCulture));
            Assert.IsTrue(PublishStatusQueryService.IsDocumentPublished(Subpage2.Key, DefaultCulture));
            Assert.IsTrue(PublishStatusQueryService.IsDocumentPublished(Subpage3.Key, DefaultCulture));

            Assert.IsFalse(PublishStatusQueryService.IsDocumentPublished(Trashed.Key, DefaultCulture));

            Assert.IsFalse(PublishStatusQueryService.IsDocumentPublished(Textpage.Key, UnusedCulture));
            Assert.IsFalse(PublishStatusQueryService.IsDocumentPublished(Subpage.Key, UnusedCulture));
            Assert.IsFalse(PublishStatusQueryService.IsDocumentPublished(Subpage2.Key, UnusedCulture));
            Assert.IsFalse(PublishStatusQueryService.IsDocumentPublished(Subpage3.Key, UnusedCulture));

            Assert.IsFalse(PublishStatusQueryService.IsDocumentPublished(Trashed.Key, UnusedCulture));

            Assert.IsTrue(PublishStatusQueryService.IsDocumentPublishedInAnyCulture(Textpage.Key));
            Assert.IsTrue(PublishStatusQueryService.IsDocumentPublishedInAnyCulture(Subpage.Key));
            Assert.IsTrue(PublishStatusQueryService.IsDocumentPublishedInAnyCulture(Subpage2.Key));
            Assert.IsTrue(PublishStatusQueryService.IsDocumentPublishedInAnyCulture(Subpage3.Key));

            Assert.IsTrue(PublishStatusQueryService.HasPublishedAncestorPath(Textpage.Key));
            Assert.IsTrue(PublishStatusQueryService.HasPublishedAncestorPath(Subpage.Key));
        });
    }

    [Test]
    public void Published_Document_With_UnPublished_Parent_Has_Unpublished_Path()
    {
        Assert.Multiple(() =>
        {
            Assert.IsFalse(PublishStatusQueryService.IsDocumentPublished(Textpage.Key, DefaultCulture));
            Assert.IsFalse(PublishStatusQueryService.IsDocumentPublished(Subpage.Key, DefaultCulture));
        });

        ContentService.PublishBranch(Textpage, PublishBranchFilter.IncludeUnpublished, ["*"]);

        Assert.Multiple(() =>
        {
            Assert.IsTrue(PublishStatusQueryService.IsDocumentPublished(Textpage.Key, DefaultCulture));
            Assert.IsTrue(PublishStatusQueryService.IsDocumentPublished(Subpage.Key, DefaultCulture));
        });

        ContentService.Unpublish(Textpage);

        // Unpublish the root item - the sub page will still be published but it won't have a published path.
        Assert.Multiple(() =>
        {
            Assert.IsFalse(PublishStatusQueryService.IsDocumentPublished(Textpage.Key, DefaultCulture));
            Assert.IsTrue(PublishStatusQueryService.IsDocumentPublished(Subpage.Key, DefaultCulture));

            Assert.IsFalse(PublishStatusQueryService.HasPublishedAncestorPath(Subpage.Key));
        });
    }
}
