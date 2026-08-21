using System.Linq.Expressions;
using System.Net;
using System.Net.Http.Json;
using NUnit.Framework;
using Umbraco.Cms.Api.Management.Controllers.Dictionary;
using Umbraco.Cms.Api.Management.ViewModels.Dictionary;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Services;

namespace Umbraco.Cms.Tests.Integration.ManagementApi.SchemaLockdown;

/// <summary>
/// Dictionary items have no per-operation carve-out: like every other locked entity type, they are blocked
/// for every operation, including <see cref="UpdateDictionaryController"/>.
/// </summary>
public class UpdatingADictionaryItemIsForbiddenTests : ManagementApiTest<UpdateDictionaryController>
{
    private IDictionaryItemService DictionaryItemService => GetRequiredService<IDictionaryItemService>();

    private Guid _dictionaryKey;

    protected override Expression<Func<UpdateDictionaryController, object>> MethodSelector { get; set; }

    [SetUp]
    public override void Setup()
    {
        InMemoryConfiguration[$"{Constants.Configuration.ConfigSchemaLockdown}:Enabled"] = "true";
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
    public async Task Update_Returns_Forbidden()
    {
        UpdateDictionaryItemRequestModel updateModel = new()
        {
            Name = Guid.NewGuid().ToString(),
            Translations = [new DictionaryItemTranslationModel { IsoCode = "en-US", Translation = "Translated" }],
        };

        var response = await Client.PutAsync(Url, JsonContent.Create(updateModel));

        Assert.AreEqual(HttpStatusCode.Forbidden, response.StatusCode);
    }
}
