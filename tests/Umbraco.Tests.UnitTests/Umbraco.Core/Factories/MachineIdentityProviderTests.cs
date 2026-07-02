using Microsoft.Extensions.Options;
using NUnit.Framework;
using Umbraco.Cms.Core.Configuration.Models;
using Umbraco.Cms.Core.Factories;

namespace Umbraco.Cms.Tests.UnitTests.Umbraco.Core.Factories;

[TestFixture]
public class MachineIdentityProviderTests
{
    [Test]
    public void ConfiguredProvider_WhenMachineIdentifierIsSet_ReturnsConfiguredValue()
    {
        var provider = new ConfiguredMachineIdentityProvider(
            Options.Create(new HostingSettings { MachineIdentifier = "my-stable-id" }));
        Assert.AreEqual("my-stable-id", provider.GetMachineIdentifier());
    }

    [Test]
    public void ConfiguredProvider_WhenMachineIdentifierIsNull_ReturnsNull()
    {
        var provider = new ConfiguredMachineIdentityProvider(
            Options.Create(new HostingSettings { MachineIdentifier = null }));
        Assert.IsNull(provider.GetMachineIdentifier());
    }

    [Test]
    public void ConfiguredProvider_WhenMachineIdentifierIsEmptyOrWhitespace_ReturnsNull()
    {
        var provider = new ConfiguredMachineIdentityProvider(
            Options.Create(new HostingSettings { MachineIdentifier = "   " }));
        Assert.IsNull(provider.GetMachineIdentifier());
    }

    [Test]
    public void DefaultProvider_AlwaysReturnsMachineName()
    {
        var provider = new DefaultMachineIdentityProvider();
        Assert.AreEqual(Environment.MachineName, provider.GetMachineIdentifier());
    }
}

[TestFixture]
public class AzureWebsiteInstanceIdMachineIdentityProviderTests
{
    private string? _savedWebsiteInstanceId;

    [SetUp]
    public void SetUp()
    {
        _savedWebsiteInstanceId = Environment.GetEnvironmentVariable(AzureWebsiteInstanceIdMachineIdentityProvider.WebsiteInstanceIdEnvironmentVariable);
        Environment.SetEnvironmentVariable(AzureWebsiteInstanceIdMachineIdentityProvider.WebsiteInstanceIdEnvironmentVariable, null);
    }

    [TearDown]
    public void TearDown() =>
        Environment.SetEnvironmentVariable(AzureWebsiteInstanceIdMachineIdentityProvider.WebsiteInstanceIdEnvironmentVariable, _savedWebsiteInstanceId);

    [Test]
    public void GetMachineIdentifier_WhenEnvironmentVariableIsSet_ReturnsInstanceId()
    {
        Environment.SetEnvironmentVariable(AzureWebsiteInstanceIdMachineIdentityProvider.WebsiteInstanceIdEnvironmentVariable, "abc123instanceid");
        var provider = new AzureWebsiteInstanceIdMachineIdentityProvider();
        Assert.AreEqual("abc123instanceid", provider.GetMachineIdentifier());
    }

    [Test]
    public void GetMachineIdentifier_WhenEnvironmentVariableIsAbsent_ReturnsNull()
    {
        var provider = new AzureWebsiteInstanceIdMachineIdentityProvider();
        Assert.IsNull(provider.GetMachineIdentifier());
    }

}
