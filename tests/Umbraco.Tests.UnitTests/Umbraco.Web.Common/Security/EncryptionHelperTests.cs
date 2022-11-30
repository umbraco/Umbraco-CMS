// Copyright (c) Umbraco.
// See LICENSE for more details.

using System.Collections.Generic;
using Microsoft.AspNetCore.DataProtection;
using NUnit.Framework;
using Umbraco.Cms.Web.Common.Security;

namespace Umbraco.Cms.Tests.UnitTests.Umbraco.Web.Common.Security;

[TestFixture]
public class EncryptionHelperTests
{
    private IDataProtectionProvider DataProtectionProvider { get; } = new EphemeralDataProtectionProvider();

    [Test]
    public void Create_Encrypted_RouteString_From_Anonymous_Object()
    {
        var additionalRouteValues = new { key1 = "value1", key2 = "value2", Key3 = "Value3", keY4 = "valuE4" };

        var encryptedRouteString = EncryptionHelper.CreateEncryptedRouteString(
            DataProtectionProvider,
            "FormController",
            "FormAction",
            string.Empty,
            additionalRouteValues);

        var result = EncryptionHelper.Decrypt(encryptedRouteString, DataProtectionProvider);

        const string expectedResult =
            "c=FormController&a=FormAction&ar=&key1=value1&key2=value2&Key3=Value3&keY4=valuE4";

        Assert.AreEqual(expectedResult, result);
    }

    [Test]
    public void Create_Encrypted_RouteString_From_Dictionary()
    {
        var additionalRouteValues = new Dictionary<string, object>
        {
            { "key1", "value1" }, { "key2", "value2" }, { "Key3", "Value3" }, { "keY4", "valuE4" },
        };

        var encryptedRouteString = EncryptionHelper.CreateEncryptedRouteString(
            DataProtectionProvider,
            "FormController",
            "FormAction",
            string.Empty,
            additionalRouteValues);

        var result = EncryptionHelper.Decrypt(encryptedRouteString, DataProtectionProvider);

        const string expectedResult =
            "c=FormController&a=FormAction&ar=&key1=value1&key2=value2&Key3=Value3&keY4=valuE4";

        Assert.AreEqual(expectedResult, result);
    }
}
