using System;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;
using Umbraco.Core.Security;
using Umbraco.Tests.Integration.Testing;
using Umbraco.Web.BackOffice.DependencyInjection;
using Umbraco.Web.Common.Security;

namespace Umbraco.Tests.Integration.Umbraco.Web.BackOffice
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
            Assert.AreEqual(typeof(BackOfficeClaimsPrincipalFactory), principalFactory.GetType());
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
