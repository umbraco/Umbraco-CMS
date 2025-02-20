using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;
using Umbraco.Cms.Core.Configuration.Models;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Tests.Common.Builders;

namespace Umbraco.Cms.Tests.Integration.Umbraco.Core.Services;

public class PublishedUrlInfoProvider_hidetoplevel_false : PublishedUrlInfoProviderTestsBase
{
    protected override void CustomTestSetup(IUmbracoBuilder builder)
    {
        builder.Services.Configure<GlobalSettings>(x => x.HideTopLevelNodeFromPath = false);
        base.CustomTestSetup(builder);
    }

    [Test]
    public async Task Two_items_in_level_1_with_same_name_will_not_have_conflicting_routes()
    {
        // Create a second root
        var secondRoot = ContentBuilder.CreateSimpleContent(ContentType, "Second Root", null);
        var contentSchedule = ContentScheduleCollection.CreateWithEntry(DateTime.Now.AddMinutes(-5), null);
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
        Assert.IsTrue(subPageUrls.First().IsUrl);
        Assert.AreEqual("/textpage/text-page-1/", subPageUrls.First().Text);

        Assert.AreEqual(1, childOfSecondRootUrls.Count);
        Assert.IsTrue(childOfSecondRootUrls.First().IsUrl);
        Assert.AreEqual("/second-root/text-page-1/", childOfSecondRootUrls.First().Text);
    }
}
