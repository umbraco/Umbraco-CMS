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
            Assert.IsTrue(result.Success);
            Assert.AreEqual(EntityContainerOperationStatus.Success, result.Status);
        });

        var created = await ElementContainerService.GetAsync(result.Result.Key);
        Assert.NotNull(created);
        Assert.Multiple(() =>
        {
            Assert.AreEqual("Root Container", created.Name);
            Assert.AreEqual(Constants.System.Root, created.ParentId);
        });
    }

    [Test]
    public async Task Can_Create_Child_Container()
    {
        EntityContainer root = (await ElementContainerService.CreateAsync(null, "Root Container", null, Constants.Security.SuperUserKey)).Result;

        var result = await ElementContainerService.CreateAsync(null, "Child Container", root.Key, Constants.Security.SuperUserKey);
        Assert.Multiple(() =>
        {
            Assert.IsTrue(result.Success);
            Assert.AreEqual(EntityContainerOperationStatus.Success, result.Status);
        });

        var created = await ElementContainerService.GetAsync(result.Result.Key);
        Assert.NotNull(created);
        Assert.Multiple(() =>
        {
            Assert.AreEqual("Child Container", created.Name);
            Assert.AreEqual(root.Id, created.ParentId);
        });
    }

    [Test]
    public async Task Can_Create_Container_With_Explicit_Key()
    {
        var key = Guid.NewGuid();
        var result = await ElementContainerService.CreateAsync(key, "Root Container", null, Constants.Security.SuperUserKey);
        Assert.Multiple(() =>
        {
            Assert.IsTrue(result.Success);
            Assert.AreEqual(EntityContainerOperationStatus.Success, result.Status);
            Assert.AreEqual(key, result.Result.Key);
        });

        var created = await ElementContainerService.GetAsync(key);
        Assert.NotNull(created);
        Assert.Multiple(() =>
        {
            Assert.AreEqual("Root Container", created.Name);
            Assert.AreEqual(Constants.System.Root, created.ParentId);
        });
    }

    [Test]
    public async Task Cannot_Create_Child_Container_Below_Invalid_Parent()
    {
        var key = Guid.NewGuid();
        var result = await ElementContainerService.CreateAsync(key, "Child Container", Guid.NewGuid(), Constants.Security.SuperUserKey);
        Assert.Multiple(() =>
        {
            Assert.IsFalse(result.Success);
            Assert.AreEqual(EntityContainerOperationStatus.ParentNotFound, result.Status);
        });

        var created = await ElementContainerService.GetAsync(key);
        Assert.IsNull(created);
    }
}
