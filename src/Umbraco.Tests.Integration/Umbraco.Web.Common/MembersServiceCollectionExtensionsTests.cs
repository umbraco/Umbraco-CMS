using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;
using Umbraco.Core.DependencyInjection;
using Umbraco.Infrastructure.Security;
using Umbraco.Tests.Integration.Testing;
using Umbraco.Web.Common.DependencyInjection;

namespace Umbraco.Tests.Integration.Umbraco.Web.Common
{
    [TestFixture]
    public class MembersServiceCollectionExtensionsTests : UmbracoIntegrationTest
    {
        protected override void CustomTestSetup(IUmbracoBuilder builder) => builder.Services.AddMembersIdentity();

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
            IMemberManager userManager = Services.GetService<IMemberManager>();

            Assert.NotNull(userManager);
        }
    }
}
