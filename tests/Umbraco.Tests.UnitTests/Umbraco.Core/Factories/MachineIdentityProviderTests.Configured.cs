// Copyright (c) Umbraco.
// See LICENSE for more details.

using Microsoft.Extensions.Options;
using NUnit.Framework;
using Umbraco.Cms.Core.Configuration.Models;
using Umbraco.Cms.Core.Factories;

namespace Umbraco.Cms.Tests.UnitTests.Umbraco.Core.Factories;

public partial class MachineIdentityProviderTests
{
    [TestFixture]
    public class ConfiguredProvider
    {
        [Test]
        public void WhenMachineIdentifierIsSet_ReturnsConfiguredValue()
        {
            var provider = new ConfiguredMachineIdentityProvider(
                Options.Create(new HostingSettings { MachineIdentifier = "my-stable-id" }));
            Assert.AreEqual("my-stable-id", provider.GetMachineIdentifier());
        }

        [Test]
        public void WhenMachineIdentifierIsNull_ReturnsNull()
        {
            var provider = new ConfiguredMachineIdentityProvider(
                Options.Create(new HostingSettings { MachineIdentifier = null }));
            Assert.IsNull(provider.GetMachineIdentifier());
        }

        [Test]
        public void WhenMachineIdentifierIsEmptyOrWhitespace_ReturnsNull()
        {
            var provider = new ConfiguredMachineIdentityProvider(
                Options.Create(new HostingSettings { MachineIdentifier = "   " }));
            Assert.IsNull(provider.GetMachineIdentifier());
        }
    }
}
