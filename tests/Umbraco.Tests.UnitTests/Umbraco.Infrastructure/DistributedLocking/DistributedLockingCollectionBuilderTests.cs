using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.DependencyInjection;
using Umbraco.Cms.Core.DistributedLocking;
using Umbraco.Cms.Infrastructure.DistributedLocking;
using Umbraco.Cms.Tests.UnitTests.TestHelpers;

namespace Umbraco.Cms.Tests.UnitTests.Umbraco.Infrastructure.DistributedLocking;

[TestFixture]
public class DistributedLockingCollectionBuilderTests
{
    [Test]
    public void RegisterWith_WithNonExistingImplementationSpecified_ThrowsException()
    {
        var appSettings = new[]
        {
            new KeyValuePair<string, string>(Constants.Configuration.ConfigGlobalDistributedLockingMechanism,
                "BazDistributedLockingMechanism"),
        };

        var services = new ServiceCollection();
        var config = new ConfigurationBuilder()
            .AddInMemoryCollection(appSettings).Build();
        var builder = new UmbracoBuilder(services, config, TestHelper.GetMockedTypeLoader());

        builder.WithCollectionBuilder<DistributedLockingCollectionBuilder>()
            .AddDistributedLockingMechanism<FooDistributedLockingMechanism>()
            .AddDistributedLockingMechanism<BarDistributedLockingMechanism>();

        var ex = Assert.Throws<Exception>(() => builder.Build());
        Assert.That(ex!.Message.Contains("BazDistributedLockingMechanism"));
    }

    [Test]
    public void RegisterWith_WithExistingImplementationSpecified_RegistersOnlySelected()
    {
        var appSettings = new[]
        {
            new KeyValuePair<string, string>(Constants.Configuration.ConfigGlobalDistributedLockingMechanism,
                "BarDistributedLockingMechanism"),
        };

        var services = new ServiceCollection();
        var config = new ConfigurationBuilder()
            .AddInMemoryCollection(appSettings).Build();
        var builder = new UmbracoBuilder(services, config, TestHelper.GetMockedTypeLoader());

        builder.WithCollectionBuilder<DistributedLockingCollectionBuilder>()
            .AddDistributedLockingMechanism<FooDistributedLockingMechanism>()
            .AddDistributedLockingMechanism<BarDistributedLockingMechanism>();

        builder.Build();

        var registered = services.Where(x => x.ServiceType == typeof(IDistributedLockingMechanism)).ToList();

        Assert.Multiple(() =>
        {
            Assert.AreEqual(1, registered.Count);
            Assert.AreEqual(typeof(BarDistributedLockingMechanism), registered.First().ImplementationType);
        });
    }

    [Test]
    public void RegisterWith_WithNomplementationSpecified_RegistersFirstFoundOnly()
    {
        var services = new ServiceCollection();
        var config = new ConfigurationBuilder().Build();
        var builder = new UmbracoBuilder(services, config, TestHelper.GetMockedTypeLoader());

        builder.WithCollectionBuilder<DistributedLockingCollectionBuilder>()
            .AddDistributedLockingMechanism<FooDistributedLockingMechanism>()
            .AddDistributedLockingMechanism<BarDistributedLockingMechanism>();

        builder.Build();

        var registered = services.Where(x => x.ServiceType == typeof(IDistributedLockingMechanism)).ToList();

        Assert.Multiple(() =>
        {
            Assert.AreEqual(1, registered.Count);
            Assert.AreEqual(typeof(FooDistributedLockingMechanism), registered.First().ImplementationType);
        });
    }

    private class FooDistributedLockingMechanism : IDistributedLockingMechanism
    {
        public IDistributedLock ReadLock(int lockId, TimeSpan? obtainLockTimeout = null) =>
            throw new NotImplementedException();

        public IDistributedLock WriteLock(int lockId, TimeSpan? obtainLockTimeout = null) =>
            throw new NotImplementedException();
    }

    private class BarDistributedLockingMechanism : IDistributedLockingMechanism
    {
        public IDistributedLock ReadLock(int lockId, TimeSpan? obtainLockTimeout = null) =>
            throw new NotImplementedException();

        public IDistributedLock WriteLock(int lockId, TimeSpan? obtainLockTimeout = null) =>
            throw new NotImplementedException();
    }
}
