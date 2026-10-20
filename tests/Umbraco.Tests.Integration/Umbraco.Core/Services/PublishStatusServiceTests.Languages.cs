using NUnit.Framework;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.PublishedContent;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Services.Navigation;
using Umbraco.Cms.Tests.Common.Builders;
using Umbraco.Cms.Tests.Common.Builders.Extensions;

namespace Umbraco.Cms.Tests.Integration.Umbraco.Core.Services;

internal sealed partial class PublishStatusServiceTests
{
    private ILanguageService LanguageService => GetRequiredService<ILanguageService>();

    [Test]
    public async Task Published_Invariant_Content_Is_Published_In_Added_Language()
    {
        ContentService.PublishBranch(Textpage, PublishBranchFilter.IncludeUnpublished, ["*"]);
        Content unpublished = ContentBuilder.CreateSimpleContent(ContentType, "Unpublished", Textpage.Id);
        ContentService.Save(unpublished);

        Assert.IsFalse(PublishStatusQueryService.IsDocumentPublished(Textpage.Key, UnusedCulture));

        await LanguageService.CreateAsync(new Language(UnusedCulture, "Danish"), Constants.Security.SuperUserKey);

        Assert.Multiple(() =>
        {
            Assert.IsTrue(PublishStatusQueryService.IsDocumentPublished(Textpage.Key, UnusedCulture));
            Assert.IsTrue(PublishStatusQueryService.IsDocumentPublished(Subpage.Key, UnusedCulture));
            Assert.IsTrue(PublishStatusQueryService.HasPublishedAncestorPath(Subpage.Key, UnusedCulture));
            Assert.IsFalse(PublishStatusQueryService.IsDocumentPublished(unpublished.Key, UnusedCulture));
            Assert.IsFalse(PublishStatusQueryService.IsDocumentPublished(Trashed.Key, UnusedCulture));
        });

        IPublishedContent[] available = GetRequiredService<IPublishedContentStatusFilteringService>()
            .FilterAvailable([Subpage.Key], UnusedCulture)
            .ToArray();
        Assert.AreEqual(1, available.Length);
        Assert.AreEqual(Subpage.Key, available[0].Key);
    }

    [Test]
    public async Task Published_Invariant_Content_Is_Published_In_Language_With_Changed_IsoCode()
    {
        const string changedIsoCode = "da";

        await LanguageService.CreateAsync(new Language(UnusedCulture, "Danish"), Constants.Security.SuperUserKey);
        ContentService.PublishBranch(Textpage, PublishBranchFilter.IncludeUnpublished, ["*"]);

        ILanguage language = (await LanguageService.GetAsync(UnusedCulture))!;
        language.IsoCode = changedIsoCode;
        language.CultureName = "Danish";
        await LanguageService.UpdateAsync(language, Constants.Security.SuperUserKey);

        Assert.Multiple(() =>
        {
            Assert.IsTrue(PublishStatusQueryService.IsDocumentPublished(Textpage.Key, changedIsoCode));
            Assert.IsTrue(PublishStatusQueryService.IsDocumentPublished(Subpage.Key, changedIsoCode));
            Assert.IsTrue(PublishStatusQueryService.HasPublishedAncestorPath(Subpage.Key, changedIsoCode));
        });
    }

    [Test]
    public async Task Empty_Culture_Resolves_To_Changed_Default_Language()
    {
        await GetRequiredService<IPublishStatusManagementService>().InitializeAsync(CancellationToken.None);
        await LanguageService.CreateAsync(new Language(UnusedCulture, "Danish"), Constants.Security.SuperUserKey);

        IContentType contentType = new ContentTypeBuilder()
            .WithAlias("variant")
            .WithContentVariation(ContentVariation.Culture)
            .WithAllowAsRoot(true)
            .Build();
        await ContentTypeService.CreateAsync(contentType, Constants.Security.SuperUserKey);

        IContent document = new ContentBuilder()
            .WithContentType(contentType)
            .WithCultureName(DefaultCulture, "Document EN")
            .Build();
        ContentService.Save(document);
        ContentService.Publish(document, [DefaultCulture]);

        Assert.IsTrue(PublishStatusQueryService.IsDocumentPublished(document.Key, string.Empty));

        ILanguage language = (await LanguageService.GetAsync(UnusedCulture))!;
        language.IsDefault = true;
        await LanguageService.UpdateAsync(language, Constants.Security.SuperUserKey);

        Assert.Multiple(() =>
        {
            Assert.IsFalse(PublishStatusQueryService.IsDocumentPublished(document.Key, string.Empty));
            Assert.IsTrue(PublishStatusQueryService.IsDocumentPublished(document.Key, DefaultCulture));
        });
    }
}
