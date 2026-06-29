// Copyright (c) Umbraco.
// See LICENSE for more details.

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
    private string? _savedWebsiteInstanceId;

    [SetUp]
    public void SetUp()
    {
        _savedWebsiteInstanceId = Environment.GetEnvironmentVariable(MachineInfoFactory.AzureWebsiteInstanceIdEnvironmentVariable);
        Environment.SetEnvironmentVariable(MachineInfoFactory.AzureWebsiteInstanceIdEnvironmentVariable, null);
    }

    [TearDown]
    public void TearDown() =>
        Environment.SetEnvironmentVariable(MachineInfoFactory.AzureWebsiteInstanceIdEnvironmentVariable, _savedWebsiteInstanceId);

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
    public void GetMachineIdentifier_WithNoSiteName_ReturnsSameResultAsBuildMachineIdentifier()
    {
        var factory = CreateFactory(siteName: null);
        var result = factory.GetMachineIdentifier();
        Assert.AreEqual(MachineInfoFactory.BuildMachineIdentifier(Environment.MachineName, null), result);
    }

    [Test]
    public void GetMachineIdentifier_WithEmptySiteName_ReturnsSameResultAsBuildMachineIdentifier()
    {
        var factory = CreateFactory(siteName: string.Empty);
        var result = factory.GetMachineIdentifier();
        Assert.AreEqual(MachineInfoFactory.BuildMachineIdentifier(Environment.MachineName, string.Empty), result);
    }

    [Test]
    public void GetMachineIdentifier_WithWhitespaceSiteName_ReturnsSameResultAsBuildMachineIdentifier()
    {
        var factory = CreateFactory(siteName: "   ");
        var result = factory.GetMachineIdentifier();
        Assert.AreEqual(MachineInfoFactory.BuildMachineIdentifier(Environment.MachineName, "   "), result);
    }

    [Test]
    public void GetMachineIdentifier_WithSiteName_ReturnsSameResultAsBuildMachineIdentifier()
    {
        var factory = CreateFactory(siteName: "site1");
        var result = factory.GetMachineIdentifier();
        Assert.AreEqual(MachineInfoFactory.BuildMachineIdentifier(Environment.MachineName, "site1"), result);
    }

    [Test]
    public void GetMachineIdentifier_WithSiteNameThatExceedsMaxLength_ThrowsInvalidOperationException()
    {
        var siteName = new string('x', MachineInfoFactory.MaxMachineIdentifierLength);

        var factory = CreateFactory(siteName);

        Assert.Throws<InvalidOperationException>(() => factory.GetMachineIdentifier());
    }

    [Test]
    public void GetMachineIdentifier_WithMachineIdentifier_ReturnsMachineIdentifierDirectly()
    {
        var factory = CreateFactory(siteName: null, machineIdentifier: "my-stable-id");
        Assert.AreEqual("my-stable-id", factory.GetMachineIdentifier());
    }

    [Test]
    public void GetMachineIdentifier_WithMachineIdentifier_AppendsSiteName()
    {
        var factory = CreateFactory(siteName: "site1", machineIdentifier: "my-stable-id");
        Assert.AreEqual("my-stable-id/site1", factory.GetMachineIdentifier());
    }

    [Test]
    public void GetMachineIdentifier_WithMachineIdentifierThatExceedsMaxLength_ThrowsInvalidOperationException()
    {
        var factory = CreateFactory(siteName: null, machineIdentifier: new string('x', MachineInfoFactory.MaxMachineIdentifierLength + 1));
        Assert.Throws<InvalidOperationException>(() => factory.GetMachineIdentifier());
    }

    [Test]
    public void GetMachineIdentifier_WithWebsiteInstanceId_UsesInstanceId()
    {
        Environment.SetEnvironmentVariable(MachineInfoFactory.AzureWebsiteInstanceIdEnvironmentVariable, "abc123instanceid");
        var factory = CreateFactory(siteName: null);
        Assert.AreEqual("abc123instanceid", factory.GetMachineIdentifier());
    }

    [Test]
    public void GetMachineIdentifier_WithWebsiteInstanceIdAndSiteName_UsesInstanceIdSlashSiteName()
    {
        Environment.SetEnvironmentVariable(MachineInfoFactory.AzureWebsiteInstanceIdEnvironmentVariable, "abc123instanceid");
        var factory = CreateFactory(siteName: "mysite");
        Assert.AreEqual("abc123instanceid/mysite", factory.GetMachineIdentifier());
    }

    [Test]
    public void GetMachineIdentifier_WithMachineIdentifierAndWebsiteInstanceId_PrefersMachineIdentifier()
    {
        Environment.SetEnvironmentVariable(MachineInfoFactory.AzureWebsiteInstanceIdEnvironmentVariable, "abc123instanceid");
        var factory = CreateFactory(siteName: null, machineIdentifier: "explicit-override");
        Assert.AreEqual("explicit-override", factory.GetMachineIdentifier());
    }

    private static MachineInfoFactory CreateFactory(string? siteName, string? machineIdentifier = null)
    {
        var hostingEnvironment = Mock.Of<IHostingEnvironment>();
        var hostingSettings = Options.Create(new HostingSettings { SiteName = siteName, MachineIdentifier = machineIdentifier });
        return new MachineInfoFactory(hostingEnvironment, hostingSettings);
    }
}
