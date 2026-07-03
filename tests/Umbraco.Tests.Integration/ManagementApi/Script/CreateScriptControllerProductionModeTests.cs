using System.Linq.Expressions;
using System.Net;
using System.Net.Http.Json;
using NUnit.Framework;
using Umbraco.Cms.Api.Management.Controllers.Script;
using Umbraco.Cms.Api.Management.ViewModels.Script;
using Umbraco.Cms.Core;

namespace Umbraco.Cms.Tests.Integration.ManagementApi.Script;

public class CreateScriptControllerProductionModeTests : ManagementApiTest<CreateScriptController>
{
    protected override Expression<Func<CreateScriptController, object>> MethodSelector { get; set; }
        = x => x.Create(CancellationToken.None, null);

    [SetUp]
    public override void Setup()
    {
        InMemoryConfiguration[Constants.Configuration.ConfigRuntimeMode] = "Production";
        InMemoryConfiguration["Umbraco:CMS:WebRouting:UmbracoApplicationUrl"] = "https://localhost";
        base.Setup();
        AuthenticateClientAsync(Client, "admin@test.com", "1234567890", true).GetAwaiter().GetResult();
    }

    [Test]
    public async Task Create_Returns_Bad_Request()
    {
        CreateScriptRequestModel createModel = new()
        {
            Name = Guid.NewGuid() + ".js",
            Content = "empty",
        };

        var response = await Client.PostAsync(Url, JsonContent.Create(createModel));

        Assert.AreEqual(HttpStatusCode.BadRequest, response.StatusCode);
    }
}
