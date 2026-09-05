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
        Assert.That(result.Success, Is.True);
        Assert.That(result.Status, Is.EqualTo(EntityContainerOperationStatus.Success));

        var created = await MediaTypeContainerService.GetAsync(result.Result.Key);
        Assert.That(created, Is.Not.Null);
        Assert.That(created.Name, Is.EqualTo("Root Container"));
        Assert.That(created.ParentId, Is.EqualTo(Constants.System.Root));
    }

    [Test]
    public async Task Can_Create_Child_Container()
    {
        EntityContainer root = (await MediaTypeContainerService.CreateAsync(null, "Root Container", null, Constants.Security.SuperUserKey)).Result;

        var result = await MediaTypeContainerService.CreateAsync(null, "Child Container", root.Key, Constants.Security.SuperUserKey);
        Assert.That(result.Success, Is.True);
        Assert.That(result.Status, Is.EqualTo(EntityContainerOperationStatus.Success));

        var created = await MediaTypeContainerService.GetAsync(result.Result.Key);
        Assert.That(created, Is.Not.Null);
        Assert.That(created.Name, Is.EqualTo("Child Container"));
        Assert.That(created.ParentId, Is.EqualTo(root.Id));
    }

    [Test]
    public async Task Can_Create_Container_With_Explicit_Key()
    {
        var key = Guid.NewGuid();
        var result = await MediaTypeContainerService.CreateAsync(key, "Root Container", null, Constants.Security.SuperUserKey);
        Assert.That(result.Success, Is.True);
        Assert.That(result.Status, Is.EqualTo(EntityContainerOperationStatus.Success));
        Assert.That(result.Result.Key, Is.EqualTo(key));

        var created = await MediaTypeContainerService.GetAsync(key);
        Assert.That(created, Is.Not.Null);
        Assert.That(created.Name, Is.EqualTo("Root Container"));
        Assert.That(created.ParentId, Is.EqualTo(Constants.System.Root));
    }

    [Test]
    public async Task Can_Update_Container_At_Root()
    {
        var key = (await MediaTypeContainerService.CreateAsync(null, "Root Container", null, Constants.Security.SuperUserKey)).Result.Key;

        var result = await MediaTypeContainerService.UpdateAsync(key, "Root Container UPDATED", Constants.Security.SuperUserKey);
        Assert.That(result.Success, Is.True);
        Assert.That(result.Status, Is.EqualTo(EntityContainerOperationStatus.Success));

        var updated = await MediaTypeContainerService.GetAsync(key);
        Assert.That(updated, Is.Not.Null);
        Assert.That(updated.Name, Is.EqualTo("Root Container UPDATED"));
        Assert.That(updated.ParentId, Is.EqualTo(Constants.System.Root));
    }

    [Test]
    public async Task Can_Update_Child_Container()
    {
        EntityContainer root = (await MediaTypeContainerService.CreateAsync(null, "Root Container", null, Constants.Security.SuperUserKey)).Result;
        EntityContainer child = (await MediaTypeContainerService.CreateAsync(null, "Child Container", root.Key, Constants.Security.SuperUserKey)).Result;

        var result = await MediaTypeContainerService.UpdateAsync(child.Key, "Child Container UPDATED", Constants.Security.SuperUserKey);
        Assert.That(result.Success, Is.True);
        Assert.That(result.Status, Is.EqualTo(EntityContainerOperationStatus.Success));

        EntityContainer updated = await MediaTypeContainerService.GetAsync(child.Key);
        Assert.That(updated, Is.Not.Null);
        Assert.That(updated.Name, Is.EqualTo("Child Container UPDATED"));
        Assert.That(updated.ParentId, Is.EqualTo(root.Id));
    }

    [Test]
    public async Task Can_Get_Container_At_Root()
    {
        EntityContainer root = (await MediaTypeContainerService.CreateAsync(null,"Root Container", null, Constants.Security.SuperUserKey)).Result;

        EntityContainer created = await MediaTypeContainerService.GetAsync(root.Key);
        Assert.That(created, Is.Not.Null);
        Assert.That(created.Name, Is.EqualTo("Root Container"));
        Assert.That(created.ParentId, Is.EqualTo(Constants.System.Root));
    }

    [Test]
    public async Task Can_Get_Child_Container()
    {
        EntityContainer root = (await MediaTypeContainerService.CreateAsync(null,"Root Container", null, Constants.Security.SuperUserKey)).Result;
        EntityContainer child = (await MediaTypeContainerService.CreateAsync(null, "Child Container", root.Key, Constants.Security.SuperUserKey)).Result;

        EntityContainer created = await MediaTypeContainerService.GetAsync(child.Key);
        Assert.That(created, Is.Not.Null);
        Assert.That(created.Name, Is.EqualTo("Child Container"));
        Assert.That(child.ParentId, Is.EqualTo(root.Id));
    }

    [Test]
    public async Task Can_Delete_Container_At_Root()
    {
        EntityContainer root = (await MediaTypeContainerService.CreateAsync(null,"Root Container", null, Constants.Security.SuperUserKey)).Result;

        var result = await MediaTypeContainerService.DeleteAsync(root.Key, Constants.Security.SuperUserKey);
        Assert.That(result.Success, Is.True);
        Assert.That(result.Status, Is.EqualTo(EntityContainerOperationStatus.Success));

        var current = await MediaTypeContainerService.GetAsync(root.Key);
        Assert.That(current, Is.Null);
    }

    [Test]
    public async Task Can_Delete_Child_Container()
    {
        EntityContainer root = (await MediaTypeContainerService.CreateAsync(null,"Root Container", null, Constants.Security.SuperUserKey)).Result;
        EntityContainer child = (await MediaTypeContainerService.CreateAsync(null, "Child Container", root.Key, Constants.Security.SuperUserKey)).Result;

        var result = await MediaTypeContainerService.DeleteAsync(child.Key, Constants.Security.SuperUserKey);
        Assert.That(result.Success, Is.True);
        Assert.That(result.Status, Is.EqualTo(EntityContainerOperationStatus.Success));

        child = await MediaTypeContainerService.GetAsync(child.Key);
        Assert.That(child, Is.Null);

        root = await MediaTypeContainerService.GetAsync(root.Key);
        Assert.That(root, Is.Not.Null);
    }

    [Test]
    public async Task Cannot_Create_Child_Container_Below_Invalid_Parent()
    {
        var key = Guid.NewGuid();
        var result = await MediaTypeContainerService.CreateAsync(key, "Child Container", Guid.NewGuid(), Constants.Security.SuperUserKey);
        Assert.That(result.Success, Is.False);
        Assert.That(result.Status, Is.EqualTo(EntityContainerOperationStatus.ParentNotFound));

        var created = await MediaTypeContainerService.GetAsync(key);
        Assert.That(created, Is.Null);
    }

    [Test]
    public async Task Cannot_Delete_Container_With_Child_Container()
    {
        EntityContainer root = (await MediaTypeContainerService.CreateAsync(null,"Root Container", null, Constants.Security.SuperUserKey)).Result;
        EntityContainer child = (await MediaTypeContainerService.CreateAsync(null, "Child Container", root.Key, Constants.Security.SuperUserKey)).Result;

        var result = await MediaTypeContainerService.DeleteAsync(root.Key, Constants.Security.SuperUserKey);
        Assert.That(result.Success, Is.False);
        Assert.That(result.Status, Is.EqualTo(EntityContainerOperationStatus.NotEmpty));

        var current = await MediaTypeContainerService.GetAsync(root.Key);
        Assert.That(current, Is.Not.Null);
    }

    [Test]
    public async Task Cannot_Delete_Container_With_Child_MediaType()
    {
        EntityContainer container = (await MediaTypeContainerService.CreateAsync(null,"Root Container", null, Constants.Security.SuperUserKey)).Result;

        IMediaType mediaType = new MediaType(ShortStringHelper, container.Id)
        {
            Alias = "test", Name = "Test"
        };
        await MediaTypeService.CreateAsync(mediaType, Constants.Security.SuperUserKey);

        var result = await MediaTypeContainerService.DeleteAsync(container.Key, Constants.Security.SuperUserKey);
        Assert.That(result.Success, Is.False);
        Assert.That(result.Status, Is.EqualTo(EntityContainerOperationStatus.NotEmpty));

        var currentContainer = await MediaTypeContainerService.GetAsync(container.Key);
        Assert.That(currentContainer, Is.Not.Null);

        var currentMediaType = MediaTypeService.Get(mediaType.Key);
        Assert.That(currentMediaType, Is.Not.Null);
    }

    [Test]
    public async Task Cannot_Delete_Non_Existing_Container()
    {
        var result = await MediaTypeContainerService.DeleteAsync(Guid.NewGuid(), Constants.Security.SuperUserKey);
        Assert.That(result.Success, Is.False);
        Assert.That(result.Status, Is.EqualTo(EntityContainerOperationStatus.NotFound));
    }
}
