using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.OutputCaching;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Primitives;
using Moq;
using NUnit.Framework;
using Umbraco.Cms.Api.Delivery.Caching;
using Umbraco.Cms.Core.Cache;
using Umbraco.Cms.Core.Models.PublishedContent;

namespace Umbraco.Cms.Tests.UnitTests.Umbraco.Cms.Api.Delivery.Caching;

[TestFixture]
public class DeliveryApiOutputCacheMediaPolicyTests
{
    private static readonly TimeSpan _defaultDuration = TimeSpan.FromSeconds(60);
    private static readonly StringValues _defaultHeaders = new("Start-Item");

    private Mock<IDeliveryApiOutputCacheRequestFilter> _requestFilterMock = null!;
    private Mock<IDeliveryApiOutputCacheDurationProvider> _durationProviderMock = null!;

    [SetUp]
    public void SetUp()
    {
        _requestFilterMock = new Mock<IDeliveryApiOutputCacheRequestFilter>();
        _requestFilterMock
            .Setup(f => f.IsCacheable(It.IsAny<HttpContext>()))
            .Returns(true);

        _durationProviderMock = new Mock<IDeliveryApiOutputCacheDurationProvider>();
        _durationProviderMock.Setup(p => p.GetDuration(It.IsAny<IPublishedContent>())).Returns((TimeSpan?)null);
    }

    [Test]
    public async Task CacheRequestAsync_WhenEnabled_EnablesCaching()
    {
        var policy = CreatePolicy();
        OutputCacheContext context = CreateOutputCacheContext();

        await ((IOutputCachePolicy)policy).CacheRequestAsync(context, CancellationToken.None);

        Assert.That(context.EnableOutputCaching, Is.True);
        Assert.That(context.AllowCacheLookup, Is.True);
        Assert.That(context.AllowCacheStorage, Is.True);
    }

    [Test]
    public async Task CacheRequestAsync_TagsWithAllMediaTag()
    {
        var policy = CreatePolicy();
        OutputCacheContext context = CreateOutputCacheContext();

        await ((IOutputCachePolicy)policy).CacheRequestAsync(context, CancellationToken.None);

        Assert.That(context.Tags, Does.Contain("umb-dapi-media-all"));
    }

    [Test]
    public async Task CacheRequestAsync_TagsWithAllTag()
    {
        var policy = CreatePolicy();
        OutputCacheContext context = CreateOutputCacheContext();

        await ((IOutputCachePolicy)policy).CacheRequestAsync(context, CancellationToken.None);

        Assert.That(context.Tags, Does.Contain("umb-dapi-all"));
    }

    [Test]
    public async Task CacheRequestAsync_SetsDefaultVaryByHeaders()
    {
        var policy = CreatePolicy();
        OutputCacheContext context = CreateOutputCacheContext();

        await ((IOutputCachePolicy)policy).CacheRequestAsync(context, CancellationToken.None);

        Assert.That(context.CacheVaryByRules.HeaderNames, Is.EqualTo(_defaultHeaders));
    }

    [Test]
    public async Task ServeResponseAsync_TagsWithMediaKey()
    {
        var key = Guid.NewGuid();
        var policy = CreatePolicy();
        OutputCacheContext context = CreateOutputCacheContext(
            resolvedMediaItems: [CreateMedia(key)]);

        await ((IOutputCachePolicy)policy).ServeResponseAsync(context, CancellationToken.None);

        Assert.That(context.Tags, Does.Contain($"umb-dapi-media-{key}"));
    }

    [Test]
    public async Task ServeResponseAsync_InvokesTagProviders()
    {
        var tagProviderMock = new Mock<IDeliveryApiOutputCacheTagProvider>();
        tagProviderMock.Setup(p => p.GetTags(It.IsAny<IPublishedContent>())).Returns(["media-custom-tag"]);

        var policy = CreatePolicy();
        OutputCacheContext context = CreateOutputCacheContext(
            resolvedMediaItems: [CreateMedia()],
            tagProviders: [tagProviderMock.Object]);

        await ((IOutputCachePolicy)policy).ServeResponseAsync(context, CancellationToken.None);

        Assert.That(context.Tags, Does.Contain("media-custom-tag"));
    }

    private static DeliveryApiOutputCacheMediaPolicy CreatePolicy()
        => new(_defaultDuration, _defaultHeaders);

    private static IPublishedContent CreateMedia(Guid? key = null)
    {
        key ??= Guid.NewGuid();
        var contentType = Mock.Of<IPublishedContentType>(ct => ct.Alias == "Image");
        return Mock.Of<IPublishedContent>(c =>
            c.Key == key.Value &&
            c.ContentType == contentType);
    }

    private OutputCacheContext CreateOutputCacheContext(
        IPublishedContent[]? resolvedMediaItems = null,
        IEnumerable<IDeliveryApiOutputCacheTagProvider>? tagProviders = null)
    {
        var services = new ServiceCollection();
        services.AddSingleton<ILoggerFactory>(NullLoggerFactory.Instance);
        services.AddSingleton(_requestFilterMock.Object);
        services.AddSingleton(_durationProviderMock.Object);
        services.AddSingleton<IEnumerable<IDeliveryApiOutputCacheTagProvider>>(
            (tagProviders ?? [new ContentTypeDeliveryApiOutputCacheTagProvider()]).ToList());
        services.AddSingleton<IEnumerable<IDeliveryApiOutputCacheVaryByProvider>>(
            Array.Empty<IDeliveryApiOutputCacheVaryByProvider>().ToList());

        var httpContext = new DefaultHttpContext { RequestServices = services.BuildServiceProvider() };

        if (resolvedMediaItems is not null)
        {
            httpContext.Items[DeliveryApiOutputCacheKeys.ResolvedMediaItemsKey] = resolvedMediaItems;
        }

        return new OutputCacheContext { HttpContext = httpContext };
    }
}
