using System;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;
using Umbraco.Core.Members;
using Umbraco.Extensions;
using Umbraco.Infrastructure.Members;
using Umbraco.Tests.Integration.Testing;

namespace Umbraco.Tests.Integration.Umbraco.Web.BackOffice
{
    [TestFixture]
    public class UmbracoMembersServiceCollectionExtensionsTests : UmbracoIntegrationTest
    {
        [Test]
        public void AddUmbracoMembersIdentity_ExpectUmbracoMembersUserStoreResolvable()
        {
            var userStore = Services.GetService<IUserStore<UmbracoMembersIdentityUser>>();

            Assert.IsNotNull(userStore);
            Assert.AreEqual(typeof(UmbracoMembersUserStore), userStore.GetType());
        }

        //[Test]
        //public void AddUmbracoMembersIdentity_ExpectUmbracoMembersClaimsPrincipalFactoryResolvable()
        //{
        //    var principalFactory = Services.GetService<IUserClaimsPrincipalFactory<UmbracoMembersIdentityUser>>();

        //    Assert.IsNotNull(principalFactory);
        //    Assert.AreEqual(typeof(UmbracoMembersClaimsPrincipalFactory<UmbracoMembersIdentityUser>), principalFactory.GetType());
        //}

        [Test]
        public void AddUmbracoMembersIdentity_ExpectUmbracomMembersUserManagerResolvable()
        {
            var userManager = Services.GetService<IUmbracoMembersUserManager>();

            Assert.NotNull(userManager);
        }

        protected override Action<IServiceCollection> CustomTestSetup => (services) => services.AddUmbracoMembersIdentity();
    }
}
