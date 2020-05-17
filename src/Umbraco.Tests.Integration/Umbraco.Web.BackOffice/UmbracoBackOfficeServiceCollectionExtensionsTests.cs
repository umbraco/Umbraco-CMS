using System;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Moq;
using NUnit.Framework;
using Umbraco.Extensions;
using Umbraco.Core.BackOffice;
using Umbraco.Core.Cache;
using Umbraco.Core.Composing;
using Umbraco.Tests.Integration.Extensions;
using Umbraco.Tests.Integration.Implementations;
using Umbraco.Tests.Integration.Testing;

namespace Umbraco.Tests.UnitTests.Umbraco.Web.BackOffice.Extensions
{
    [TestFixture]
    public class UmbracoBackOfficeServiceCollectionExtensionsTests : UmbracoIntegrationTest
    {
        [Test]
        public void AddUmbracoBackOfficeIdentity_ExpectBackOfficeUserStoreResolvable()
        {
            var userStore = Services.GetService<IUserStore<BackOfficeIdentityUser>>();

            Assert.IsNotNull(userStore);
            Assert.AreEqual(typeof(BackOfficeUserStore), userStore.GetType());
        }

        [Test]
        public void AddUmbracoBackOfficeIdentity_ExpectBackOfficeUserManagerResolvable()
        {
            var userManager = Services.GetService<BackOfficeUserManager>();

            Assert.NotNull(userManager);
        }

        protected override Action<IServiceCollection> CustomTestSetup => (services) => services.AddUmbracoBackOfficeIdentity();
    }
}
