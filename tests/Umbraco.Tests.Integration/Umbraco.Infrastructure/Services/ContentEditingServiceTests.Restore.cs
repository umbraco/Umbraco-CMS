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
        Assert.That(result.Success, Is.True);
        Assert.That(result.Status, Is.EqualTo(ContentEditingOperationStatus.Success));

        var restoredParent = await ContentEditingService.GetAsync(parent.Key);
        var leftBehindChild = await ContentEditingService.GetAsync(child.Key);
        var leftBehindGrandchild = await ContentEditingService.GetAsync(grandchild.Key);

        Assert.Multiple(() =>
        {
            // the restored item leaves the bin and is re-parented under the target
            Assert.That(restoredParent!.Trashed, Is.False);
            Assert.That(restoredParent.ParentId, Is.EqualTo(topRoot.Id));

            // the direct child stays trashed as a top-level recycle bin item
            Assert.That(leftBehindChild!.Trashed, Is.True);
            Assert.That(leftBehindChild.ParentId, Is.EqualTo(Constants.System.RecycleBinContent));
            Assert.That(leftBehindChild.Path, Is.EqualTo($"{Constants.System.RecycleBinContentPathPrefix}{child.Id}"));
            Assert.That(leftBehindChild.Level, Is.EqualTo(1));

            // the grandchild stays trashed underneath its (now top-level) parent
            Assert.That(leftBehindGrandchild!.Trashed, Is.True);
            Assert.That(leftBehindGrandchild.ParentId, Is.EqualTo(child.Id));
            Assert.That(leftBehindGrandchild.Path, Is.EqualTo($"{Constants.System.RecycleBinContentPathPrefix}{child.Id},{grandchild.Id}"));
            Assert.That(leftBehindGrandchild.Level, Is.EqualTo(2));
        });
    }

    [Test]
    public async Task Can_Restore_With_Descendants_Restores_The_Whole_Subtree()
    {
        var contentType = await CreateTextPageContentTypeAsync();
        (IContent topRoot, IContent parent, IContent child, IContent grandchild) = await CreateFourLevelStructureAsync(contentType);

        await ContentEditingService.MoveToRecycleBinAsync(parent.Key, Constants.Security.SuperUserKey);

        var result = await ContentEditingService.RestoreAsync(parent.Key, topRoot.Key, Constants.Security.SuperUserKey, includeDescendants: true);
        Assert.That(result.Success, Is.True);

        var restoredParent = await ContentEditingService.GetAsync(parent.Key);
        var restoredChild = await ContentEditingService.GetAsync(child.Key);
        var restoredGrandchild = await ContentEditingService.GetAsync(grandchild.Key);

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

    private async Task<(IContent TopRoot, IContent Parent, IContent Child, IContent Grandchild)> CreateFourLevelStructureAsync(IContentType contentType)
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
