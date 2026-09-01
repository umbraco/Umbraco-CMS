using NUnit.Framework;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.PropertyEditors;
using Umbraco.Cms.Core.Serialization;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Services.OperationStatus;
using Umbraco.Cms.Tests.Common.Testing;
using Umbraco.Cms.Tests.Integration.Testing;

namespace Umbraco.Cms.Tests.Integration.Umbraco.Infrastructure.Services;

/// <summary>
///     Tests covering the DataTypeContainerService
/// </summary>
[TestFixture]
[UmbracoTest(Database = UmbracoTestOptions.Database.NewSchemaPerTest)]
internal sealed class DataTypeContainerServiceTests : UmbracoIntegrationTest
{
    private IDataTypeContainerService DataTypeContainerService => GetRequiredService<IDataTypeContainerService>();

    private IDataTypeService DataTypeService => GetRequiredService<IDataTypeService>();

    private IDataValueEditorFactory DataValueEditorFactory => GetRequiredService<IDataValueEditorFactory>();

    private IConfigurationEditorJsonSerializer ConfigurationEditorJsonSerializer => GetRequiredService<IConfigurationEditorJsonSerializer>();

    [Test]
    public async Task Can_Create_Container_At_Root()
    {
        var result = await DataTypeContainerService.CreateAsync(null, "Root Container", null, Constants.Security.SuperUserKey);
        Assert.That(result.Success, Is.True);
        Assert.That(result.Status, Is.EqualTo(EntityContainerOperationStatus.Success));

        var created = await DataTypeContainerService.GetAsync(result.Result.Key);
        Assert.That(created, Is.Not.Null);
        Assert.That(created.Name, Is.EqualTo("Root Container"));
        Assert.That(created.ParentId, Is.EqualTo(Constants.System.Root));
    }

    [Test]
    public async Task Can_Create_Child_Container()
    {
        EntityContainer root = (await DataTypeContainerService.CreateAsync(null, "Root Container", null, Constants.Security.SuperUserKey)).Result;

        var result = await DataTypeContainerService.CreateAsync(null, "Child Container", root.Key, Constants.Security.SuperUserKey);
        Assert.That(result.Success, Is.True);
        Assert.That(result.Status, Is.EqualTo(EntityContainerOperationStatus.Success));

        var created = await DataTypeContainerService.GetAsync(result.Result.Key);
        Assert.That(created, Is.Not.Null);
        Assert.That(created.Name, Is.EqualTo("Child Container"));
        Assert.That(created.ParentId, Is.EqualTo(root.Id));
    }

    [TestCase("Existing Child Container", false)]
    [TestCase("New Child Container", true)]
    [TestCase("Root Container", true)]
    public async Task Can_Create_Child_Container_Without_Duplicate_Name_At_Level(string containerName, bool expectSuccess)
    {
        EntityContainer root = (await DataTypeContainerService.CreateAsync(null, "Root Container", null, Constants.Security.SuperUserKey)).Result;
        EntityContainer existingChild = (await DataTypeContainerService.CreateAsync(null, "Existing Child Container", root.Key, Constants.Security.SuperUserKey)).Result;

        var result = await DataTypeContainerService.CreateAsync(null, containerName, root.Key, Constants.Security.SuperUserKey);
        Assert.That(result.Success, Is.EqualTo(expectSuccess));
        if (expectSuccess)
        {
            Assert.That(result.Status, Is.EqualTo(EntityContainerOperationStatus.Success));
        }
        else
        {
            Assert.That(result.Status, Is.EqualTo(EntityContainerOperationStatus.DuplicateName));
        }
    }

    [Test]
    public async Task Can_Create_Container_With_Explicit_Key()
    {
        var key = Guid.NewGuid();
        var result = await DataTypeContainerService.CreateAsync(key, "Root Container", null, Constants.Security.SuperUserKey);
        Assert.That(result.Success, Is.True);
        Assert.That(result.Status, Is.EqualTo(EntityContainerOperationStatus.Success));
        Assert.That(result.Result.Key, Is.EqualTo(key));

        var created = await DataTypeContainerService.GetAsync(key);
        Assert.That(created, Is.Not.Null);
        Assert.That(created.Name, Is.EqualTo("Root Container"));
        Assert.That(created.ParentId, Is.EqualTo(Constants.System.Root));
    }

    [Test]
    public async Task Can_Update_Container_At_Root()
    {
        var key = (await DataTypeContainerService.CreateAsync(null, "Root Container", null, Constants.Security.SuperUserKey)).Result.Key;

        var result = await DataTypeContainerService.UpdateAsync(key, "Root Container UPDATED", Constants.Security.SuperUserKey);
        Assert.That(result.Success, Is.True);
        Assert.That(result.Status, Is.EqualTo(EntityContainerOperationStatus.Success));

        var updated = await DataTypeContainerService.GetAsync(key);
        Assert.That(updated, Is.Not.Null);
        Assert.That(updated.Name, Is.EqualTo("Root Container UPDATED"));
        Assert.That(updated.ParentId, Is.EqualTo(Constants.System.Root));
    }

    [Test]
    public async Task Can_Update_Child_Container()
    {
        EntityContainer root = (await DataTypeContainerService.CreateAsync(null, "Root Container", null, Constants.Security.SuperUserKey)).Result;
        EntityContainer child = (await DataTypeContainerService.CreateAsync(null, "Child Container", root.Key, Constants.Security.SuperUserKey)).Result;

        var result = await DataTypeContainerService.UpdateAsync(child.Key, "Child Container UPDATED", Constants.Security.SuperUserKey);
        Assert.That(result.Success, Is.True);
        Assert.That(result.Status, Is.EqualTo(EntityContainerOperationStatus.Success));

        EntityContainer updated = await DataTypeContainerService.GetAsync(child.Key);
        Assert.That(updated, Is.Not.Null);
        Assert.That(updated.Name, Is.EqualTo("Child Container UPDATED"));
        Assert.That(updated.ParentId, Is.EqualTo(root.Id));
    }

    [Test]
    public async Task Can_Get_Container_At_Root()
    {
        EntityContainer root = (await DataTypeContainerService.CreateAsync(null,"Root Container", null, Constants.Security.SuperUserKey)).Result;

        EntityContainer created = await DataTypeContainerService.GetAsync(root.Key);
        Assert.That(created, Is.Not.Null);
        Assert.That(created.Name, Is.EqualTo("Root Container"));
        Assert.That(created.ParentId, Is.EqualTo(Constants.System.Root));
    }

    [Test]
    public async Task Can_Get_Child_Container()
    {
        EntityContainer root = (await DataTypeContainerService.CreateAsync(null,"Root Container", null, Constants.Security.SuperUserKey)).Result;
        EntityContainer child = (await DataTypeContainerService.CreateAsync(null, "Child Container", root.Key, Constants.Security.SuperUserKey)).Result;

        EntityContainer created = await DataTypeContainerService.GetAsync(child.Key);
        Assert.That(created, Is.Not.Null);
        Assert.That(created.Name, Is.EqualTo("Child Container"));
        Assert.That(child.ParentId, Is.EqualTo(root.Id));
    }

    [Test]
    public async Task Can_Delete_Container_At_Root()
    {
        EntityContainer root = (await DataTypeContainerService.CreateAsync(null,"Root Container", null, Constants.Security.SuperUserKey)).Result;

        var result = await DataTypeContainerService.DeleteAsync(root.Key, Constants.Security.SuperUserKey);
        Assert.That(result.Success, Is.True);
        Assert.That(result.Status, Is.EqualTo(EntityContainerOperationStatus.Success));

        var current = await DataTypeContainerService.GetAsync(root.Key);
        Assert.That(current, Is.Null);
    }

    [Test]
    public async Task Can_Delete_Child_Container()
    {
        EntityContainer root = (await DataTypeContainerService.CreateAsync(null,"Root Container", null, Constants.Security.SuperUserKey)).Result;
        EntityContainer child = (await DataTypeContainerService.CreateAsync(null, "Child Container", root.Key, Constants.Security.SuperUserKey)).Result;

        var result = await DataTypeContainerService.DeleteAsync(child.Key, Constants.Security.SuperUserKey);
        Assert.That(result.Success, Is.True);
        Assert.That(result.Status, Is.EqualTo(EntityContainerOperationStatus.Success));

        child = await DataTypeContainerService.GetAsync(child.Key);
        Assert.That(child, Is.Null);

        root = await DataTypeContainerService.GetAsync(root.Key);
        Assert.That(root, Is.Not.Null);
    }

    [Test]
    public async Task Cannot_Create_Child_Container_Below_Invalid_Parent()
    {
        var key = Guid.NewGuid();
        var result = await DataTypeContainerService.CreateAsync(key, "Child Container", Guid.NewGuid(), Constants.Security.SuperUserKey);
        Assert.That(result.Success, Is.False);
        Assert.That(result.Status, Is.EqualTo(EntityContainerOperationStatus.ParentNotFound));

        var created = await DataTypeContainerService.GetAsync(key);
        Assert.That(created, Is.Null);
    }

    [Test]
    public async Task Cannot_Delete_Container_With_Child_Container()
    {
        EntityContainer root = (await DataTypeContainerService.CreateAsync(null,"Root Container", null, Constants.Security.SuperUserKey)).Result;
        EntityContainer child = (await DataTypeContainerService.CreateAsync(null, "Child Container", root.Key, Constants.Security.SuperUserKey)).Result;

        var result = await DataTypeContainerService.DeleteAsync(root.Key, Constants.Security.SuperUserKey);
        Assert.That(result.Success, Is.False);
        Assert.That(result.Status, Is.EqualTo(EntityContainerOperationStatus.NotEmpty));

        var current = await DataTypeContainerService.GetAsync(root.Key);
        Assert.That(current, Is.Not.Null);
    }

    [Test]
    public async Task Cannot_Delete_Container_With_Child_DataType()
    {
        EntityContainer container = (await DataTypeContainerService.CreateAsync(null,"Root Container", null, Constants.Security.SuperUserKey)).Result;

        IDataType dataType =
            new DataType(new TextboxPropertyEditor(DataValueEditorFactory, IOHelper), ConfigurationEditorJsonSerializer)
            {
                Name = Guid.NewGuid().ToString(),
                DatabaseType = ValueStorageType.Nvarchar,
                ParentId = container.Id
            };
        await DataTypeService.CreateAsync(dataType, Constants.Security.SuperUserKey);

        var result = await DataTypeContainerService.DeleteAsync(container.Key, Constants.Security.SuperUserKey);
        Assert.That(result.Success, Is.False);
        Assert.That(result.Status, Is.EqualTo(EntityContainerOperationStatus.NotEmpty));

        var currentContainer = await DataTypeContainerService.GetAsync(container.Key);
        Assert.That(currentContainer, Is.Not.Null);

        var currentDataType = await DataTypeService.GetAsync(dataType.Key);
        Assert.That(currentDataType, Is.Not.Null);
    }

    [Test]
    public async Task Cannot_Delete_Non_Existing_Container()
    {
        var result = await DataTypeContainerService.DeleteAsync(Guid.NewGuid(), Constants.Security.SuperUserKey);
        Assert.That(result.Success, Is.False);
        Assert.That(result.Status, Is.EqualTo(EntityContainerOperationStatus.NotFound));
    }
}
