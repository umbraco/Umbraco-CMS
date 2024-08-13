using NUnit.Framework;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Services.OperationStatus;

namespace Umbraco.Cms.Tests.Integration.Umbraco.Infrastructure.Services;

public partial class ContentEditingServiceTests
{
    [TestCase(true)]
    [TestCase(false)]
    public async Task Can_Move_To_Root(bool allowedAtRoot)
    {
        var contentType = await CreateTextPageContentTypeAsync();
        (IContent root, IContent child) = await CreateRootAndChildAsync(contentType);

        contentType.AllowedAsRoot = allowedAtRoot;
        ContentTypeService.Save(contentType);

        var result = await ContentEditingService.MoveAsync(child.Key, Constants.System.RootKey, Constants.Security.SuperUserKey);

        if (allowedAtRoot)
        {
            Assert.IsTrue(result.Success);
            Assert.AreEqual(ContentEditingOperationStatus.Success, result.Status);
            VerifyMove(result.Result);

            // re-get and re-test
            VerifyMove(await ContentEditingService.GetAsync(result.Result!.Key));

            void VerifyMove(IContent? movedContent)
            {
                Assert.IsNotNull(movedContent);
                Assert.AreEqual(Constants.System.Root, movedContent.ParentId);
            }
        }
        else
        {
            Assert.IsFalse(result.Success);
            Assert.AreEqual(ContentEditingOperationStatus.NotAllowed, result.Status);
        }
    }

    [TestCase(true)]
    [TestCase(false)]
    public async Task Can_Move_To_Another_Parent(bool allowedAtParent)
    {
        var contentType = await CreateTextPageContentTypeAsync();
        (IContent root1, IContent child1) = await CreateRootAndChildAsync(contentType, "Root 1", "Child 1");
        (IContent root2, IContent child2) = await CreateRootAndChildAsync(contentType, "Root 2", "Child 2");

        if (allowedAtParent is false)
        {
            contentType.AllowedContentTypes = Enumerable.Empty<ContentTypeSort>();
        }

        ContentTypeService.Save(contentType);

        var result = await ContentEditingService.MoveAsync(child1.Key, root2.Key, Constants.Security.SuperUserKey);

        if (allowedAtParent)
        {
            Assert.IsTrue(result.Success);
            Assert.AreEqual(ContentEditingOperationStatus.Success, result.Status);
            VerifyMove(result.Result);

            // re-get and re-test
            VerifyMove(await ContentEditingService.GetAsync(result.Result!.Key));

            void VerifyMove(IContent? movedContent)
            {
                Assert.IsNotNull(movedContent);
                Assert.AreEqual(root2.Id, movedContent.ParentId);
            }
        }
        else
        {
            Assert.IsFalse(result.Success);
            Assert.AreEqual(ContentEditingOperationStatus.NotAllowed, result.Status);
        }
    }

    [Test]
    public async Task Can_Move_Entire_Structure()
    {
        var contentType = await CreateTextPageContentTypeAsync();
        (IContent root1, IContent child1) = await CreateRootAndChildAsync(contentType, "Root 1", "Child 1");
        (IContent root2, IContent child2) = await CreateRootAndChildAsync(contentType, "Root 2", "Child 2");

        var result = await ContentEditingService.MoveAsync(root1.Key, root2.Key, Constants.Security.SuperUserKey);
        Assert.IsTrue(result.Success);
        Assert.AreEqual(ContentEditingOperationStatus.Success, result.Status);

        VerifyMove(result.Result);

        // re-get and re-test
        VerifyMove(await ContentEditingService.GetAsync(result.Result!.Key));

        child1 = await ContentEditingService.GetAsync(child1.Key);
        Assert.IsNotNull(child1);
        var ancestorIds = child1.GetAncestorIds()!.ToArray();
        Assert.AreEqual(2, ancestorIds.Length);
        Assert.AreEqual(root2.Id, ancestorIds.First());
        Assert.AreEqual(root1.Id, ancestorIds.Last());

        void VerifyMove(IContent? movedContent)
        {
            Assert.IsNotNull(movedContent);
            Assert.AreEqual(root2.Id, movedContent.ParentId);
        }
    }

    [Test]
    public async Task Can_Move_To_Existing_Parent()
    {
        var contentType = await CreateTextPageContentTypeAsync();
        (IContent root, IContent child) = await CreateRootAndChildAsync(contentType);

        var result = await ContentEditingService.MoveAsync(child.Key, root.Key, Constants.Security.SuperUserKey);
        Assert.IsTrue(result.Success);
        Assert.AreEqual(ContentEditingOperationStatus.Success, result.Status);

        VerifyMove(result.Result);

        // re-get and re-test
        VerifyMove(await ContentEditingService.GetAsync(result.Result!.Key));

        void VerifyMove(IContent? movedContent)
        {
            Assert.IsNotNull(movedContent);
            Assert.AreEqual(root.Id, movedContent.ParentId);
        }
    }

    [Test]
    public async Task Can_Move_From_Root_To_Root()
    {
        var contentType = await CreateTextPageContentTypeAsync();
        (IContent root, IContent child) = await CreateRootAndChildAsync(contentType);

        var result = await ContentEditingService.MoveAsync(root.Key, Constants.System.RootKey, Constants.Security.SuperUserKey);
        Assert.IsTrue(result.Success);
        Assert.AreEqual(ContentEditingOperationStatus.Success, result.Status);

        VerifyMove(result.Result);

        // re-get and re-test
        VerifyMove(await ContentEditingService.GetAsync(result.Result!.Key));

        void VerifyMove(IContent? movedContent)
        {
            Assert.IsNotNull(movedContent);
            Assert.AreEqual(Constants.System.Root, movedContent.ParentId);
        }
    }

    [Test]
    public async Task Cannot_Move_Non_Existing_Content()
    {
        var result = await ContentEditingService.MoveAsync(Guid.NewGuid(), Constants.System.RootKey, Constants.Security.SuperUserKey);
        Assert.IsFalse(result.Success);
        Assert.AreEqual(ContentEditingOperationStatus.NotFound, result.Status);
    }

    [Test]
    public async Task Cannot_Move_To_Non_Existing_Parent()
    {
        var contentType = await CreateTextPageContentTypeAsync();
        (IContent root, IContent child) = await CreateRootAndChildAsync(contentType);

        var result = await ContentEditingService.MoveAsync(child.Key, Guid.NewGuid(), Constants.Security.SuperUserKey);
        Assert.IsFalse(result.Success);
        Assert.AreEqual(ContentEditingOperationStatus.ParentNotFound, result.Status);
    }

    [Test]
    public async Task Cannot_Move_To_Trashed_Parent()
    {
        var contentType = await CreateTextPageContentTypeAsync();
        (IContent root1, IContent child1) = await CreateRootAndChildAsync(contentType);
        (IContent root2, IContent child2) = await CreateRootAndChildAsync(contentType);
        await ContentEditingService.MoveToRecycleBinAsync(root1.Key, Constants.Security.SuperUserKey);

        var result = await ContentEditingService.MoveAsync(root2.Key, root1.Key, Constants.Security.SuperUserKey);
        Assert.IsFalse(result.Success);
        Assert.AreEqual(ContentEditingOperationStatus.InTrash, result.Status);
    }

    [Test]
    public async Task Cannot_Move_Beneath_Self()
    {
        var contentType = await CreateTextPageContentTypeAsync();
        (IContent root, IContent child) = await CreateRootAndChildAsync(contentType);

        var result = await ContentEditingService.MoveAsync(root.Key, child.Key, Constants.Security.SuperUserKey);
        Assert.IsFalse(result.Success);
        Assert.AreEqual(ContentEditingOperationStatus.ParentInvalid, result.Status);
    }
}
