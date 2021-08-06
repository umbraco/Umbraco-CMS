using System.Linq;
using System.Net;
using Microsoft.Extensions.Options;
using Moq;
using NUnit.Framework;
using Umbraco.Cms.Core.Configuration.Models;
using Umbraco.Cms.Core.Services.Implement;

namespace Umbraco.Cms.Tests.UnitTests.Umbraco.Core.Services
{
    [TestFixture]
    public class BasicAuthServiceTests
    {
        [TestCase(true, ExpectedResult = true)]
        [TestCase(false, ExpectedResult = false)]
        public bool IsBasicAuthEnabled(bool enabled)
        {
           var sut = new BasicAuthService(Mock.Of<IOptionsMonitor<BasicAuthSettings>>(_ => _.CurrentValue == new BasicAuthSettings() {Enabled = enabled}));

           return sut.IsBasicAuthEnabled();
        }

        [TestCase("::1", "1.1.1.1", ExpectedResult = false)]
        [TestCase("::1", "1.1.1.1, ::1", ExpectedResult = true)]
        [TestCase("127.0.0.1", "127.0.0.1, ::1", ExpectedResult = true)]
        [TestCase("127.0.0.1", "", ExpectedResult = false)]
        [TestCase("125.125.125.1", "125.125.125.0/24", ExpectedResult = true)]
        [TestCase("125.125.124.1", "125.125.125.0/24", ExpectedResult = false)]
        public bool IsIpAllowListed(string clientIpAddress, string commaSeperatedAllowlist)
        {
            var allowedIPs = commaSeperatedAllowlist.Split(",").Select(x=>x.Trim()).ToArray();
            var sut = new BasicAuthService(Mock.Of<IOptionsMonitor<BasicAuthSettings>>(_ => _.CurrentValue == new BasicAuthSettings() {AllowedIPs = allowedIPs}));

            return sut.IsIpAllowListed(IPAddress.Parse(clientIpAddress));
        }
    }
}
