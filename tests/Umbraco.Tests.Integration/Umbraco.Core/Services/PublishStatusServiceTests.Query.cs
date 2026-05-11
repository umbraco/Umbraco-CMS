using NUnit.Framework;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Services.Navigation;
using Umbraco.Cms.Tests.Common.Builders;
using Umbraco.Cms.Tests.Common.Builders.Extensions;

namespace Umbraco.Cms.Tests.Integration.Umbraco.Core.Services;

internal sealed partial class PublishStatusServiceTests
{
    private IPublishStatusQueryService PublishStatusQueryService => GetRequiredService<IPublishStatusQueryService>();

    [Test]
    public void When_Nothing_Is_Publised_All_Documents_Have_Unpublished_Status()
    {
        Assert.Multiple(() =>
        {
            Assert.IsFalse(PublishStatusQueryService.IsPublished(Textpage.Key, DefaultCulture));
            Assert.IsFalse(PublishStatusQueryService.IsPublished(Subpage.Key, DefaultCulture));
            Assert.IsFalse(PublishStatusQueryService.IsPublished(Subpage2.Key, DefaultCulture));
            Assert.IsFalse(PublishStatusQueryService.IsPublished(Subpage3.Key, DefaultCulture));

            Assert.IsFalse(PublishStatusQueryService.IsPublished(Trashed.Key, DefaultCulture));

            Assert.IsFalse(PublishStatusQueryService.IsPublished(Textpage.Key, UnusedCulture));
            Assert.IsFalse(PublishStatusQueryService.IsPublished(Subpage.Key, UnusedCulture));
            Assert.IsFalse(PublishStatusQueryService.IsPublished(Subpage2.Key, UnusedCulture));
            Assert.IsFalse(PublishStatusQueryService.IsPublished(Subpage3.Key, UnusedCulture));

            Assert.IsFalse(PublishStatusQueryService.IsPublished(Trashed.Key, UnusedCulture));

            Assert.IsFalse(PublishStatusQueryService.IsPublishedInAnyCulture(Textpage.Key));
            Assert.IsFalse(PublishStatusQueryService.IsPublishedInAnyCulture(Subpage.Key));
            Assert.IsFalse(PublishStatusQueryService.IsPublishedInAnyCulture(Subpage2.Key));
            Assert.IsFalse(PublishStatusQueryService.IsPublishedInAnyCulture(Subpage3.Key));

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
            Assert.IsTrue(PublishStatusQueryService.IsPublished(Textpage.Key, DefaultCulture));
            Assert.IsFalse(PublishStatusQueryService.IsPublished(Subpage2.Key, DefaultCulture));
            Assert.IsTrue(PublishStatusQueryService.IsPublished(grandchild.Key, DefaultCulture)); // grandchild is still published, but it will not be routable

            Assert.IsFalse(PublishStatusQueryService.IsPublished(Textpage.Key, UnusedCulture));
            Assert.IsFalse(PublishStatusQueryService.IsPublished(Subpage2.Key, UnusedCulture));
            Assert.IsFalse(PublishStatusQueryService.IsPublished(grandchild.Key, UnusedCulture));

            Assert.IsTrue(PublishStatusQueryService.IsPublishedInAnyCulture(Textpage.Key));
            Assert.IsFalse(PublishStatusQueryService.IsPublishedInAnyCulture(Subpage2.Key));
            Assert.IsTrue(PublishStatusQueryService.IsPublishedInAnyCulture(grandchild.Key));
        });
    }

    [Test]
    public void Publish_Branch_Updates_Document_Path_Published_Status()
    {
        var publishResults = ContentService.PublishBranch(Textpage, PublishBranchFilter.IncludeUnpublished, ["*"]);
        Assert.Multiple(() =>
        {
            Assert.IsTrue(publishResults.All(x => x.Result == PublishResultType.SuccessPublish));
            Assert.IsTrue(PublishStatusQueryService.IsPublished(Textpage.Key, DefaultCulture));
            Assert.IsTrue(PublishStatusQueryService.IsPublished(Subpage.Key, DefaultCulture));
            Assert.IsTrue(PublishStatusQueryService.IsPublished(Subpage2.Key, DefaultCulture));
            Assert.IsTrue(PublishStatusQueryService.IsPublished(Subpage3.Key, DefaultCulture));

            Assert.IsFalse(PublishStatusQueryService.IsPublished(Trashed.Key, DefaultCulture));

            Assert.IsFalse(PublishStatusQueryService.IsPublished(Textpage.Key, UnusedCulture));
            Assert.IsFalse(PublishStatusQueryService.IsPublished(Subpage.Key, UnusedCulture));
            Assert.IsFalse(PublishStatusQueryService.IsPublished(Subpage2.Key, UnusedCulture));
            Assert.IsFalse(PublishStatusQueryService.IsPublished(Subpage3.Key, UnusedCulture));

            Assert.IsFalse(PublishStatusQueryService.IsPublished(Trashed.Key, UnusedCulture));

            Assert.IsTrue(PublishStatusQueryService.IsPublishedInAnyCulture(Textpage.Key));
            Assert.IsTrue(PublishStatusQueryService.IsPublishedInAnyCulture(Subpage.Key));
            Assert.IsTrue(PublishStatusQueryService.IsPublishedInAnyCulture(Subpage2.Key));
            Assert.IsTrue(PublishStatusQueryService.IsPublishedInAnyCulture(Subpage3.Key));

            Assert.IsTrue(PublishStatusQueryService.HasPublishedAncestorPath(Textpage.Key));
            Assert.IsTrue(PublishStatusQueryService.HasPublishedAncestorPath(Subpage.Key));
        });
    }

    [Test]
    public void Published_Document_With_UnPublished_Parent_Has_Unpublished_Path()
    {
        Assert.Multiple(() =>
        {
            Assert.IsFalse(PublishStatusQueryService.IsPublished(Textpage.Key, DefaultCulture));
            Assert.IsFalse(PublishStatusQueryService.IsPublished(Subpage.Key, DefaultCulture));
        });

        ContentService.PublishBranch(Textpage, PublishBranchFilter.IncludeUnpublished, ["*"]);

        Assert.Multiple(() =>
        {
            Assert.IsTrue(PublishStatusQueryService.IsPublished(Textpage.Key, DefaultCulture));
            Assert.IsTrue(PublishStatusQueryService.IsPublished(Subpage.Key, DefaultCulture));
        });

        ContentService.Unpublish(Textpage);

        // Unpublish the root item - the sub page will still be published but it won't have a published path.
        Assert.Multiple(() =>
        {
            Assert.IsFalse(PublishStatusQueryService.IsPublished(Textpage.Key, DefaultCulture));
            Assert.IsTrue(PublishStatusQueryService.IsPublished(Subpage.Key, DefaultCulture));

            Assert.IsFalse(PublishStatusQueryService.HasPublishedAncestorPath(Subpage.Key));
        });
    }

    [TestCase("en-US")]
    [TestCase("da-DK")]
    public async Task Unpublished_Document_Culture_Yields_Correct_Published_Ancestor_Path(string cultureToUnpublish)
    {
        await GetRequiredService<ILanguageService>()
            .CreateAsync(new Language("da-DK", "Danish"), Constants.Security.SuperUserKey);

        var contentTypeKey = Guid.NewGuid();
        var contentType = new ContentTypeBuilder()
            .WithKey(contentTypeKey)
            .WithAlias("variant")
            .WithContentVariation(ContentVariation.Culture)
            .WithAllowAsRoot(true)
            .Build();
        await ContentTypeService.CreateAsync(contentType, Constants.Security.SuperUserKey);
        contentType.AllowedContentTypes = [new ContentTypeSort(contentTypeKey, 1, "variant")];
        await ContentTypeService.UpdateAsync(contentType, Constants.Security.SuperUserKey);

        IContent root = new ContentBuilder()
            .WithContentType(contentType)
            .WithCultureName("en-US", "Root EN")
            .WithCultureName("da-DK", "Root DA")
            .Build();
        ContentService.Save(root);

        IContent child = new ContentBuilder()
            .WithContentType(contentType)
            .WithCultureName("en-US", "Child EN")
            .WithCultureName("da-DK", "Child DA")
            .WithParent(root)
            .Build();
        ContentService.Save(child);

        IContent grandchild = new ContentBuilder()
            .WithContentType(contentType)
            .WithCultureName("en-US", "Grandchild EN")
            .WithCultureName("da-DK", "Grandchild DA")
            .WithParent(child)
            .Build();
        ContentService.Save(grandchild);

        ContentService.PublishBranch(root, PublishBranchFilter.IncludeUnpublished, ["en-US", "da-DK"]);

        // must refresh the child instance before unpublishing it, to reflect the state changes from the branch publish above
        child = ContentService.GetById(child.Key)!;
        ContentService.Unpublish(child, cultureToUnpublish);

        var publishedCulture = cultureToUnpublish is "en-US" ? "da-DK" : "en-US";
        Assert.IsTrue(PublishStatusQueryService.HasPublishedAncestorPath(grandchild.Key, publishedCulture));
        Assert.IsFalse(PublishStatusQueryService.HasPublishedAncestorPath(grandchild.Key, cultureToUnpublish));
        Assert.IsTrue(PublishStatusQueryService.HasPublishedAncestorPath(grandchild.Key, Constants.System.InvariantCulture));
    }
}
