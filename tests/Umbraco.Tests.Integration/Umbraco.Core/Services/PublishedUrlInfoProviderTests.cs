using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Configuration.Models;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.ContentEditing;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Tests.Common.Builders;
using Umbraco.Cms.Tests.Common.Builders.Extensions;
using Umbraco.Cms.Tests.Integration.Attributes;

namespace Umbraco.Cms.Tests.Integration.Umbraco.Core.Services;

internal sealed class PublishedUrlInfoProviderTests : PublishedUrlInfoProviderTestsBase
{
    private ILanguageService LanguageService => GetRequiredService<ILanguageService>();

    private IDomainService DomainService => GetRequiredService<IDomainService>();

    public static void ConfigureHideTopLevelNodeFalse(IUmbracoBuilder builder)
        => builder.Services.Configure<GlobalSettings>(x => x.HideTopLevelNodeFromPath = false);

    [Test]
    public async Task Invariant_Content_Without_Domain_Returns_Only_Default_Language_Url()
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
    public async Task Invariant_Content_Under_Non_Default_Language_Domain_Returns_Only_Domain_Url()
    {
        // Arrange: Add a second language (Danish) alongside the default English
        var danishLanguage = new LanguageBuilder()
            .WithCultureInfo("da-DK")
            .WithCultureName("Danish")
            .Build();
        await LanguageService.CreateAsync(danishLanguage, Constants.Security.SuperUserKey);

        // Publish the branch (invariant content from base class)
        ContentService.PublishBranch(Textpage, PublishBranchFilter.IncludeUnpublished, ["*"]);

        // Assign a domain with the non-default culture (da-DK) to the root node
        var updateDomainResult = await DomainService.UpdateDomainsAsync(
            Textpage.Key,
            new DomainsUpdateModel
            {
                Domains = [new DomainModel { DomainName = "test.dk", IsoCode = "da-DK" }],
            });
        Assert.IsTrue(updateDomainResult.Success, "Domain assignment should succeed");

        // Act: Get all URLs for a child of the root with the da-DK domain
        var urls = await PublishedUrlInfoProvider.GetAllAsync(Subpage);

        // Assert: Should contain only the da-DK domain URL, not the default culture fallback
        Assert.AreEqual(1, urls.Count, "Should return exactly one URL (the domain-based URL)");
        Assert.IsNotNull(urls.First().Url);
        Assert.AreEqual("da-DK", urls.First().Culture, "The URL should be for the domain culture (da-DK)");
        Assert.That(urls.First().Url!.Host, Is.EqualTo("test.dk"), "The URL should use the assigned domain");
    }

    [Test]
    [ConfigureBuilder(ActionName = nameof(ConfigureHideTopLevelNodeFalse))]
    public async Task Two_Items_In_Level_1_With_Same_Name_Will_Not_Have_Conflicting_Routes_When_HideTopLevel_False()
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

        Assert.AreEqual(1, subPageUrls.Count);
        Assert.IsNotNull(subPageUrls.First().Url);
        Assert.AreEqual("/textpage/text-page-1/", subPageUrls.First().Url!.ToString());
        Assert.AreEqual(Constants.UrlProviders.Content, subPageUrls.First().Provider);

        Assert.AreEqual(1, childOfSecondRootUrls.Count);
        Assert.IsNotNull(childOfSecondRootUrls.First().Url);
        Assert.AreEqual("/second-root/text-page-1/", childOfSecondRootUrls.First().Url!.ToString());
        Assert.AreEqual(Constants.UrlProviders.Content, childOfSecondRootUrls.First().Provider);
    }

    [Test]
    public async Task Two_Items_In_Level_1_With_Same_Name_Will_Have_Conflicting_Routes()
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
