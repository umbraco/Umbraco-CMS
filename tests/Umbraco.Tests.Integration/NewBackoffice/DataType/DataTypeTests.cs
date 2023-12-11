using NUnit.Framework;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Services.OperationStatus;
using Umbraco.Cms.Tests.Common.Builders;
using Umbraco.Cms.Tests.Common.Builders.Extensions;
using Umbraco.Cms.Tests.Integration.TestServerTest;

namespace Umbraco.Cms.Tests.Integration.NewBackoffice.DataType;

public class DataTypeTests : UmbracoTestServerTestBase
{
    private IDataTypeService DataTypeService => GetRequiredService<IDataTypeService>();

    private IDataTypeContainerService DataTypeContainerService => GetRequiredService<IDataTypeContainerService>();

    [Test]
    public async Task Can_Create_DataType_In_DataTypeFolder()
    {
        var createdDataTypeFolder =
            await DataTypeContainerService.CreateAsync(null, "TestFolder", null, Constants.Security.SuperUserKey);

        var dataType = new DataTypeBuilder()
            .WithId(0)
            .WithParentId(createdDataTypeFolder.Result.Id)
            .Build();
        var createdChildDataType = await DataTypeService.CreateAsync(dataType, Constants.Security.SuperUserKey);

        Assert.AreEqual(EntityContainerOperationStatus.Success, createdDataTypeFolder.Status);
        Assert.AreEqual(DataTypeOperationStatus.Success, createdChildDataType.Status);
        Assert.AreEqual(createdDataTypeFolder.Result.Id, createdChildDataType.Result.ParentId);
    }

    [Test]
    public async Task Can_Create_DataTypeFolder_In_DataTypeFolder()
    {
        var createdDataTypeFolder =
            await DataTypeContainerService.CreateAsync(null, "TestFolder", null, Constants.Security.SuperUserKey);

        var createdChildDataTypeFolder =
            await DataTypeContainerService.CreateAsync(null, "TestChildFolder", createdDataTypeFolder.Result.Key, Constants.Security.SuperUserKey);

        Assert.AreEqual(EntityContainerOperationStatus.Success, createdDataTypeFolder.Status);
        Assert.AreEqual(EntityContainerOperationStatus.Success, createdChildDataTypeFolder.Status);
        Assert.AreEqual(createdDataTypeFolder.Result.Id, createdChildDataTypeFolder.Result.ParentId);
    }
}
