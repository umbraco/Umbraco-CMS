// Copyright (c) Umbraco.
// See LICENSE for more details.

using NUnit.Framework;
using Umbraco.Cms.Core.Factories;

namespace Umbraco.Cms.Tests.UnitTests.Umbraco.Core.Factories;

public partial class MachineIdentityProviderTests
{
    [TestFixture]
    public class DefaultProvider
    {
        [Test]
        public void AlwaysReturnsMachineName()
        {
            var provider = new DefaultMachineIdentityProvider();
            Assert.AreEqual(Environment.MachineName, provider.GetMachineIdentifier());
        }
    }
}
