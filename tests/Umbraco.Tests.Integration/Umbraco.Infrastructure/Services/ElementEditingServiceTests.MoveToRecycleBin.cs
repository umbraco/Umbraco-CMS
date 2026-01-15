using NUnit.Framework;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Services.OperationStatus;

namespace Umbraco.Cms.Tests.Integration.Umbraco.Infrastructure.Services;

public partial class ElementEditingServiceTests
{
    [Test]
    public async Task Can_Move_Element_From_Root_To_Recycle_Bin()
    {
        var element = await CreateInvariantElement();
        Assert.AreEqual(1, EntityService.GetRootEntities(UmbracoObjectTypes.Element).Count());

        var moveResult = await ElementEditingService.MoveToRecycleBinAsync(element.Key, Constants.Security.SuperUserKey);
        Assert.Multiple(() =>
        {
            Assert.IsTrue(moveResult.Success);
            Assert.AreEqual(ContentEditingOperationStatus.Success, moveResult.Result);
        });

        await AssertElementIsInRecycleBin(element.Key);
        Assert.AreEqual(0, EntityService.GetRootEntities(UmbracoObjectTypes.Element).Count());
    }

    [Test]
    public async Task Can_Move_Element_From_A_Folder_To_Recycle_Bin()
    {
        var containerKey = Guid.NewGuid();
        var container = (await ElementContainerService.CreateAsync(containerKey, "Root Container", null, Constants.Security.SuperUserKey)).Result;

        var element = await CreateInvariantElement(containerKey);
        Assert.AreEqual(container.Id, element.ParentId);
        Assert.AreEqual(1, GetFolderChildren(containerKey).Length);

        var moveResult = await ElementEditingService.MoveToRecycleBinAsync(element.Key, Constants.Security.SuperUserKey);
        Assert.Multiple(() =>
        {
            Assert.IsTrue(moveResult.Success);
            Assert.AreEqual(ContentEditingOperationStatus.Success, moveResult.Result);
        });

        await AssertElementIsInRecycleBin(element.Key);
        Assert.AreEqual(0, EntityService.GetRootEntities(UmbracoObjectTypes.Element).Count());
    }

    [Test]
    public async Task Can_Count_Recycle_Bin_Children_At_Root()
    {
        var container = (await ElementContainerService.CreateAsync(Guid.NewGuid(), "Root Container", null, Constants.Security.SuperUserKey)).Result;
        var element = await CreateInvariantElement();

        await ElementContainerService.MoveToRecycleBinAsync(container.Key, Constants.Security.SuperUserKey);
        await ElementEditingService.MoveToRecycleBinAsync(element.Key, Constants.Security.SuperUserKey);

        var entities = EntityService
            .GetPagedChildren(
                Constants.System.RecycleBinElementKey,
                parentObjectTypes: [UmbracoObjectTypes.ElementContainer],
                childObjectTypes: [UmbracoObjectTypes.ElementContainer, UmbracoObjectTypes.Element],
                skip: 0,
                take: 0,
                trashed: true,
                out var total)
            .ToArray();

        Assert.AreEqual(2, total);
        Assert.IsEmpty(entities);
    }

    [Test]
    public async Task Can_Count_Recycle_Bin_Children_In_Trashed_Folder()
    {
        var rootContainer = (await ElementContainerService.CreateAsync(Guid.NewGuid(), "Root Container", null, Constants.Security.SuperUserKey)).Result;
        await ElementContainerService.CreateAsync(Guid.NewGuid(), "Child Container 1", rootContainer.Key, Constants.Security.SuperUserKey);
        await ElementContainerService.CreateAsync(Guid.NewGuid(), "Child Container 2", rootContainer.Key, Constants.Security.SuperUserKey);
        await CreateInvariantElement(rootContainer.Key);

        await ElementContainerService.MoveToRecycleBinAsync(rootContainer.Key, Constants.Security.SuperUserKey);

        var entities = EntityService
            .GetPagedChildren(
                rootContainer.Key,
                parentObjectTypes: [UmbracoObjectTypes.ElementContainer],
                childObjectTypes: [UmbracoObjectTypes.ElementContainer, UmbracoObjectTypes.Element],
                skip: 0,
                take: 0,
                trashed: true,
                out var total)
            .ToArray();

        Assert.AreEqual(3, total);
        Assert.IsEmpty(entities);
    }

    private async Task AssertElementIsInRecycleBin(Guid elementKey)
    {
        var element = await ElementEditingService.GetAsync(elementKey);
        Assert.NotNull(element);
        Assert.Multiple(() =>
        {
            Assert.AreEqual(Constants.System.RecycleBinElement, element.ParentId);
            Assert.AreEqual($"{Constants.System.RecycleBinElementPathPrefix}{element.Id}", element.Path);
            Assert.IsTrue(element.Trashed);
        });

        var recycleBinItems = EntityService
            .GetPagedTrashedChildren(Constants.System.RecycleBinElementKey, UmbracoObjectTypes.Element, 0, 10, out var total)
            .ToArray();
        Assert.Multiple(() =>
        {
            Assert.AreEqual(1, total);
            Assert.AreEqual(1, recycleBinItems.Length);
        });

        Assert.AreEqual(element.Key, recycleBinItems[0].Key);
    }
}
