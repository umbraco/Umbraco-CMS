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
internal sealed class UmbracoBackOfficeServiceCollectionExtensionsTests : UmbracoIntegrationTest
{
    protected override void CustomTestSetup(IUmbracoBuilder builder) => builder.AddBackOfficeIdentity();

    [Test]
    public void AddUmbracoBackOfficeIdentity_ExpectBackOfficeUserStoreResolvable()
    {
        var userStore = Services.GetService<IUserStore<BackOfficeIdentityUser>>();

        Assert.That(userStore, Is.Not.Null);
        Assert.That(userStore.GetType(), Is.EqualTo(typeof(BackOfficeUserStore)));
    }

    [Test]
    public void AddUmbracoBackOfficeIdentity_ExpectBackOfficeClaimsPrincipalFactoryResolvable()
    {
        var principalFactory = Services.GetService<IUserClaimsPrincipalFactory<BackOfficeIdentityUser>>();

        Assert.That(principalFactory, Is.Not.Null);
        Assert.That(principalFactory.GetType(), Is.EqualTo(typeof(BackOfficeClaimsPrincipalFactory)));
    }

    [Test]
    public void AddUmbracoBackOfficeIdentity_ExpectBackOfficeUserManagerResolvable()
    {
        var userManager = Services.GetService<IBackOfficeUserManager>();

        Assert.That(userManager, Is.Not.Null);
    }
}
