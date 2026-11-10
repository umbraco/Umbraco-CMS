using NUnit.Framework;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.ContentEditing;
using Umbraco.Cms.Core.Models.ContentPublishing;
using Umbraco.Cms.Core.Models.PublishedContent;
using Umbraco.Cms.Core.Persistence.Repositories;
using Umbraco.Cms.Core.Scoping;
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

        Assert.IsFalse(PublishStatusQueryService.IsPublished(Textpage.Key, UnusedCulture));

        await LanguageService.CreateAsync(new Language(UnusedCulture, "Danish"), Constants.Security.SuperUserKey);

        Assert.Multiple(() =>
        {
            Assert.IsTrue(PublishStatusQueryService.IsPublished(Textpage.Key, UnusedCulture));
            Assert.IsTrue(PublishStatusQueryService.IsPublished(Subpage.Key, UnusedCulture));
            Assert.IsTrue(PublishStatusQueryService.HasPublishedAncestorPath(Subpage.Key, UnusedCulture));
            Assert.IsFalse(PublishStatusQueryService.IsPublished(unpublished.Key, UnusedCulture));
            Assert.IsFalse(PublishStatusQueryService.IsPublished(Trashed.Key, UnusedCulture));
        });

        IPublishedContent[] available = GetRequiredService<IPublishedContentStatusFilteringService>()
            .FilterAvailable([Subpage.Key], UnusedCulture)
            .ToArray();
        Assert.AreEqual(1, available.Length);
        Assert.AreEqual(Subpage.Key, available[0].Key);
    }

    [Test]
    public async Task Published_Invariant_Element_Is_Published_In_Added_Language()
    {
        IContentType elementType = ContentTypeBuilder.CreateBasicElementType();
        elementType.AllowedAsRoot = true;
        await ContentTypeService.CreateAsync(elementType, Constants.Security.SuperUserKey);

        var createResult = await GetRequiredService<IElementEditingService>().CreateAsync(
            new ElementCreateModel
            {
                ContentTypeKey = elementType.Key,
                Variants = [new() { Name = "Element" }],
            },
            Constants.Security.SuperUserKey);
        Assert.IsTrue(createResult.Success);
        Guid elementKey = createResult.Result.Content!.Key;

        var publishResult = await GetRequiredService<IElementPublishingService>().PublishAsync(
            elementKey,
            [new CulturePublishScheduleModel { Culture = "*", Schedule = null }],
            Constants.Security.SuperUserKey);
        Assert.IsTrue(publishResult.Success);

        await GetRequiredService<IElementPublishStatusManagementService>().InitializeAsync(CancellationToken.None);
        IElementPublishStatusQueryService elementPublishStatusQueryService = GetRequiredService<IElementPublishStatusQueryService>();
        Assert.IsFalse(elementPublishStatusQueryService.IsPublished(elementKey, UnusedCulture));

        await LanguageService.CreateAsync(new Language(UnusedCulture, "Danish"), Constants.Security.SuperUserKey);

        Assert.IsTrue(elementPublishStatusQueryService.IsPublished(elementKey, UnusedCulture));
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
            Assert.IsTrue(PublishStatusQueryService.IsPublished(Textpage.Key, changedIsoCode));
            Assert.IsTrue(PublishStatusQueryService.IsPublished(Subpage.Key, changedIsoCode));
            Assert.IsTrue(PublishStatusQueryService.HasPublishedAncestorPath(Subpage.Key, changedIsoCode));
        });
    }

    [Test]
    public async Task Empty_Culture_Resolves_To_Changed_Default_Language()
    {
        await GetRequiredService<IDocumentPublishStatusManagementService>().InitializeAsync(CancellationToken.None);
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

        Assert.IsTrue(PublishStatusQueryService.IsPublished(document.Key, string.Empty));

        ILanguage language = (await LanguageService.GetAsync(UnusedCulture))!;
        language.IsDefault = true;
        await LanguageService.UpdateAsync(language, Constants.Security.SuperUserKey);

        Assert.Multiple(() =>
        {
            Assert.IsFalse(PublishStatusQueryService.IsPublished(document.Key, string.Empty));
            Assert.IsTrue(PublishStatusQueryService.IsPublished(document.Key, DefaultCulture));
        });
    }

    [Test]
    public async Task Content_Is_Not_Published_In_A_Deleted_Language_After_It_Is_Added_Again()
    {
        await LanguageService.CreateAsync(new Language(UnusedCulture, "Danish"), Constants.Security.SuperUserKey);

        IContentType variantContentType = new ContentTypeBuilder()
            .WithAlias("variant")
            .WithContentVariation(ContentVariation.Culture)
            .WithAllowAsRoot(true)
            .Build();
        await ContentTypeService.CreateAsync(variantContentType, Constants.Security.SuperUserKey);

        IContent publishedInBothCultures = new ContentBuilder()
            .WithContentType(variantContentType)
            .WithCultureName(DefaultCulture, "Both EN")
            .WithCultureName(UnusedCulture, "Both DA")
            .Build();
        ContentService.Save(publishedInBothCultures);
        ContentService.Publish(publishedInBothCultures, [DefaultCulture, UnusedCulture]);

        IContent publishedInDeletedCultureOnly = new ContentBuilder()
            .WithContentType(variantContentType)
            .WithCultureName(DefaultCulture, "Only EN")
            .WithCultureName(UnusedCulture, "Only DA")
            .Build();
        ContentService.Save(publishedInDeletedCultureOnly);
        ContentService.Publish(publishedInDeletedCultureOnly, [UnusedCulture]);

        ContentService.Publish(Textpage, ["*"]);

        await LanguageService.DeleteAsync(UnusedCulture, Constants.Security.SuperUserKey);

        Assert.Multiple(() =>
        {
            Assert.IsFalse(PublishStatusQueryService.IsPublished(publishedInBothCultures.Key, UnusedCulture));
            Assert.IsTrue(PublishStatusQueryService.IsPublished(publishedInBothCultures.Key, DefaultCulture));
            Assert.IsFalse(PublishStatusQueryService.IsPublishedInAnyCulture(publishedInDeletedCultureOnly.Key));
            Assert.IsFalse(PublishStatusQueryService.IsPublished(Textpage.Key, UnusedCulture));
        });

        await LanguageService.CreateAsync(new Language(UnusedCulture, "Danish"), Constants.Security.SuperUserKey);

        Assert.Multiple(() =>
        {
            Assert.IsFalse(PublishStatusQueryService.IsPublished(publishedInBothCultures.Key, UnusedCulture));
            Assert.IsTrue(PublishStatusQueryService.IsPublished(publishedInBothCultures.Key, DefaultCulture));
            Assert.IsFalse(PublishStatusQueryService.IsPublished(publishedInDeletedCultureOnly.Key, UnusedCulture));
            Assert.IsTrue(PublishStatusQueryService.IsPublished(Textpage.Key, UnusedCulture));
        });
    }

    [Test]
    public async Task Content_Is_Not_Published_In_An_Added_Language_Reusing_A_Changed_IsoCode()
    {
        const string changedIsoCode = "da";

        await LanguageService.CreateAsync(new Language(UnusedCulture, "Danish"), Constants.Security.SuperUserKey);

        IContentType variantContentType = new ContentTypeBuilder()
            .WithAlias("variant")
            .WithContentVariation(ContentVariation.Culture)
            .WithAllowAsRoot(true)
            .Build();
        await ContentTypeService.CreateAsync(variantContentType, Constants.Security.SuperUserKey);

        IContent document = new ContentBuilder()
            .WithContentType(variantContentType)
            .WithCultureName(DefaultCulture, "Document EN")
            .WithCultureName(UnusedCulture, "Document DA")
            .Build();
        ContentService.Save(document);
        ContentService.Publish(document, [DefaultCulture, UnusedCulture]);

        ILanguage language = (await LanguageService.GetAsync(UnusedCulture))!;
        language.IsoCode = changedIsoCode;
        language.CultureName = "Danish";
        await LanguageService.UpdateAsync(language, Constants.Security.SuperUserKey);

        await LanguageService.CreateAsync(new Language(UnusedCulture, "Danish (Denmark)"), Constants.Security.SuperUserKey);

        Assert.Multiple(() =>
        {
            Assert.IsTrue(PublishStatusQueryService.IsPublished(document.Key, changedIsoCode));
            Assert.IsFalse(PublishStatusQueryService.IsPublished(document.Key, UnusedCulture));
        });
    }

    [Test]
    public async Task GetPublishedKeysAsync_Returns_Documents_Published_In_The_Culture()
    {
        await LanguageService.CreateAsync(new Language(UnusedCulture, "Danish"), Constants.Security.SuperUserKey);

        IContentType variantContentType = new ContentTypeBuilder()
            .WithAlias("variant")
            .WithContentVariation(ContentVariation.Culture)
            .WithAllowAsRoot(true)
            .Build();
        await ContentTypeService.CreateAsync(variantContentType, Constants.Security.SuperUserKey);

        IContent publishedInCulture = new ContentBuilder()
            .WithContentType(variantContentType)
            .WithCultureName(DefaultCulture, "Published EN")
            .WithCultureName(UnusedCulture, "Published DA")
            .Build();
        ContentService.Save(publishedInCulture);
        ContentService.Publish(publishedInCulture, [UnusedCulture]);

        IContent publishedInOtherCulture = new ContentBuilder()
            .WithContentType(variantContentType)
            .WithCultureName(DefaultCulture, "Other EN")
            .WithCultureName(UnusedCulture, "Other DA")
            .Build();
        ContentService.Save(publishedInOtherCulture);
        ContentService.Publish(publishedInOtherCulture, [DefaultCulture]);

        // Publishes the invariant Textpage only; its children stay unpublished.
        ContentService.Publish(Textpage, ["*"]);

        IEnumerable<Guid> publishedKeys;
        using (ICoreScope scope = GetRequiredService<ICoreScopeProvider>().CreateCoreScope())
        {
            publishedKeys = await GetRequiredService<IPublishStatusRepository>().GetPublishedKeysAsync(UnusedCulture, CancellationToken.None);
            scope.Complete();
        }

        CollectionAssert.AreEquivalent(new[] { Textpage.Key, publishedInCulture.Key }, publishedKeys);
    }
}
