// Copyright (c) Umbraco.
// See LICENSE for more details.

using Microsoft.AspNetCore.Authentication;
using NUnit.Framework;
using Umbraco.Cms.Core;
using Umbraco.Cms.Web.BackOffice.Security;

namespace Umbraco.Cms.Tests.UnitTests.Umbraco.Web.BackOffice.Security;

[TestFixture]
public class BackOfficeAuthenticationBuilderTests
{
    [Test]
    public void EnsureBackOfficeScheme_When_Backoffice_Auth_Scheme_Expect_Updated_SignInScheme()
    {
        var scheme = $"{Constants.Security.BackOfficeExternalAuthenticationTypePrefix}test";
        var options = new RemoteAuthenticationOptions { SignInScheme = "my_cookie" };

        var sut = new BackOfficeAuthenticationBuilder.EnsureBackOfficeScheme<RemoteAuthenticationOptions>();
        sut.PostConfigure(scheme, options);

        Assert.AreEqual(options.SignInScheme, Constants.Security.BackOfficeExternalAuthenticationType);
    }

    [Test]
    public void EnsureBackOfficeScheme_When_Not_Backoffice_Auth_Scheme_Expect_No_Change()
    {
        var scheme = "test";
        var options = new RemoteAuthenticationOptions { SignInScheme = "my_cookie" };

        var sut = new BackOfficeAuthenticationBuilder.EnsureBackOfficeScheme<RemoteAuthenticationOptions>();
        sut.PostConfigure(scheme, options);

        Assert.AreNotEqual(options.SignInScheme, Constants.Security.BackOfficeExternalAuthenticationType);
    }
}
