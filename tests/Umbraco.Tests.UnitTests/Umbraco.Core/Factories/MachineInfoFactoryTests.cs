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
    [Test]
    public void BuildMachineIdentifier_WithNoSiteName_ReturnsMachineNameOnly()
    {
        var result = MachineInfoFactory.BuildMachineIdentifier("SERVER01", null);
        Assert.That(result, Is.EqualTo("SERVER01"));
    }

    [Test]
    public void BuildMachineIdentifier_WithEmptySiteName_ReturnsMachineNameOnly()
    {
        var result = MachineInfoFactory.BuildMachineIdentifier("SERVER01", string.Empty);
        Assert.That(result, Is.EqualTo("SERVER01"));
    }

    [Test]
    public void BuildMachineIdentifier_WithWhitespaceSiteName_ReturnsMachineNameOnly()
    {
        var result = MachineInfoFactory.BuildMachineIdentifier("SERVER01", "   ");
        Assert.That(result, Is.EqualTo("SERVER01"));
    }

    [Test]
    public void BuildMachineIdentifier_WithSiteName_ReturnsMachineNameSlashSiteName()
    {
        var result = MachineInfoFactory.BuildMachineIdentifier("SERVER01", "site1");
        Assert.That(result, Is.EqualTo("SERVER01/site1"));
    }

    [Test]
    public void GetMachineIdentifier_WithNoSiteName_ReturnsSameResultAsBuildMachineIdentifier()
    {
        var factory = CreateFactory(siteName: null);
        var result = factory.GetMachineIdentifier();
        Assert.That(result, Is.EqualTo(MachineInfoFactory.BuildMachineIdentifier(Environment.MachineName, null)));
    }

    [Test]
    public void GetMachineIdentifier_WithEmptySiteName_ReturnsSameResultAsBuildMachineIdentifier()
    {
        var factory = CreateFactory(siteName: string.Empty);
        var result = factory.GetMachineIdentifier();
        Assert.That(result, Is.EqualTo(MachineInfoFactory.BuildMachineIdentifier(Environment.MachineName, string.Empty)));
    }

    [Test]
    public void GetMachineIdentifier_WithWhitespaceSiteName_ReturnsSameResultAsBuildMachineIdentifier()
    {
        var factory = CreateFactory(siteName: "   ");
        var result = factory.GetMachineIdentifier();
        Assert.That(result, Is.EqualTo(MachineInfoFactory.BuildMachineIdentifier(Environment.MachineName, "   ")));
    }

    [Test]
    public void GetMachineIdentifier_WithSiteName_ReturnsSameResultAsBuildMachineIdentifier()
    {
        var factory = CreateFactory(siteName: "site1");
        var result = factory.GetMachineIdentifier();
        Assert.That(result, Is.EqualTo(MachineInfoFactory.BuildMachineIdentifier(Environment.MachineName, "site1")));
    }

    [Test]
    public void GetMachineIdentifier_WithSiteNameThatExceedsMaxLength_ThrowsInvalidOperationException()
    {
        var siteName = new string('x', MachineInfoFactory.MaxMachineIdentifierLength);

        var factory = CreateFactory(siteName);

        Assert.Throws<InvalidOperationException>(() => factory.GetMachineIdentifier());
    }

    private static MachineInfoFactory CreateFactory(string? siteName)
    {
        var hostingEnvironment = Mock.Of<IHostingEnvironment>();
        var hostingSettings = Options.Create(new HostingSettings { SiteName = siteName });
        return new MachineInfoFactory(hostingEnvironment, hostingSettings);
    }
}
