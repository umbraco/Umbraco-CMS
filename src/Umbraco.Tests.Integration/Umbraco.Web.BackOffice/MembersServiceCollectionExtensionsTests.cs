using System;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;
using Umbraco.Extensions;
using Umbraco.Infrastructure.Security;
using Umbraco.Tests.Integration.Testing;
using Umbraco.Web.BackOffice.Extensions;

namespace Umbraco.Tests.Integration.Umbraco.Web.BackOffice
{
    [TestFixture]
    public class MembersServiceCollectionExtensionsTests : UmbracoIntegrationTest
    {
        [Test]
        public void AddMembersIdentity_ExpectMembersUserStoreResolvable()
        {
            IUserStore<MembersIdentityUser> userStore = Services.GetService<IUserStore<MembersIdentityUser>>();

            Assert.IsNotNull(userStore);
            Assert.AreEqual(typeof(MembersUserStore), userStore.GetType());
        }

        [Test]
        public void AddMembersIdentity_ExpectMembersUserManagerResolvable()
        {
            IMembersUserManager userManager = Services.GetService<IMembersUserManager>();

            Assert.NotNull(userManager);
        }

        protected override Action<IServiceCollection> CustomTestSetup => (services) => services.AddMembersIdentity();
    }
}
