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
        Assert.IsTrue(result.Success);
        Assert.AreEqual(ContentEditingOperationStatus.Success, result.Status);

        var restoredParent = await MediaEditingService.GetAsync(parent.Key);
        var leftBehindChild = await MediaEditingService.GetAsync(child.Key);
        var leftBehindGrandchild = await MediaEditingService.GetAsync(grandchild.Key);

        Assert.Multiple(() =>
        {
            // the restored item leaves the bin and is re-parented under the target
            Assert.IsFalse(restoredParent!.Trashed);
            Assert.AreEqual(topRoot.Id, restoredParent.ParentId);

            // the direct child stays trashed as a top-level recycle bin item
            Assert.IsTrue(leftBehindChild!.Trashed);
            Assert.AreEqual(Constants.System.RecycleBinMedia, leftBehindChild.ParentId);
            Assert.AreEqual($"{Constants.System.RecycleBinMediaPathPrefix}{child.Id}", leftBehindChild.Path);
            Assert.AreEqual(1, leftBehindChild.Level);

            // the grandchild stays trashed underneath its (now top-level) parent
            Assert.IsTrue(leftBehindGrandchild!.Trashed);
            Assert.AreEqual(child.Id, leftBehindGrandchild.ParentId);
            Assert.AreEqual($"{Constants.System.RecycleBinMediaPathPrefix}{child.Id},{grandchild.Id}", leftBehindGrandchild.Path);
            Assert.AreEqual(2, leftBehindGrandchild.Level);
        });
    }

    [Test]
    public async Task Can_Restore_With_Descendants_Restores_The_Whole_Subtree()
    {
        (IMedia topRoot, IMedia parent, IMedia child, IMedia grandchild) = await CreateFourLevelStructureAsync();

        await MediaEditingService.MoveToRecycleBinAsync(parent.Key, Constants.Security.SuperUserKey);

        var result = await MediaEditingService.RestoreAsync(parent.Key, topRoot.Key, Constants.Security.SuperUserKey, includeDescendants: true);
        Assert.IsTrue(result.Success);

        var restoredParent = await MediaEditingService.GetAsync(parent.Key);
        var restoredChild = await MediaEditingService.GetAsync(child.Key);
        var restoredGrandchild = await MediaEditingService.GetAsync(grandchild.Key);

        Assert.Multiple(() =>
        {
            Assert.IsFalse(restoredParent!.Trashed);
            Assert.AreEqual(topRoot.Id, restoredParent.ParentId);

            Assert.IsFalse(restoredChild!.Trashed);
            Assert.AreEqual(parent.Id, restoredChild.ParentId);

            Assert.IsFalse(restoredGrandchild!.Trashed);
            Assert.AreEqual(child.Id, restoredGrandchild.ParentId);
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
