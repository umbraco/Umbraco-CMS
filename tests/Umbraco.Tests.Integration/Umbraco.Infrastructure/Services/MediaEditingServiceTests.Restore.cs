using NUnit.Framework;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Services.OperationStatus;

namespace Umbraco.Cms.Tests.Integration.Umbraco.Infrastructure.Services;

internal sealed partial class MediaEditingServiceTests
{
    [Test]
    public async Task Can_Restore_Without_Descendants_Leaving_Them_In_The_Recycle_Bin()
    {
        (IMedia topRoot, IMedia parent, IMedia child, IMedia grandchild) = await CreateFourLevelStructureAsync();

        await MediaEditingService.MoveToRecycleBinAsync(parent.Key, Constants.Security.SuperUserKey);

        var result = await MediaEditingService.RestoreAsync(parent.Key, topRoot.Key, Constants.Security.SuperUserKey, includeDescendants: false);
        Assert.That(result.Success, Is.True);
        Assert.That(result.Status, Is.EqualTo(ContentEditingOperationStatus.Success));

        var restoredParent = await MediaEditingService.GetAsync(parent.Key);
        var leftBehindChild = await MediaEditingService.GetAsync(child.Key);
        var leftBehindGrandchild = await MediaEditingService.GetAsync(grandchild.Key);

        Assert.Multiple(() =>
        {
            // the restored item leaves the bin and is re-parented under the target
            Assert.That(restoredParent!.Trashed, Is.False);
            Assert.That(restoredParent.ParentId, Is.EqualTo(topRoot.Id));

            // the direct child stays trashed as a top-level recycle bin item
            Assert.That(leftBehindChild!.Trashed, Is.True);
            Assert.That(leftBehindChild.ParentId, Is.EqualTo(Constants.System.RecycleBinMedia));
            Assert.That(leftBehindChild.Path, Is.EqualTo($"{Constants.System.RecycleBinMediaPathPrefix}{child.Id}"));
            Assert.That(leftBehindChild.Level, Is.EqualTo(1));

            // the grandchild stays trashed underneath its (now top-level) parent
            Assert.That(leftBehindGrandchild!.Trashed, Is.True);
            Assert.That(leftBehindGrandchild.ParentId, Is.EqualTo(child.Id));
            Assert.That(leftBehindGrandchild.Path, Is.EqualTo($"{Constants.System.RecycleBinMediaPathPrefix}{child.Id},{grandchild.Id}"));
            Assert.That(leftBehindGrandchild.Level, Is.EqualTo(2));
        });
    }

    [Test]
    public async Task Can_Restore_With_Descendants_Restores_The_Whole_Subtree()
    {
        (IMedia topRoot, IMedia parent, IMedia child, IMedia grandchild) = await CreateFourLevelStructureAsync();

        await MediaEditingService.MoveToRecycleBinAsync(parent.Key, Constants.Security.SuperUserKey);

        var result = await MediaEditingService.RestoreAsync(parent.Key, topRoot.Key, Constants.Security.SuperUserKey, includeDescendants: true);
        Assert.That(result.Success, Is.True);

        var restoredParent = await MediaEditingService.GetAsync(parent.Key);
        var restoredChild = await MediaEditingService.GetAsync(child.Key);
        var restoredGrandchild = await MediaEditingService.GetAsync(grandchild.Key);

        Assert.Multiple(() =>
        {
            Assert.That(restoredParent!.Trashed, Is.False);
            Assert.That(restoredParent.ParentId, Is.EqualTo(topRoot.Id));

            Assert.That(restoredChild!.Trashed, Is.False);
            Assert.That(restoredChild.ParentId, Is.EqualTo(parent.Id));

            Assert.That(restoredGrandchild!.Trashed, Is.False);
            Assert.That(restoredGrandchild.ParentId, Is.EqualTo(child.Id));
        });
    }

    private async Task<(IMedia TopRoot, IMedia Parent, IMedia Child, IMedia Grandchild)> CreateFourLevelStructureAsync()
    {
        var topRoot = await CreateFolderMediaAsync("Top Root", Constants.Security.SuperUserKey, parentKey: null);
        var parent = await CreateFolderMediaAsync("Parent", Constants.Security.SuperUserKey, topRoot.Key);
        var child = await CreateFolderMediaAsync("Child", Constants.Security.SuperUserKey, parent.Key);
        var grandchild = await CreateFolderMediaAsync("Grandchild", Constants.Security.SuperUserKey, child.Key);

        return (topRoot, parent, child, grandchild);
    }
}
