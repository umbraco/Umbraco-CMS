using Microsoft.Extensions.Options;
using Moq;
using NUnit.Framework;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Configuration.Models;
using Umbraco.Cms.Core.PublishedCache;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Infrastructure.HybridCache;

namespace Umbraco.Cms.Tests.UnitTests.Umbraco.PublishedCache.HybridCache;

[TestFixture]
internal sealed class DomainCacheTests
{
    private const string SiteDefaultCulture = "en-NZ";
    private const string ConfiguredUiLanguage = "en-GB";

    [Test]
    public void Can_Get_Site_Default_Culture_When_Constructed_Before_Runtime_Level_Is_Run()
    {
        var level = RuntimeLevel.Install;
        DomainCache sut = CreateSut(() => level, out _);

        // Constructing before the database can be read must not retain the DefaultUILanguage fallback.
        Assert.AreEqual(ConfiguredUiLanguage, sut.DefaultCulture);

        level = RuntimeLevel.Run;

        Assert.AreEqual(SiteDefaultCulture, sut.DefaultCulture);
    }

    [Test]
    public void Can_Get_Site_Default_Culture_When_Constructed_While_Upgrading()
    {
        DomainCache sut = CreateSut(() => RuntimeLevel.Upgrading, out _);

        Assert.AreEqual(SiteDefaultCulture, sut.DefaultCulture);
    }

    [Test]
    public void Can_Get_Site_Default_Culture_When_Constructed_At_Run()
    {
        DomainCache sut = CreateSut(() => RuntimeLevel.Run, out _);

        Assert.AreEqual(SiteDefaultCulture, sut.DefaultCulture);
    }

    [Test]
    public void Can_Cache_Site_Default_Culture_Once_Runtime_Level_Is_Run()
    {
#pragma warning disable CS0618 // Type or member is obsolete
        DomainCache sut = CreateSut(() => RuntimeLevel.Run, out Mock<ILocalizationService> localizationService);
#pragma warning restore CS0618 // Type or member is obsolete

        for (var i = 0; i < 5; i++)
        {
            Assert.AreEqual(SiteDefaultCulture, sut.DefaultCulture);
        }

        // Read per URL generated, so the resolved value must not be looked up again once it can be trusted.
#pragma warning disable CS0618 // Type or member is obsolete. This is what DefaultCultureAccessor still calls.
        localizationService.Verify(x => x.GetDefaultLanguageIsoCode(), Times.Once);
#pragma warning restore CS0618 // Type or member is obsolete
    }

    [Test]
    public void Cannot_Retain_Empty_Site_Default_Culture()
    {
#pragma warning disable CS0618 // Type or member is obsolete
        DomainCache sut = CreateSut(() => RuntimeLevel.Run, out Mock<ILocalizationService> localizationService, siteDefaultCulture: null);
#pragma warning restore CS0618 // Type or member is obsolete

        Assert.AreEqual(string.Empty, sut.DefaultCulture);

#pragma warning disable CS0618 // Type or member is obsolete. This is what DefaultCultureAccessor still calls.
        localizationService.Setup(x => x.GetDefaultLanguageIsoCode()).Returns(SiteDefaultCulture);
#pragma warning restore CS0618 // Type or member is obsolete

        Assert.AreEqual(SiteDefaultCulture, sut.DefaultCulture);
    }

    private static DomainCache CreateSut(
        Func<RuntimeLevel> level,
#pragma warning disable CS0618 // Type or member is obsolete
        out Mock<ILocalizationService> localizationService,
#pragma warning restore CS0618 // Type or member is obsolete
        string? siteDefaultCulture = SiteDefaultCulture)
    {
#pragma warning disable CS0618 // Type or member is obsolete
        localizationService = new Mock<ILocalizationService>();
        localizationService.Setup(x => x.GetDefaultLanguageIsoCode()).Returns(siteDefaultCulture!);
#pragma warning restore CS0618 // Type or member is obsolete

        var runtimeState = new Mock<IRuntimeState>();
        runtimeState.SetupGet(x => x.Level).Returns(level);

        var globalSettings = new GlobalSettings { DefaultUILanguage = ConfiguredUiLanguage };
        var globalSettingsMonitor = Mock.Of<IOptionsMonitor<GlobalSettings>>(x => x.CurrentValue == globalSettings);

        // The real accessor, so the test exercises the actual runtime-level-dependent behaviour it provides.
        var defaultCultureAccessor = new DefaultCultureAccessor(
            localizationService.Object,
            runtimeState.Object,
            globalSettingsMonitor);

        return new DomainCache(defaultCultureAccessor, Mock.Of<IDomainCacheService>(), runtimeState.Object);
    }
}
