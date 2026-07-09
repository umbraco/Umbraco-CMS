using System.Linq.Expressions;
using System.Net;
using NUnit.Framework;
using Umbraco.Cms.Api.Management.Controllers.Dictionary;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Services;

namespace Umbraco.Cms.Tests.Integration.ManagementApi.Dictionary;

public class DeleteDictionaryControllerProductionModeTests : ManagementApiTest<DeleteDictionaryController>
{
    private IDictionaryItemService DictionaryItemService => GetRequiredService<IDictionaryItemService>();

    private Guid _dictionaryKey;

    protected override Expression<Func<DeleteDictionaryController, object>> MethodSelector { get; set; }

    [SetUp]
    public override void Setup()
    {
        InMemoryConfiguration[Constants.Configuration.ConfigRuntimeMode] = "Production";
        InMemoryConfiguration["Umbraco:CMS:WebRouting:UmbracoApplicationUrl"] = "https://localhost";
        base.Setup();
        CreateDictionaryItem().GetAwaiter().GetResult();
    }

    private async Task CreateDictionaryItem()
    {
        // Create the dictionary item via the service layer (allowed in production mode).
        var dictionaryItem = new DictionaryItem(Constants.System.RootKey, Guid.NewGuid().ToString());
        _dictionaryKey = dictionaryItem.Key;
        var result = await DictionaryItemService.CreateAsync(dictionaryItem, Constants.Security.SuperUserKey);
        Assert.IsTrue(result.Success);

        MethodSelector = x => x.Delete(CancellationToken.None, _dictionaryKey);

        await AuthenticateClientAsync(Client, "admin@test.com", "1234567890", true);
    }

    [Test]
    public async Task Delete_Returns_Bad_Request()
    {
        var response = await Client.DeleteAsync(Url);

        Assert.AreEqual(HttpStatusCode.BadRequest, response.StatusCode);
    }
}
