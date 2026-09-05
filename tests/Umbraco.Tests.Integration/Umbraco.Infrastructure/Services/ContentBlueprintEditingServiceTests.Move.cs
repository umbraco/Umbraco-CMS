using NUnit.Framework;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Models;

namespace Umbraco.Cms.Tests.Integration.Umbraco.Infrastructure.Services;

public partial class ContentBlueprintEditingServiceTests
{
    [Test]
    public async Task Can_Move_Blueprint_From_Root_To_A_Folder()
    {
        var containerKey = Guid.NewGuid();
        var container = (await ContentBlueprintContainerService.CreateAsync(containerKey, "Root Container", null, Constants.Security.SuperUserKey)).Result;
        Assert.That(GetBlueprintChildren(containerKey), Is.Empty);

        var blueprintKey = Guid.NewGuid();
        await ContentBlueprintEditingService.CreateAsync(SimpleContentBlueprintCreateModel(blueprintKey, null), Constants.Security.SuperUserKey);

        await ContentBlueprintEditingService.MoveAsync(blueprintKey, containerKey, Constants.Security.SuperUserKey);

        var blueprint = await ContentBlueprintEditingService.GetAsync(blueprintKey);
        Assert.That(blueprint, Is.Not.Null);
        Assert.Multiple(() =>
        {
            Assert.That(blueprint.ParentId, Is.EqualTo(container.Id));
            Assert.That(blueprint.Path, Is.EqualTo($"{container.Path},{blueprint.Id}"));
        });

        var result = GetBlueprintChildren(containerKey);
        Assert.Multiple(() =>
        {
            Assert.That(result, Has.Length.EqualTo(1));
            Assert.That(result.First().Key, Is.EqualTo(blueprintKey));
        });

        var auditLog = (await AuditService.GetItemsByEntityAsync(blueprint!.Id, 0, 1)).Items.First();
        Assert.That(auditLog.AuditType, Is.EqualTo(AuditType.Move));
    }

    [Test]
    public async Task Can_Move_Blueprint_From_A_Folder_To_Root()
    {
        var containerKey = Guid.NewGuid();
        await ContentBlueprintContainerService.CreateAsync(containerKey, "Root Container", null, Constants.Security.SuperUserKey);

        var blueprintKey = Guid.NewGuid();
        await ContentBlueprintEditingService.CreateAsync(SimpleContentBlueprintCreateModel(blueprintKey, containerKey), Constants.Security.SuperUserKey);
        Assert.That(GetBlueprintChildren(containerKey), Has.Length.EqualTo(1));

        await ContentBlueprintEditingService.MoveAsync(blueprintKey, null, Constants.Security.SuperUserKey);
        Assert.That(GetBlueprintChildren(containerKey), Is.Empty);

        var blueprint = await ContentBlueprintEditingService.GetAsync(blueprintKey);
        Assert.That(blueprint, Is.Not.Null);
        Assert.Multiple(() =>
        {
            Assert.That(blueprint.ParentId, Is.EqualTo(Constants.System.Root));
            Assert.That(blueprint.Path, Is.EqualTo($"{Constants.System.Root},{blueprint.Id}"));
        });

        var result = GetBlueprintChildren(null);
        Assert.Multiple(() =>
        {
            Assert.That(result, Has.Length.EqualTo(1));
            Assert.That(result.First().Key, Is.EqualTo(blueprintKey));
        });

        var auditLog = (await AuditService.GetItemsByEntityAsync(blueprint!.Id, 0, 1)).Items.First();
        Assert.That(auditLog.AuditType, Is.EqualTo(AuditType.Move));
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
        Assert.That(blueprint, Is.Not.Null);
        Assert.Multiple(() =>
        {
            Assert.That(blueprint.ParentId, Is.EqualTo(container2.Id));
            Assert.That(blueprint.Path, Is.EqualTo($"{container2.Path},{blueprint.Id}"));
        });

        var result = GetBlueprintChildren(containerKey2);
        Assert.Multiple(() =>
        {
            Assert.That(result, Has.Length.EqualTo(1));
            Assert.That(result.First().Key, Is.EqualTo(blueprintKey));
        });

        var auditLog = (await AuditService.GetItemsByEntityAsync(blueprint!.Id, 0, 1)).Items.First();
        Assert.That(auditLog.AuditType, Is.EqualTo(AuditType.Move));
    }
}
