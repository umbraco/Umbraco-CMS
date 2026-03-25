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
    public void GetMachineIdentifier_WithNoSiteNameConfigured_ReturnsMachineNameOnly()
    {
        var factory = CreateFactory(siteName: null);

        var result = factory.GetMachineIdentifier();

        Assert.AreEqual(Environment.MachineName, result);
    }

    [Test]
    public void GetMachineIdentifier_WithEmptySiteName_ReturnsMachineNameOnly()
    {
        var factory = CreateFactory(siteName: string.Empty);

        var result = factory.GetMachineIdentifier();

        Assert.AreEqual(Environment.MachineName, result);
    }

    [Test]
    public void GetMachineIdentifier_WithWhitespaceSiteName_ReturnsMachineNameOnly()
    {
        var factory = CreateFactory(siteName: "   ");

        var result = factory.GetMachineIdentifier();

        Assert.AreEqual(Environment.MachineName, result);
    }

    [Test]
    public void GetMachineIdentifier_WithSiteNameConfigured_ReturnsMachineNameSlashSiteName()
    {
        var factory = CreateFactory(siteName: "site1");

        var result = factory.GetMachineIdentifier();

        Assert.AreEqual($"{Environment.MachineName}/site1", result);
    }

    [Test]
    public void GetMachineIdentifier_TwoInstancesWithDifferentSiteNames_ReturnsDistinctIdentifiers()
    {
        var factory1 = CreateFactory(siteName: "site1");
        var factory2 = CreateFactory(siteName: "site2");

        var id1 = factory1.GetMachineIdentifier();
        var id2 = factory2.GetMachineIdentifier();

        Assert.AreNotEqual(id1, id2);
    }

    private static MachineInfoFactory CreateFactory(string? siteName)
    {
        var hostingEnvironment = Mock.Of<IHostingEnvironment>();
        var hostingSettings = Options.Create(new HostingSettings { SiteName = siteName });
        return new MachineInfoFactory(hostingEnvironment, hostingSettings);
    }
}
