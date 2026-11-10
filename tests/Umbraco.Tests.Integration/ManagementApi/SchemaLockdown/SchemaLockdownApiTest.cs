using NUnit.Framework;
using Umbraco.Cms.Api.Management.Controllers;

namespace Umbraco.Cms.Tests.Integration.ManagementApi.SchemaLockdown;

/// <summary>
/// Base class for the schema lockdown endpoint tests. The client is authenticated, as the filter only runs after
/// authorization.
/// </summary>
/// <typeparam name="T">The controller under test.</typeparam>
public abstract class SchemaLockdownApiTest<T> : ManagementApiTest<T>
    where T : ManagementApiControllerBase
{
    /// <inheritdoc />
    [SetUp]
    public override void Setup()
    {
        InMemoryConfiguration["Umbraco:CMS:WebRouting:UmbracoApplicationUrl"] = "https://localhost";

        base.Setup();

        AuthenticateClientAsync(Client, "admin@test.com", "1234567890", true).GetAwaiter().GetResult();
    }
}
