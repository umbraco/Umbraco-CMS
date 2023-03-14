// Copyright (c) Umbraco.
// See LICENSE for more details.

using System;
using System.Text;
using Microsoft.AspNetCore.Http;
using NUnit.Framework;
using Umbraco.Extensions;

namespace Umbraco.Cms.Tests.UnitTests.Umbraco.Web.Common.Extensions;

[TestFixture]
public class HttpContextExtensionTests
{
    [Test]
    public void TryGetBasicAuthCredentials_WithoutHeader_ReturnsFalse()
    {
        var httpContext = new DefaultHttpContext();

        var result = httpContext.TryGetBasicAuthCredentials(out var _, out var _);

        Assert.IsFalse(result);
    }

    [Test]
    public void TryGetBasicAuthCredentials_WithHeader_ReturnsTrueWithCredentials()
    {
        const string testUsername = "fred";
        const string testPassword = "test";

        var httpContext = new DefaultHttpContext();
        var credentials = Convert.ToBase64String(Encoding.ASCII.GetBytes($"{testUsername}:{testPassword}"));
        httpContext.Request.Headers.Add("Authorization", $"Basic {credentials}");

        var result = httpContext.TryGetBasicAuthCredentials(out var username, out var password);

        Assert.IsTrue(result);
        Assert.AreEqual(testUsername, username);
        Assert.AreEqual(testPassword, password);
    }
}
