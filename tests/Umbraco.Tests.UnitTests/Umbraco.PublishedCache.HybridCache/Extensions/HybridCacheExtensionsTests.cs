using Microsoft.Extensions.Caching.Hybrid;
using Moq;
using NUnit.Framework;
using Umbraco.Cms.Infrastructure.HybridCache.Extensions;

namespace Umbraco.Cms.Tests.UnitTests.Umbraco.PublishedCache.HybridCache.Extensions;

/// <summary>
/// Provides tests to cover the <see cref="HybridCacheExtensions"/> class.
/// </summary>
/// <remarks>
/// Hat-tip: https://github.com/dotnet/aspnetcore/discussions/57191
/// </remarks>
[TestFixture]
public class HybridCacheExtensionsTests
{
    private Mock<Microsoft.Extensions.Caching.Hybrid.HybridCache> _cacheMock;
    private readonly HybridCacheEntryFlags _flags = HybridCacheEntryFlags.DisableLocalCacheWrite | HybridCacheEntryFlags.DisableDistributedCacheWrite;

    [SetUp]
    public void TestInitialize()
    {
        _cacheMock = new Mock<Microsoft.Extensions.Caching.Hybrid.HybridCache>();
    }

    [Test]
    public async Task ExistsAsync_WhenKeyExists_ShouldReturnTrue()
    {
        // Arrange
        string key = "test-key";
        var expectedValue = "test-value";

        _cacheMock
            .Setup(cache => cache.GetOrCreateAsync(
                key,
                null!,
                It.IsAny<Func<object, CancellationToken, ValueTask<object>>>(),
                It.Is<HybridCacheEntryOptions>(value => value.Flags == _flags),
                null,
                CancellationToken.None))
            .ReturnsAsync(expectedValue);

        // Act
        var exists = await HybridCacheExtensions.ExistsAsync(_cacheMock.Object, key);

        // Assert
        Assert.IsTrue(exists);
    }

    [Test]
    public async Task ExistsAsync_WhenKeyDoesNotExist_ShouldReturnFalse()
    {
        // Arrange
        string key = "test-key";

        _cacheMock
            .Setup(cache => cache.GetOrCreateAsync(
                key,
                null!,
                It.IsAny<Func<object, CancellationToken, ValueTask<object>>>(),
                It.Is<HybridCacheEntryOptions>(value => value.Flags == _flags),
                null,
                CancellationToken.None))
            .Returns((
                string key,
                object? state,
                Func<object, CancellationToken, ValueTask<object>> factory,
                HybridCacheEntryOptions? options,
                IEnumerable<string>? tags,
                CancellationToken token) =>
            {
                return factory(state!, token);
            });

        // Act
        var exists = await HybridCacheExtensions.ExistsAsync(_cacheMock.Object, key);

        // Assert
        Assert.IsFalse(exists);
    }

    [Test]
    public async Task TryGetValueAsync_WhenKeyExists_ShouldReturnTrueAndValueAsString()
    {
        // Arrange
        string key = "test-key";
        var expectedValue = "test-value";

        _cacheMock
            .Setup(cache => cache.GetOrCreateAsync(
                key,
                null!,
                It.IsAny<Func<object, CancellationToken, ValueTask<string>>>(),
                It.Is<HybridCacheEntryOptions>(value => value.Flags == _flags),
                null,
                CancellationToken.None))
            .ReturnsAsync(expectedValue);

        // Act
        var (exists, value) = await HybridCacheExtensions.TryGetValueAsync<string>(_cacheMock.Object, key);

        // Assert
        Assert.IsTrue(exists);
        Assert.AreEqual(expectedValue, value);
    }

    [Test]
    public async Task TryGetValueAsync_WhenKeyExists_ShouldReturnTrueAndValueAsInteger()
    {
        // Arrange
        string key = "test-key";
        var expectedValue = 5;

        _cacheMock
            .Setup(cache => cache.GetOrCreateAsync(
                key,
                null!,
                It.IsAny<Func<object, CancellationToken, ValueTask<int>>>(),
                It.Is<HybridCacheEntryOptions>(value => value.Flags == _flags),
                null,
                CancellationToken.None))
            .ReturnsAsync(expectedValue);

        // Act
        var (exists, value) = await HybridCacheExtensions.TryGetValueAsync<int>(_cacheMock.Object, key);

        // Assert
        Assert.IsTrue(exists);
        Assert.AreEqual(expectedValue, value);
    }

    [Test]
    public async Task TryGetValueAsync_WhenKeyExistsButValueIsNull_ShouldReturnTrueAndNullValue()
    {
        // Arrange
        string key = "test-key";

        _cacheMock
            .Setup(cache => cache.GetOrCreateAsync(
                key,
                null!,
                It.IsAny<Func<object, CancellationToken, ValueTask<object>>>(),
                It.Is<HybridCacheEntryOptions>(value => value.Flags == _flags),
                null,
                CancellationToken.None))
            .ReturnsAsync(null!);

        // Act
        var (exists, value) = await HybridCacheExtensions.TryGetValueAsync<int?>(_cacheMock.Object, key);

        // Assert
        Assert.IsTrue(exists);
        Assert.IsNull(value);
    }

    [Test]
    public async Task TryGetValueAsync_WhenKeyDoesNotExist_ShouldReturnFalseAndNull()
    {
        // Arrange
        string key = "test-key";

        _cacheMock.Setup(cache => cache.GetOrCreateAsync(
            key,
            null,
            It.IsAny<Func<object?, CancellationToken, ValueTask<string>>>(),
            It.Is<HybridCacheEntryOptions>(value => value.Flags == _flags),
            null,
            CancellationToken.None))
            .Returns((
                string key,
                object? state,
                Func<object?, CancellationToken, ValueTask<string>> factory,
                HybridCacheEntryOptions? options,
                IEnumerable<string>? tags,
                CancellationToken token) =>
            {
                return factory(state, token);
            });

        // Act
        var (exists, value) = await HybridCacheExtensions.TryGetValueAsync<string>(_cacheMock.Object, key);

        // Assert
        Assert.IsFalse(exists);
        Assert.IsNull(value);
    }
}
