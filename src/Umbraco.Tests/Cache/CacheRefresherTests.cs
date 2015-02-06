using NUnit.Framework;
using umbraco.presentation.webservices;

namespace Umbraco.Tests.Cache
{
    [TestFixture]
    public class CacheRefresherTests
    {
        [TestCase("", "123456", "testmachine", true)] //empty hash will continue
        [TestCase("fffffff28449cf33", "123456", "testmachine", false)] //match, don't continue
        [TestCase("fffffff28449cf33", "12345", "testmachine", true)]
        [TestCase("fffffff28449cf33", "123456", "testmachin", true)]
        [TestCase("fffffff28449cf3", "123456", "testmachine", true)]
        public void Continue_Refreshing_For_Request(string hash, string appDomainAppId, string machineName, bool expected)
        {
            var refresher = new CacheRefresher();
            Assert.AreEqual(expected, refresher.ContinueRefreshingForRequest(hash, appDomainAppId, machineName));
        }

    }
}