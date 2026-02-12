using NUnit.Framework;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Tests.Common.Builders;
using Umbraco.Cms.Tests.Common.Builders.Extensions;

namespace Umbraco.Cms.Tests.Integration.Umbraco.Core.Services;

internal sealed class PublishedUrlInfoProviderTests : PublishedUrlInfoProviderTestsBase
{
    private ILanguageService LanguageService => GetRequiredService<ILanguageService>();

    [Test]
    public async Task Invariant_content_returns_only_default_language_url()
    {
        // Arrange: Add a second language (Danish) alongside the default English
        var danishLanguage = new LanguageBuilder()
            .WithCultureInfo("da-DK")
            .WithCultureName("Danish")
            .Build();
        await LanguageService.CreateAsync(danishLanguage, Constants.Security.SuperUserKey);

        // The base class creates invariant content (ContentType doesn't vary by culture)
        // Publish the root content
        ContentService.PublishBranch(Textpage, PublishBranchFilter.IncludeUnpublished, ["*"]);

        // Act: Get all URLs for the invariant content
        var urls = await PublishedUrlInfoProvider.GetAllAsync(Textpage);

        // Assert: Invariant content should only return ONE URL (for the default language)
        // not multiple URLs for each configured language
        Assert.AreEqual(1, urls.Count, "Invariant content should only return one URL for the default language, not URLs for all configured languages");
        Assert.IsNotNull(urls.First().Url);
        Assert.AreEqual("en-US", urls.First().Culture, "The URL should be for the default language (en-US)");
    }

    [Test]
    public async Task Two_items_in_level_1_with_same_name_will_have_conflicting_routes()
    {
        // Create a second root
        var secondRoot = ContentBuilder.CreateSimpleContent(ContentType, "Second Root", null);
        var contentSchedule = ContentScheduleCollection.CreateWithEntry(DateTime.UtcNow.AddMinutes(-5), null);
        ContentService.Save(secondRoot, -1, contentSchedule);

        // Create a child of second root
        var childOfSecondRoot = ContentBuilder.CreateSimpleContent(ContentType, Subpage.Name, secondRoot);
        childOfSecondRoot.Key = new Guid("FF6654FB-BC68-4A65-8C6C-135567F50BD6");
        ContentService.Save(childOfSecondRoot, -1, contentSchedule);

        // Publish both the main root and the second root with descendants
        ContentService.PublishBranch(Textpage, PublishBranchFilter.IncludeUnpublished, ["*"]);
        ContentService.PublishBranch(secondRoot, PublishBranchFilter.IncludeUnpublished, ["*"]);

        var subPageUrls = await PublishedUrlInfoProvider.GetAllAsync(Subpage);
        var childOfSecondRootUrls = await PublishedUrlInfoProvider.GetAllAsync(childOfSecondRoot);

        // Assert the url of subpage is correct
        Assert.AreEqual(1, subPageUrls.Count);
        Assert.IsNotNull(subPageUrls.First().Url);
        Assert.AreEqual("/text-page-1/", subPageUrls.First().Url!.ToString());
        Assert.AreEqual(Constants.UrlProviders.Content, subPageUrls.First().Provider);
        Assert.AreEqual(Subpage.Key, DocumentUrlService.GetDocumentKeyByRoute("/text-page-1/", "en-US", null, false));

        // Assert the url of child of second root is not exposed
        Assert.AreEqual(1, childOfSecondRootUrls.Count);
        Assert.IsNull(childOfSecondRootUrls.First().Url);
        Assert.AreEqual(Constants.UrlProviders.Content, childOfSecondRootUrls.First().Provider);

        // Ensure the url without hide top level is not finding the child of second root
        Assert.AreNotEqual(childOfSecondRoot.Key, DocumentUrlService.GetDocumentKeyByRoute("/second-root/text-page-1/", "en-US", null, false));
    }
}
