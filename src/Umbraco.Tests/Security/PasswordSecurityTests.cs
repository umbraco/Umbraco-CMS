using Moq;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Umbraco.Core.Configuration;
using Umbraco.Core.Security;

namespace Umbraco.Tests.Security
{
    [TestFixture]
    public class PasswordSecurityTests
    {
        [Test]
        public void Get_Hash_Algorithm_Legacy()
        {
            var passwordSecurity = new PasswordSecurity(Mock.Of<IPasswordConfiguration>(x => x.UseLegacyEncoding == true && x.HashAlgorithmType == "HMACSHA256"));
            var alg = passwordSecurity.GetHashAlgorithm("blah");
            Assert.IsTrue(alg is HMACSHA1);
        }

        [Test]
        public void Get_Hash_Algorithm_Default()
        {
            var passwordSecurity = new PasswordSecurity(Mock.Of<IPasswordConfiguration>(x => x.HashAlgorithmType == "HMACSHA256"));
            var alg = passwordSecurity.GetHashAlgorithm("blah"); // not resolved
            Assert.IsTrue(alg is HMACSHA256);
        }

        [Test]
        public void Check_Password_Hashed_Non_KeyedHashAlgorithm()
        {
            var passwordSecurity = new PasswordSecurity(Mock.Of<IPasswordConfiguration>(x => x.HashAlgorithmType == "SHA256"));

            string salt;
            var pass = "ThisIsAHashedPassword";
            var hashed = passwordSecurity.HashNewPassword(pass, out salt);
            var storedPassword = passwordSecurity.FormatPasswordForStorage(hashed, salt);

            var result = passwordSecurity.CheckPassword("ThisIsAHashedPassword", storedPassword);

            Assert.IsTrue(result);
        }

        [Test]
        public void Check_Password_Hashed_KeyedHashAlgorithm()
        {
            var passwordSecurity = new PasswordSecurity(Mock.Of<IPasswordConfiguration>(x => x.HashAlgorithmType == "HMACSHA256"));

            string salt;
            var pass = "ThisIsAHashedPassword";
            var hashed = passwordSecurity.HashNewPassword(pass, out salt);
            var storedPassword = passwordSecurity.FormatPasswordForStorage(hashed, salt);

            var result = passwordSecurity.CheckPassword("ThisIsAHashedPassword", storedPassword);

            Assert.IsTrue(result);
        }

        [Test]
        public void Format_Pass_For_Storage_Hashed()
        {
            var passwordSecurity = new PasswordSecurity(Mock.Of<IPasswordConfiguration>(x => x.HashAlgorithmType == "HMACSHA256"));

            var salt = PasswordSecurity.GenerateSalt();
            var stored = "ThisIsAHashedPassword";

            var result = passwordSecurity.FormatPasswordForStorage(stored, salt);

            Assert.AreEqual(salt + "ThisIsAHashedPassword", result);
        }

        [Test]
        public void Get_Stored_Password_Hashed()
        {
            var passwordSecurity = new PasswordSecurity(Mock.Of<IPasswordConfiguration>(x => x.HashAlgorithmType == "HMACSHA256"));

            var salt = PasswordSecurity.GenerateSalt();
            var stored = salt + "ThisIsAHashedPassword";

            string initSalt;
            var result = passwordSecurity.StoredPassword(stored, out initSalt);

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
                var result = PasswordSecurity.GenerateSalt();

                if (i > 0)
                {
                    Assert.AreEqual(lastLength, result.Length);
                }

                lastLength = result.Length;
            }
        }

    }
}
