using NUnit.Framework;
using Umbraco.Cms.Search.Core;
using Umbraco.Cms.Search.Core.Models.Indexing;
using Umbraco.Cms.Search.Core.Services;

namespace Umbraco.Cms.Tests.Integration.Umbraco.Search.Provider.Examine.Tests;

[TestFixture]
public class HealthStatusTests : SearcherTestBase
{
    private const string IndexAlias = global::Umbraco.Cms.Core.Constants.IndexAliases.PublishedContent;

    [Test]
    [Order(1)]
    public async Task GetHealthStatus_IndexWithDocuments_ReturnsHealthy()
    {
        IIndexer indexer = GetRequiredService<IIndexer>();

        HealthStatus status = (await indexer.GetMetadataAsync(IndexAlias)).HealthStatus;

        Assert.That(status, Is.EqualTo(HealthStatus.Healthy));
    }

    [Test]
    [Order(2)]
    public async Task GetHealthStatus_EmptyIndex_ReturnsEmpty()
    {
        IIndexer indexer = GetRequiredService<IIndexer>();

        // Reset to ensure index exists but is empty
        await indexer.ResetAsync(IndexAlias);

        HealthStatus status = (await indexer.GetMetadataAsync(IndexAlias)).HealthStatus;

        Assert.That(status, Is.EqualTo(HealthStatus.Empty));
    }

    [Test]
    [Order(3)]
    public async Task GetHealthStatus_UnknownIndex_ReturnsUnknown()
    {
        IIndexer indexer = GetRequiredService<IIndexer>();

        HealthStatus status = (await indexer.GetMetadataAsync("NonExistentIndex")).HealthStatus;

        Assert.That(status, Is.EqualTo(HealthStatus.Unknown));
    }
}
