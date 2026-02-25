using NUnit.Framework;

namespace Umbraco.Cms.Tests.Integration.Umbraco.Api.Delivery.OpenApi;

/// <summary>
/// Tests the Delivery API OpenAPI contract for correctness and consistency.
/// </summary>
[TestFixture]
internal sealed class OpenApiContractTest : OpenApiTestBase
{
    private const string ExpectedContractFileName = "default.json";

    [Test]
    public async Task OpenApiDocument_IsValid()
    {
        var openApiSpec = await FetchOpenApiSpecAsync();
        await ValidateOpenApiSpecAsync(openApiSpec);
    }

    [Test]
    public async Task OpenApiContract_MatchesExpected()
    {
        var openApiSpec = await FetchOpenApiSpecAsync();
        await ValidateContractAsync(openApiSpec, ExpectedContractFileName);
    }
}
