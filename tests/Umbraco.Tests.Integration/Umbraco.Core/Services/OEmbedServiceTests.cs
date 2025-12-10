using NUnit.Framework;
using Umbraco.Cms.Core.DependencyInjection;
using Umbraco.Cms.Core.Media.EmbedProviders;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Services.OperationStatus;
using Umbraco.Cms.Tests.Common.Testing;
using Umbraco.Cms.Tests.Integration.Testing;

namespace Umbraco.Cms.Tests.Integration.Umbraco.Core.Services;

[TestFixture]
[UmbracoTest(Database = UmbracoTestOptions.Database.None)]
internal sealed class OEmbedServiceTests : UmbracoIntegrationTest
{
    private IOEmbedService OEmbedService => GetRequiredService<IOEmbedService>();

    protected override void CustomTestSetup(IUmbracoBuilder builder)
    {
        base.CustomTestSetup(builder);

        // Clear all providers and add only the X provider
        builder.EmbedProviders().Clear().Append<X>();
    }

    /// <summary>
    /// Verifies resolution to https://github.com/umbraco/Umbraco-CMS/issues/21052.
    /// </summary>
    /// <remarks>
    /// Tests marked as [Explicit] as we don't want a random external service call to X to fail during regular test runs.
    /// </remarks>
    [Explicit]
    [TestCase("https://x.com/THR/status/1995620384344080849?s=20")]
    [TestCase("https://x.com/SquareEnix/status/1995780120888705216?s=20")]
    [TestCase("https://x.com/sem_sep/status/1991750339427700739?s=20")]
    public async Task GetMarkupAsync_WithXUrls_ReturnsSuccessAndMarkup(string url)
    {
        // Arrange
        var uri = new Uri(url);

        // Act
        var result = await OEmbedService.GetMarkupAsync(uri, width: null, height: null, CancellationToken.None);

        // Assert
        Assert.Multiple(() =>
        {
            Assert.That(result.Success, Is.True);
            Assert.That(result.Status, Is.EqualTo(OEmbedOperationStatus.Success));
            Assert.That(result.Result, Is.Not.Null.And.Not.Empty);
            Assert.That(result.Result, Does.Contain("blockquote"));
        });
    }
}
