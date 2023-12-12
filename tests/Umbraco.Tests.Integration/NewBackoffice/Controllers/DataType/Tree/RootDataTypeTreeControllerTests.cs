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

public class RootDataTypeTreeControllerTests : UmbracoTestServerTestBase
{
    private RootDataTypeTreeController Controller =>
        new RootDataTypeTreeController(GetRequiredService<IEntityService>(), GetRequiredService<IDataTypeService>());

    private IDataTypeService DataTypeService => GetRequiredService<IDataTypeService>();

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
        await CreateTestRootDataTypes(itemsToCreate);

        var controllerResult = await Controller.Root(0, 0, false);
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
        await CreateTestRootDataTypes(itemsToCreate);

        var controllerResult = await Controller.Root(0, 1, false);
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
        await CreateTestRootDataTypes(itemsToCreate);

        var controllerResult = await Controller.Root(0, 10000, false);
        var controllerValueResult =
            (controllerResult.Result as OkObjectResult)!.Value as PagedViewModel<DataTypeTreeItemResponseModel>;

        Assert.AreEqual(itemsToCreate, controllerValueResult!.Total);
    }

    private async Task CreateTestRootDataTypes(int amount)
    {
        for (var i = 0; i < amount; i++)
        {
            var attempt = await DataTypeService.CreateAsync(
                new Core.Models.DataType(
                    new PlainStringPropertyEditor(DataValueEditorFactory),
                    ConfigurationEditorJsonSerializer) { ParentId = Constants.System.Root, Name = "TestDataType" + i, },
                Constants.Security.SuperUserKey);
            if (attempt.Success is false)
            {
                throw new Exception("Setup failed unexpectedly => Assert might be compromised");
            }
        }
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
