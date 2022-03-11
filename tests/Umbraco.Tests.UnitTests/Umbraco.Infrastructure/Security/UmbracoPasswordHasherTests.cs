using AutoFixture.NUnit3;
using Microsoft.AspNetCore.Identity;
using Moq;
using NUnit.Framework;
using Umbraco.Cms.Core.Models.Membership;
using Umbraco.Cms.Core.Security;
using Umbraco.Cms.Core.Serialization;
using Umbraco.Cms.Tests.UnitTests.AutoFixture;

namespace Umbraco.Cms.Tests.UnitTests.Umbraco.Infrastructure.Security
{
    [TestFixture]
    public class UmbracoPasswordHasherTests
    {
        // Technically MD5, HMACSHA384 & HMACSHA512 were also possible but opt in as opposed to historic defaults.
        [Test]
        [InlineAutoMoqData("HMACSHA256", "Umbraco9Rocks!", "uB/pLEhhe1W7EtWMv/pSgg==1y8+aso9+h3AKRtJXlVYeg2TZKJUr64hccj82ZZ7Ksk=")] // Actually HMACSHA256
        [InlineAutoMoqData("HMACSHA256", "Umbraco9Rocks!", "t0U8atXTX/efNCtTafukwZeIpr8=")] // v4 site legacy password, with incorrect algorithm specified in database actually HMACSHA1 with password used as key.
        [InlineAutoMoqData("SHA1", "Umbraco9Rocks!", "6tZGfG9NTxJJYp19Fac9og==zzRggqANxhb+CbD/VabEt8cIde8=")] // When SHA1 is set on machine key.
        public void VerifyHashedPassword_WithValidLegacyPasswordHash_ReturnsSuccessRehashNeeded(
            string algorithm,
            string providedPassword,
            string hashedPassword,
            [Frozen] IJsonSerializer jsonSerializer,
            TestUserStub aUser,
            UmbracoPasswordHasher<TestUserStub> sut)
        {
            Mock.Get(jsonSerializer)
                .Setup(x => x.Deserialize<PersistedPasswordSettings>(It.IsAny<string>()))
                .Returns(new PersistedPasswordSettings{ HashAlgorithm = algorithm });

            var result = sut.VerifyHashedPassword(aUser, hashedPassword, providedPassword);

            Assert.AreEqual(PasswordVerificationResult.SuccessRehashNeeded, result);
        }


        [Test]
        [InlineAutoMoqData("PBKDF2.ASPNETCORE.V3", "Umbraco9Rocks!", "AQAAAAEAACcQAAAAEDCrYcnIhHKr38yuchsDu6AFqqmLNvRooKObV25GC1LC1tLY+gWGU4xNug0lc17PHA==")]
        public void VerifyHashedPassword_WithValidModernPasswordHash_ReturnsSuccess(
            string algorithm,
            string providedPassword,
            string hashedPassword,
            [Frozen] IJsonSerializer jsonSerializer,
            TestUserStub aUser,
            UmbracoPasswordHasher<TestUserStub> sut)
        {
            Mock.Get(jsonSerializer)
                .Setup(x => x.Deserialize<PersistedPasswordSettings>(It.IsAny<string>()))
                .Returns(new PersistedPasswordSettings { HashAlgorithm = algorithm });

            var result = sut.VerifyHashedPassword(aUser, hashedPassword, providedPassword);

            Assert.AreEqual(PasswordVerificationResult.Success, result);
        }

        [Test]
        [InlineAutoMoqData("HMACSHA256", "Umbraco9Rocks!", "aB/cDeFaBcDefAbcD/EfaB==1y8+aso9+h3AKRtJXlVYeg2TZKJUr64hccj82ZZ7Ksk=")]
        public void VerifyHashedPassword_WithIncorrectPassword_ReturnsFailed(
            string algorithm,
            string providedPassword,
            string hashedPassword,
            [Frozen] IJsonSerializer jsonSerializer,
            TestUserStub aUser,
            UmbracoPasswordHasher<TestUserStub> sut)
        {
            Mock.Get(jsonSerializer)
                .Setup(x => x.Deserialize<PersistedPasswordSettings>(It.IsAny<string>()))
                .Returns(new PersistedPasswordSettings { HashAlgorithm = algorithm });

            var result = sut.VerifyHashedPassword(aUser, hashedPassword, providedPassword);

            Assert.AreEqual(PasswordVerificationResult.Failed, result);
        }

        public class TestUserStub : UmbracoIdentityUser
        {
            public TestUserStub() => PasswordConfig = "not null or empty";
        }
    }
}
