using System.Linq.Expressions;
using System.Net;
using System.Net.Http.Json;
using NUnit.Framework;
using Umbraco.Cms.Api.Management.Controllers.DocumentType;
using Umbraco.Cms.Api.Management.ViewModels.DocumentType;
using Umbraco.Cms.Core.DependencyInjection;
using Umbraco.Cms.Tests.Integration.ManagementApi.SchemaLockdown.Configurators;

namespace Umbraco.Cms.Tests.Integration.ManagementApi.SchemaLockdown;

public class AvailableCompositionsIsStillPermittedTests : ManagementApiTest<AvailableCompositionDocumentTypeController>
{
    protected override Expression<Func<AvailableCompositionDocumentTypeController, object>> MethodSelector { get; set; }
        = x => x.AvailableCompositions(CancellationToken.None, null);

    protected override void CustomTestSetup(IUmbracoBuilder builder)
        => builder.SchemaLockdownConfigurators().Append<LockDocumentTypesConfigurator>();

    [SetUp]
    public override void Setup()
    {
        InMemoryConfiguration["Umbraco:CMS:WebRouting:UmbracoApplicationUrl"] = "https://localhost";
        base.Setup();
        AuthenticateClientAsync(Client, "admin@test.com", "1234567890", true).GetAwaiter().GetResult();
    }

    [Test]
    public async Task AvailableCompositions_Returns_Ok()
    {
        DocumentTypeCompositionRequestModel requestModel = new();

        var response = await Client.PostAsync(Url, JsonContent.Create(requestModel));

        Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
    }
}
