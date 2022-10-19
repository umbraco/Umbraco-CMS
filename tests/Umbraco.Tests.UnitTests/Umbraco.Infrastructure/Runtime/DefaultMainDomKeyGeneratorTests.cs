using AutoFixture.NUnit3;
using Microsoft.Extensions.Options;
using NUnit.Framework;
using Umbraco.Cms.Core.Configuration.Models;
using Umbraco.Cms.Core.Hosting;
using Umbraco.Cms.Infrastructure.Runtime;
using Umbraco.Cms.Tests.UnitTests.AutoFixture;

namespace Umbraco.Cms.Tests.UnitTests.Umbraco.Infrastructure.Runtime;

[TestFixture]
internal class DefaultMainDomKeyGeneratorTests
{
    [Test]
    [AutoMoqData]
    public void GenerateKey_WithConfiguredDiscriminatorValue_AltersHash(
        [Frozen] IHostingEnvironment hostingEnvironment,
        [Frozen] GlobalSettings globalSettings,
        [Frozen] IOptionsMonitor<GlobalSettings> globalSettingsMonitor,
        DefaultMainDomKeyGenerator sut,
        string aDiscriminator)
    {
        var withoutDiscriminator = sut.GenerateKey();
        globalSettings.MainDomKeyDiscriminator = aDiscriminator;
        var withDiscriminator = sut.GenerateKey();

        Assert.AreNotEqual(withoutDiscriminator, withDiscriminator);
    }

    [Test]
    [AutoMoqData]
    public void GenerateKey_WithUnchangedDiscriminatorValue_ReturnsSameValue(
        [Frozen] IHostingEnvironment hostingEnvironment,
        [Frozen] GlobalSettings globalSettings,
        [Frozen] IOptionsMonitor<GlobalSettings> globalSettingsMonitor,
        DefaultMainDomKeyGenerator sut,
        string aDiscriminator)
    {
        globalSettings.MainDomKeyDiscriminator = aDiscriminator;

        var a = sut.GenerateKey();
        var b = sut.GenerateKey();

        Assert.AreEqual(a, b);
    }
}
