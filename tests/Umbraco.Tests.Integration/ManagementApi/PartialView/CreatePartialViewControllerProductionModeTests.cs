using System.Linq.Expressions;
using System.Net;
using System.Net.Http.Json;
using NUnit.Framework;
using Umbraco.Cms.Api.Management.Controllers.PartialView;
using Umbraco.Cms.Api.Management.ViewModels.PartialView;
using Umbraco.Cms.Core;

namespace Umbraco.Cms.Tests.Integration.ManagementApi.PartialView;

public class CreatePartialViewControllerProductionModeTests : ManagementApiTest<CreatePartialViewController>
{
    protected override Expression<Func<CreatePartialViewController, object>> MethodSelector { get; set; }
        = x => x.Create(CancellationToken.None, null);

    [SetUp]
    public override void Setup()
    {
        InMemoryConfiguration[Constants.Configuration.ConfigRuntimeMode] = "Production";
        InMemoryConfiguration["Umbraco:CMS:WebRouting:UmbracoApplicationUrl"] = "https://localhost";
        base.Setup();
        Authenticate().GetAwaiter().GetResult();
    }

    private async Task Authenticate()
        => await AuthenticateClientAsync(Client, "admin@test.com", "1234567890", true);

    [Test]
    public async Task Create_Returns_Forbidden()
    {
        CreatePartialViewRequestModel createModel = new()
        {
            Name = $"{Guid.NewGuid():N}.cshtml",
            Content = "<h1>Test</h1>",
        };

        var response = await Client.PostAsync(Url, JsonContent.Create(createModel));

        Assert.AreEqual(HttpStatusCode.Forbidden, response.StatusCode);
        Assert.That(
            response.Headers.GetValues(Constants.Headers.SchemaLockdown),
            Is.EqualTo(new[] { $"{Constants.UdiEntityType.PartialView}:create" }));
    }
}
