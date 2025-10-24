using NUnit.Framework;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Services.OperationStatus;

namespace Umbraco.Cms.Tests.Integration.Umbraco.Infrastructure.Services;

public partial class ElementEditingServiceTests
{
    [Test]
    public async Task Can_Move_Element_From_Root_To_A_Folder()
    {
        var containerKey = Guid.NewGuid();
        var container = (await ElementContainerService.CreateAsync(containerKey, "Root Container", null, Constants.Security.SuperUserKey)).Result;
        Assert.AreEqual(0, GetFolderChildren(containerKey).Length);

        var element = await CreateInvariantElement();

        var moveResult = await ElementEditingService.MoveAsync(element.Key, containerKey, Constants.Security.SuperUserKey);
        Assert.Multiple(() =>
        {
            Assert.IsTrue(moveResult.Success);
            Assert.AreEqual(ContentEditingOperationStatus.Success, moveResult.Result);
        });

        element = await ElementEditingService.GetAsync(element.Key);
        Assert.NotNull(element);
        Assert.Multiple(() =>
        {
            Assert.AreEqual(container.Id, element.ParentId);
            Assert.AreEqual($"{container.Path},{element.Id}", element.Path);
        });

        var result = GetFolderChildren(containerKey);
        Assert.AreEqual(1, result.Length);
        Assert.AreEqual(element.Key, result.First().Key);
    }

    [Test]
    public async Task Can_Move_Element_From_A_Folder_To_Root()
    {
        var containerKey = Guid.NewGuid();
        var container = (await ElementContainerService.CreateAsync(containerKey, "Root Container", null, Constants.Security.SuperUserKey)).Result;

        var element = await CreateInvariantElement(containerKey);
        Assert.AreEqual(container.Id, element.ParentId);
        Assert.AreEqual(1, GetFolderChildren(containerKey).Length);

        var moveResult = await ElementEditingService.MoveAsync(element.Key, null, Constants.Security.SuperUserKey);
        Assert.Multiple(() =>
        {
            Assert.IsTrue(moveResult.Success);
            Assert.AreEqual(ContentEditingOperationStatus.Success, moveResult.Result);
        });

        element = await ElementEditingService.GetAsync(element.Key);
        Assert.NotNull(element);
        Assert.Multiple(() =>
        {
            Assert.AreEqual(Constants.System.Root, element.ParentId);
            Assert.AreEqual($"{Constants.System.Root},{element.Id}", element.Path);
        });

        Assert.AreEqual(0, GetFolderChildren(containerKey).Length);
    }

    [Test]
    public async Task Can_Move_Element_Between_Folders()
    {
        var containerKey1 = Guid.NewGuid();
        await ElementContainerService.CreateAsync(containerKey1, "Container #1", null, Constants.Security.SuperUserKey);
        var containerKey2 = Guid.NewGuid();
        var container2 = (await ElementContainerService.CreateAsync(containerKey2, "Container #2", null, Constants.Security.SuperUserKey)).Result;

        var element = await CreateInvariantElement(containerKey1);
        Assert.AreEqual(1, GetFolderChildren(containerKey1).Length);

        await ElementEditingService.MoveAsync(element.Key, containerKey2, Constants.Security.SuperUserKey);

        element = await ElementEditingService.GetAsync(element.Key);
        Assert.NotNull(element);
        Assert.Multiple(() =>
        {
            Assert.AreEqual(container2.Id, element.ParentId);
            Assert.AreEqual($"{container2.Path},{element.Id}", element.Path);
        });

        var result = GetFolderChildren(containerKey2);
        Assert.AreEqual(1, result.Length);
        Assert.AreEqual(element.Key, result.First().Key);

        Assert.AreEqual(0, GetFolderChildren(containerKey1).Length);
    }
}
