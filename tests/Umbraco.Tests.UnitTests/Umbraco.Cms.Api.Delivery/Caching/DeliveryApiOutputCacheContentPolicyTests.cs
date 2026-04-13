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
using Umbraco.Cms.Core.Services.Navigation;

namespace Umbraco.Cms.Tests.UnitTests.Umbraco.Cms.Api.Delivery.Caching;

[TestFixture]
public class DeliveryApiOutputCacheContentPolicyTests
{
    private static readonly TimeSpan _defaultDuration = TimeSpan.FromSeconds(60);
    private static readonly StringValues _defaultHeaders = new(["Accept-Language", "Accept-Segment", "Start-Item"]);

    private Mock<IDeliveryApiOutputCacheRequestFilter> _requestFilterMock = null!;
    private Mock<IDeliveryApiOutputCacheDurationProvider> _durationProviderMock = null!;
    private Mock<IDocumentNavigationQueryService> _navigationServiceMock = null!;

    [SetUp]
    public void SetUp()
    {
        _requestFilterMock = new Mock<IDeliveryApiOutputCacheRequestFilter>();
        _requestFilterMock
            .Setup(f => f.IsCacheable(It.IsAny<HttpContext>()))
            .Returns(true);

        _durationProviderMock = new Mock<IDeliveryApiOutputCacheDurationProvider>();
        _durationProviderMock.Setup(p => p.GetDuration(It.IsAny<IPublishedContent>())).Returns((TimeSpan?)null);

        _navigationServiceMock = new Mock<IDocumentNavigationQueryService>();
        IEnumerable<Guid> emptyKeys = Enumerable.Empty<Guid>();
        _navigationServiceMock
            .Setup(n => n.TryGetAncestorsKeys(It.IsAny<Guid>(), out emptyKeys))
            .Returns(true);
    }

    [Test]
    public async Task CacheRequestAsync_WhenRequestFilterReturnsNotCacheable_DoesNotCache()
    {
        _requestFilterMock
            .Setup(f => f.IsCacheable(It.IsAny<HttpContext>()))
            .Returns(false);
        var policy = CreatePolicy();
        OutputCacheContext context = CreateOutputCacheContext();

        await ((IOutputCachePolicy)policy).CacheRequestAsync(context, CancellationToken.None);

        Assert.That(context.EnableOutputCaching, Is.False);
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
    public async Task CacheRequestAsync_SetsDefaultDuration()
    {
        var policy = CreatePolicy();
        OutputCacheContext context = CreateOutputCacheContext();

        await ((IOutputCachePolicy)policy).CacheRequestAsync(context, CancellationToken.None);

        Assert.That(context.ResponseExpirationTimeSpan, Is.EqualTo(_defaultDuration));
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
    public async Task CacheRequestAsync_TagsWithAllContentTag()
    {
        var policy = CreatePolicy();
        OutputCacheContext context = CreateOutputCacheContext();

        await ((IOutputCachePolicy)policy).CacheRequestAsync(context, CancellationToken.None);

        Assert.That(context.Tags, Does.Contain("umb-dapi-content-all"));
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
    public async Task CacheRequestAsync_InvokesVaryByProviders()
    {
        var varyByMock = new Mock<IDeliveryApiOutputCacheVaryByProvider>();
        varyByMock.Setup(p => p.ConfigureVaryBy(It.IsAny<HttpContext>(), It.IsAny<CacheVaryByRules>()))
            .Callback<HttpContext, CacheVaryByRules>((_, rules) => rules.VaryByValues["custom"] = "value");

        var policy = CreatePolicy();
        OutputCacheContext context = CreateOutputCacheContext(varyByProviders: [varyByMock.Object]);

        await ((IOutputCachePolicy)policy).CacheRequestAsync(context, CancellationToken.None);

        Assert.That(context.CacheVaryByRules.VaryByValues["custom"], Is.EqualTo("value"));
    }

    [Test]
    public async Task ServeResponseAsync_TagsWithContentKey()
    {
        var key = Guid.NewGuid();
        var policy = CreatePolicy();
        OutputCacheContext context = CreateOutputCacheContext(
            resolvedContentItems: [CreateContent(key)]);

        await ((IOutputCachePolicy)policy).ServeResponseAsync(context, CancellationToken.None);

        Assert.That(context.Tags, Does.Contain($"umb-dapi-content-{key}"));
    }

    [Test]
    public async Task ServeResponseAsync_InvokesTagProviders()
    {
        var tagProviderMock = new Mock<IDeliveryApiOutputCacheTagProvider>();
        tagProviderMock.Setup(p => p.GetTags(It.IsAny<IPublishedContent>())).Returns(["custom-tag-1"]);

        var policy = CreatePolicy();
        OutputCacheContext context = CreateOutputCacheContext(
            resolvedContentItems: [CreateContent()],
            tagProviders: [tagProviderMock.Object]);

        await ((IOutputCachePolicy)policy).ServeResponseAsync(context, CancellationToken.None);

        Assert.That(context.Tags, Does.Contain("custom-tag-1"));
    }

    [Test]
    public async Task ServeResponseAsync_UsesDurationProvider_WhenNonNull()
    {
        var customDuration = TimeSpan.FromMinutes(5);
        _durationProviderMock.Setup(p => p.GetDuration(It.IsAny<IPublishedContent>())).Returns(customDuration);

        var policy = CreatePolicy();
        OutputCacheContext context = CreateOutputCacheContext(
            resolvedContentItems: [CreateContent()]);

        await ((IOutputCachePolicy)policy).ServeResponseAsync(context, CancellationToken.None);

        Assert.That(context.ResponseExpirationTimeSpan, Is.EqualTo(customDuration));
    }

    [Test]
    public async Task ServeResponseAsync_FallsBackToConfigDuration_WhenProviderReturnsNull()
    {
        _durationProviderMock.Setup(p => p.GetDuration(It.IsAny<IPublishedContent>())).Returns((TimeSpan?)null);

        var policy = CreatePolicy();
        OutputCacheContext context = CreateOutputCacheContext(
            resolvedContentItems: [CreateContent()]);

        // Set a known initial duration to verify it's not overwritten.
        context.ResponseExpirationTimeSpan = _defaultDuration;

        await ((IOutputCachePolicy)policy).ServeResponseAsync(context, CancellationToken.None);

        Assert.That(context.ResponseExpirationTimeSpan, Is.EqualTo(_defaultDuration));
    }

    [Test]
    public async Task ServeResponseAsync_WhenDurationProviderReturnsZero_DisablesCaching()
    {
        _durationProviderMock.Setup(p => p.GetDuration(It.IsAny<IPublishedContent>())).Returns(TimeSpan.Zero);

        var policy = CreatePolicy();
        OutputCacheContext context = CreateOutputCacheContext(
            resolvedContentItems: [CreateContent()]);

        await ((IOutputCachePolicy)policy).ServeResponseAsync(context, CancellationToken.None);

        Assert.That(context.AllowCacheStorage, Is.False);
    }

    [Test]
    public async Task ServeResponseAsync_MultipleItems_UsesMinimumDuration()
    {
        var shortDuration = TimeSpan.FromSeconds(10);
        var longDuration = TimeSpan.FromSeconds(300);

        var content1 = CreateContent();
        var content2 = CreateContent();

        _durationProviderMock.Setup(p => p.GetDuration(content1)).Returns(longDuration);
        _durationProviderMock.Setup(p => p.GetDuration(content2)).Returns(shortDuration);

        var policy = CreatePolicy();
        OutputCacheContext context = CreateOutputCacheContext(
            resolvedContentItems: [content1, content2]);

        await ((IOutputCachePolicy)policy).ServeResponseAsync(context, CancellationToken.None);

        Assert.That(context.ResponseExpirationTimeSpan, Is.EqualTo(shortDuration));
    }

    [Test]
    public async Task ServeResponseAsync_MultipleItems_TagsEachItem()
    {
        var key1 = Guid.NewGuid();
        var key2 = Guid.NewGuid();

        var policy = CreatePolicy();
        OutputCacheContext context = CreateOutputCacheContext(
            resolvedContentItems: [CreateContent(key1), CreateContent(key2)]);

        await ((IOutputCachePolicy)policy).ServeResponseAsync(context, CancellationToken.None);

        Assert.That(context.Tags, Does.Contain($"umb-dapi-content-{key1}"));
        Assert.That(context.Tags, Does.Contain($"umb-dapi-content-{key2}"));
    }

    [Test]
    public async Task ServeResponseAsync_WhenNoResolvedItems_DoesNotAddContentTags()
    {
        var policy = CreatePolicy();
        OutputCacheContext context = CreateOutputCacheContext();
        var initialTagCount = context.Tags.Count;

        await ((IOutputCachePolicy)policy).ServeResponseAsync(context, CancellationToken.None);

        Assert.That(context.Tags.Count, Is.EqualTo(initialTagCount));
    }

    [Test]
    public async Task ServeResponseAsync_TagsWithAncestorKeys()
    {
        var ancestorKey = Guid.NewGuid();
        IEnumerable<Guid> ancestorKeys = new[] { ancestorKey };
        _navigationServiceMock
            .Setup(n => n.TryGetAncestorsKeys(It.IsAny<Guid>(), out ancestorKeys))
            .Returns(true);

        var policy = CreatePolicy();
        OutputCacheContext context = CreateOutputCacheContext(
            resolvedContentItems: [CreateContent()]);

        await ((IOutputCachePolicy)policy).ServeResponseAsync(context, CancellationToken.None);

        Assert.That(context.Tags, Does.Contain($"umb-dapi-content-ancestor-{ancestorKey}"));
    }

    private static DeliveryApiOutputCacheContentPolicy CreatePolicy()
        => new(_defaultDuration, _defaultHeaders);

    private static IPublishedContent CreateContent(Guid? key = null)
    {
        key ??= Guid.NewGuid();
        var contentType = Mock.Of<IPublishedContentType>(ct => ct.Alias == "testPage");
        return Mock.Of<IPublishedContent>(c =>
            c.Key == key.Value &&
            c.ContentType == contentType);
    }

    private OutputCacheContext CreateOutputCacheContext(
        IPublishedContent[]? resolvedContentItems = null,
        IEnumerable<IDeliveryApiOutputCacheTagProvider>? tagProviders = null,
        IEnumerable<IDeliveryApiOutputCacheVaryByProvider>? varyByProviders = null)
    {
        var services = new ServiceCollection();
        services.AddSingleton<ILoggerFactory>(NullLoggerFactory.Instance);
        services.AddSingleton(_requestFilterMock.Object);
        services.AddSingleton(_durationProviderMock.Object);
        services.AddSingleton(_navigationServiceMock.Object);
        services.AddSingleton<IEnumerable<IDeliveryApiOutputCacheTagProvider>>(
            (tagProviders ?? [new ContentTypeDeliveryApiOutputCacheTagProvider()]).ToList());
        services.AddSingleton<IEnumerable<IDeliveryApiOutputCacheVaryByProvider>>(
            (varyByProviders ?? Array.Empty<IDeliveryApiOutputCacheVaryByProvider>()).ToList());

        var httpContext = new DefaultHttpContext { RequestServices = services.BuildServiceProvider() };

        if (resolvedContentItems is not null)
        {
            httpContext.Items[DeliveryApiOutputCacheKeys.ResolvedContentItemsKey] = resolvedContentItems;
        }

        return new OutputCacheContext { HttpContext = httpContext };
    }
}
