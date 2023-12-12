using Microsoft.AspNetCore.Mvc;
using NUnit.Framework;
using Umbraco.Cms.Api.Common.ViewModels.Pagination;
using Umbraco.Cms.Api.Management.Controllers.DataType.Tree;
using Umbraco.Cms.Api.Management.ViewModels.DataType.Item;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.PropertyEditors;
using Umbraco.Cms.Core.Serialization;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Tests.Integration.TestServerTest;

namespace Umbraco.Cms.Tests.Integration.NewBackoffice.Controllers.DataType.Tree;

public class ChildrenDataTypeTreeControllerTests : UmbracoTestServerTestBase
{
    private ChildrenDataTypeTreeController Controller =>
        new ChildrenDataTypeTreeController(GetRequiredService<IEntityService>(),
            GetRequiredService<IDataTypeService>());

    private IDataTypeService DataTypeService => GetRequiredService<IDataTypeService>();

    private IDataTypeContainerService DataTypeContainerService => GetRequiredService<IDataTypeContainerService>();

    private IDataValueEditorFactory DataValueEditorFactory => GetRequiredService<IDataValueEditorFactory>();

    private IConfigurationEditorJsonSerializer ConfigurationEditorJsonSerializer =>
        GetRequiredService<IConfigurationEditorJsonSerializer>();

    [TestCase(0)]
    [TestCase(1)]
    [TestCase(10)]
    public async Task Total_Equals_CreateAmount_When_PageSize_Zero(int itemsToCreate)
    {
        // is there a way to start an integration setup without the seeding?
        await ClearDataTypes();
        var folderKey = await CreateParentedRootDataTypes(itemsToCreate);

        var controllerResult = await Controller.Children(folderKey,0, 0, false);
        var controllerValueResult =
            (controllerResult.Result as OkObjectResult)!.Value as PagedViewModel<DataTypeTreeItemResponseModel>;

        Assert.AreEqual(itemsToCreate, controllerValueResult!.Total);
    }

    [TestCase(0)]
    [TestCase(1)]
    [TestCase(10)]
    public async Task Total_Equals_CreateAmount_When_PageSize_Positive(int itemsToCreate)
    {
        await ClearDataTypes();
        var folderKey = await CreateParentedRootDataTypes(itemsToCreate);

        var controllerResult = await Controller.Children(folderKey,0, 1, false);
        var controllerValueResult =
            (controllerResult.Result as OkObjectResult)!.Value as PagedViewModel<DataTypeTreeItemResponseModel>;

        Assert.AreEqual(itemsToCreate, controllerValueResult!.Total);
    }

    [TestCase(0)]
    [TestCase(1)]
    [TestCase(10)]
    public async Task Total_Equals_CreateAmount_When_PageSize_GreaterThanTotal(int itemsToCreate)
    {
        await ClearDataTypes();
        var folderKey = await CreateParentedRootDataTypes(itemsToCreate);

        var controllerResult = await Controller.Children(folderKey, 0, 10000, false);
        var controllerValueResult =
            (controllerResult.Result as OkObjectResult)!.Value as PagedViewModel<DataTypeTreeItemResponseModel>;

        Assert.AreEqual(itemsToCreate, controllerValueResult!.Total);
    }

    private async Task<Guid> CreateParentedRootDataTypes(int amount)
    {
        var folderGuid = Guid.NewGuid();
        var folderCreateAttempt =
            await DataTypeContainerService.CreateAsync(folderGuid, "TestFolder", null, Constants.Security.SuperUserKey);

        if (folderCreateAttempt.Success is false)
        {
            throw new Exception("Setup failed unexpectedly => Assert might be compromised");
        }

        for (var i = 0; i < amount; i++)
        {
            var attempt = await DataTypeService.CreateAsync(
                new Core.Models.DataType(
                    new PlainStringPropertyEditor(DataValueEditorFactory),
                    ConfigurationEditorJsonSerializer)
                {
                    ParentId = folderCreateAttempt.Result.Id, Name = "TestDataType" + i,
                },
                Constants.Security.SuperUserKey);
            if (attempt.Success is false)
            {
                throw new Exception("Setup failed unexpectedly => Assert might be compromised");
            }
        }

        return folderGuid;
    }

    private async Task ClearDataTypes()
    {
        var dataTypes = DataTypeService.GetAll();
        foreach (var dataType in dataTypes)
        {
            await DataTypeService.DeleteAsync(dataType.Key, Constants.Security.SuperUserKey);
        }
    }
}
