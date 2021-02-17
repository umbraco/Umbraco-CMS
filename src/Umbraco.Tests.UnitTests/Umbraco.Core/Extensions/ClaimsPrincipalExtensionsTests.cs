// Copyright (c) Umbraco.
// See LICENSE for more details.

using System;
using System.Linq;
using System.Security.Claims;
using NUnit.Framework;
using Umbraco.Core;
using Umbraco.Core.Security;
using Umbraco.Extensions;
using ClaimsIdentityExtensions = Umbraco.Extensions.ClaimsIdentityExtensions;

namespace Umbraco.Tests.UnitTests.Umbraco.Core.Extensions
{
    [TestFixture]
    public class ClaimsPrincipalExtensionsTests
    {
        [Test]
        public void Get_Remaining_Ticket_Seconds()
        {
            var backOfficeIdentity = new ClaimsIdentity();
            backOfficeIdentity.AddRequiredClaims(
                Constants.Security.SuperUserIdAsString,
                "test",
                "test",
                Enumerable.Empty<int>(),
                Enumerable.Empty<int>(),
                "en-US",
                Guid.NewGuid().ToString(),
                Enumerable.Empty<string>(),
                Enumerable.Empty<string>());

            var principal = new ClaimsPrincipal(backOfficeIdentity);

            var expireSeconds = 99;
            var elapsedSeconds = 3;
            var remainingSeconds = expireSeconds - elapsedSeconds;
            DateTimeOffset now = DateTimeOffset.Now;
            DateTimeOffset then = now.AddSeconds(elapsedSeconds);
            var expires = now.AddSeconds(expireSeconds).ToString("o");

            backOfficeIdentity.AddClaim(new Claim(
                        Constants.Security.TicketExpiresClaimType,
                        expires,
                        ClaimValueTypes.DateTime,
                        ClaimsIdentityExtensions.Issuer,
                        ClaimsIdentityExtensions.Issuer,
                        backOfficeIdentity));

            var ticketRemainingSeconds = principal.GetRemainingAuthSeconds(then);

            Assert.AreEqual(remainingSeconds, ticketRemainingSeconds);
        }
    }
}
