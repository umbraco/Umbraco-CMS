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
            Assert.That(PublishStatusQueryService.IsPublished(Textpage.Key, DefaultCulture), Is.False);
            Assert.That(PublishStatusQueryService.IsPublished(Subpage.Key, DefaultCulture), Is.False);
            Assert.That(PublishStatusQueryService.IsPublished(Subpage2.Key, DefaultCulture), Is.False);
            Assert.That(PublishStatusQueryService.IsPublished(Subpage3.Key, DefaultCulture), Is.False);

            Assert.That(PublishStatusQueryService.IsPublished(Trashed.Key, DefaultCulture), Is.False);

            Assert.That(PublishStatusQueryService.IsPublished(Textpage.Key, UnusedCulture), Is.False);
            Assert.That(PublishStatusQueryService.IsPublished(Subpage.Key, UnusedCulture), Is.False);
            Assert.That(PublishStatusQueryService.IsPublished(Subpage2.Key, UnusedCulture), Is.False);
            Assert.That(PublishStatusQueryService.IsPublished(Subpage3.Key, UnusedCulture), Is.False);

            Assert.That(PublishStatusQueryService.IsPublished(Trashed.Key, UnusedCulture), Is.False);

            Assert.That(PublishStatusQueryService.IsPublishedInAnyCulture(Textpage.Key), Is.False);
            Assert.That(PublishStatusQueryService.IsPublishedInAnyCulture(Subpage.Key), Is.False);
            Assert.That(PublishStatusQueryService.IsPublishedInAnyCulture(Subpage2.Key), Is.False);
            Assert.That(PublishStatusQueryService.IsPublishedInAnyCulture(Subpage3.Key), Is.False);

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
            Assert.That(publishResults.All(x => x.Result == PublishResultType.SuccessPublish), Is.True);
            Assert.That(publishResult.Success, Is.True);
            Assert.That(PublishStatusQueryService.IsPublished(Textpage.Key, DefaultCulture), Is.True);
            Assert.That(PublishStatusQueryService.IsPublished(Subpage2.Key, DefaultCulture), Is.False);
            Assert.That(PublishStatusQueryService.IsPublished(grandchild.Key, DefaultCulture), Is.True); // grandchild is still published, but it will not be routable

            Assert.That(PublishStatusQueryService.IsPublished(Textpage.Key, UnusedCulture), Is.False);
            Assert.That(PublishStatusQueryService.IsPublished(Subpage2.Key, UnusedCulture), Is.False);
            Assert.That(PublishStatusQueryService.IsPublished(grandchild.Key, UnusedCulture), Is.False);

            Assert.That(PublishStatusQueryService.IsPublishedInAnyCulture(Textpage.Key), Is.True);
            Assert.That(PublishStatusQueryService.IsPublishedInAnyCulture(Subpage2.Key), Is.False);
            Assert.That(PublishStatusQueryService.IsPublishedInAnyCulture(grandchild.Key), Is.True);
        });
    }

    [Test]
    public void Publish_Branch_Updates_Document_Path_Published_Status()
    {
        var publishResults = ContentService.PublishBranch(Textpage, PublishBranchFilter.IncludeUnpublished, ["*"]);
        Assert.Multiple(() =>
        {
            Assert.That(publishResults.All(x => x.Result == PublishResultType.SuccessPublish), Is.True);
            Assert.That(PublishStatusQueryService.IsPublished(Textpage.Key, DefaultCulture), Is.True);
            Assert.That(PublishStatusQueryService.IsPublished(Subpage.Key, DefaultCulture), Is.True);
            Assert.That(PublishStatusQueryService.IsPublished(Subpage2.Key, DefaultCulture), Is.True);
            Assert.That(PublishStatusQueryService.IsPublished(Subpage3.Key, DefaultCulture), Is.True);

            Assert.That(PublishStatusQueryService.IsPublished(Trashed.Key, DefaultCulture), Is.False);

            Assert.That(PublishStatusQueryService.IsPublished(Textpage.Key, UnusedCulture), Is.False);
            Assert.That(PublishStatusQueryService.IsPublished(Subpage.Key, UnusedCulture), Is.False);
            Assert.That(PublishStatusQueryService.IsPublished(Subpage2.Key, UnusedCulture), Is.False);
            Assert.That(PublishStatusQueryService.IsPublished(Subpage3.Key, UnusedCulture), Is.False);

            Assert.That(PublishStatusQueryService.IsPublished(Trashed.Key, UnusedCulture), Is.False);

            Assert.That(PublishStatusQueryService.IsPublishedInAnyCulture(Textpage.Key), Is.True);
            Assert.That(PublishStatusQueryService.IsPublishedInAnyCulture(Subpage.Key), Is.True);
            Assert.That(PublishStatusQueryService.IsPublishedInAnyCulture(Subpage2.Key), Is.True);
            Assert.That(PublishStatusQueryService.IsPublishedInAnyCulture(Subpage3.Key), Is.True);

            Assert.That(PublishStatusQueryService.HasPublishedAncestorPath(Textpage.Key), Is.True);
            Assert.That(PublishStatusQueryService.HasPublishedAncestorPath(Subpage.Key), Is.True);
        });
    }

    [Test]
    public void Published_Document_With_UnPublished_Parent_Has_Unpublished_Path()
    {
        Assert.Multiple(() =>
        {
            Assert.That(PublishStatusQueryService.IsPublished(Textpage.Key, DefaultCulture), Is.False);
            Assert.That(PublishStatusQueryService.IsPublished(Subpage.Key, DefaultCulture), Is.False);
        });

        ContentService.PublishBranch(Textpage, PublishBranchFilter.IncludeUnpublished, ["*"]);

        Assert.Multiple(() =>
        {
            Assert.That(PublishStatusQueryService.IsPublished(Textpage.Key, DefaultCulture), Is.True);
            Assert.That(PublishStatusQueryService.IsPublished(Subpage.Key, DefaultCulture), Is.True);
        });

        ContentService.Unpublish(Textpage);

        // Unpublish the root item - the sub page will still be published but it won't have a published path.
        Assert.Multiple(() =>
        {
            Assert.That(PublishStatusQueryService.IsPublished(Textpage.Key, DefaultCulture), Is.False);
            Assert.That(PublishStatusQueryService.IsPublished(Subpage.Key, DefaultCulture), Is.True);

            Assert.That(PublishStatusQueryService.HasPublishedAncestorPath(Subpage.Key), Is.False);
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
        Assert.That(PublishStatusQueryService.HasPublishedAncestorPath(grandchild.Key, publishedCulture), Is.True);
        Assert.That(PublishStatusQueryService.HasPublishedAncestorPath(grandchild.Key, cultureToUnpublish), Is.False);
        Assert.That(PublishStatusQueryService.HasPublishedAncestorPath(grandchild.Key, Constants.System.InvariantCulture), Is.True);
    }

    [TestCase(true)]
    [TestCase(false)]
    public async Task Fully_Unpublished_Culture_Variant_Document_Tracks_Unpublished_State_For_All_Cultures(bool unpublishAllCulturesAtOnce)
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

        if (unpublishAllCulturesAtOnce)
        {
            ContentService.Unpublish(child);
        }
        else
        {
            ContentService.Unpublish(child, "en-US");

            // refresh again before unpublishing the last culture
            child = ContentService.GetById(child.Key)!;
            ContentService.Unpublish(child, "da-DK");
        }

        // refresh to get the latest state
        child = ContentService.GetById(child.Key)!;
        Assert.That(child.Published, Is.False);
        Assert.That(child.PublishedCultures, Is.Empty);
        Assert.That(child.PublishCultureInfos!, Is.Empty);

        Assert.That(PublishStatusQueryService.IsDocumentPublished(child.Key, "en-US"), Is.False);
        Assert.That(PublishStatusQueryService.IsDocumentPublished(child.Key, "da-DK"), Is.False);
        Assert.That(PublishStatusQueryService.IsDocumentPublished(child.Key, Constants.System.InvariantCulture), Is.False);

        Assert.That(PublishStatusQueryService.HasPublishedAncestorPath(grandchild.Key, "da-DK"), Is.False);
        Assert.That(PublishStatusQueryService.HasPublishedAncestorPath(grandchild.Key, "en-US"), Is.False);
        Assert.That(PublishStatusQueryService.HasPublishedAncestorPath(grandchild.Key, Constants.System.InvariantCulture), Is.False);
    }
}
