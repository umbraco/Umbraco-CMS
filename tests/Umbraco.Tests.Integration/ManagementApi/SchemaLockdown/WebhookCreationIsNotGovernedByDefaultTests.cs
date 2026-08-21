using System.Linq.Expressions;
using System.Net;
using System.Net.Http.Json;
using NUnit.Framework;
using Umbraco.Cms.Api.Management.Controllers.Webhook;
using Umbraco.Cms.Api.Management.ViewModels.Webhook;
using Umbraco.Cms.Core;

namespace Umbraco.Cms.Tests.Integration.ManagementApi.SchemaLockdown;

public class WebhookCreationIsNotGovernedByDefaultTests : ManagementApiTest<CreateWebhookController>
{
    protected override Expression<Func<CreateWebhookController, object>> MethodSelector { get; set; }
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
    public async Task Create_Returns_Created()
    {
        CreateWebhookRequestModel createModel = new()
        {
            Url = "https://example.com",
            Events = new[] { "umbracoContentPublish" },
        };

        var response = await Client.PostAsync(Url, JsonContent.Create(createModel));

        Assert.AreEqual(HttpStatusCode.Created, response.StatusCode);
    }
}
