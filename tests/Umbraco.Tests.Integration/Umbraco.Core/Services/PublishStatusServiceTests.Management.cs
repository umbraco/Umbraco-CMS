using Microsoft.Extensions.Logging;
using NUnit.Framework;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Persistence.Repositories;
using Umbraco.Cms.Core.Scoping;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Services.Navigation;

namespace Umbraco.Cms.Tests.Integration.Umbraco.Core.Services;

internal sealed partial class PublishStatusServiceTests
{
    [Test]
    public async Task InitializeAsync_Loads_From_Database()
    {
        var sut = CreatePublishedStatusService();

        Assert.Multiple(() =>
        {
            Assert.IsFalse(sut.IsDocumentPublished(Textpage.Key, DefaultCulture));
            Assert.IsFalse(sut.IsDocumentPublished(Subpage2.Key, DefaultCulture));
            Assert.IsFalse(sut.IsDocumentPublished(Subpage.Key, DefaultCulture));
            Assert.IsFalse(sut.IsDocumentPublished(Subpage2.Key, DefaultCulture));
            Assert.IsFalse(sut.IsDocumentPublished(Subpage3.Key, DefaultCulture));

            Assert.IsFalse(sut.IsDocumentPublished(Trashed.Key, DefaultCulture));

            Assert.IsFalse(sut.IsDocumentPublished(Textpage.Key, UnusedCulture));
            Assert.IsFalse(sut.IsDocumentPublished(Subpage2.Key, UnusedCulture));
            Assert.IsFalse(sut.IsDocumentPublished(Subpage.Key, UnusedCulture));
            Assert.IsFalse(sut.IsDocumentPublished(Subpage2.Key, UnusedCulture));
            Assert.IsFalse(sut.IsDocumentPublished(Subpage3.Key, UnusedCulture));

            Assert.IsFalse(sut.IsDocumentPublished(Trashed.Key, UnusedCulture));
        });

        // Act
        var publishResults = ContentService.PublishBranch(Textpage, PublishBranchFilter.IncludeUnpublished, ["*"]);
        await sut.InitializeAsync(CancellationToken.None);

        Assert.Multiple(() =>
        {
            Assert.IsTrue(publishResults.All(x => x.Result == PublishResultType.SuccessPublish));
            Assert.IsTrue(sut.IsDocumentPublished(Textpage.Key, DefaultCulture));
            Assert.IsTrue(sut.IsDocumentPublished(Subpage2.Key, DefaultCulture));
            Assert.IsTrue(sut.IsDocumentPublished(Subpage.Key, DefaultCulture));
            Assert.IsTrue(sut.IsDocumentPublished(Subpage2.Key, DefaultCulture));
            Assert.IsTrue(sut.IsDocumentPublished(Subpage3.Key, DefaultCulture));

            Assert.IsFalse(sut.IsDocumentPublished(Trashed.Key, DefaultCulture));

            Assert.IsFalse(sut.IsDocumentPublished(Textpage.Key, UnusedCulture));
            Assert.IsFalse(sut.IsDocumentPublished(Subpage2.Key, UnusedCulture));
            Assert.IsFalse(sut.IsDocumentPublished(Subpage.Key, UnusedCulture));
            Assert.IsFalse(sut.IsDocumentPublished(Subpage2.Key, UnusedCulture));
            Assert.IsFalse(sut.IsDocumentPublished(Subpage3.Key, UnusedCulture));

            Assert.IsFalse(sut.IsDocumentPublished(Trashed.Key, UnusedCulture));
        });
    }

    [Test]
    public async Task AddOrUpdateStatusWithDescendantsAsync_Updates_Document_Path_Published_Status()
    {
        var sut = new PublishStatusService(
            GetRequiredService<ILogger<PublishStatusService>>(),
            GetRequiredService<IPublishStatusRepository>(),
            GetRequiredService<ICoreScopeProvider>(),
            GetRequiredService<ILanguageService>(),
            GetRequiredService<IDocumentNavigationQueryService>());

        Assert.IsFalse(sut.IsDocumentPublished(Textpage.Key, DefaultCulture));

        // Act
        var publishResults = ContentService.PublishBranch(Textpage, PublishBranchFilter.IncludeUnpublished, ["*"]);
        await sut.AddOrUpdateStatusWithDescendantsAsync(Textpage.Key, CancellationToken.None);

        Assert.IsTrue(sut.IsDocumentPublished(Textpage.Key, DefaultCulture));
        Assert.IsTrue(sut.IsDocumentPublished(Subpage.Key, DefaultCulture)); // Updated due to being an descendant
        Assert.IsFalse(sut.IsDocumentPublished(Textpage.Key, UnusedCulture)); // Do not exist
        Assert.IsFalse(sut.IsDocumentPublished(Subpage.Key, UnusedCulture)); // Do not exist
    }

    [Test]
    public async Task AddOrUpdateStatusAsync_Updates_Document_Published_Status()
    {
        var sut = new PublishStatusService(
            GetRequiredService<ILogger<PublishStatusService>>(),
            GetRequiredService<IPublishStatusRepository>(),
            GetRequiredService<ICoreScopeProvider>(),
            GetRequiredService<ILanguageService>(),
            GetRequiredService<IDocumentNavigationQueryService>());

        Assert.IsFalse(sut.IsDocumentPublished(Textpage.Key, DefaultCulture));

        // Act
        var publishResults = ContentService.PublishBranch(Textpage, PublishBranchFilter.IncludeUnpublished, ["*"]);
        await sut.AddOrUpdateStatusAsync(Textpage.Key, CancellationToken.None);

        Assert.IsTrue(sut.IsDocumentPublished(Textpage.Key, DefaultCulture));
        Assert.IsFalse(sut.IsDocumentPublished(Subpage.Key, DefaultCulture)); // Not updated
        Assert.IsFalse(sut.IsDocumentPublished(Textpage.Key, UnusedCulture)); // Do not exist
        Assert.IsFalse(sut.IsDocumentPublished(Subpage.Key, UnusedCulture)); // Do not exist
    }

    private PublishStatusService CreatePublishedStatusService()
        => new(
            GetRequiredService<ILogger<PublishStatusService>>(),
            GetRequiredService<IPublishStatusRepository>(),
            GetRequiredService<ICoreScopeProvider>(),
            GetRequiredService<ILanguageService>(),
            GetRequiredService<IDocumentNavigationQueryService>());
}
