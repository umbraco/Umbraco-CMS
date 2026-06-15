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
        Assert.That(urls, Has.Count.EqualTo(1), "Invariant content should only return one URL for the default language, not URLs for all configured languages");
        Assert.That(urls.First().Url, Is.Not.Null);
        Assert.That(urls.First().Culture, Is.EqualTo("en-US"), "The URL should be for the default language (en-US)");
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
        Assert.That(updateDomainResult.Success, Is.True, "Domain assignment should succeed");

        // Act: Get all URLs for a child of the root with the da-DK domain
        var urls = await PublishedUrlInfoProvider.GetAllAsync(Subpage);

        // Assert: Should contain only the da-DK domain URL, not the default culture fallback
        Assert.That(urls, Has.Count.EqualTo(1), "Should return exactly one URL (the domain-based URL)");
        Assert.That(urls.First().Url, Is.Not.Null);
        Assert.That(urls.First().Culture, Is.EqualTo("da-DK"), "The URL should be for the domain culture (da-DK)");
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

        Assert.That(subPageUrls, Has.Count.EqualTo(1));
        Assert.That(subPageUrls.First().Url, Is.Not.Null);
        Assert.That(subPageUrls.First().Url!.ToString(), Is.EqualTo("/textpage/text-page-1/"));
        Assert.That(subPageUrls.First().Provider, Is.EqualTo(Constants.UrlProviders.Content));

        Assert.That(childOfSecondRootUrls, Has.Count.EqualTo(1));
        Assert.That(childOfSecondRootUrls.First().Url, Is.Not.Null);
        Assert.That(childOfSecondRootUrls.First().Url!.ToString(), Is.EqualTo("/second-root/text-page-1/"));
        Assert.That(childOfSecondRootUrls.First().Provider, Is.EqualTo(Constants.UrlProviders.Content));
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
        Assert.That(subPageUrls, Has.Count.EqualTo(1));
        Assert.That(subPageUrls.First().Url, Is.Not.Null);
        Assert.That(subPageUrls.First().Url!.ToString(), Is.EqualTo("/text-page-1/"));
        Assert.That(subPageUrls.First().Provider, Is.EqualTo(Constants.UrlProviders.Content));
        Assert.That(DocumentUrlService.GetDocumentKeyByRoute("/text-page-1/", "en-US", null, false), Is.EqualTo(Subpage.Key));

        // Assert the url of child of second root is not exposed
        Assert.That(childOfSecondRootUrls, Has.Count.EqualTo(1));
        Assert.That(childOfSecondRootUrls.First().Url, Is.Null);
        Assert.That(childOfSecondRootUrls.First().Provider, Is.EqualTo(Constants.UrlProviders.Content));

        // Ensure the url without hide top level is not finding the child of second root
        Assert.That(DocumentUrlService.GetDocumentKeyByRoute("/second-root/text-page-1/", "en-US", null, false), Is.Not.EqualTo(childOfSecondRoot.Key));
    }
}
