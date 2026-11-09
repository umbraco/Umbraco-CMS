using Microsoft.IdentityModel.Tokens;
using NUnit.Framework;
using Umbraco.Cms.Api.Common.Security;

namespace Umbraco.Cms.Tests.UnitTests.Umbraco.Cms.Api.Common.Security;

[TestFixture]
internal class EphemeralSecurityTokenCredentialsProviderTests
{
    [Test]
    public void Provides_An_Asymmetric_Signing_Credential_And_An_Encryption_Credential()
    {
        var sut = new EphemeralSecurityTokenCredentialsProvider();

        SigningCredentials signingCredentials = sut.GetSigningCredentials().Single();
        EncryptingCredentials encryptionCredentials = sut.GetEncryptionCredentials().Single();

        Assert.Multiple(() =>
        {
            Assert.That(signingCredentials.Key, Is.InstanceOf<RsaSecurityKey>());
            Assert.That(encryptionCredentials.Key, Is.InstanceOf<SymmetricSecurityKey>());
        });
    }

    [Test]
    public void Provides_The_Same_Credentials_For_The_Lifetime_Of_The_Instance()
    {
        var sut = new EphemeralSecurityTokenCredentialsProvider();

        Assert.Multiple(() =>
        {
            Assert.That(sut.GetSigningCredentials().Single(), Is.SameAs(sut.GetSigningCredentials().Single()));
            Assert.That(sut.GetEncryptionCredentials().Single(), Is.SameAs(sut.GetEncryptionCredentials().Single()));
        });
    }

    [Test]
    public void Provides_Different_Credentials_Per_Instance()
    {
        var first = new EphemeralSecurityTokenCredentialsProvider();
        var second = new EphemeralSecurityTokenCredentialsProvider();

        Assert.Multiple(() =>
        {
            Assert.That(GetModulus(second), Is.Not.EqualTo(GetModulus(first)));
            Assert.That(GetEncryptionKey(second), Is.Not.EqualTo(GetEncryptionKey(first)));
        });
    }

    private static byte[] GetModulus(ISecurityTokenCredentialsProvider provider)
        => ((RsaSecurityKey)provider.GetSigningCredentials().Single().Key).Rsa.ExportParameters(false).Modulus!;

    private static byte[] GetEncryptionKey(ISecurityTokenCredentialsProvider provider)
        => ((SymmetricSecurityKey)provider.GetEncryptionCredentials().Single().Key).Key;
}
