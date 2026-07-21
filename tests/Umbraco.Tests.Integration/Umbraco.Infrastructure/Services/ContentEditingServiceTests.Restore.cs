using NUnit.Framework;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.ContentEditing;
using Umbraco.Cms.Core.Services.OperationStatus;

namespace Umbraco.Cms.Tests.Integration.Umbraco.Infrastructure.Services;

public partial class ContentEditingServiceTests
{
    [Test]
    public async Task Can_Restore_Without_Descendants_Leaving_Them_In_The_Recycle_Bin()
    {
        var contentType = await CreateTextPageContentTypeAsync();
        (IContent topRoot, IContent parent, IContent child, IContent grandchild) = await CreateFourLevelStructureAsync(contentType);

        await ContentEditingService.MoveToRecycleBinAsync(parent.Key, Constants.Security.SuperUserKey);

        var result = await ContentEditingService.RestoreAsync(parent.Key, topRoot.Key, Constants.Security.SuperUserKey, includeDescendants: false);
        Assert.IsTrue(result.Success);
        Assert.AreEqual(ContentEditingOperationStatus.Success, result.Status);

        var restoredParent = await ContentEditingService.GetAsync(parent.Key);
        var leftBehindChild = await ContentEditingService.GetAsync(child.Key);
        var leftBehindGrandchild = await ContentEditingService.GetAsync(grandchild.Key);

        Assert.Multiple(() =>
        {
            // the restored item leaves the bin and is re-parented under the target
            Assert.IsFalse(restoredParent!.Trashed);
            Assert.AreEqual(topRoot.Id, restoredParent.ParentId);

            // the direct child stays trashed as a top-level recycle bin item
            Assert.IsTrue(leftBehindChild!.Trashed);
            Assert.AreEqual(Constants.System.RecycleBinContent, leftBehindChild.ParentId);
            Assert.AreEqual($"{Constants.System.RecycleBinContentPathPrefix}{child.Id}", leftBehindChild.Path);
            Assert.AreEqual(1, leftBehindChild.Level);

            // the grandchild stays trashed underneath its (now top-level) parent
            Assert.IsTrue(leftBehindGrandchild!.Trashed);
            Assert.AreEqual(child.Id, leftBehindGrandchild.ParentId);
            Assert.AreEqual($"{Constants.System.RecycleBinContentPathPrefix}{child.Id},{grandchild.Id}", leftBehindGrandchild.Path);
            Assert.AreEqual(2, leftBehindGrandchild.Level);
        });
    }

    [Test]
    public async Task Can_Restore_With_Descendants_Restores_The_Whole_Subtree()
    {
        var contentType = await CreateTextPageContentTypeAsync();
        (IContent topRoot, IContent parent, IContent child, IContent grandchild) = await CreateFourLevelStructureAsync(contentType);

        await ContentEditingService.MoveToRecycleBinAsync(parent.Key, Constants.Security.SuperUserKey);

        var result = await ContentEditingService.RestoreAsync(parent.Key, topRoot.Key, Constants.Security.SuperUserKey, includeDescendants: true);
        Assert.IsTrue(result.Success);

        var restoredParent = await ContentEditingService.GetAsync(parent.Key);
        var restoredChild = await ContentEditingService.GetAsync(child.Key);
        var restoredGrandchild = await ContentEditingService.GetAsync(grandchild.Key);

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

    private async Task<(IContent topRoot, IContent parent, IContent child, IContent grandchild)> CreateFourLevelStructureAsync(IContentType contentType)
    {
        contentType.AllowedContentTypes = new List<ContentTypeSort> { new(contentType.Key, 1, contentType.Alias) };
        await ContentTypeService.UpdateAsync(contentType, Constants.Security.SuperUserKey);

        async Task<IContent> Create(string name, Guid? parentKey)
        {
            var createModel = new ContentCreateModel
            {
                ContentTypeKey = contentType.Key,
                ParentKey = parentKey,
                Variants = [new() { Name = name }],
            };

            return (await ContentEditingService.CreateAsync(createModel, Constants.Security.SuperUserKey)).Result.Content!;
        }

        var topRoot = await Create("Top Root", Constants.System.RootKey);
        var parent = await Create("Parent", topRoot.Key);
        var child = await Create("Child", parent.Key);
        var grandchild = await Create("Grandchild", child.Key);

        return (topRoot, parent, child, grandchild);
    }
}
