﻿using Moq;
using NUnit.Framework;
using Umbraco.Core;
using Umbraco.Core.Configuration;
using Umbraco.Core.Security;

namespace Umbraco.Tests.UnitTests.Umbraco.Core.Security
{
    [TestFixture]
    public class LegacyPasswordSecurityTests
    {

        [Test]
        public void Check_Password_Hashed_Non_KeyedHashAlgorithm()
        {
            var passwordConfiguration = Mock.Of<IPasswordConfiguration>(x => x.HashAlgorithmType == "SHA256");
            var passwordSecurity = new LegacyPasswordSecurity();

            string salt;
            var pass = "ThisIsAHashedPassword";
            var hashed = passwordSecurity.HashNewPassword(passwordConfiguration.HashAlgorithmType, pass, out salt);
            var storedPassword = passwordSecurity.FormatPasswordForStorage(passwordConfiguration.HashAlgorithmType, hashed, salt);

            var result = passwordSecurity.VerifyPassword(passwordConfiguration.HashAlgorithmType, "ThisIsAHashedPassword", storedPassword);

            Assert.IsTrue(result);
        }

        [Test]
        public void Check_Password_Hashed_KeyedHashAlgorithm()
        {
            var passwordConfiguration = Mock.Of<IPasswordConfiguration>(x => x.HashAlgorithmType == Constants.Security.AspNetUmbraco8PasswordHashAlgorithmName);
            var passwordSecurity = new LegacyPasswordSecurity();

            string salt;
            var pass = "ThisIsAHashedPassword";
            var hashed = passwordSecurity.HashNewPassword(passwordConfiguration.HashAlgorithmType, pass, out salt);
            var storedPassword = passwordSecurity.FormatPasswordForStorage(passwordConfiguration.HashAlgorithmType, hashed, salt);

            var result = passwordSecurity.VerifyPassword(passwordConfiguration.HashAlgorithmType, "ThisIsAHashedPassword", storedPassword);

            Assert.IsTrue(result);
        }

        [Test]
        public void Check_Password_Legacy_v4_SHA1()
        {
            var passwordConfiguration = Mock.Of<IPasswordConfiguration>(x => x.HashAlgorithmType == Constants.Security.AspNetUmbraco4PasswordHashAlgorithmName);
            var passwordSecurity = new LegacyPasswordSecurity();

            string salt;
            var pass = "ThisIsAHashedPassword";
            var hashed = passwordSecurity.HashNewPassword(passwordConfiguration.HashAlgorithmType, pass, out salt);
            var storedPassword = passwordSecurity.FormatPasswordForStorage(passwordConfiguration.HashAlgorithmType, hashed, salt);

            var result = passwordSecurity.VerifyPassword(passwordConfiguration.HashAlgorithmType, "ThisIsAHashedPassword", storedPassword);

            Assert.IsTrue(result);
        }

        [Test]
        public void Format_Pass_For_Storage_Hashed()
        {
            var passwordConfiguration = Mock.Of<IPasswordConfiguration>(x => x.HashAlgorithmType == Constants.Security.AspNetUmbraco8PasswordHashAlgorithmName);
            var passwordSecurity = new LegacyPasswordSecurity();

            var salt = LegacyPasswordSecurity.GenerateSalt();
            var stored = "ThisIsAHashedPassword";

            var result = passwordSecurity.FormatPasswordForStorage(passwordConfiguration.HashAlgorithmType, stored, salt);

            Assert.AreEqual(salt + "ThisIsAHashedPassword", result);
        }

        [Test]
        public void Get_Stored_Password_Hashed()
        {
            var passwordConfiguration = Mock.Of<IPasswordConfiguration>(x => x.HashAlgorithmType == Constants.Security.AspNetUmbraco8PasswordHashAlgorithmName);
            var passwordSecurity = new LegacyPasswordSecurity();

            var salt = LegacyPasswordSecurity.GenerateSalt();
            var stored = salt + "ThisIsAHashedPassword";

            string initSalt;
            var result = passwordSecurity.ParseStoredHashPassword(passwordConfiguration.HashAlgorithmType, stored, out initSalt);

            Assert.AreEqual("ThisIsAHashedPassword", result);
        }

        /// <summary>
        /// The salt generated is always the same length
        /// </summary>
        [Test]
        public void Check_Salt_Length()
        {
            var lastLength = 0;
            for (var i = 0; i < 10000; i++)
            {
                var result = LegacyPasswordSecurity.GenerateSalt();

                if (i > 0)
                    Assert.AreEqual(lastLength, result.Length);

                lastLength = result.Length;
            }
        }

    }
}
