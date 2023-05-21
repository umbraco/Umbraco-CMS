// Copyright (c) Umbraco.
// See LICENSE for more details.

using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;
using Umbraco.Cms.Core.DependencyInjection;
using Umbraco.Cms.Core.Security;
using Umbraco.Cms.Tests.Integration.Testing;
using Umbraco.Extensions;

namespace Umbraco.Cms.Tests.Integration.Umbraco.Web.BackOffice;

[TestFixture]
public class UmbracoBackOfficeServiceCollectionExtensionsTests : UmbracoIntegrationTest
{
    protected override void CustomTestSetup(IUmbracoBuilder builder) => builder.AddBackOfficeIdentity();

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
}
