using System.Linq.Expressions;
using System.Net;
using System.Net.Http.Json;
using NUnit.Framework;
using Umbraco.Cms.Api.Management.Controllers.Dictionary;
using Umbraco.Cms.Api.Management.ViewModels;
using Umbraco.Cms.Api.Management.ViewModels.Dictionary;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.DependencyInjection;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Services;

namespace Umbraco.Cms.Tests.Integration.ManagementApi.SchemaLockdown;

/// <summary>
/// Regression test: <see cref="MoveDictionaryController"/> was not covered by the old per-controller
/// production-mode attribute mechanism, which motivated moving enforcement onto <see cref="DictionaryControllerBase"/> instead.
/// </summary>
public class MovingADictionaryItemIsForbiddenTests : ManagementApiTest<MoveDictionaryController>
{
    private IDictionaryItemService DictionaryItemService => GetRequiredService<IDictionaryItemService>();

    private Guid _dictionaryKey;

    protected override Expression<Func<MoveDictionaryController, object>> MethodSelector { get; set; }

    protected override void CustomTestSetup(IUmbracoBuilder builder)
        => builder.SchemaLockdownConfigurators().Append<LockDictionaryItemsConfigurator>();

    [SetUp]
    public override void Setup()
    {
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

        MethodSelector = x => x.Move(CancellationToken.None, _dictionaryKey, null);

        await AuthenticateClientAsync(Client, "admin@test.com", "1234567890", true);
    }

    [Test]
    public async Task Move_Returns_Forbidden()
    {
        MoveDictionaryRequestModel moveModel = new()
        {
            Target = new ReferenceByIdModel(Guid.NewGuid()),
        };

        var response = await Client.PutAsync(Url, JsonContent.Create(moveModel));

        Assert.AreEqual(HttpStatusCode.Forbidden, response.StatusCode);
    }
}
