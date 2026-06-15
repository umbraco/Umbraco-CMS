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
            Assert.That(sut.IsPublished(Textpage.Key, DefaultCulture), Is.False);
            Assert.That(sut.IsPublished(Subpage2.Key, DefaultCulture), Is.False);
            Assert.That(sut.IsPublished(Subpage.Key, DefaultCulture), Is.False);
            Assert.That(sut.IsPublished(Subpage2.Key, DefaultCulture), Is.False);
            Assert.That(sut.IsPublished(Subpage3.Key, DefaultCulture), Is.False);

            Assert.That(sut.IsPublished(Trashed.Key, DefaultCulture), Is.False);

            Assert.That(sut.IsPublished(Textpage.Key, UnusedCulture), Is.False);
            Assert.That(sut.IsPublished(Subpage2.Key, UnusedCulture), Is.False);
            Assert.That(sut.IsPublished(Subpage.Key, UnusedCulture), Is.False);
            Assert.That(sut.IsPublished(Subpage2.Key, UnusedCulture), Is.False);
            Assert.That(sut.IsPublished(Subpage3.Key, UnusedCulture), Is.False);

            Assert.That(sut.IsPublished(Trashed.Key, UnusedCulture), Is.False);
        });

        // Act
        var publishResults = ContentService.PublishBranch(Textpage, PublishBranchFilter.IncludeUnpublished, ["*"]);
        await sut.InitializeAsync(CancellationToken.None);

        Assert.Multiple(() =>
        {
            Assert.That(publishResults.All(x => x.Result == PublishResultType.SuccessPublish), Is.True);
            Assert.That(sut.IsPublished(Textpage.Key, DefaultCulture), Is.True);
            Assert.That(sut.IsPublished(Subpage2.Key, DefaultCulture), Is.True);
            Assert.That(sut.IsPublished(Subpage.Key, DefaultCulture), Is.True);
            Assert.That(sut.IsPublished(Subpage2.Key, DefaultCulture), Is.True);
            Assert.That(sut.IsPublished(Subpage3.Key, DefaultCulture), Is.True);

            Assert.That(sut.IsPublished(Trashed.Key, DefaultCulture), Is.False);

            Assert.That(sut.IsPublished(Textpage.Key, UnusedCulture), Is.False);
            Assert.That(sut.IsPublished(Subpage2.Key, UnusedCulture), Is.False);
            Assert.That(sut.IsPublished(Subpage.Key, UnusedCulture), Is.False);
            Assert.That(sut.IsPublished(Subpage2.Key, UnusedCulture), Is.False);
            Assert.That(sut.IsPublished(Subpage3.Key, UnusedCulture), Is.False);

            Assert.That(sut.IsPublished(Trashed.Key, UnusedCulture), Is.False);
        });
    }

    [Test]
    public async Task AddOrUpdateStatusWithDescendantsAsync_Updates_Document_Path_Published_Status()
    {
        var sut = new DocumentPublishStatusService(
            GetRequiredService<ILogger<DocumentPublishStatusService>>(),
            GetRequiredService<IPublishStatusRepository>(),
            GetRequiredService<ICoreScopeProvider>(),
            GetRequiredService<ILanguageService>(),
            GetRequiredService<IDocumentNavigationQueryService>());

        Assert.That(sut.IsPublished(Textpage.Key, DefaultCulture), Is.False);

        // Act
        var publishResults = ContentService.PublishBranch(Textpage, PublishBranchFilter.IncludeUnpublished, ["*"]);
        await sut.AddOrUpdateStatusWithDescendantsAsync(Textpage.Key, CancellationToken.None);

        Assert.That(sut.IsPublished(Textpage.Key, DefaultCulture), Is.True);
        Assert.That(sut.IsPublished(Subpage.Key, DefaultCulture), Is.True); // Updated due to being an descendant
        Assert.That(sut.IsPublished(Textpage.Key, UnusedCulture), Is.False); // Do not exist
        Assert.That(sut.IsPublished(Subpage.Key, UnusedCulture), Is.False); // Do not exist
    }

    [Test]
    public async Task AddOrUpdateStatusAsync_Updates_Document_Published_Status()
    {
        var sut = new DocumentPublishStatusService(
            GetRequiredService<ILogger<DocumentPublishStatusService>>(),
            GetRequiredService<IPublishStatusRepository>(),
            GetRequiredService<ICoreScopeProvider>(),
            GetRequiredService<ILanguageService>(),
            GetRequiredService<IDocumentNavigationQueryService>());

        Assert.That(sut.IsPublished(Textpage.Key, DefaultCulture), Is.False);

        // Act
        var publishResults = ContentService.PublishBranch(Textpage, PublishBranchFilter.IncludeUnpublished, ["*"]);
        await sut.AddOrUpdateStatusAsync(Textpage.Key, CancellationToken.None);

        Assert.That(sut.IsPublished(Textpage.Key, DefaultCulture), Is.True);
        Assert.That(sut.IsPublished(Subpage.Key, DefaultCulture), Is.False); // Not updated
        Assert.That(sut.IsPublished(Textpage.Key, UnusedCulture), Is.False); // Do not exist
        Assert.That(sut.IsPublished(Subpage.Key, UnusedCulture), Is.False); // Do not exist
    }

    [Test]
    public async Task AddOrUpdateStatusWithDescendantsAsync_Does_Not_Throw_When_Content_Does_Not_Exist()
    {
        // Arrange - use a random key that doesn't exist
        var sut = CreatePublishedStatusService();
        var nonExistentKey = Guid.NewGuid();

        // Act & Assert - should not throw, just return gracefully
        Assert.DoesNotThrowAsync(async () =>
            await sut.AddOrUpdateStatusWithDescendantsAsync(nonExistentKey, CancellationToken.None));
    }

    [Test]
    public async Task AddOrUpdateStatusAsync_Does_Not_Throw_When_Content_Does_Not_Exist()
    {
        // Arrange - use a random key that doesn't exist
        var sut = CreatePublishedStatusService();
        var nonExistentKey = Guid.NewGuid();

        // Act & Assert - should not throw, just return gracefully
        Assert.DoesNotThrowAsync(async () =>
            await sut.AddOrUpdateStatusAsync(nonExistentKey, CancellationToken.None));
    }

    private DocumentPublishStatusService CreatePublishedStatusService()
        => new(
            GetRequiredService<ILogger<DocumentPublishStatusService>>(),
            GetRequiredService<IPublishStatusRepository>(),
            GetRequiredService<ICoreScopeProvider>(),
            GetRequiredService<ILanguageService>(),
            GetRequiredService<IDocumentNavigationQueryService>());
}
