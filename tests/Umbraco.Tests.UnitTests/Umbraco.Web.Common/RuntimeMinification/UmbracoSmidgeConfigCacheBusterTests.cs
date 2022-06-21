using AutoFixture;
using AutoFixture.AutoMoq;
using Microsoft.Extensions.Options;
using Moq;
using NUnit.Framework;
using Umbraco.Cms.Core.Configuration;
using Umbraco.Cms.Core.Configuration.Models;
using Umbraco.Cms.Core.Semver;
using Umbraco.Cms.Web.Common.RuntimeMinification;
using Umbraco.Extensions;

namespace Umbraco.Cms.Tests.UnitTests.Umbraco.Web.Common.RuntimeMinification;

/// <remarks>
///     UmbracoCustomizations kindly configures an IUmbracoVersion so we need to go verbose without AutoMoqData
/// </remarks>
[TestFixture]
public class UmbracoSmidgeConfigCacheBusterTests
{
    [Test]
    public void GetValue_DefaultReleaseSetupWithNoConfiguredVersion_HasSensibleDefaults()
    {
        var fixture = new Fixture();
        fixture.Customize(new AutoMoqCustomization());

        var umbracoVersion = fixture.Freeze<Mock<IUmbracoVersion>>();
        var entryAssemblyMetadata = fixture.Freeze<Mock<IEntryAssemblyMetadata>>();
        var sut = fixture.Create<UmbracoSmidgeConfigCacheBuster>();

        umbracoVersion.Setup(x => x.SemanticVersion).Returns(new SemVersion(9, 4, 5, "beta", "41658f99"));
        entryAssemblyMetadata.Setup(x => x.Name).Returns("Bills.Brilliant.Bakery");
        entryAssemblyMetadata.Setup(x => x.InformationalVersion).Returns("42.1.2-alpha+41658f99");

        var result = sut.GetValue();

        var expected = "Bills.Brilliant.Bakery_9.4.5-beta+41658f99_42.1.2-alpha+41658f99".GenerateHash();
        Assert.AreEqual(expected, result);
    }

    [Test]
    public void GetValue_DefaultReleaseSetupWithConfiguredVersion_HasSensibleDefaults()
    {
        var config = Options.Create(new RuntimeMinificationSettings { Version = "1" });
        var fixture = new Fixture();
        fixture.Customize(new AutoMoqCustomization());
        fixture.Inject(config);

        var umbracoVersion = fixture.Freeze<Mock<IUmbracoVersion>>();
        var entryAssemblyMetadata = fixture.Freeze<Mock<IEntryAssemblyMetadata>>();
        var sut = fixture.Create<UmbracoSmidgeConfigCacheBuster>();

        umbracoVersion.Setup(x => x.SemanticVersion).Returns(new SemVersion(9, 4, 5, "beta", "41658f99"));
        entryAssemblyMetadata.Setup(x => x.Name).Returns("Bills.Brilliant.Bakery");
        entryAssemblyMetadata.Setup(x => x.InformationalVersion).Returns("42.1.2-alpha+41658f99");

        var result = sut.GetValue();

        var expected = "1_9.4.5-beta+41658f99_42.1.2-alpha+41658f99".GenerateHash();
        Assert.AreEqual(expected, result);
    }

    [Test]
    public void GetValue_DefaultReleaseSetupWithNoConfiguredVersion_ChangesWhenUmbracoVersionChanges()
    {
        var fixture = new Fixture();
        fixture.Customize(new AutoMoqCustomization());

        var umbracoVersion = fixture.Freeze<Mock<IUmbracoVersion>>();
        var entryAssemblyMetadata = fixture.Freeze<Mock<IEntryAssemblyMetadata>>();
        var sut = fixture.Create<UmbracoSmidgeConfigCacheBuster>();

        umbracoVersion.Setup(x => x.SemanticVersion).Returns(new SemVersion(9, 4, 5, "beta", "41658f99"));
        entryAssemblyMetadata.Setup(x => x.Name).Returns("Bills.Brilliant.Bakery");
        entryAssemblyMetadata.Setup(x => x.InformationalVersion).Returns("42.1.2-alpha+41658f99");

        var before = sut.GetValue();

        umbracoVersion.Setup(x => x.SemanticVersion).Returns(new SemVersion(9, 5, 0, "beta", "41658f99"));
        sut = fixture.Create<UmbracoSmidgeConfigCacheBuster>();

        var after = sut.GetValue();

        Assert.AreNotEqual(before, after);
    }

    [Test]
    public void GetValue_DefaultReleaseSetupWithNoConfiguredVersion_ChangesWhenDownstreamVersionChanges()
    {
        var fixture = new Fixture();
        fixture.Customize(new AutoMoqCustomization());

        var umbracoVersion = fixture.Freeze<Mock<IUmbracoVersion>>();
        var entryAssemblyMetadata = fixture.Freeze<Mock<IEntryAssemblyMetadata>>();
        var sut = fixture.Create<UmbracoSmidgeConfigCacheBuster>();

        umbracoVersion.Setup(x => x.SemanticVersion).Returns(new SemVersion(9, 4, 5, "beta", "41658f99"));
        entryAssemblyMetadata.Setup(x => x.Name).Returns("Bills.Brilliant.Bakery");
        entryAssemblyMetadata.Setup(x => x.InformationalVersion).Returns("42.1.2-alpha+41658f99");

        var before = sut.GetValue();

        entryAssemblyMetadata.Setup(x => x.InformationalVersion).Returns("42.2.0-rc");
        sut = fixture.Create<UmbracoSmidgeConfigCacheBuster>();

        var after = sut.GetValue();

        Assert.AreNotEqual(before, after);
    }
}
