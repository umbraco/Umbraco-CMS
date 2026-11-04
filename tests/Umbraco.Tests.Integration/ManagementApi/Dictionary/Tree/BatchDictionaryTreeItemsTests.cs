using System.Linq.Expressions;
using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using NUnit.Framework;
using Umbraco.Cms.Api.Management.Controllers.Dictionary.Tree;
using Umbraco.Cms.Api.Management.ViewModels;
using Umbraco.Cms.Api.Management.ViewModels.Tree;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Services;

namespace Umbraco.Cms.Tests.Integration.ManagementApi.Dictionary.Tree;

/// <summary>
/// Asserts that the dictionary tree batch endpoint, which resolves its items outside the shared entity tree batch,
/// returns the items in the requested order with their parents.
/// </summary>
public class BatchDictionaryTreeItemsTests : ManagementApiTest<BatchDictionaryTreeController>
{
    private IDictionaryItemService DictionaryItemService => GetRequiredService<IDictionaryItemService>();

    private JsonSerializerOptions BackOfficeJsonSerializerOptions => GetRequiredService<IOptionsMonitor<JsonOptions>>()
        .Get(Constants.JsonOptionsNames.BackOffice)
        .JsonSerializerOptions;

    private Guid _parentKey;

    private Guid _childKey;

    protected override Expression<Func<BatchDictionaryTreeController, object>> MethodSelector { get; set; }

    [SetUp]
    public new async Task Setup()
    {
        _parentKey = await CreateDictionaryItemAsync(null);
        _childKey = await CreateDictionaryItemAsync(_parentKey);

        await AuthenticateClientAsync(Client, "test@umbraco.com", UserPassword, isAdmin: true);
    }

    [Test]
    public async Task Batch_Returns_Items_In_The_Requested_Order_Omitting_Unknown_Keys()
    {
        BatchResponseModel<NamedEntityTreeItemResponseModel> result = await GetBatchAsync(_childKey, Guid.NewGuid(), _parentKey);

        Assert.AreEqual(2, result.Total);
        Assert.AreEqual(new[] { _childKey, _parentKey }, result.Items.Select(item => item.Id).ToArray());
    }

    [Test]
    public async Task Batch_Populates_The_Parent()
    {
        BatchResponseModel<NamedEntityTreeItemResponseModel> result = await GetBatchAsync(_childKey, _parentKey);

        NamedEntityTreeItemResponseModel[] items = result.Items.ToArray();
        Assert.AreEqual(_parentKey, items[0].Parent?.Id);
        Assert.IsNull(items[1].Parent);
    }

    private async Task<Guid> CreateDictionaryItemAsync(Guid? parentKey)
    {
        var dictionaryItem = new DictionaryItem(parentKey, Guid.NewGuid().ToString());
        await DictionaryItemService.CreateAsync(dictionaryItem, Constants.Security.SuperUserKey);
        return dictionaryItem.Key;
    }

    private async Task<BatchResponseModel<NamedEntityTreeItemResponseModel>> GetBatchAsync(params Guid[] ids)
    {
        // GetManagementApiUrl names query parameters after the C# parameter (ids), not the bound name (id).
        HttpResponseMessage response = await Client.GetAsync(
            $"/umbraco/management/api/v1/tree/dictionary/batch?{string.Join("&", ids.Select(id => $"id={id}"))}");

        Assert.AreEqual(HttpStatusCode.OK, response.StatusCode, await response.Content.ReadAsStringAsync());

        BatchResponseModel<NamedEntityTreeItemResponseModel>? result =
            await response.Content.ReadFromJsonAsync<BatchResponseModel<NamedEntityTreeItemResponseModel>>(BackOfficeJsonSerializerOptions);
        Assert.IsNotNull(result);
        return result!;
    }
}
