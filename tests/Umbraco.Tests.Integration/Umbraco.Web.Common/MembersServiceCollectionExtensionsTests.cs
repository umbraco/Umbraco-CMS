using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;
using Umbraco.Cms.Core.DependencyInjection;
using Umbraco.Cms.Core.Security;
using Umbraco.Cms.Tests.Integration.Testing;
using Umbraco.Extensions;

namespace Umbraco.Tests.Integration.Umbraco.Web.Common;

[TestFixture]
public class MembersServiceCollectionExtensionsTests : UmbracoIntegrationTest
{
    protected override void CustomTestSetup(IUmbracoBuilder builder) => builder.AddMembersIdentity();

    [Test]
    public void AddMembersIdentity_ExpectMembersUserStoreResolvable()
    {
        var userStore = Services.GetService<IUserStore<MemberIdentityUser>>();

        Assert.IsNotNull(userStore);
        Assert.AreEqual(typeof(MemberUserStore), userStore.GetType());
    }

    [Test]
    public void AddMembersIdentity_ExpectMembersUserManagerResolvable()
    {
        var userManager = Services.GetService<IMemberManager>();

        Assert.NotNull(userManager);
    }
}
