// Copyright (c) Umbraco.
// See LICENSE for more details.

using System.Security.Cryptography;
using System.Text;
using Moq;
using NUnit.Framework;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Configuration;
using Umbraco.Cms.Core.Security;

namespace Umbraco.Cms.Tests.UnitTests.Umbraco.Core.Security;

/// <summary>
/// Contains unit tests for verifying the functionality of the <see cref="LegacyPasswordSecurity"/> class.
/// </summary>
[TestFixture]
public class LegacyPasswordSecurityTests
{
    /// <summary>
    /// Tests that a password hashed using a non-keyed hash algorithm can be hashed, stored, and verified correctly.
    /// </summary>
    [Test]
    public void Check_Password_Hashed_Non_KeyedHashAlgorithm()
    {
        var passwordConfiguration = Mock.Of<IPasswordConfiguration>(x => x.HashAlgorithmType == "SHA1");
        var passwordSecurity = new LegacyPasswordSecurity();

        var pass = "ThisIsAHashedPassword";
        var hashed = passwordSecurity.HashNewPassword(passwordConfiguration.HashAlgorithmType, pass, out var salt);
        var storedPassword =
            passwordSecurity.FormatPasswordForStorage(passwordConfiguration.HashAlgorithmType, hashed, salt);

        var result = passwordSecurity.VerifyPassword(passwordConfiguration.HashAlgorithmType, "ThisIsAHashedPassword", storedPassword);

        Assert.IsTrue(result);
    }

    /// <summary>
    /// Tests that a password hashed using a keyed hash algorithm can be correctly verified.
    /// </summary>
    [Test]
    public void Check_Password_Hashed_KeyedHashAlgorithm()
    {
        var passwordConfiguration = Mock.Of<IPasswordConfiguration>(x =>
            x.HashAlgorithmType == Constants.Security.AspNetUmbraco8PasswordHashAlgorithmName);
        var passwordSecurity = new LegacyPasswordSecurity();

        var pass = "ThisIsAHashedPassword";
        var hashed = passwordSecurity.HashNewPassword(passwordConfiguration.HashAlgorithmType, pass, out var salt);
        var storedPassword =
            passwordSecurity.FormatPasswordForStorage(passwordConfiguration.HashAlgorithmType, hashed, salt);

        var result = passwordSecurity.VerifyPassword(
            passwordConfiguration.HashAlgorithmType,
            "ThisIsAHashedPassword",
            storedPassword);

        Assert.IsTrue(result);
    }

    /// <summary>
    /// Tests the verification of a legacy v4 SHA1 hashed password.
    /// </summary>
    [Test]
    public void Check_Password_Legacy_v4_SHA1()
    {
        const string clearText = "ThisIsAHashedPassword";
        var clearTextUnicodeBytes = Encoding.Unicode.GetBytes(clearText);
        using var algorithm = new HMACSHA1(clearTextUnicodeBytes);
        var dbPassword = Convert.ToBase64String(algorithm.ComputeHash(clearTextUnicodeBytes));

        var result = new LegacyPasswordSecurity().VerifyLegacyHashedPassword(clearText, dbPassword);

        Assert.IsTrue(result);
    }

    /// <summary>
    /// Tests that the FormatPasswordForStorage method correctly formats a hashed password with a salt.
    /// </summary>
    [Test]
    public void Format_Pass_For_Storage_Hashed()
    {
        var passwordConfiguration = Mock.Of<IPasswordConfiguration>(x =>
            x.HashAlgorithmType == Constants.Security.AspNetUmbraco8PasswordHashAlgorithmName);
        var passwordSecurity = new LegacyPasswordSecurity();

        var salt = LegacyPasswordSecurity.GenerateSalt();
        var stored = "ThisIsAHashedPassword";

        var result = passwordSecurity.FormatPasswordForStorage(passwordConfiguration.HashAlgorithmType, stored, salt);

        Assert.AreEqual(salt + "ThisIsAHashedPassword", result);
    }

    /// <summary>
    /// Verifies that when a stored password string containing a salt and a hashed password is parsed,
    /// the hashed password portion is correctly extracted and returned.
    /// </summary>
    [Test]
    public void Get_Stored_Password_Hashed()
    {
        var passwordConfiguration = Mock.Of<IPasswordConfiguration>(x =>
            x.HashAlgorithmType == Constants.Security.AspNetUmbraco8PasswordHashAlgorithmName);
        var passwordSecurity = new LegacyPasswordSecurity();

        var salt = LegacyPasswordSecurity.GenerateSalt();
        var stored = salt + "ThisIsAHashedPassword";

        var result =
            passwordSecurity.ParseStoredHashPassword(passwordConfiguration.HashAlgorithmType, stored, out _);

        Assert.AreEqual("ThisIsAHashedPassword", result);
    }

    /// <summary>
    ///     The salt generated is always the same length
    /// </summary>
    [Test]
    public void Check_Salt_Length()
    {
        var lastLength = 0;
        for (var i = 0; i < 10000; i++)
        {
            var result = LegacyPasswordSecurity.GenerateSalt();

            if (i > 0)
            {
                Assert.AreEqual(lastLength, result.Length);
            }

            lastLength = result.Length;
        }
    }
}
