using NUnit.Framework;
using umbraco.presentation.webservices;

namespace Umbraco.Tests.Cache
{
    [TestFixture]
    public class CacheRefresherTests
    {
        [TestCase("", "123456", "testmachine", true)] //empty hash will continue
        [TestCase("2c8aabac795d189d444a9cdc6e2a1819", "123456", "testmachine", false)] //match, don't continue
        [TestCase("2c8aabac795d189d444a9cdc6e2a1819", "12345", "testmachine", true)]
        [TestCase("2c8aabac795d189d444a9cdc6e2a1819", "123456", "testmachin", true)]
        [TestCase("2c8aabac795d189d444a9cdc6e2a181", "123456", "testmachine", true)]
        public void Continue_Refreshing_For_Request(string hash, string appDomainAppId, string machineName, bool expected)
        {
            var refresher = new CacheRefresher();
            Assert.AreEqual(expected, refresher.ContinueRefreshingForRequest(hash, appDomainAppId, machineName));
        }

    }
}