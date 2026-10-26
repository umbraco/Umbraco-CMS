using System.Linq.Expressions;
using System.Net;
using System.Net.Http.Json;
using NUnit.Framework;
using Umbraco.Cms.Api.Management.Controllers.Dictionary;
using Umbraco.Cms.Api.Management.ViewModels;
using Umbraco.Cms.Api.Management.ViewModels.Dictionary;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.DependencyInjection;
using Umbraco.Cms.Core.SchemaLockdown;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Services;

namespace Umbraco.Cms.Tests.Integration.ManagementApi.SchemaLockdown;

/// <summary>
/// Regression test: <see cref="MoveDictionaryController"/> was not covered by the old per-controller
/// production-mode attribute mechanism, which motivated moving enforcement onto <see cref="DictionaryControllerBase"/> instead.
/// </summary>
public class MovingADictionaryItemIsForbiddenTests : SchemaLockdownApiTest<MoveDictionaryController>
{
    private IDictionaryItemService DictionaryItemService => GetRequiredService<IDictionaryItemService>();

    private Guid _dictionaryKey;

    protected override Expression<Func<MoveDictionaryController, object>> MethodSelector { get; set; }

    protected override void CustomTestSetup(IUmbracoBuilder builder)
        => builder.SchemaLockdownConfigurators().Add<LockDictionaryItemsConfigurator>();

    [SetUp]
    public override void Setup()
    {
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

    /// <summary>
    /// Locks dictionary items, and nothing else.
    /// </summary>
    private sealed class LockDictionaryItemsConfigurator : ISchemaLockdownConfigurator
    {
        /// <inheritdoc />
        public void Configure(ISchemaRestrictionsBuilder builder)
            => builder.BlockMutations(Constants.UdiEntityType.DictionaryItem);
    }
}
