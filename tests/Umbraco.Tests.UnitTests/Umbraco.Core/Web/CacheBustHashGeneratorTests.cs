using Moq;
using NUnit.Framework;
using Umbraco.Cms.Core.Configuration;
using Umbraco.Cms.Core.Hosting;
using Umbraco.Cms.Core.Semver;
using Umbraco.Cms.Core.Web;
using Umbraco.Extensions;

namespace Umbraco.Cms.Tests.UnitTests.Umbraco.Core.Web;

[TestFixture]
public class CacheBustHashGeneratorTests
{
    [Test]
    public void Generate_HashesSemanticVersion_WhenNotInDebugMode()
    {
        var hostingEnv = new Mock<IHostingEnvironment>();
        hostingEnv.SetupGet(h => h.IsDebugMode).Returns(false);

        var umbracoVersion = new Mock<IUmbracoVersion>();
        umbracoVersion.SetupGet(v => v.SemanticVersion).Returns(new SemVersion(17, 0, 0));

        var expected = new SemVersion(17, 0, 0).ToSemanticString().GenerateHash();
        Assert.That(CacheBustHashGenerator.Generate(hostingEnv.Object, umbracoVersion.Object), Is.EqualTo(expected));
    }

    [Test]
    public void Generate_DoesNotUseSemanticVersion_WhenInDebugMode()
    {
        var hostingEnv = new Mock<IHostingEnvironment>();
        hostingEnv.SetupGet(h => h.IsDebugMode).Returns(true);

        var umbracoVersion = new Mock<IUmbracoVersion>();
        umbracoVersion.SetupGet(v => v.SemanticVersion).Returns(new SemVersion(17, 0, 0));

        var versionHash = new SemVersion(17, 0, 0).ToSemanticString().GenerateHash();

        // In debug mode the hash varies with time so the cache always busts; it must not equal the stable version hash.
        Assert.That(CacheBustHashGenerator.Generate(hostingEnv.Object, umbracoVersion.Object), Is.Not.EqualTo(versionHash));
    }

    [Test]
    public void Generate_SaltsWithSeed_WhenSeedProvided()
    {
        var hostingEnv = new Mock<IHostingEnvironment>();
        hostingEnv.SetupGet(h => h.IsDebugMode).Returns(false);

        var umbracoVersion = new Mock<IUmbracoVersion>();
        umbracoVersion.SetupGet(v => v.SemanticVersion).Returns(new SemVersion(17, 0, 0));

        var versionHash = new SemVersion(17, 0, 0).ToSemanticString().GenerateHash();
        var seeded = CacheBustHashGenerator.Generate(hostingEnv.Object, umbracoVersion.Object, "deploy-1");

        Assert.Multiple(() =>
        {
            Assert.That(seeded, Is.EqualTo($"{versionHash}|deploy-1".GenerateHash()));
            Assert.That(seeded, Is.Not.EqualTo(versionHash));
        });
    }

    [TestCase(null)]
    [TestCase("")]
    [TestCase("   ")]
    public void Generate_EmptySeed_IsNoOp(string? seed)
    {
        var hostingEnv = new Mock<IHostingEnvironment>();
        hostingEnv.SetupGet(h => h.IsDebugMode).Returns(false);

        var umbracoVersion = new Mock<IUmbracoVersion>();
        umbracoVersion.SetupGet(v => v.SemanticVersion).Returns(new SemVersion(17, 0, 0));

        var withSeed = CacheBustHashGenerator.Generate(hostingEnv.Object, umbracoVersion.Object, seed);
        var withoutSeed = CacheBustHashGenerator.Generate(hostingEnv.Object, umbracoVersion.Object);
        Assert.That(withSeed, Is.EqualTo(withoutSeed));
    }
}
