using System.Linq.Expressions;
using System.Net;
using System.Net.Http.Json;
using NUnit.Framework;
using Umbraco.Cms.Api.Management.Controllers.Template;
using Umbraco.Cms.Api.Management.ViewModels.Template;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Services;

namespace Umbraco.Cms.Tests.Integration.ManagementApi.Template;

public class UpdateTemplateControllerProductionModeTests : ManagementApiTest<UpdateTemplateController>
{
    private ITemplateService TemplateService => GetRequiredService<ITemplateService>();

    private ITemplate _template = null!;

    protected override Expression<Func<UpdateTemplateController, object>> MethodSelector { get; set; }

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
        // Create template via the service layer (allowed in production mode).
        var alias = "test" + Guid.NewGuid().ToString("N");
        var result = await TemplateService.CreateAsync(alias, alias, "<h1>Original</h1>", Constants.Security.SuperUserKey);
        Assert.IsTrue(result.Success);
        _template = result.Result;

        MethodSelector = x => x.Update(CancellationToken.None, _template.Key, null);

        await AuthenticateClientAsync(Client, "admin@test.com", "1234567890", true);
    }

    [Test]
    public async Task Content_Change_Returns_Bad_Request()
    {
        UpdateTemplateRequestModel updateModel = new()
        {
            Name = _template.Name!,
            Alias = _template.Alias,
            Content = "<h1>Modified</h1>",
        };

        var response = await Client.PutAsync(Url, JsonContent.Create(updateModel));

        Assert.AreEqual(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Test]
    public async Task Metadata_Change_Returns_Ok()
    {
        // Re-fetch the template to get the actual resolved content (lazy-loaded from disk).
        var current = await TemplateService.GetAsync(_template.Key);

        UpdateTemplateRequestModel updateModel = new()
        {
            Name = "Updated Name",
            Alias = current!.Alias,
            Content = current.Content,
        };

        var response = await Client.PutAsync(Url, JsonContent.Create(updateModel));

        Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
    }
}
