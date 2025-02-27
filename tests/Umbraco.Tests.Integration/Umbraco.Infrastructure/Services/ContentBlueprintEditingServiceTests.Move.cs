using NUnit.Framework;
using Umbraco.Cms.Core;

namespace Umbraco.Cms.Tests.Integration.Umbraco.Infrastructure.Services;

public partial class ContentBlueprintEditingServiceTests
{
    [Test]
    public async Task Can_Move_Blueprint_From_Root_To_A_Folder()
    {
        var containerKey = Guid.NewGuid();
        var container = (await ContentBlueprintContainerService.CreateAsync(containerKey, "Root Container", null, Constants.Security.SuperUserKey)).Result;
        Assert.AreEqual(0,  GetBlueprintChildren(containerKey).Length);

        var blueprintKey = Guid.NewGuid();
        await ContentBlueprintEditingService.CreateAsync(SimpleContentBlueprintCreateModel(blueprintKey, null), Constants.Security.SuperUserKey);

        await ContentBlueprintEditingService.MoveAsync(blueprintKey, containerKey, Constants.Security.SuperUserKey);

        var blueprint = await ContentBlueprintEditingService.GetAsync(blueprintKey);
        Assert.NotNull(blueprint);
        Assert.Multiple(() =>
        {
            Assert.AreEqual(container.Id, blueprint.ParentId);
            Assert.AreEqual($"{container.Path},{blueprint.Id}", blueprint.Path);
        });

        var result = GetBlueprintChildren(containerKey);
        Assert.Multiple(() =>
        {
            Assert.AreEqual(1, result.Length);
            Assert.AreEqual(blueprintKey, result.First().Key);
        });
    }

    [Test]
    public async Task Can_Move_Blueprint_From_A_Folder_To_Root()
    {
        var containerKey = Guid.NewGuid();
        await ContentBlueprintContainerService.CreateAsync(containerKey, "Root Container", null, Constants.Security.SuperUserKey);

        var blueprintKey = Guid.NewGuid();
        await ContentBlueprintEditingService.CreateAsync(SimpleContentBlueprintCreateModel(blueprintKey, containerKey), Constants.Security.SuperUserKey);
        Assert.AreEqual(1, GetBlueprintChildren(containerKey).Length);

        await ContentBlueprintEditingService.MoveAsync(blueprintKey, null, Constants.Security.SuperUserKey);
        Assert.AreEqual(0, GetBlueprintChildren(containerKey).Length);

        var blueprint = await ContentBlueprintEditingService.GetAsync(blueprintKey);
        Assert.NotNull(blueprint);
        Assert.Multiple(() =>
        {
            Assert.AreEqual(Constants.System.Root, blueprint.ParentId);
            Assert.AreEqual($"{Constants.System.Root},{blueprint.Id}", blueprint.Path);
        });

        var result = GetBlueprintChildren(null);
        Assert.Multiple(() =>
        {
            Assert.AreEqual(1, result.Length);
            Assert.AreEqual(blueprintKey, result.First().Key);
        });
    }

    [Test]
    public async Task Can_Move_Blueprint_Between_Folders()
    {
        var containerKey1 = Guid.NewGuid();
        await ContentBlueprintContainerService.CreateAsync(containerKey1, "Container #1", null, Constants.Security.SuperUserKey);
        var containerKey2 = Guid.NewGuid();
        var container2 = (await ContentBlueprintContainerService.CreateAsync(containerKey2, "Container #2", null, Constants.Security.SuperUserKey)).Result;

        var blueprintKey = Guid.NewGuid();
        await ContentBlueprintEditingService.CreateAsync(SimpleContentBlueprintCreateModel(blueprintKey, containerKey1), Constants.Security.SuperUserKey);

        await ContentBlueprintEditingService.MoveAsync(blueprintKey, containerKey2, Constants.Security.SuperUserKey);

        var blueprint = await ContentBlueprintEditingService.GetAsync(blueprintKey);
        Assert.NotNull(blueprint);
        Assert.Multiple(() =>
        {
            Assert.AreEqual(container2.Id, blueprint.ParentId);
            Assert.AreEqual($"{container2.Path},{blueprint.Id}", blueprint.Path);
        });

        var result = GetBlueprintChildren(containerKey2);
        Assert.Multiple(() =>
        {
            Assert.AreEqual(1, result.Length);
            Assert.AreEqual(blueprintKey, result.First().Key);
        });
    }
}
