// Copyright (c) Umbraco.
// See LICENSE for more details.

using System.Linq;
using System.Net.Http;
using NUnit.Framework;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Net;

namespace Umbraco.Cms.Tests.UnitTests.Umbraco.Core.Net;

[TestFixture]
public class SharedHttpClientTests
{
    [Test]
    public void Can_Get_Instance_Configured_With_Umbraco_User_Agent()
    {
        HttpClient client = SharedHttpClient.Instance;

        // The headers must be in place before any caller can observe the instance; configuring them
        // afterwards is what caused #23697.
        Assert.That(
            client.DefaultRequestHeaders.UserAgent.Select(userAgent => userAgent.Product?.Name),
            Does.Contain(Constants.HttpClients.Headers.UserAgentProductName));
    }

    [Test]
    public void Can_Reuse_Same_Instance() => Assert.That(SharedHttpClient.Instance, Is.SameAs(SharedHttpClient.Instance));
}
