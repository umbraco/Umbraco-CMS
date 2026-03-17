using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;
using Umbraco.Cms.Core.Services;

namespace Umbraco.Cms.Tests.Integration.Testing.Fixtures.Examples;

/// <summary>
///     Example test fixture demonstrating the SetUpFixture pattern.
///     This test fixture does NOT boot its own Umbraco instance — it uses the shared instance
///     from <see cref="ExampleSetUpFixture"/> which was started once for all tests in this namespace.
/// </summary>
/// <remarks>
///     Key benefits:
///     - No per-test host bootstrap overhead (the expensive part)
///     - Tests still get isolated databases via the base class DB management
///     - Services are resolved from the shared host
/// </remarks>
[TestFixture]
public class ExampleServiceTests
{
    private IServiceProvider Services => ExampleSetUpFixture.Instance.GetService<IServiceProvider>();

    [Test]
    public void Can_Resolve_ContentService()
    {
        // Arrange & Act
        var contentService = ExampleSetUpFixture.Instance.GetService<IContentService>();

        // Assert
        Assert.That(contentService, Is.Not.Null);
    }

    [Test]
    public void Can_Resolve_MediaService()
    {
        // Arrange & Act
        var mediaService = ExampleSetUpFixture.Instance.GetService<IMediaService>();

        // Assert
        Assert.That(mediaService, Is.Not.Null);
    }

    [Test]
    public void Can_Resolve_ContentTypeService()
    {
        // Arrange & Act
        var contentTypeService = ExampleSetUpFixture.Instance.GetService<IContentTypeService>();

        // Assert
        Assert.That(contentTypeService, Is.Not.Null);
    }
}
