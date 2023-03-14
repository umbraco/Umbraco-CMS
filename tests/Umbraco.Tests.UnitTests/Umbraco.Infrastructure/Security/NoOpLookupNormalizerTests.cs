// Copyright (c) Umbraco.
// See LICENSE for more details.

using System;
using NUnit.Framework;
using Umbraco.Cms.Core.Security;

namespace Umbraco.Cms.Tests.UnitTests.Umbraco.Infrastructure.Security;

public class NoopLookupNormalizerTests
{
    [Test]
    public void NormalizeName_Expect_Input_Returned()
    {
        var name = Guid.NewGuid().ToString();
        var sut = new NoopLookupNormalizer();

        var normalizedName = sut.NormalizeName(name);

        Assert.AreEqual(name, normalizedName);
    }

    [Test]
    public void NormalizeEmail_Expect_Input_Returned()
    {
        var email = $"{Guid.NewGuid()}@umbraco";
        var sut = new NoopLookupNormalizer();

        var normalizedEmail = sut.NormalizeEmail(email);

        Assert.AreEqual(email, normalizedEmail);
    }

    [Test]
    [TestCase(null)]
    [TestCase("")]
    [TestCase(" ")]
    public void NormalizeName_When_Name_Null_Or_Whitespace_Expect_Same_Returned(string name)
    {
        var sut = new NoopLookupNormalizer();

        var normalizedName = sut.NormalizeName(name);

        Assert.AreEqual(name, normalizedName);
    }

    [Test]
    [TestCase(null)]
    [TestCase("")]
    [TestCase(" ")]
    public void NormalizeEmail_When_Name_Null_Or_Whitespace_Expect_Same_Returned(string email)
    {
        var sut = new NoopLookupNormalizer();

        var normalizedEmail = sut.NormalizeEmail(email);

        Assert.AreEqual(email, normalizedEmail);
    }
}
