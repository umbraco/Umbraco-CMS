using NUnit.Framework;
using Umbraco.Cms.Api.Management.Controllers;

namespace Umbraco.Cms.Tests.Integration.ManagementApi.SchemaLockdown;

/// <summary>
/// Base class for the schema lockdown endpoint tests, which all need an authenticated client to reach the filter at
/// all: an unauthenticated request is turned away by the authorization middleware before any filter runs.
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
