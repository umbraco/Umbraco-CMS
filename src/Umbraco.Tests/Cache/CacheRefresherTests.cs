using System;
using NUnit.Framework;
using Umbraco.Core.Sync;
using umbraco.presentation.webservices;

namespace Umbraco.Tests.Cache
{
    [TestFixture]
    public class CacheRefresherTests
    {
        [TestCase("", "123456", "testmachine", true)] //empty hash will continue
        [TestCase("2e6deefea4444a69dbd15a01b4c2749d", "123456", "testmachine", false)] //match, don't continue
        [TestCase("2e6deefea4444a69dbd15a01b4c2749d", "12345", "testmachine", true)] // no match, continue
        [TestCase("2e6deefea4444a69dbd15a01b4c2749d", "123456", "testmachin", true)] // same
        [TestCase("2e6deefea4444a69dbd15a01b4c2749", "123456", "testmachine", true)] // same
        public void Continue_Refreshing_For_Request(string hash, string appDomainAppId, string machineName, bool expected)
        {
            if (expected)
                Assert.IsTrue(Continue(hash, WebServiceServerMessenger.GetServerHash(appDomainAppId, machineName)));
            else
                Assert.IsFalse(Continue(hash, WebServiceServerMessenger.GetServerHash(appDomainAppId, machineName)));
        }

        // that's what CacheRefresher.asmx.cs does...
        private bool Continue(string hash1, string hash2)
        {
            if (string.IsNullOrEmpty(hash1)) return true;
            return hash1 != hash2;
        }
    }
}