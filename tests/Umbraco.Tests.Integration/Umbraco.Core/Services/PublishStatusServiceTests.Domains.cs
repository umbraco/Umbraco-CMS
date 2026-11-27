using NUnit.Framework;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.ContentEditing;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Tests.Common.Builders;
using Umbraco.Cms.Tests.Common.Builders.Extensions;

namespace Umbraco.Cms.Tests.Integration.Umbraco.Core.Services;

internal sealed partial class PublishStatusServiceTests
{
    private const string SecondCulture = "da-DK";

    private IDomainService DomainService => GetRequiredService<IDomainService>();

    [Test]
    public async Task Culture_Domain_Below_Ancestor_Unpublished_In_Culture_Yields_Published_Ancestor_Path()
    {
        (IContent root, IContent branch, IContent child) = await CreateBranchUnderRootPublishedOnlyInDefaultCulture();
        await AssignDomains(root, new DomainModel { DomainName = "test.local", IsoCode = DefaultCulture });
        await AssignDomains(
            branch,
            new DomainModel { DomainName = "test.local/branch", IsoCode = DefaultCulture },
            new DomainModel { DomainName = "test.local/branch/da", IsoCode = SecondCulture });

        Assert.Multiple(() =>
        {
            Assert.IsTrue(PublishStatusQueryService.HasPublishedAncestorPath(branch.Key, SecondCulture));
            Assert.IsTrue(PublishStatusQueryService.HasPublishedAncestorPath(child.Key, SecondCulture));
            Assert.IsTrue(PublishStatusQueryService.HasPublishedAncestorPath(branch.Key, DefaultCulture));
            Assert.IsTrue(PublishStatusQueryService.HasPublishedAncestorPath(child.Key, DefaultCulture));
        });
    }

    [Test]
    public async Task No_Culture_Domain_Below_Ancestor_Unpublished_In_Culture_Yields_Unpublished_Ancestor_Path()
    {
        (_, IContent branch, IContent child) = await CreateBranchUnderRootPublishedOnlyInDefaultCulture();

        Assert.Multiple(() =>
        {
            Assert.IsFalse(PublishStatusQueryService.HasPublishedAncestorPath(branch.Key, SecondCulture));
            Assert.IsFalse(PublishStatusQueryService.HasPublishedAncestorPath(child.Key, SecondCulture));
        });
    }

    [Test]
    public async Task Culture_Domain_On_Ancestor_Unpublished_In_Culture_Yields_Unpublished_Ancestor_Path()
    {
        (IContent root, IContent branch, IContent child) = await CreateBranchUnderRootPublishedOnlyInDefaultCulture();
        await AssignDomains(
            root,
            new DomainModel { DomainName = "test.local", IsoCode = DefaultCulture },
            new DomainModel { DomainName = "test.local/da", IsoCode = SecondCulture });

        Assert.Multiple(() =>
        {
            Assert.IsFalse(PublishStatusQueryService.HasPublishedAncestorPath(branch.Key, SecondCulture));
            Assert.IsFalse(PublishStatusQueryService.HasPublishedAncestorPath(child.Key, SecondCulture));
        });
    }

    [Test]
    public async Task Wildcard_Culture_Domain_Below_Ancestor_Unpublished_In_Culture_Yields_Unpublished_Ancestor_Path()
    {
        (_, IContent branch, IContent child) = await CreateBranchUnderRootPublishedOnlyInDefaultCulture();
        await AssignDomains(branch, SecondCulture);

        Assert.Multiple(() =>
        {
            Assert.IsFalse(PublishStatusQueryService.HasPublishedAncestorPath(branch.Key, SecondCulture));
            Assert.IsFalse(PublishStatusQueryService.HasPublishedAncestorPath(child.Key, SecondCulture));
        });
    }

    [Test]
    public async Task Culture_Domain_On_Ancestor_Unpublished_In_Culture_Is_Itself_Part_Of_The_Ancestor_Path()
    {
        (_, IContent branch, IContent child) = await CreateBranchUnderRootPublishedOnlyInDefaultCulture();
        await AssignDomains(branch, new DomainModel { DomainName = "test.local/branch/da", IsoCode = SecondCulture });

        branch = ContentService.GetById(branch.Key)!;
        ContentService.Unpublish(branch, SecondCulture);

        Assert.IsFalse(PublishStatusQueryService.HasPublishedAncestorPath(child.Key, SecondCulture));
    }

    [Test]
    public async Task Culture_Domain_On_Document_Outside_The_Content_Tree_Yields_Unpublished_Ancestor_Path()
    {
        (_, IContent branch, _) = await CreateBranchUnderRootPublishedOnlyInDefaultCulture();
        await AssignDomains(branch, new DomainModel { DomainName = "test.local/branch/da", IsoCode = SecondCulture });

        ContentService.MoveToRecycleBin(ContentService.GetById(branch.Key)!);

        Assert.IsFalse(PublishStatusQueryService.HasPublishedAncestorPath(branch.Key, SecondCulture));
    }

    [Test]
    public async Task Culture_Domain_Below_Ancestor_Unpublished_In_Culture_Does_Not_Affect_Ancestor_Path_In_Any_Culture()
    {
        (_, IContent branch, IContent child) = await CreateBranchUnderRootPublishedOnlyInDefaultCulture();
        await AssignDomains(branch, new DomainModel { DomainName = "test.local/branch/da", IsoCode = SecondCulture });

        Assert.Multiple(() =>
        {
            Assert.IsTrue(PublishStatusQueryService.HasPublishedAncestorPath(branch.Key));
            Assert.IsTrue(PublishStatusQueryService.HasPublishedAncestorPath(child.Key));
            Assert.IsTrue(PublishStatusQueryService.HasPublishedAncestorPath(child.Key, Constants.System.InvariantCulture));
        });
    }

    private async Task<(IContent Root, IContent Branch, IContent Child)> CreateBranchUnderRootPublishedOnlyInDefaultCulture()
    {
        await GetRequiredService<ILanguageService>()
            .CreateAsync(new Language(SecondCulture, "Danish"), Constants.Security.SuperUserKey);

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
            .WithCultureName(DefaultCulture, "Root EN")
            .Build();
        ContentService.Save(root);
        ContentService.Publish(root, [DefaultCulture]);

        IContent branch = new ContentBuilder()
            .WithContentType(contentType)
            .WithCultureName(DefaultCulture, "Branch EN")
            .WithCultureName(SecondCulture, "Branch DA")
            .WithParent(root)
            .Build();
        ContentService.Save(branch);
        ContentService.Publish(branch, [DefaultCulture, SecondCulture]);

        IContent child = new ContentBuilder()
            .WithContentType(contentType)
            .WithCultureName(DefaultCulture, "Child EN")
            .WithCultureName(SecondCulture, "Child DA")
            .WithParent(branch)
            .Build();
        ContentService.Save(child);
        ContentService.Publish(child, [DefaultCulture, SecondCulture]);

        Assert.Multiple(() =>
        {
            Assert.IsTrue(PublishStatusQueryService.IsDocumentPublished(root.Key, DefaultCulture));
            Assert.IsFalse(PublishStatusQueryService.IsDocumentPublished(root.Key, SecondCulture));
            Assert.IsTrue(PublishStatusQueryService.IsDocumentPublished(branch.Key, SecondCulture));
            Assert.IsTrue(PublishStatusQueryService.IsDocumentPublished(child.Key, SecondCulture));
        });

        return (root, branch, child);
    }

    private async Task AssignDomains(IContent content, params DomainModel[] domains)
        => await AssignDomains(content, null, domains);

    private async Task AssignDomains(IContent content, string? wildcardCulture, params DomainModel[] domains)
    {
        var result = await DomainService.UpdateDomainsAsync(
            content.Key,
            new DomainsUpdateModel { DefaultIsoCode = wildcardCulture, Domains = domains });
        Assert.IsTrue(result.Success, $"Could not assign domains: {result.Status}");
    }
}
