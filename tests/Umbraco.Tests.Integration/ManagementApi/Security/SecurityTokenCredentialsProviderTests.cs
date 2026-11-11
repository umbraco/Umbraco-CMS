using System.Net;
using System.Security.Cryptography;
using System.Text.Json;
using Microsoft.IdentityModel.Tokens;
using NUnit.Framework;
using Umbraco.Cms.Api.Common.Security;
using Umbraco.Cms.Tests.Common.Testing;
using Umbraco.Cms.Tests.Integration.TestServerTest;

namespace Umbraco.Cms.Tests.Integration.ManagementApi.Security;

/// <summary>
/// Verifies that the OpenIddict server credentials come from the registered <see cref="ISecurityTokenCredentialsProvider"/>.
/// </summary>
[TestFixture]
[UmbracoTest(Database = UmbracoTestOptions.Database.NewSchemaPerFixture, Boot = true)]
public class SecurityTokenCredentialsProviderTests : UmbracoTestServerTestBase
{
    private static readonly RSA _signingKey = RSA.Create(2048);

    protected override void CustomTestSetup(IUmbracoBuilder builder)
        => builder.Services.AddUnique<ISecurityTokenCredentialsProvider, FixedCredentialsProvider>();

    [Test]
    public async Task Published_Signing_Keys_Are_Those_Of_The_Registered_Provider()
    {
        HttpResponseMessage response = await Client.GetAsync("/.well-known/jwks");

        Assert.AreEqual(HttpStatusCode.OK, response.StatusCode, await response.Content.ReadAsStringAsync());

        using JsonDocument jwks = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        string[] publishedModuli = jwks.RootElement
            .GetProperty("keys")
            .EnumerateArray()
            .Select(key => key.GetProperty("n").GetString()!)
            .ToArray();

        Assert.That(publishedModuli, Is.EqualTo(new[] { Base64UrlEncoder.Encode(_signingKey.ExportParameters(false).Modulus) }));
    }

    private sealed class FixedCredentialsProvider : ISecurityTokenCredentialsProvider
    {
        private static readonly byte[] _encryptionKey = RandomNumberGenerator.GetBytes(32);

        public IEnumerable<SigningCredentials> GetSigningCredentials()
            => [new SigningCredentials(new RsaSecurityKey(_signingKey), SecurityAlgorithms.RsaSha256)];

        public IEnumerable<EncryptingCredentials> GetEncryptionCredentials()
            =>
            [
                new EncryptingCredentials(
                    new SymmetricSecurityKey(_encryptionKey),
                    SecurityAlgorithms.Aes256KW,
                    SecurityAlgorithms.Aes256CbcHmacSha512),
            ];
    }
}
