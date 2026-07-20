using System.Linq.Expressions;
using System.Net;
using NUnit.Framework;
using Umbraco.Cms.Api.Management.Controllers.Template;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Services;

namespace Umbraco.Cms.Tests.Integration.ManagementApi.Template;

public class DeleteTemplateControllerProductionModeTests : ManagementApiTest<DeleteTemplateController>
{
    private ITemplateService TemplateService => GetRequiredService<ITemplateService>();

    private ITemplate _template = null!;

    protected override Expression<Func<DeleteTemplateController, object>> MethodSelector { get; set; }

    [SetUp]
    public override void Setup()
    {
        InMemoryConfiguration[Constants.Configuration.ConfigRuntimeMode] = "Production";
        InMemoryConfiguration["Umbraco:CMS:WebRouting:UmbracoApplicationUrl"] = "https://localhost";
        base.Setup();
        CreateTemplate().GetAwaiter().GetResult();
    }

    private async Task CreateTemplate()
    {
        var alias = "test" + Guid.NewGuid().ToString("N");
        var result = await TemplateService.CreateAsync(alias, alias, "<h1>Test</h1>", Constants.Security.SuperUserKey);
        Assert.IsTrue(result.Success);
        _template = result.Result;

        MethodSelector = x => x.Delete(CancellationToken.None, _template.Key);

        await AuthenticateClientAsync(Client, "admin@test.com", "1234567890", true);
    }

    [Test]
    public async Task Delete_Returns_Bad_Request()
    {
        var response = await Client.DeleteAsync(Url);

        Assert.AreEqual(HttpStatusCode.BadRequest, response.StatusCode);
    }
}
