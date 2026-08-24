// Copyright (c) Umbraco.
// See LICENSE for more details.

using Moq;
using NUnit.Framework;
using Umbraco.Cms.Core.Security;
using Umbraco.Extensions;

namespace Umbraco.Cms.Tests.UnitTests.Umbraco.Core.Extensions;

[TestFixture]
public class CspNonceServiceExtensionsTests
{
    [Test]
    public void Can_Get_Nonce_Attribute_When_Nonce_Is_Available()
    {
        ICspNonceService cspNonceService = CreateCspNonceService("s0m3-n0nc3");

        var result = cspNonceService.GetNonceAttribute();

        Assert.That(result, Is.EqualTo(@" nonce=""s0m3-n0nc3"""));
    }

    [TestCase(null)]
    [TestCase("")]
    public void Cannot_Get_Nonce_Attribute_When_No_Nonce_Is_Available(string? nonce)
    {
        ICspNonceService cspNonceService = CreateCspNonceService(nonce);

        var result = cspNonceService.GetNonceAttribute();

        Assert.That(result, Is.Empty);
    }

    [Test]
    public void Can_Get_Nonce_Attribute_For_Base64_Nonce_Without_Escaping()
    {
        ICspNonceService cspNonceService = CreateCspNonceService("bJ+g/tCnZ0k=");

        var result = cspNonceService.GetNonceAttribute();

        Assert.That(result, Is.EqualTo(@" nonce=""bJ+g/tCnZ0k="""));
    }

    [Test]
    public void Can_Get_Nonce_Attribute_With_Html_Encoded_Nonce()
    {
        ICspNonceService cspNonceService = CreateCspNonceService(@"""><script>alert(1)</script>");

        var result = cspNonceService.GetNonceAttribute();

        Assert.Multiple(() =>
        {
            Assert.That(result, Is.EqualTo(@" nonce=""&quot;&gt;&lt;script&gt;alert(1)&lt;/script&gt;"""));
            Assert.That(result.Count(c => c == '"'), Is.EqualTo(2), "The nonce must not be able to close the attribute.");
        });
    }

    private static ICspNonceService CreateCspNonceService(string? nonce)
        => Mock.Of<ICspNonceService>(x => x.GetNonce() == nonce);
}
