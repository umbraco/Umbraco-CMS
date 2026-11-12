using System.Security.Cryptography;
using Microsoft.IdentityModel.Tokens;
using Moq;
using NUnit.Framework;
using OpenIddict.Server;
using Umbraco.Cms.Api.Common.Configuration;
using Umbraco.Cms.Api.Common.Security;

namespace Umbraco.Cms.Tests.UnitTests.Umbraco.Cms.Api.Common.Configuration;

[TestFixture]
internal class ConfigureOpenIddictServerCredentialsTests
{
    [Test]
    public void Adds_All_Credentials_From_The_Provider()
    {
        SigningCredentials[] signingCredentials =
        [
            new(new RsaSecurityKey(RSA.Create(2048)), SecurityAlgorithms.RsaSha256),
            new(new RsaSecurityKey(RSA.Create(2048)), SecurityAlgorithms.RsaSha256),
        ];
        EncryptingCredentials[] encryptionCredentials =
        [
            new(new SymmetricSecurityKey(RandomNumberGenerator.GetBytes(32)), SecurityAlgorithms.Aes256KW, SecurityAlgorithms.Aes256CbcHmacSha512),
        ];

        var provider = new Mock<ISecurityTokenCredentialsProvider>();
        provider.Setup(x => x.GetSigningCredentials()).Returns(signingCredentials);
        provider.Setup(x => x.GetEncryptionCredentials()).Returns(encryptionCredentials);

        var options = new OpenIddictServerOptions();
        new ConfigureOpenIddictServerCredentials(provider.Object).Configure(options);

        Assert.Multiple(() =>
        {
            Assert.That(options.SigningCredentials, Is.EqualTo(signingCredentials));
            Assert.That(options.EncryptionCredentials, Is.EqualTo(encryptionCredentials));
        });
    }
}
