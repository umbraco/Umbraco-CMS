using System.Linq.Expressions;
using System.Net;
using System.Net.Http.Json;
using NUnit.Framework;
using Umbraco.Cms.Api.Management.Controllers.Dictionary;
using Umbraco.Cms.Api.Management.ViewModels.Dictionary;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Services;

namespace Umbraco.Cms.Tests.Integration.ManagementApi.Dictionary;

/// <summary>
/// In production mode, adding and removing dictionary keys is blocked, but editing the translations
/// of an existing key must remain allowed (translators work in production). This asserts the latter.
/// </summary>
public class UpdateDictionaryControllerProductionModeTests : ManagementApiTest<UpdateDictionaryController>
{
    private IDictionaryItemService DictionaryItemService => GetRequiredService<IDictionaryItemService>();

    private Guid _dictionaryKey;

    protected override Expression<Func<UpdateDictionaryController, object>> MethodSelector { get; set; }

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
        var dictionaryItem = new DictionaryItem(Constants.System.RootKey, Guid.NewGuid().ToString());
        _dictionaryKey = dictionaryItem.Key;
        var result = await DictionaryItemService.CreateAsync(dictionaryItem, Constants.Security.SuperUserKey);
        Assert.IsTrue(result.Success);

        MethodSelector = x => x.Update(CancellationToken.None, _dictionaryKey, null);

        await AuthenticateClientAsync(Client, "admin@test.com", "1234567890", true);
    }

    [Test]
    public async Task Update_Is_Allowed_In_Production_Mode()
    {
        UpdateDictionaryItemRequestModel updateModel = new() { Name = Guid.NewGuid().ToString() };

        var response = await Client.PutAsync(Url, JsonContent.Create(updateModel));

        Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
    }
}
