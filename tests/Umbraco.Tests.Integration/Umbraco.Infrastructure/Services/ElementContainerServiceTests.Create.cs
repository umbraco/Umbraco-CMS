using NUnit.Framework;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Services.OperationStatus;

namespace Umbraco.Cms.Tests.Integration.Umbraco.Infrastructure.Services;

public partial class ElementContainerServiceTests
{
    [Test]
    public async Task Can_Create_Container_At_Root()
    {
        var result = await ElementContainerService.CreateAsync(null, "Root Container", null, Constants.Security.SuperUserKey);
        Assert.Multiple(() =>
        {
            Assert.That(result.Success, Is.True);
            Assert.That(result.Status, Is.EqualTo(EntityContainerOperationStatus.Success));
        });

        var created = await ElementContainerService.GetAsync(result.Result.Key);
        Assert.That(created, Is.Not.Null);
        Assert.Multiple(() =>
        {
            Assert.That(created.Name, Is.EqualTo("Root Container"));
            Assert.That(created.ParentId, Is.EqualTo(Constants.System.Root));
        });
    }

    [Test]
    public async Task Can_Create_Child_Container()
    {
        EntityContainer root = (await ElementContainerService.CreateAsync(null, "Root Container", null, Constants.Security.SuperUserKey)).Result;

        var result = await ElementContainerService.CreateAsync(null, "Child Container", root.Key, Constants.Security.SuperUserKey);
        Assert.Multiple(() =>
        {
            Assert.That(result.Success, Is.True);
            Assert.That(result.Status, Is.EqualTo(EntityContainerOperationStatus.Success));
        });

        var created = await ElementContainerService.GetAsync(result.Result.Key);
        Assert.That(created, Is.Not.Null);
        Assert.Multiple(() =>
        {
            Assert.That(created.Name, Is.EqualTo("Child Container"));
            Assert.That(created.ParentId, Is.EqualTo(root.Id));
        });
    }

    [Test]
    public async Task Can_Create_Container_With_Explicit_Key()
    {
        var key = Guid.NewGuid();
        var result = await ElementContainerService.CreateAsync(key, "Root Container", null, Constants.Security.SuperUserKey);
        Assert.Multiple(() =>
        {
            Assert.That(result.Success, Is.True);
            Assert.That(result.Status, Is.EqualTo(EntityContainerOperationStatus.Success));
            Assert.That(result.Result.Key, Is.EqualTo(key));
        });

        var created = await ElementContainerService.GetAsync(key);
        Assert.That(created, Is.Not.Null);
        Assert.Multiple(() =>
        {
            Assert.That(created.Name, Is.EqualTo("Root Container"));
            Assert.That(created.ParentId, Is.EqualTo(Constants.System.Root));
        });
    }

    [Test]
    public async Task Cannot_Create_Child_Container_Below_Invalid_Parent()
    {
        var key = Guid.NewGuid();
        var result = await ElementContainerService.CreateAsync(key, "Child Container", Guid.NewGuid(), Constants.Security.SuperUserKey);
        Assert.Multiple(() =>
        {
            Assert.That(result.Success, Is.False);
            Assert.That(result.Status, Is.EqualTo(EntityContainerOperationStatus.ParentNotFound));
        });

        var created = await ElementContainerService.GetAsync(key);
        Assert.That(created, Is.Null);
    }
}
