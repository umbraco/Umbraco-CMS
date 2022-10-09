using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using NUnit.Framework;
using Umbraco.Cms.Core.Configuration.Models;
using Umbraco.Cms.Core.Security;
using Umbraco.Cms.Infrastructure.Serialization;

namespace Umbraco.Cms.Tests.Integration.Umbraco.Infrastructure.Security;

[TestFixture]
public class MemberPasswordHasherTests
{
    [Test]
    [TestCase(
        "Password123!",
        "AQAAAAEAACcQAAAAEGF/tTVoL6ef3bQPZFYfbgKFu1CDQIAMgyY1N4EDt9jqdG/hsOX93X1U6LNvlIQ3mw==",
        null,
        ExpectedResult = PasswordVerificationResult.Success,
        Description = "AspNetCoreIdentityPasswordHash: Correct password")]
    [TestCase(
        "wrongPassword",
        "AQAAAAEAACcQAAAAEGF/tTVoL6ef3bQPZFYfbgKFu1CDQIAMgyY1N4EDt9jqdG/hsOX93X1U6LNvlIQ3mw==",
        null,
        ExpectedResult = PasswordVerificationResult.Failed,
        Description = "AspNetCoreIdentityPasswordHash: Wrong password")]
    [TestCase(
        "Password123!",
        "yDiU2YyuYZU4jz6F0fpErQ==BxNRHkXBVyJs9gwWF6ktWdfDwYf5bwm+rvV7tOcNNx8=",
        null,
        ExpectedResult = PasswordVerificationResult.SuccessRehashNeeded,
        Description = "GivenALegacyPasswordHash: Correct password")]
    [TestCase(
        "wrongPassword",
        "yDiU2YyuYZU4jz6F0fpErQ==BxNRHkXBVyJs9gwWF6ktWdfDwYf5bwm+rvV7tOcNNx8=",
        null,
        ExpectedResult = PasswordVerificationResult.Failed,
        Description = "GivenALegacyPasswordHash: Wrong password")]
    [TestCase(
        "Password123!",
        "AJszAsQqxOYbASKfL3JVUu6cjU18ouizXDfX4j7wLlir8SWj2yQaTepE9e5bIohIsQ==",
        null,
        ExpectedResult = PasswordVerificationResult.SuccessRehashNeeded,
        Description = "GivenALegacyPasswordHash: Correct password")]
    [TestCase(
        "wrongPassword",
        "AJszAsQqxOYbASKfL3JVUu6cjU18ouizXDfX4j7wLlir8SWj2yQaTepE9e5bIohIsQ==",
        null,
        ExpectedResult = PasswordVerificationResult.Failed,
        Description = "GivenALegacyPasswordHash: Wrong password")]
    [TestCase(
        "1234567890",
        "1234567890",
        null,
        ExpectedResult = PasswordVerificationResult.Failed,
        Description = "ClearText: Correct password, but not supported")]
    [TestCase(
        "wrongPassword",
        "1234567890",
        null,
        ExpectedResult = PasswordVerificationResult.Failed,
        Description = "ClearText: Wrong password")]
    [TestCase(
        "1234567890",
        "XyFRG4/xJ5JGQJYqqIFK70BjHdM=",
        null,
        ExpectedResult = PasswordVerificationResult.SuccessRehashNeeded,
        Description = "Hashed: Correct password")]
    [TestCase(
        "wrongPassword",
        "XyFRG4/xJ5JGQJYqqIFK70BjHdM=",
        null,
        ExpectedResult = PasswordVerificationResult.Failed,
        Description = "Hashed: Wrong password")]
    [TestCase(
        "1234567890",
        "K2JPOhoqNoysfnnD67QsWDSliHrjoSTRTvv9yiaKf30=",
        "1D43BFA074DF6DCEF6E44A7F5B5F56CDDD60BE198FBBB0222C96A5BD696F3CAA",
        ExpectedResult = PasswordVerificationResult.SuccessRehashNeeded,
        Description = "Encrypted: Correct password and correct decryptionKey")]
    [TestCase(
        "wrongPassword",
        "K2JPOhoqNoysfnnD67QsWDSliHrjoSTRTvv9yiaKf30=",
        "1D43BFA074DF6DCEF6E44A7F5B5F56CDDD60BE198FBBB0222C96A5BD696F3CAA",
        ExpectedResult = PasswordVerificationResult.Failed,
        Description = "Encrypted: Wrong password but correct decryptionKey")]
    [TestCase(
        "1234567890",
        "qiuwRr4K7brpTcIzLFfR3iGG9zj4/z4ewHCVZmYUDKM=",
        "B491B602E0CE1D52450A8089FD2013B340743A7EFCC12B039BD11977A083ACA1",
        ExpectedResult = PasswordVerificationResult.Failed,
        Description = "Encrypted: Correct password but wrong decryptionKey")]
    [TestCase(
        "1234567890",
        "qiuwRr4K7brpTcIzLFfR3iGG9zj4/z4ewHCVZmYUDKM=",
        "InvalidDecryptionKey",
        ExpectedResult = PasswordVerificationResult.Failed,
        Description = "Encrypted: Invalid decryptionKey")]
    public PasswordVerificationResult VerifyHashedPassword(string password, string encryptedPassword, string decryptionKey)
    {
        var member = new MemberIdentityUser { PasswordConfig = null };

        var sut = new MemberPasswordHasher(
            new LegacyPasswordSecurity(),
            new JsonNetSerializer(),
            Options.Create(new LegacyPasswordMigrationSettings { MachineKeyDecryptionKey = decryptionKey }),
            NullLoggerFactory.Instance.CreateLogger<MemberPasswordHasher>());

        return sut.VerifyHashedPassword(member, encryptedPassword, password);
    }
}
