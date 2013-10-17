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
