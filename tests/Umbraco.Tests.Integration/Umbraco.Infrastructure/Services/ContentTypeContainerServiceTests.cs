using NUnit.Framework;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Services.OperationStatus;
using Umbraco.Cms.Tests.Common.Testing;
using Umbraco.Cms.Tests.Integration.Testing;

namespace Umbraco.Cms.Tests.Integration.Umbraco.Infrastructure.Services;

/// <summary>
///     Tests covering the ContentTypeContainerService
/// </summary>
[TestFixture]
[UmbracoTest(Database = UmbracoTestOptions.Database.NewSchemaPerTest)]
internal sealed class ContentTypeContainerServiceTests : UmbracoIntegrationTest
{
    private IContentTypeContainerService ContentTypeContainerService => GetRequiredService<IContentTypeContainerService>();

    private IContentTypeService ContentTypeService => GetRequiredService<IContentTypeService>();

    [Test]
    public async Task Can_Create_Container_At_Root()
    {
        var result = await ContentTypeContainerService.CreateAsync(null, "Root Container", null, Constants.Security.SuperUserKey);
        Assert.That(result.Success, Is.True);
        Assert.That(result.Status, Is.EqualTo(EntityContainerOperationStatus.Success));

        var created = await ContentTypeContainerService.GetAsync(result.Result.Key);
        Assert.That(created, Is.Not.Null);
        Assert.That(created.Name, Is.EqualTo("Root Container"));
        Assert.That(created.ParentId, Is.EqualTo(Constants.System.Root));
    }

    [Test]
    public async Task Can_Create_Child_Container()
    {
        EntityContainer root = (await ContentTypeContainerService.CreateAsync(null, "Root Container", null, Constants.Security.SuperUserKey)).Result;

        var result = await ContentTypeContainerService.CreateAsync(null, "Child Container", root.Key, Constants.Security.SuperUserKey);
        Assert.That(result.Success, Is.True);
        Assert.That(result.Status, Is.EqualTo(EntityContainerOperationStatus.Success));

        var created = await ContentTypeContainerService.GetAsync(result.Result.Key);
        Assert.That(created, Is.Not.Null);
        Assert.That(created.Name, Is.EqualTo("Child Container"));
        Assert.That(created.ParentId, Is.EqualTo(root.Id));
    }

    [Test]
    public async Task Can_Create_Container_With_Explicit_Key()
    {
        var key = Guid.NewGuid();
        var result = await ContentTypeContainerService.CreateAsync(key, "Root Container", null, Constants.Security.SuperUserKey);
        Assert.That(result.Success, Is.True);
        Assert.That(result.Status, Is.EqualTo(EntityContainerOperationStatus.Success));
        Assert.That(result.Result.Key, Is.EqualTo(key));

        var created = await ContentTypeContainerService.GetAsync(key);
        Assert.That(created, Is.Not.Null);
        Assert.That(created.Name, Is.EqualTo("Root Container"));
        Assert.That(created.ParentId, Is.EqualTo(Constants.System.Root));
    }

    [Test]
    public async Task Can_Update_Container_At_Root()
    {
        var key = (await ContentTypeContainerService.CreateAsync(null, "Root Container", null, Constants.Security.SuperUserKey)).Result.Key;

        var result = await ContentTypeContainerService.UpdateAsync(key, "Root Container UPDATED", Constants.Security.SuperUserKey);
        Assert.That(result.Success, Is.True);
        Assert.That(result.Status, Is.EqualTo(EntityContainerOperationStatus.Success));

        var updated = await ContentTypeContainerService.GetAsync(key);
        Assert.That(updated, Is.Not.Null);
        Assert.That(updated.Name, Is.EqualTo("Root Container UPDATED"));
        Assert.That(updated.ParentId, Is.EqualTo(Constants.System.Root));
    }

    [Test]
    public async Task Can_Update_Child_Container()
    {
        EntityContainer root = (await ContentTypeContainerService.CreateAsync(null, "Root Container", null, Constants.Security.SuperUserKey)).Result;
        EntityContainer child = (await ContentTypeContainerService.CreateAsync(null, "Child Container", root.Key, Constants.Security.SuperUserKey)).Result;

        var result = await ContentTypeContainerService.UpdateAsync(child.Key, "Child Container UPDATED", Constants.Security.SuperUserKey);
        Assert.That(result.Success, Is.True);
        Assert.That(result.Status, Is.EqualTo(EntityContainerOperationStatus.Success));

        EntityContainer updated = await ContentTypeContainerService.GetAsync(child.Key);
        Assert.That(updated, Is.Not.Null);
        Assert.That(updated.Name, Is.EqualTo("Child Container UPDATED"));
        Assert.That(updated.ParentId, Is.EqualTo(root.Id));
    }

    [Test]
    public async Task Can_Get_Container_At_Root()
    {
        EntityContainer root = (await ContentTypeContainerService.CreateAsync(null,"Root Container", null, Constants.Security.SuperUserKey)).Result;

        EntityContainer created = await ContentTypeContainerService.GetAsync(root.Key);
        Assert.That(created, Is.Not.Null);
        Assert.That(created.Name, Is.EqualTo("Root Container"));
        Assert.That(created.ParentId, Is.EqualTo(Constants.System.Root));
    }

    [Test]
    public async Task Can_Get_Child_Container()
    {
        EntityContainer root = (await ContentTypeContainerService.CreateAsync(null,"Root Container", null, Constants.Security.SuperUserKey)).Result;
        EntityContainer child = (await ContentTypeContainerService.CreateAsync(null, "Child Container", root.Key, Constants.Security.SuperUserKey)).Result;

        EntityContainer created = await ContentTypeContainerService.GetAsync(child.Key);
        Assert.That(created, Is.Not.Null);
        Assert.That(created.Name, Is.EqualTo("Child Container"));
        Assert.That(child.ParentId, Is.EqualTo(root.Id));
    }

    [Test]
    public async Task Can_Delete_Container_At_Root()
    {
        EntityContainer root = (await ContentTypeContainerService.CreateAsync(null,"Root Container", null, Constants.Security.SuperUserKey)).Result;

        var result = await ContentTypeContainerService.DeleteAsync(root.Key, Constants.Security.SuperUserKey);
        Assert.That(result.Success, Is.True);
        Assert.That(result.Status, Is.EqualTo(EntityContainerOperationStatus.Success));

        var current = await ContentTypeContainerService.GetAsync(root.Key);
        Assert.That(current, Is.Null);
    }

    [Test]
    public async Task Can_Delete_Child_Container()
    {
        EntityContainer root = (await ContentTypeContainerService.CreateAsync(null,"Root Container", null, Constants.Security.SuperUserKey)).Result;
        EntityContainer child = (await ContentTypeContainerService.CreateAsync(null, "Child Container", root.Key, Constants.Security.SuperUserKey)).Result;

        var result = await ContentTypeContainerService.DeleteAsync(child.Key, Constants.Security.SuperUserKey);
        Assert.That(result.Success, Is.True);
        Assert.That(result.Status, Is.EqualTo(EntityContainerOperationStatus.Success));

        child = await ContentTypeContainerService.GetAsync(child.Key);
        Assert.That(child, Is.Null);

        root = await ContentTypeContainerService.GetAsync(root.Key);
        Assert.That(root, Is.Not.Null);
    }

    [Test]
    public async Task Cannot_Create_Child_Container_Below_Invalid_Parent()
    {
        var key = Guid.NewGuid();
        var result = await ContentTypeContainerService.CreateAsync(key, "Child Container", Guid.NewGuid(), Constants.Security.SuperUserKey);
        Assert.That(result.Success, Is.False);
        Assert.That(result.Status, Is.EqualTo(EntityContainerOperationStatus.ParentNotFound));

        var created = await ContentTypeContainerService.GetAsync(key);
        Assert.That(created, Is.Null);
    }

    [Test]
    public async Task Cannot_Delete_Container_With_Child_Container()
    {
        EntityContainer root = (await ContentTypeContainerService.CreateAsync(null,"Root Container", null, Constants.Security.SuperUserKey)).Result;
        EntityContainer child = (await ContentTypeContainerService.CreateAsync(null, "Child Container", root.Key, Constants.Security.SuperUserKey)).Result;

        var result = await ContentTypeContainerService.DeleteAsync(root.Key, Constants.Security.SuperUserKey);
        Assert.That(result.Success, Is.False);
        Assert.That(result.Status, Is.EqualTo(EntityContainerOperationStatus.NotEmpty));

        var current = await ContentTypeContainerService.GetAsync(root.Key);
        Assert.That(current, Is.Not.Null);
    }

    [Test]
    public async Task Cannot_Delete_Container_With_Child_ContentType()
    {
        EntityContainer container = (await ContentTypeContainerService.CreateAsync(null,"Root Container", null, Constants.Security.SuperUserKey)).Result;

        IContentType contentType = new ContentType(ShortStringHelper, container.Id)
        {
            Alias = "test", Name = "Test"
        };
        await ContentTypeService.CreateAsync(contentType, Constants.Security.SuperUserKey);

        var result = await ContentTypeContainerService.DeleteAsync(container.Key, Constants.Security.SuperUserKey);
        Assert.That(result.Success, Is.False);
        Assert.That(result.Status, Is.EqualTo(EntityContainerOperationStatus.NotEmpty));

        var currentContainer = await ContentTypeContainerService.GetAsync(container.Key);
        Assert.That(currentContainer, Is.Not.Null);

        var currentContentType = ContentTypeService.Get(contentType.Key);
        Assert.That(currentContentType, Is.Not.Null);
    }

    [Test]
    public async Task Cannot_Delete_Non_Existing_Container()
    {
        var result = await ContentTypeContainerService.DeleteAsync(Guid.NewGuid(), Constants.Security.SuperUserKey);
        Assert.That(result.Success, Is.False);
        Assert.That(result.Status, Is.EqualTo(EntityContainerOperationStatus.NotFound));
    }
}
