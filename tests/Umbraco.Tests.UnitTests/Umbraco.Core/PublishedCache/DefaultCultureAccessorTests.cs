using Microsoft.Extensions.Options;
using Moq;
using NUnit.Framework;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Configuration.Models;
using Umbraco.Cms.Core.PublishedCache;
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
        DefaultCultureAccessor sut = CreateSut(level);

        Assert.AreEqual(SiteDefaultCulture, sut.DefaultCulture);
    }

    [TestCase(RuntimeLevel.Boot)]
    [TestCase(RuntimeLevel.Install)]
    [TestCase(RuntimeLevel.Upgrade)]
    [TestCase(RuntimeLevel.Unknown)]
    public void Can_Get_Configured_Ui_Language_When_Database_Is_Unavailable(RuntimeLevel level)
    {
        DefaultCultureAccessor sut = CreateSut(level);

        Assert.AreEqual(ConfiguredUiLanguage, sut.DefaultCulture);
    }

    [Test]
    public void Can_Get_Empty_Culture_When_No_Default_Language_Is_Set()
    {
        DefaultCultureAccessor sut = CreateSut(RuntimeLevel.Run, siteDefaultCulture: null);

        Assert.AreEqual(string.Empty, sut.DefaultCulture);
    }

    private static DefaultCultureAccessor CreateSut(RuntimeLevel level, string? siteDefaultCulture = SiteDefaultCulture)
    {
#pragma warning disable CS0618 // Type or member is obsolete. This is what DefaultCultureAccessor still calls.
        var localizationService = new Mock<ILocalizationService>();
        localizationService.Setup(x => x.GetDefaultLanguageIsoCode()).Returns(siteDefaultCulture!);
#pragma warning restore CS0618 // Type or member is obsolete

        var runtimeState = new Mock<IRuntimeState>();
        runtimeState.SetupGet(x => x.Level).Returns(level);

        var globalSettings = new GlobalSettings { DefaultUILanguage = ConfiguredUiLanguage };

        return new DefaultCultureAccessor(
            localizationService.Object,
            runtimeState.Object,
            Mock.Of<IOptionsMonitor<GlobalSettings>>(x => x.CurrentValue == globalSettings));
    }
}
