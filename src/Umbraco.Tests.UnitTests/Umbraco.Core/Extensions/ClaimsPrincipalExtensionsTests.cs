using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using Umbraco.Extensions;
using Umbraco.Core;
using Umbraco.Core.Security;

namespace Umbraco.Tests.UnitTests.Umbraco.Core.Extensions
{
    [TestFixture]
    public class ClaimsPrincipalExtensionsTests
    {
        [Test]
        public void Get_Remaining_Ticket_Seconds()
        {
            var backOfficeIdentity = new UmbracoBackOfficeIdentity(Constants.Security.SuperUserIdAsString, "test", "test",
                Enumerable.Empty<int>(), Enumerable.Empty<int>(), "en-US", Guid.NewGuid().ToString(),
                Enumerable.Empty<string>(), Enumerable.Empty<string>());
            var principal = new ClaimsPrincipal(backOfficeIdentity);

            var expireSeconds = 99;
            var elapsedSeconds = 3;
            var remainingSeconds = expireSeconds - elapsedSeconds;
            var now = DateTimeOffset.Now;
            var then = now.AddSeconds(elapsedSeconds);
            var expires = now.AddSeconds(expireSeconds).ToString("o");

            backOfficeIdentity.AddClaim(new Claim(
                        Constants.Security.TicketExpiresClaimType,
                        expires,
                        ClaimValueTypes.DateTime,
                        UmbracoBackOfficeIdentity.Issuer,
                        UmbracoBackOfficeIdentity.Issuer,
                        backOfficeIdentity));

            var ticketRemainingSeconds = principal.GetRemainingAuthSeconds(then);

            Assert.AreEqual(remainingSeconds, ticketRemainingSeconds);
        }
    }
}
