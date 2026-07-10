// Copyright (c) Umbraco.
// See LICENSE for more details.

using NUnit.Framework;
using Umbraco.Cms.Core.Factories;

namespace Umbraco.Cms.Tests.UnitTests.Umbraco.Core.Factories;

public partial class MachineIdentityProviderTests
{
    [TestFixture]
    public class AzureWebsiteInstanceIdProvider
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
        public void WhenEnvironmentVariableIsSet_ReturnsInstanceId()
        {
            Environment.SetEnvironmentVariable(AzureWebsiteInstanceIdMachineIdentityProvider.WebsiteInstanceIdEnvironmentVariable, "abc123instanceid");
            var provider = new AzureWebsiteInstanceIdMachineIdentityProvider();
            Assert.AreEqual("abc123instanceid", provider.GetMachineIdentifier());
        }

        [Test]
        public void WhenEnvironmentVariableIsAbsent_ReturnsNull()
        {
            var provider = new AzureWebsiteInstanceIdMachineIdentityProvider();
            Assert.IsNull(provider.GetMachineIdentifier());
        }
    }
}
