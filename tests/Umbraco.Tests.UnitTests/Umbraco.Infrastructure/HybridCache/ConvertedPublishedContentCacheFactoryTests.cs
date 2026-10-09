// Copyright (c) Umbraco.
// See LICENSE for more details.

using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using NUnit.Framework;
using Umbraco.Cms.Core.Models.PublishedContent;
using Umbraco.Cms.Infrastructure.HybridCache.Services;

namespace Umbraco.Cms.Tests.UnitTests.Umbraco.Infrastructure.HybridCache;

[TestFixture]
public class ConvertedPublishedContentCacheFactoryTests
{
    private const string CacheName = "Test cache";

    [Test]
    public void Can_Create_Unbounded_Cache_When_No_Maximum_Configured()
    {
        var boundedFactory = new Mock<IBoundedConvertedPublishedContentCacheFactory>();
        var factory = CreateFactory(boundedFactory.Object);

        IConvertedPublishedContentCache<string> cache = factory.Create<string>(maximumItems: null, CacheName);

        Assert.That(cache, Is.TypeOf<UnboundedConvertedPublishedContentCache<string>>());
        boundedFactory.Verify(x => x.CreateBounded<string>(It.IsAny<int>()), Times.Never);
    }

    [Test]
    public void Can_Create_Bounded_Cache_When_Maximum_Set_And_Provider_Present()
    {
        var expected = Mock.Of<IConvertedPublishedContentCache<string>>();
        var boundedFactory = new Mock<IBoundedConvertedPublishedContentCacheFactory>();
        boundedFactory.Setup(x => x.CreateBounded<string>(5)).Returns(expected);
        var factory = CreateFactory(boundedFactory.Object);

        IConvertedPublishedContentCache<string> cache = factory.Create<string>(maximumItems: 5, CacheName);

        Assert.That(cache, Is.SameAs(expected));
        boundedFactory.Verify(x => x.CreateBounded<string>(5), Times.Once);
    }

    [Test]
    public void Can_Fall_Back_To_Unbounded_When_Maximum_Set_But_No_Bounded_Provider()
    {
        var factory = CreateFactory(boundedFactory: null);

        IConvertedPublishedContentCache<string> cache = factory.Create<string>(maximumItems: 5, CacheName);

        Assert.That(cache, Is.TypeOf<UnboundedConvertedPublishedContentCache<string>>());
    }

    private static ConvertedPublishedContentCacheFactory CreateFactory(
        IBoundedConvertedPublishedContentCacheFactory? boundedFactory)
        => new(boundedFactory, new NullLogger<ConvertedPublishedContentCacheFactory>());
}
