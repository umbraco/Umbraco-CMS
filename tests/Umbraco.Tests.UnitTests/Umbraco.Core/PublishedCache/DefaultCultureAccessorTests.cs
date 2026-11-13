using Microsoft.Extensions.Options;
using Moq;
using NUnit.Framework;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Configuration.Models;
using Umbraco.Cms.Core.PublishedCache;
using Umbraco.Cms.Core.Scoping;
using Umbraco.Cms.Core.Services;

namespace Umbraco.Cms.Tests.UnitTests.Umbraco.Core.PublishedCache;

[TestFixture]
internal sealed class DefaultCultureAccessorTests
{
    private const string SiteDefaultCulture = "en-NZ";
    private const string ConfiguredUiLanguage = "en-GB";

    [TestCase(RuntimeLevel.Run)]
    [TestCase(RuntimeLevel.Upgrading)]
    public void Can_Get_Site_Default_Culture_When_Database_Is_Available(RuntimeLevel level)
    {
        DefaultCultureAccessor sut = CreateSut(level, out _);

        Assert.AreEqual(SiteDefaultCulture, sut.DefaultCulture);
    }

    [TestCase(RuntimeLevel.Boot)]
    [TestCase(RuntimeLevel.Install)]
    [TestCase(RuntimeLevel.Upgrade)]
    [TestCase(RuntimeLevel.Unknown)]
    public void Can_Get_Configured_Ui_Language_When_Database_Is_Unavailable(RuntimeLevel level)
    {
        DefaultCultureAccessor sut = CreateSut(level, out _);

        Assert.AreEqual(ConfiguredUiLanguage, sut.DefaultCulture);
    }

    [Test]
    public void Can_Get_Empty_Culture_When_No_Default_Language_Is_Set()
    {
        DefaultCultureAccessor sut = CreateSut(RuntimeLevel.Run, out _, siteDefaultCulture: null);

        Assert.AreEqual(string.Empty, sut.DefaultCulture);
    }

    [Test]
    public void Can_Read_Site_Default_Culture_Once()
    {
        DefaultCultureAccessor sut = CreateSut(RuntimeLevel.Run, out Mock<ILanguageService> languageService);

        for (var i = 0; i < 5; i++)
        {
            Assert.AreEqual(SiteDefaultCulture, sut.DefaultCulture);
        }

        languageService.Verify(x => x.GetDefaultIsoCodeAsync(), Times.Once);
    }

    [Test]
    public void Can_Get_Changed_Site_Default_Culture_After_ClearCache()
    {
        DefaultCultureAccessor sut = CreateSut(RuntimeLevel.Run, out Mock<ILanguageService> languageService);
        Assert.AreEqual(SiteDefaultCulture, sut.DefaultCulture);

        languageService.Setup(x => x.GetDefaultIsoCodeAsync()).ReturnsAsync("da-DK");
        Assert.AreEqual(SiteDefaultCulture, sut.DefaultCulture);

        sut.ClearCache();

        Assert.AreEqual("da-DK", sut.DefaultCulture);
    }

    [Test]
    public void Cannot_Cache_Empty_Site_Default_Culture()
    {
        DefaultCultureAccessor sut = CreateSut(RuntimeLevel.Run, out Mock<ILanguageService> languageService, siteDefaultCulture: null);
        Assert.AreEqual(string.Empty, sut.DefaultCulture);

        languageService.Setup(x => x.GetDefaultIsoCodeAsync()).ReturnsAsync(SiteDefaultCulture);

        Assert.AreEqual(SiteDefaultCulture, sut.DefaultCulture);
    }

    [Test]
    public void Cannot_Cache_Site_Default_Culture_Read_While_Upgrading()
    {
        DefaultCultureAccessor sut = CreateSut(RuntimeLevel.Upgrading, out Mock<ILanguageService> languageService);
        Assert.AreEqual(SiteDefaultCulture, sut.DefaultCulture);

        languageService.Setup(x => x.GetDefaultIsoCodeAsync()).ReturnsAsync("da-DK");

        Assert.AreEqual("da-DK", sut.DefaultCulture);
    }

    [Test]
    public void Cannot_Cache_Site_Default_Culture_Read_Within_An_Ambient_Scope()
    {
        var scopeProvider = new Mock<ICoreScopeProvider>();
        scopeProvider.SetupGet(x => x.Context).Returns(Mock.Of<IScopeContext>());
        DefaultCultureAccessor sut = CreateSut(RuntimeLevel.Run, out Mock<ILanguageService> languageService, scopeProvider: scopeProvider.Object);

        Assert.AreEqual(SiteDefaultCulture, sut.DefaultCulture);
        Assert.AreEqual(SiteDefaultCulture, sut.DefaultCulture);

        languageService.Verify(x => x.GetDefaultIsoCodeAsync(), Times.Exactly(2));
    }

    [Test]
    public void Cannot_Cache_Site_Default_Culture_Read_While_The_Cache_Is_Cleared()
    {
        DefaultCultureAccessor sut = CreateSut(RuntimeLevel.Run, out Mock<ILanguageService> languageService);

        // The default changes and the cache is cleared while the previous default is being read.
        languageService
            .Setup(x => x.GetDefaultIsoCodeAsync())
            .ReturnsAsync(SiteDefaultCulture)
            .Callback(() =>
            {
                languageService.Setup(x => x.GetDefaultIsoCodeAsync()).ReturnsAsync("da-DK");
                sut.ClearCache();
            });

        Assert.AreEqual(SiteDefaultCulture, sut.DefaultCulture);
        Assert.AreEqual("da-DK", sut.DefaultCulture);
    }

    private static DefaultCultureAccessor CreateSut(
        RuntimeLevel level,
        out Mock<ILanguageService> languageService,
        string? siteDefaultCulture = SiteDefaultCulture,
        ICoreScopeProvider? scopeProvider = null)
    {
        languageService = new Mock<ILanguageService>();
        languageService.Setup(x => x.GetDefaultIsoCodeAsync()).ReturnsAsync(siteDefaultCulture!);

        var runtimeState = new Mock<IRuntimeState>();
        runtimeState.SetupGet(x => x.Level).Returns(level);

        var globalSettings = new GlobalSettings { DefaultUILanguage = ConfiguredUiLanguage };

        return new DefaultCultureAccessor(
            languageService.Object,
            runtimeState.Object,
            scopeProvider ?? Mock.Of<ICoreScopeProvider>(),
            Mock.Of<IOptionsMonitor<GlobalSettings>>(x => x.CurrentValue == globalSettings));
    }
}
