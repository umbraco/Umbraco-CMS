using NUnit.Framework;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.PropertyEditors;
using Umbraco.Cms.Core.Serialization;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Services.OperationStatus;
using Umbraco.Cms.Infrastructure.Services;
using Umbraco.Cms.Tests.Common.Testing;
using Umbraco.Cms.Tests.Integration.Testing;

namespace Umbraco.Cms.Tests.Integration.Umbraco.Infrastructure.Services;

/// <summary>
///     Tests covering the DataTypeContainerService
/// </summary>
[TestFixture]
[UmbracoTest(Database = UmbracoTestOptions.Database.NewSchemaPerTest)]
public class DataTypeContainerServiceTests : UmbracoIntegrationTest
{
    private IDataTypeContainerService DataTypeContainerService => GetRequiredService<IDataTypeContainerService>();

    private IDataTypeService DataTypeService => GetRequiredService<IDataTypeService>();

    private IDataValueEditorFactory DataValueEditorFactory => GetRequiredService<IDataValueEditorFactory>();

    private IConfigurationEditorJsonSerializer ConfigurationEditorJsonSerializer => GetRequiredService<IConfigurationEditorJsonSerializer>();

    private IEditorConfigurationParser EditorConfigurationParser => GetRequiredService<IEditorConfigurationParser>();

    [Test]
    public async Task Can_Create_Container_At_Root()
    {
        EntityContainer toCreate = new EntityContainer(Constants.ObjectTypes.DataType) { Name = "Root Container" };

        var result = await DataTypeContainerService.CreateAsync(toCreate, null, Constants.Security.SuperUserKey);
        Assert.IsTrue(result.Success);
        Assert.AreEqual(DataTypeContainerOperationStatus.Success, result.Status);

        var created = await DataTypeContainerService.GetAsync(toCreate.Key);
        Assert.NotNull(created);
        Assert.AreEqual("Root Container", created.Name);
        Assert.AreEqual(Constants.System.Root, created.ParentId);
    }

    [Test]
    public async Task Can_Create_Child_Container()
    {
        EntityContainer root = new EntityContainer(Constants.ObjectTypes.DataType) { Name = "Root Container" };
        await DataTypeContainerService.CreateAsync(root, null, Constants.Security.SuperUserKey);

        EntityContainer child = new EntityContainer(Constants.ObjectTypes.DataType) { Name = "Child Container" };
        var result = await DataTypeContainerService.CreateAsync(child, root.Key, Constants.Security.SuperUserKey);
        Assert.IsTrue(result.Success);
        Assert.AreEqual(DataTypeContainerOperationStatus.Success, result.Status);

        var created = await DataTypeContainerService.GetAsync(child.Key);
        Assert.NotNull(created);
        Assert.AreEqual("Child Container", created.Name);
        Assert.AreEqual(root.Id, child.ParentId);
    }

    [Test]
    public async Task Can_Update_Container_At_Root()
    {
        EntityContainer root = new EntityContainer(Constants.ObjectTypes.DataType) { Name = "Root Container" };
        await DataTypeContainerService.CreateAsync(root, null, Constants.Security.SuperUserKey);

        EntityContainer toUpdate = await DataTypeContainerService.GetAsync(root.Key);
        Assert.NotNull(toUpdate);

        toUpdate.Name += " UPDATED";
        var result = await DataTypeContainerService.UpdateAsync(toUpdate, Constants.Security.SuperUserKey);
        Assert.IsTrue(result.Success);
        Assert.AreEqual(DataTypeContainerOperationStatus.Success, result.Status);

        var updated = await DataTypeContainerService.GetAsync(toUpdate.Key);
        Assert.NotNull(updated);
        Assert.AreEqual("Root Container UPDATED", updated.Name);
        Assert.AreEqual(Constants.System.Root, updated.ParentId);
    }

    [Test]
    public async Task Can_Update_Child_Container()
    {
        EntityContainer root = new EntityContainer(Constants.ObjectTypes.DataType) { Name = "Root Container" };
        await DataTypeContainerService.CreateAsync(root, null, Constants.Security.SuperUserKey);

        EntityContainer child = new EntityContainer(Constants.ObjectTypes.DataType) { Name = "Child Container" };
        await DataTypeContainerService.CreateAsync(child, root.Key, Constants.Security.SuperUserKey);

        EntityContainer toUpdate = await DataTypeContainerService.GetAsync(child.Key);
        Assert.NotNull(toUpdate);

        toUpdate.Name += " UPDATED";
        var result = await DataTypeContainerService.UpdateAsync(toUpdate, Constants.Security.SuperUserKey);
        Assert.IsTrue(result.Success);
        Assert.AreEqual(DataTypeContainerOperationStatus.Success, result.Status);

        var updated = await DataTypeContainerService.GetAsync(toUpdate.Key);
        Assert.NotNull(updated);
        Assert.AreEqual("Child Container UPDATED", updated.Name);
        Assert.AreEqual(root.Id, updated.ParentId);
    }

    [Test]
    public async Task Can_Get_Container_At_Root()
    {
        EntityContainer root = new EntityContainer(Constants.ObjectTypes.DataType) { Name = "Root Container" };
        await DataTypeContainerService.CreateAsync(root, null, Constants.Security.SuperUserKey);

        var created = await DataTypeContainerService.GetAsync(root.Key);
        Assert.NotNull(created);
        Assert.AreEqual("Root Container", created.Name);
        Assert.AreEqual(Constants.System.Root, created.ParentId);
    }

    [Test]
    public async Task Can_Get_Child_Container()
    {
        EntityContainer root = new EntityContainer(Constants.ObjectTypes.DataType) { Name = "Root Container" };
        await DataTypeContainerService.CreateAsync(root, null, Constants.Security.SuperUserKey);

        EntityContainer child = new EntityContainer(Constants.ObjectTypes.DataType) { Name = "Child Container" };
        await DataTypeContainerService.CreateAsync(child, root.Key, Constants.Security.SuperUserKey);

        var created = await DataTypeContainerService.GetAsync(child.Key);
        Assert.IsNotNull(created);
        Assert.AreEqual("Child Container", created.Name);
        Assert.AreEqual(root.Id, child.ParentId);
    }

    [Test]
    public async Task Can_Delete_Container_At_Root()
    {
        EntityContainer root = new EntityContainer(Constants.ObjectTypes.DataType) { Name = "Root Container" };
        await DataTypeContainerService.CreateAsync(root, null, Constants.Security.SuperUserKey);

        var result = await DataTypeContainerService.DeleteAsync(root.Key, Constants.Security.SuperUserKey);
        Assert.IsTrue(result.Success);
        Assert.AreEqual(DataTypeContainerOperationStatus.Success, result.Status);

        var current = await DataTypeContainerService.GetAsync(root.Key);
        Assert.IsNull(current);
    }

    [Test]
    public async Task Can_Delete_Child_Container()
    {
        Guid userKey = Constants.Security.SuperUserKey;
        EntityContainer root = new EntityContainer(Constants.ObjectTypes.DataType) { Name = "Root Container" };
        await DataTypeContainerService.CreateAsync(root, null, userKey);

        EntityContainer child = new EntityContainer(Constants.ObjectTypes.DataType) { Name = "Child Container" };
        await DataTypeContainerService.CreateAsync(child, root.Key, userKey);

        var result = await DataTypeContainerService.DeleteAsync(child.Key, userKey);
        Assert.IsTrue(result.Success);
        Assert.AreEqual(DataTypeContainerOperationStatus.Success, result.Status);

        var current = await DataTypeContainerService.GetAsync(child.Key);
        Assert.IsNull(current);

        current = await DataTypeContainerService.GetAsync(root.Key);
        Assert.IsNotNull(current);
    }

    [Test]
    public async Task Cannot_Create_Child_Container_Below_Invalid_Parent()
    {
        EntityContainer child = new EntityContainer(Constants.ObjectTypes.DataType) { Name = "Child Container" };
        var result = await DataTypeContainerService.CreateAsync(child, Guid.NewGuid(), Constants.Security.SuperUserKey);
        Assert.IsFalse(result.Success);
        Assert.AreEqual(DataTypeContainerOperationStatus.ParentNotFound, result.Status);

        var created = await DataTypeContainerService.GetAsync(child.Key);
        Assert.IsNull(created);
    }

    [Test]
    public async Task Cannot_Create_Child_Container_With_Explicit_Id()
    {
        EntityContainer toCreate = new EntityContainer(Constants.ObjectTypes.DataType) { Name = "Root Container", Id = 1234 };

        var result = await DataTypeContainerService.CreateAsync(toCreate, null, Constants.Security.SuperUserKey);
        Assert.IsFalse(result.Success);
        Assert.AreEqual(DataTypeContainerOperationStatus.InvalidId, result.Status);

        var created = await DataTypeContainerService.GetAsync(toCreate.Key);
        Assert.IsNull(created);
    }

    [TestCase(Constants.ObjectTypes.Strings.DocumentType)]
    [TestCase(Constants.ObjectTypes.Strings.MediaType)]
    public async Task Cannot_Create_Container_With_Invalid_Contained_Type(string containedObjectType)
    {
        EntityContainer toCreate = new EntityContainer(new Guid(containedObjectType)) { Name = "Root Container" };

        var result = await DataTypeContainerService.CreateAsync(toCreate, null, Constants.Security.SuperUserKey);
        Assert.IsFalse(result.Success);
        Assert.AreEqual(DataTypeContainerOperationStatus.InvalidObjectType, result.Status);

        var created = await DataTypeContainerService.GetAsync(toCreate.Key);
        Assert.IsNull(created);
    }

    [Test]
    public async Task Cannot_Update_Container_Parent()
    {
        EntityContainer root = new EntityContainer(Constants.ObjectTypes.DataType) { Name = "Root Container" };
        await DataTypeContainerService.CreateAsync(root, null, Constants.Security.SuperUserKey);

        EntityContainer root2 = new EntityContainer(Constants.ObjectTypes.DataType) { Name = "Root Container 2" };
        await DataTypeContainerService.CreateAsync(root2, null, Constants.Security.SuperUserKey);

        EntityContainer child = new EntityContainer(Constants.ObjectTypes.DataType) { Name = "Child Container" };
        await DataTypeContainerService.CreateAsync(child, root.Key, Constants.Security.SuperUserKey);

        EntityContainer toUpdate = await DataTypeContainerService.GetAsync(child.Key);
        Assert.IsNotNull(toUpdate);

        toUpdate.ParentId = root2.Id;
        var result = await DataTypeContainerService.UpdateAsync(toUpdate, Constants.Security.SuperUserKey);
        Assert.IsFalse(result.Success);
        Assert.AreEqual(DataTypeContainerOperationStatus.ParentNotFound, result.Status);

        var current = await DataTypeContainerService.GetAsync(child.Key);
        Assert.IsNotNull(current);
        Assert.AreEqual(root.Id, child.ParentId);
    }

    [Test]
    public async Task Cannot_Delete_Container_With_Child_Container()
    {
        EntityContainer root = new EntityContainer(Constants.ObjectTypes.DataType) { Name = "Root Container" };
        await DataTypeContainerService.CreateAsync(root, null, Constants.Security.SuperUserKey);

        EntityContainer child = new EntityContainer(Constants.ObjectTypes.DataType) { Name = "Child Container" };
        await DataTypeContainerService.CreateAsync(child, root.Key, Constants.Security.SuperUserKey);

        var result = await DataTypeContainerService.DeleteAsync(root.Key, Constants.Security.SuperUserKey);
        Assert.IsFalse(result.Success);
        Assert.AreEqual(DataTypeContainerOperationStatus.NotEmpty, result.Status);

        var current = await DataTypeContainerService.GetAsync(root.Key);
        Assert.IsNotNull(current);
    }

    [Test]
    public async Task Cannot_Delete_Container_With_Child_DataType()
    {
        EntityContainer container = new EntityContainer(Constants.ObjectTypes.DataType) { Name = "Root Container" };
        await DataTypeContainerService.CreateAsync(container, null, Constants.Security.SuperUserKey);

        IDataType dataType =
            new DataType(new TextboxPropertyEditor(DataValueEditorFactory, IOHelper, EditorConfigurationParser), ConfigurationEditorJsonSerializer)
            {
                Name = Guid.NewGuid().ToString(),
                DatabaseType = ValueStorageType.Nvarchar,
                ParentId = container.Id
            };
        await DataTypeService.CreateAsync(dataType, Constants.Security.SuperUserKey);

        var result = await DataTypeContainerService.DeleteAsync(container.Key, Constants.Security.SuperUserKey);
        Assert.IsFalse(result.Success);
        Assert.AreEqual(DataTypeContainerOperationStatus.NotEmpty, result.Status);

        var currentContainer = await DataTypeContainerService.GetAsync(container.Key);
        Assert.IsNotNull(currentContainer);

        var currentDataType = await DataTypeService.GetAsync(dataType.Key);
        Assert.IsNotNull(currentDataType);
    }

    [Test]
    public async Task Cannot_Delete_Non_Existing_Container()
    {
        var result = await DataTypeContainerService.DeleteAsync(Guid.NewGuid(), Constants.Security.SuperUserKey);
        Assert.IsFalse(result.Success);
        Assert.AreEqual(DataTypeContainerOperationStatus.NotFound, result.Status);
    }
}
