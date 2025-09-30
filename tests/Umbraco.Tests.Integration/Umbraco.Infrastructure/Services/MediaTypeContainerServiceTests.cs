using NUnit.Framework;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Services.OperationStatus;
using Umbraco.Cms.Tests.Common.Testing;
using Umbraco.Cms.Tests.Integration.Testing;

namespace Umbraco.Cms.Tests.Integration.Umbraco.Infrastructure.Services;

/// <summary>
///     Tests covering the MediaTypeContainerService
/// </summary>
[TestFixture]
[UmbracoTest(Database = UmbracoTestOptions.Database.NewSchemaPerTest)]
internal sealed class MediaTypeContainerServiceTests : UmbracoIntegrationTest
{
    private IMediaTypeContainerService MediaTypeContainerService => GetRequiredService<IMediaTypeContainerService>();

    private IMediaTypeService MediaTypeService => GetRequiredService<IMediaTypeService>();

    [Test]
    public async Task Can_Create_Container_At_Root()
    {
        var result = await MediaTypeContainerService.CreateAsync(null, "Root Container", null, Constants.Security.SuperUserKey);
        Assert.IsTrue(result.Success);
        Assert.AreEqual(EntityContainerOperationStatus.Success, result.Status);

        var created = await MediaTypeContainerService.GetAsync(result.Result.Key);
        Assert.NotNull(created);
        Assert.AreEqual("Root Container", created.Name);
        Assert.AreEqual(Constants.System.Root, created.ParentId);
    }

    [Test]
    public async Task Can_Create_Child_Container()
    {
        EntityContainer root = (await MediaTypeContainerService.CreateAsync(null, "Root Container", null, Constants.Security.SuperUserKey)).Result;

        var result = await MediaTypeContainerService.CreateAsync(null, "Child Container", root.Key, Constants.Security.SuperUserKey);
        Assert.IsTrue(result.Success);
        Assert.AreEqual(EntityContainerOperationStatus.Success, result.Status);

        var created = await MediaTypeContainerService.GetAsync(result.Result.Key);
        Assert.NotNull(created);
        Assert.AreEqual("Child Container", created.Name);
        Assert.AreEqual(root.Id, created.ParentId);
    }

    [Test]
    public async Task Can_Create_Container_With_Explicit_Key()
    {
        var key = Guid.NewGuid();
        var result = await MediaTypeContainerService.CreateAsync(key, "Root Container", null, Constants.Security.SuperUserKey);
        Assert.IsTrue(result.Success);
        Assert.AreEqual(EntityContainerOperationStatus.Success, result.Status);
        Assert.AreEqual(key, result.Result.Key);

        var created = await MediaTypeContainerService.GetAsync(key);
        Assert.NotNull(created);
        Assert.AreEqual("Root Container", created.Name);
        Assert.AreEqual(Constants.System.Root, created.ParentId);
    }

    [Test]
    public async Task Can_Update_Container_At_Root()
    {
        var key = (await MediaTypeContainerService.CreateAsync(null, "Root Container", null, Constants.Security.SuperUserKey)).Result.Key;

        var result = await MediaTypeContainerService.UpdateAsync(key, "Root Container UPDATED", Constants.Security.SuperUserKey);
        Assert.IsTrue(result.Success);
        Assert.AreEqual(EntityContainerOperationStatus.Success, result.Status);

        var updated = await MediaTypeContainerService.GetAsync(key);
        Assert.NotNull(updated);
        Assert.AreEqual("Root Container UPDATED", updated.Name);
        Assert.AreEqual(Constants.System.Root, updated.ParentId);
    }

    [Test]
    public async Task Can_Update_Child_Container()
    {
        EntityContainer root = (await MediaTypeContainerService.CreateAsync(null, "Root Container", null, Constants.Security.SuperUserKey)).Result;
        EntityContainer child = (await MediaTypeContainerService.CreateAsync(null, "Child Container", root.Key, Constants.Security.SuperUserKey)).Result;

        var result = await MediaTypeContainerService.UpdateAsync(child.Key, "Child Container UPDATED", Constants.Security.SuperUserKey);
        Assert.IsTrue(result.Success);
        Assert.AreEqual(EntityContainerOperationStatus.Success, result.Status);

        EntityContainer updated = await MediaTypeContainerService.GetAsync(child.Key);
        Assert.NotNull(updated);
        Assert.AreEqual("Child Container UPDATED", updated.Name);
        Assert.AreEqual(root.Id, updated.ParentId);
    }

    [Test]
    public async Task Can_Get_Container_At_Root()
    {
        EntityContainer root = (await MediaTypeContainerService.CreateAsync(null,"Root Container", null, Constants.Security.SuperUserKey)).Result;

        EntityContainer created = await MediaTypeContainerService.GetAsync(root.Key);
        Assert.NotNull(created);
        Assert.AreEqual("Root Container", created.Name);
        Assert.AreEqual(Constants.System.Root, created.ParentId);
    }

    [Test]
    public async Task Can_Get_Child_Container()
    {
        EntityContainer root = (await MediaTypeContainerService.CreateAsync(null,"Root Container", null, Constants.Security.SuperUserKey)).Result;
        EntityContainer child = (await MediaTypeContainerService.CreateAsync(null, "Child Container", root.Key, Constants.Security.SuperUserKey)).Result;

        EntityContainer created = await MediaTypeContainerService.GetAsync(child.Key);
        Assert.IsNotNull(created);
        Assert.AreEqual("Child Container", created.Name);
        Assert.AreEqual(root.Id, child.ParentId);
    }

    [Test]
    public async Task Can_Delete_Container_At_Root()
    {
        EntityContainer root = (await MediaTypeContainerService.CreateAsync(null,"Root Container", null, Constants.Security.SuperUserKey)).Result;

        var result = await MediaTypeContainerService.DeleteAsync(root.Key, Constants.Security.SuperUserKey);
        Assert.IsTrue(result.Success);
        Assert.AreEqual(EntityContainerOperationStatus.Success, result.Status);

        var current = await MediaTypeContainerService.GetAsync(root.Key);
        Assert.IsNull(current);
    }

    [Test]
    public async Task Can_Delete_Child_Container()
    {
        EntityContainer root = (await MediaTypeContainerService.CreateAsync(null,"Root Container", null, Constants.Security.SuperUserKey)).Result;
        EntityContainer child = (await MediaTypeContainerService.CreateAsync(null, "Child Container", root.Key, Constants.Security.SuperUserKey)).Result;

        var result = await MediaTypeContainerService.DeleteAsync(child.Key, Constants.Security.SuperUserKey);
        Assert.IsTrue(result.Success);
        Assert.AreEqual(EntityContainerOperationStatus.Success, result.Status);

        child = await MediaTypeContainerService.GetAsync(child.Key);
        Assert.IsNull(child);

        root = await MediaTypeContainerService.GetAsync(root.Key);
        Assert.IsNotNull(root);
    }

    [Test]
    public async Task Cannot_Create_Child_Container_Below_Invalid_Parent()
    {
        var key = Guid.NewGuid();
        var result = await MediaTypeContainerService.CreateAsync(key, "Child Container", Guid.NewGuid(), Constants.Security.SuperUserKey);
        Assert.IsFalse(result.Success);
        Assert.AreEqual(EntityContainerOperationStatus.ParentNotFound, result.Status);

        var created = await MediaTypeContainerService.GetAsync(key);
        Assert.IsNull(created);
    }

    [Test]
    public async Task Cannot_Delete_Container_With_Child_Container()
    {
        EntityContainer root = (await MediaTypeContainerService.CreateAsync(null,"Root Container", null, Constants.Security.SuperUserKey)).Result;
        EntityContainer child = (await MediaTypeContainerService.CreateAsync(null, "Child Container", root.Key, Constants.Security.SuperUserKey)).Result;

        var result = await MediaTypeContainerService.DeleteAsync(root.Key, Constants.Security.SuperUserKey);
        Assert.IsFalse(result.Success);
        Assert.AreEqual(EntityContainerOperationStatus.NotEmpty, result.Status);

        var current = await MediaTypeContainerService.GetAsync(root.Key);
        Assert.IsNotNull(current);
    }

    [Test]
    public async Task Cannot_Delete_Container_With_Child_MediaType()
    {
        EntityContainer container = (await MediaTypeContainerService.CreateAsync(null,"Root Container", null, Constants.Security.SuperUserKey)).Result;

        IMediaType mediaType = new MediaType(ShortStringHelper, container.Id)
        {
            Alias = "test", Name = "Test"
        };
        MediaTypeService.Save(mediaType);

        var result = await MediaTypeContainerService.DeleteAsync(container.Key, Constants.Security.SuperUserKey);
        Assert.IsFalse(result.Success);
        Assert.AreEqual(EntityContainerOperationStatus.NotEmpty, result.Status);

        var currentContainer = await MediaTypeContainerService.GetAsync(container.Key);
        Assert.IsNotNull(currentContainer);

        var currentMediaType = MediaTypeService.Get(mediaType.Key);
        Assert.IsNotNull(currentMediaType);
    }

    [Test]
    public async Task Cannot_Delete_Non_Existing_Container()
    {
        var result = await MediaTypeContainerService.DeleteAsync(Guid.NewGuid(), Constants.Security.SuperUserKey);
        Assert.IsFalse(result.Success);
        Assert.AreEqual(EntityContainerOperationStatus.NotFound, result.Status);
    }
}
