using System.Linq.Expressions;
using System.Net;
using NUnit.Framework;
using Umbraco.Cms.Api.Management.Controllers.DocumentType;
using Umbraco.Cms.Core;

namespace Umbraco.Cms.Tests.Integration.ManagementApi.SchemaLockdown;

public class ReadingDocumentTypesIsStillPermittedTests : ManagementApiTest<AllowedAtRootDocumentTypeController>
{
    protected override Expression<Func<AllowedAtRootDocumentTypeController, object>> MethodSelector { get; set; }
        = x => x.AllowedAtRoot(CancellationToken.None, 0, 100);

    [SetUp]
    public override void Setup()
    {
        InMemoryConfiguration[$"{Constants.Configuration.ConfigSchemaLockdown}:Enabled"] = "true";
        InMemoryConfiguration["Umbraco:CMS:WebRouting:UmbracoApplicationUrl"] = "https://localhost";
        base.Setup();
        AuthenticateClientAsync(Client, "admin@test.com", "1234567890", true).GetAwaiter().GetResult();
    }

    [Test]
    public async Task AllowedAtRoot_Returns_Ok()
    {
        var response = await Client.GetAsync(Url);

        Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
    }
}
