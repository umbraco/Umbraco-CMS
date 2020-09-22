using System;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;
using Umbraco.Extensions;
using Umbraco.Core.BackOffice;
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
        public void AddUmbracoBackOfficeIdentity_ExpectBackOfficeClaimsPrincipalFactoryResolvable()
        {
            var principalFactory = Services.GetService<IUserClaimsPrincipalFactory<BackOfficeIdentityUser>>();

            Assert.IsNotNull(principalFactory);
            Assert.AreEqual(typeof(BackOfficeClaimsPrincipalFactory<BackOfficeIdentityUser>), principalFactory.GetType());
        }

        [Test]
        public void AddUmbracoBackOfficeIdentity_ExpectBackOfficeUserManagerResolvable()
        {
            var userManager = Services.GetService<IBackOfficeUserManager>();

            Assert.NotNull(userManager);
        }

        protected override Action<IServiceCollection> CustomTestSetup => (services) => services.AddUmbracoBackOfficeIdentity();
    }
}
