using System.Linq.Expressions;
using System.Net;
using System.Net.Http.Json;
using NUnit.Framework;
using Umbraco.Cms.Api.Management.Controllers.Template;
using Umbraco.Cms.Api.Management.ViewModels.Template;
using Umbraco.Cms.Core;

namespace Umbraco.Cms.Tests.Integration.ManagementApi.Template;

public class CreateTemplateControllerProductionModeTests : ManagementApiTest<CreateTemplateController>
{
    protected override Expression<Func<CreateTemplateController, object>> MethodSelector { get; set; }
        = x => x.Create(CancellationToken.None, null);

    [SetUp]
    public override void Setup()
    {
        InMemoryConfiguration[Constants.Configuration.ConfigRuntimeMode] = "Production";
        base.Setup();
    }

    [SetUp]
    public async Task Authenticate()
        => await AuthenticateClientAsync(Client, "admin@test.com", "1234567890", true);

    [Test]
    public async Task Create_Returns_Bad_Request()
    {
        CreateTemplateRequestModel createModel = new()
        {
            Name = Guid.NewGuid().ToString(),
            Alias = Guid.NewGuid().ToString("N"),
            Content = "<h1>Test</h1>",
        };

        var response = await Client.PostAsync(Url, JsonContent.Create(createModel));

        Assert.AreEqual(HttpStatusCode.BadRequest, response.StatusCode);
    }
}
