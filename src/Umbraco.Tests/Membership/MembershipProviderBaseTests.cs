using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Security;
using NUnit.Framework;
using Umbraco.Core.Security;
using Umbraco.Web.Security.Providers;

namespace Umbraco.Tests.Membership
{
    [TestFixture]
    public class MembershipProviderBaseTests
    {
        [TestCase("hello", 0, "", 5, true)]
        [TestCase("hello", 0, "", 4, true)]
        [TestCase("hello", 0, "", 6, false)]
        [TestCase("hello", 1, "", 5, false)]
        [TestCase("hello!", 1, "", 0, true)]
        [TestCase("hello!", 2, "", 0, false)]
        [TestCase("hello!!", 2, "", 0, true)]
        //8 characters or more in length, at least 1 lowercase letter,at least 1 character that is not a lower letter.
        [TestCase("hello", 0, "(?=.{8,})[a-z]+[^a-z]+|[^a-z]+[a-z]+", 0, false)]
        [TestCase("helloooo", 0, "(?=.{8,})[a-z]+[^a-z]+|[^a-z]+[a-z]+", 0, false)]
        [TestCase("helloooO", 0, "(?=.{8,})[a-z]+[^a-z]+|[^a-z]+[a-z]+", 0, true)]
        [TestCase("HELLOOOO", 0, "(?=.{8,})[a-z]+[^a-z]+|[^a-z]+[a-z]+", 0, false)]
        [TestCase("HELLOOOo", 0, "(?=.{8,})[a-z]+[^a-z]+|[^a-z]+[a-z]+", 0, true)]
        public void Valid_Password(string password, int minRequiredNonAlphanumericChars, string strengthRegex, int minLength, bool pass)
        {
            var result = MembershipProviderBase.IsPasswordValid(password, minRequiredNonAlphanumericChars, strengthRegex, minLength);
            Assert.AreEqual(pass, result.Success);
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
                var result = MembershipProviderBase.GenerateSalt();
                
                if (i > 0)
                {
                    Assert.AreEqual(lastLength, result.Length);
                }

                lastLength = result.Length;                
            }
        }

        [Test]
        public void Get_StoredPassword()
        {
            var salt = MembershipProviderBase.GenerateSalt();
            var stored = salt + "ThisIsAHashedPassword";

            string initSalt;
            var result = MembershipProviderBase.StoredPassword(stored, MembershipPasswordFormat.Hashed, out initSalt);

            Assert.AreEqual(salt, initSalt);
        }

    }
}
