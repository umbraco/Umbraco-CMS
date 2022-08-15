using AutoFixture.NUnit3;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using Moq;
using NUnit.Framework;
using Umbraco.Cms.Core.Models.Membership;
using Umbraco.Cms.Core.Security;
using Umbraco.Cms.Core.Serialization;
using Umbraco.Cms.Tests.UnitTests.AutoFixture;

namespace Umbraco.Cms.Tests.UnitTests.Umbraco.Infrastructure.Security;

[TestFixture]
public class UmbracoPasswordHasherTests
{
    // Technically MD5, HMACSHA384 & HMACSHA512 were also possible but opt in as opposed to historic defaults.
    [Test]
    [InlineAutoMoqData("HMACSHA256", "Umbraco9Rocks!", "uB/pLEhhe1W7EtWMv/pSgg==1y8+aso9+h3AKRtJXlVYeg2TZKJUr64hccj82ZZ7Ksk=")] // Actually HMACSHA256
    [InlineAutoMoqData("SHA1", "Umbraco9Rocks!", "6tZGfG9NTxJJYp19Fac9og==zzRggqANxhb+CbD/VabEt8cIde8=")] // When SHA1 is set on machine key.
    public void VerifyHashedPassword_ValidHashWithoutLegacyEncoding_ReturnsSuccessRehashNeeded(
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

        Assert.AreEqual(PasswordVerificationResult.SuccessRehashNeeded, result);
    }

    [Test]
    [InlineAutoMoqData("HMACSHA1", "Umbraco9Rocks!", "t0U8atXTX/efNCtTafukwZeIpr8=")]
    [InlineAutoMoqData("HMACSHA256", "Umbraco9Rocks!", "t0U8atXTX/efNCtTafukwZeIpr8=")]
    [InlineAutoMoqData("FOOBARBAZQUX", "Umbraco9Rocks!", "t0U8atXTX/efNCtTafukwZeIpr8=")]
    [InlineAutoMoqData("", "Umbraco9Rocks!", "t0U8atXTX/efNCtTafukwZeIpr8=")]
    [InlineAutoMoqData(null, "Umbraco9Rocks!", "t0U8atXTX/efNCtTafukwZeIpr8=")]
    public void VerifyHashedPassword_ValidHashWithLegacyEncoding_ReturnsSuccessRehashNeeded(
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

        Assert.AreEqual(PasswordVerificationResult.SuccessRehashNeeded, result);
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

    [Test]
    [AutoMoqData]
    public void VerifyHashedPassword_WithIdentityV1OrV2StyleHash_ReturnsSuccessRehashNeeded(
        TestUserStub aUser,
        UmbracoPasswordHasher<TestUserStub> sut)
    {
        var options = Options.Create(new PasswordHasherOptions
        {
            CompatibilityMode = PasswordHasherCompatibilityMode.IdentityV2,
        });

        var upstreamHasher = new PasswordHasher<TestUserStub>(options);

        const string password = "Umbraco9Rocks!";
        var identityV1Or2StyleHash = upstreamHasher.HashPassword(aUser, password);
        var result = sut.VerifyHashedPassword(aUser, identityV1Or2StyleHash, password);

        Assert.AreEqual(PasswordVerificationResult.SuccessRehashNeeded, result);
    }

    [Test]
    [AutoMoqData]
    public void VerifyHashedPassword_WithIdentityV3StyleHash_ReturnsSuccess(
        TestUserStub aUser,
        UmbracoPasswordHasher<TestUserStub> sut)
    {
        var options = Options.Create(new PasswordHasherOptions
        {
            CompatibilityMode = PasswordHasherCompatibilityMode.IdentityV3,
        });

        var upstreamHasher = new PasswordHasher<TestUserStub>(options);

        const string password = "Umbraco9Rocks!";
        var identityV1Or2StyleHash = upstreamHasher.HashPassword(aUser, password);
        var result = sut.VerifyHashedPassword(aUser, identityV1Or2StyleHash, password);

        Assert.AreEqual(PasswordVerificationResult.Success, result);
    }

    public class TestUserStub : UmbracoIdentityUser
    {
        public TestUserStub() => PasswordConfig = "not null or empty";
    }
}
