using System.Linq.Expressions;
using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using NUnit.Framework;
using Umbraco.Cms.Api.Management.Controllers.DataType.Tree;
using Umbraco.Cms.Api.Management.ViewModels;
using Umbraco.Cms.Api.Management.ViewModels.Tree;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Tests.Common.Builders;
using Umbraco.Cms.Tests.Common.Builders.Extensions;

namespace Umbraco.Cms.Tests.Integration.ManagementApi.DataType.Tree;

/// <summary>
/// Asserts that the batch endpoint of a tree with folders returns folders as well as items, with their parents.
/// </summary>
/// <remarks>
/// Folders are resolved by the shared folder tree controller base, so the data type tree stands in for every tree
/// with folders.
/// </remarks>
public class BatchDataTypeTreeFolderTests : ManagementApiTest<BatchDataTypeTreeController>
{
    private IDataTypeService DataTypeService => GetRequiredService<IDataTypeService>();

    private IDataTypeContainerService DataTypeContainerService => GetRequiredService<IDataTypeContainerService>();

    private JsonSerializerOptions BackOfficeJsonSerializerOptions => GetRequiredService<IOptionsMonitor<JsonOptions>>()
        .Get(Constants.JsonOptionsNames.BackOffice)
        .JsonSerializerOptions;

    private Guid _outerFolderKey;

    private Guid _innerFolderKey;

    private Guid _dataTypeKey;

    protected override Expression<Func<BatchDataTypeTreeController, object>> MethodSelector { get; set; }

    [SetUp]
    public new async Task Setup()
    {
        _outerFolderKey = await CreateFolderAsync(Constants.System.RootKey);
        _innerFolderKey = await CreateFolderAsync(_outerFolderKey);

        EntityContainer? innerFolder = await DataTypeContainerService.GetAsync(_innerFolderKey);
        IDataType dataType = new DataTypeBuilder()
            .WithId(0)
            .WithName(Guid.NewGuid().ToString())
            .WithDatabaseType(ValueStorageType.Nvarchar)
            .AddEditor()
                .WithAlias(Constants.PropertyEditors.Aliases.TextBox)
                .Done()
            .WithParentId(innerFolder!.Id)
            .Build();
        _dataTypeKey = (await DataTypeService.CreateAsync(dataType, Constants.Security.SuperUserKey)).Result.Key;

        await AuthenticateClientAsync(Client, "test@umbraco.com", UserPassword, isAdmin: true);
    }

    [Test]
    public async Task Batch_Returns_Folders_And_Items_In_The_Requested_Order()
    {
        BatchResponseModel<DataTypeTreeItemResponseModel> result =
            await GetBatchAsync(_dataTypeKey, Guid.NewGuid(), _innerFolderKey, _outerFolderKey);

        Assert.AreEqual(3, result.Total);
        Assert.AreEqual(
            new[] { _dataTypeKey, _innerFolderKey, _outerFolderKey },
            result.Items.Select(item => item.Id).ToArray());
        Assert.AreEqual(
            new[] { false, true, true },
            result.Items.Select(item => item.IsFolder).ToArray());
    }

    [Test]
    public async Task Batch_Populates_The_Parent_When_It_Is_A_Folder()
    {
        BatchResponseModel<DataTypeTreeItemResponseModel> result =
            await GetBatchAsync(_dataTypeKey, _innerFolderKey, _outerFolderKey);

        DataTypeTreeItemResponseModel[] items = result.Items.ToArray();
        Assert.AreEqual(_innerFolderKey, items[0].Parent?.Id);
        Assert.AreEqual(_outerFolderKey, items[1].Parent?.Id);
        Assert.IsNull(items[2].Parent);
    }

    private async Task<Guid> CreateFolderAsync(Guid? parentKey)
    {
        var key = Guid.NewGuid();
        await DataTypeContainerService.CreateAsync(key, Guid.NewGuid().ToString(), parentKey, Constants.Security.SuperUserKey);
        return key;
    }

    private async Task<BatchResponseModel<DataTypeTreeItemResponseModel>> GetBatchAsync(params Guid[] ids)
    {
        // GetManagementApiUrl names query parameters after the C# parameter (ids), not the bound name (id).
        HttpResponseMessage response = await Client.GetAsync(
            $"/umbraco/management/api/v1/tree/data-type/batch?{string.Join("&", ids.Select(id => $"id={id}"))}");

        Assert.AreEqual(HttpStatusCode.OK, response.StatusCode, await response.Content.ReadAsStringAsync());

        BatchResponseModel<DataTypeTreeItemResponseModel>? result =
            await response.Content.ReadFromJsonAsync<BatchResponseModel<DataTypeTreeItemResponseModel>>(BackOfficeJsonSerializerOptions);
        Assert.IsNotNull(result);
        return result!;
    }
}
