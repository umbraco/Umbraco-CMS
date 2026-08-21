using System.Linq.Expressions;
using System.Net;
using System.Net.Http.Json;
using NUnit.Framework;
using Umbraco.Cms.Api.Management.Controllers.DocumentType;
using Umbraco.Cms.Api.Management.ViewModels.DocumentType;
using Umbraco.Cms.Core;

namespace Umbraco.Cms.Tests.Integration.ManagementApi.SchemaLockdown;

public class CreatingADocumentTypeIsForbiddenTests : ManagementApiTest<CreateDocumentTypeController>
{
    protected override Expression<Func<CreateDocumentTypeController, object>> MethodSelector { get; set; }
        = x => x.Create(CancellationToken.None, null);

    [SetUp]
    public override void Setup()
    {
        InMemoryConfiguration[$"{Constants.Configuration.ConfigSchemaLockdown}:Enabled"] = "true";
        InMemoryConfiguration["Umbraco:CMS:WebRouting:UmbracoApplicationUrl"] = "https://localhost";
        base.Setup();
        AuthenticateClientAsync(Client, "admin@test.com", "1234567890", true).GetAwaiter().GetResult();
    }

    [Test]
    public async Task Create_Returns_Forbidden()
    {
        CreateDocumentTypeRequestModel createModel = new()
        {
            Alias = "test" + Guid.NewGuid().ToString("N"),
            Name = "Test",
            Id = Guid.NewGuid(),
            Icon = "icon-document",
        };

        var response = await Client.PostAsync(Url, JsonContent.Create(createModel));

        Assert.AreEqual(HttpStatusCode.Forbidden, response.StatusCode);
    }
}
