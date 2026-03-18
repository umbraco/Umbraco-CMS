// Copyright (c) Umbraco.
// See LICENSE for more details.

using Microsoft.AspNetCore.Authentication;
using NUnit.Framework;
using Umbraco.Cms.Api.Management.Security;
using Umbraco.Cms.Core;

namespace Umbraco.Cms.Tests.UnitTests.Umbraco.Cms.Api.Management.Security;

/// <summary>
/// Contains unit tests for the <see cref="BackOfficeAuthenticationBuilder"/> class in the Umbraco CMS API Management Security namespace.
/// </summary>
[TestFixture]
public class BackOfficeAuthenticationBuilderTests
{
    /// <summary>
    /// Ensures that the back office authentication scheme updates the SignInScheme correctly.
    /// </summary>
    [Test]
    public void EnsureBackOfficeScheme_When_Backoffice_Auth_Scheme_Expect_Updated_SignInScheme()
    {
        var scheme = $"{Constants.Security.BackOfficeExternalAuthenticationTypePrefix}test";
        var options = new RemoteAuthenticationOptions { SignInScheme = "my_cookie" };

        var sut = new BackOfficeAuthenticationBuilder.EnsureBackOfficeScheme<RemoteAuthenticationOptions>();
        sut.PostConfigure(scheme, options);

        Assert.AreEqual(options.SignInScheme, Constants.Security.BackOfficeExternalAuthenticationType);
    }

    /// <summary>
    /// Ensures that when the authentication scheme is not the backoffice scheme, no changes are made to the options.
    /// </summary>
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
