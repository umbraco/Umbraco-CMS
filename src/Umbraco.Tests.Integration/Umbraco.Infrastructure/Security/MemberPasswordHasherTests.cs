using System;
using Microsoft.AspNetCore.Identity;
using NUnit.Framework;
using Umbraco.Cms.Core.Security;
using Umbraco.Cms.Infrastructure.Security;

namespace Umbraco.Cms.Tests.Integration.Umbraco.Infrastructure.Security
{
    [TestFixture]
    public class MemberPasswordHasherTests
    {
        private MemberPasswordHasher CreateSut() => new MemberPasswordHasher(new LegacyPasswordSecurity());

        [Test]
        public void VerifyHashedPassword_GivenAnAspNetIdentity2PasswordHash_ThenExpectSuccessRehashNeeded()
        {
            const string password = "Password123!";
            const string hash = "AJszAsQqxOYbASKfL3JVUu6cjU18ouizXDfX4j7wLlir8SWj2yQaTepE9e5bIohIsQ==";

            var sut = CreateSut();
            var result = sut.VerifyHashedPassword(null, hash, password);

            Assert.AreEqual(result, PasswordVerificationResult.SuccessRehashNeeded);
        }

        [Test]
        public void VerifyHashedPassword_GivenAnAspNetCoreIdentityPasswordHash_ThenExpectSuccess()
        {
            const string password = "Password123!";
            const string hash = "AQAAAAEAACcQAAAAEGF/tTVoL6ef3bQPZFYfbgKFu1CDQIAMgyY1N4EDt9jqdG/hsOX93X1U6LNvlIQ3mw==";

            var sut = CreateSut();
            var result = sut.VerifyHashedPassword(null, hash, password);

            Assert.AreEqual(result, PasswordVerificationResult.Success);
        }

        [Test]
        public void VerifyHashedPassword_GivenALegacyPasswordHash_ThenExpectSuccessRehashNeeded()
        {
            const string password = "Password123!";
            const string hash = "yDiU2YyuYZU4jz6F0fpErQ==BxNRHkXBVyJs9gwWF6ktWdfDwYf5bwm+rvV7tOcNNx8=";

            var sut = CreateSut();
            var result = sut.VerifyHashedPassword(null, hash, password);

            Assert.AreEqual(result, PasswordVerificationResult.SuccessRehashNeeded);
        }

        [Test]
        public void VerifyHashedPassword_GivenAnUnknownBase64Hash_ThenExpectInvalidOperationException()
        {
            var hashBytes = new byte[] {3, 2, 1};
            var hash = Convert.ToBase64String(hashBytes);

            var sut = CreateSut();
            Assert.Throws<InvalidOperationException>(() => sut.VerifyHashedPassword(null, hash, "password"));
        }

        [TestCase("AJszAsQqxOYbASKfL3JVUu6cjU18ouizXDfX4j7wLlir8SWj2yQaTepE9e5bIohIsQ==")]
        [TestCase("AQAAAAEAACcQAAAAEGF/tTVoL6ef3bQPZFYfbgKFu1CDQIAMgyY1N4EDt9jqdG/hsOX93X1U6LNvlIQ3mw==")]
        [TestCase("yDiU2YyuYZU4jz6F0fpErQ==BxNRHkXBVyJs9gwWF6ktWdfDwYf5bwm+rvV7tOcNNx8=")]
        public void VerifyHashedPassword_GivenAnInvalidPassword_ThenExpectFailure(string hash)
        {
            const string invalidPassword = "nope";

            var sut = CreateSut();
            var result = sut.VerifyHashedPassword(null, hash, invalidPassword);

            Assert.AreEqual(result, PasswordVerificationResult.Failed);
        }
    }
}
