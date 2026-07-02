using Microsoft.Extensions.Options;
using Moq;
using NUnit.Framework;
using Umbraco.Cms.Core.Configuration.Models;
using Umbraco.Cms.Core.Factories;
using Umbraco.Cms.Core.Hosting;

namespace Umbraco.Cms.Tests.UnitTests.Umbraco.Core.Factories;

[TestFixture]
public class MachineInfoFactoryTests
{
    [Test]
    public void BuildMachineIdentifier_WithNoSiteName_ReturnsMachineNameOnly()
    {
        var result = MachineInfoFactory.BuildMachineIdentifier("SERVER01", null);
        Assert.AreEqual("SERVER01", result);
    }

    [Test]
    public void BuildMachineIdentifier_WithEmptySiteName_ReturnsMachineNameOnly()
    {
        var result = MachineInfoFactory.BuildMachineIdentifier("SERVER01", string.Empty);
        Assert.AreEqual("SERVER01", result);
    }

    [Test]
    public void BuildMachineIdentifier_WithWhitespaceSiteName_ReturnsMachineNameOnly()
    {
        var result = MachineInfoFactory.BuildMachineIdentifier("SERVER01", "   ");
        Assert.AreEqual("SERVER01", result);
    }

    [Test]
    public void BuildMachineIdentifier_WithSiteName_ReturnsMachineNameSlashSiteName()
    {
        var result = MachineInfoFactory.BuildMachineIdentifier("SERVER01", "site1");
        Assert.AreEqual("SERVER01/site1", result);
    }

    [Test]
    public void GetMachineIdentifier_UsesFirstNonNullProvider()
    {
        var factory = CreateFactory(siteName: null, Provider(null), Provider("second-provider"));
        Assert.AreEqual("second-provider", factory.GetMachineIdentifier());
    }

    [Test]
    public void GetMachineIdentifier_AppendsSiteNameToProviderResult()
    {
        var factory = CreateFactory(siteName: "site1", Provider("base-id"));
        Assert.AreEqual("base-id/site1", factory.GetMachineIdentifier());
    }

    [Test]
    public void GetMachineIdentifier_WhenAllProvidersReturnNull_ThrowsInvalidOperationException()
    {
        var factory = CreateFactory(siteName: null, Provider(null), Provider(null));
        Assert.Throws<InvalidOperationException>(() => factory.GetMachineIdentifier());
    }

    [Test]
    public void GetMachineIdentifier_WhenIdentifierExceedsMaxLength_ThrowsInvalidOperationException()
    {
        var factory = CreateFactory(siteName: null, Provider(new string('x', MachineInfoFactory.MaxMachineIdentifierLength + 1)));
        Assert.Throws<InvalidOperationException>(() => factory.GetMachineIdentifier());
    }

    [Test]
    public void GetMachineIdentifier_WhenCombinedWithSiteNameExceedsMaxLength_ThrowsInvalidOperationException()
    {
        var factory = CreateFactory(
            siteName: new string('s', MachineInfoFactory.MaxMachineIdentifierLength),
            Provider(new string('x', MachineInfoFactory.MaxMachineIdentifierLength)));
        Assert.Throws<InvalidOperationException>(() => factory.GetMachineIdentifier());
    }

    private static IMachineIdentityProvider Provider(string? value)
        => Mock.Of<IMachineIdentityProvider>(p => p.GetMachineIdentifier() == value);

    private static MachineInfoFactory CreateFactory(string? siteName, params IMachineIdentityProvider[] providers)
    {
        var collection = new MachineIdentityProviderCollection(() => providers);
        var settings = Options.Create(new HostingSettings { SiteName = siteName });
        return new MachineInfoFactory(Mock.Of<IHostingEnvironment>(), collection, settings);
    }
}
