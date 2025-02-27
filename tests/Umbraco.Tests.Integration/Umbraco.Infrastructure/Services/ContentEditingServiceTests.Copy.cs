using NUnit.Framework;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Services.OperationStatus;

namespace Umbraco.Cms.Tests.Integration.Umbraco.Infrastructure.Services;

public partial class ContentEditingServiceTests
{
    [TestCase(true)]
    [TestCase(false)]
    public async Task Can_Copy_To_Root(bool allowedAtRoot)
    {
        var contentType = await CreateTextPageContentTypeAsync();
        (IContent root, IContent child) = await CreateRootAndChildAsync(contentType);

        contentType.AllowedAsRoot = allowedAtRoot;
        ContentTypeService.Save(contentType);

        var result = await ContentEditingService.CopyAsync(child.Key, Constants.System.RootKey, false, false, Constants.Security.SuperUserKey);

        if (allowedAtRoot)
        {
            Assert.IsTrue(result.Success);
            Assert.AreEqual(ContentEditingOperationStatus.Success, result.Status);
            VerifyCopy(result.Result);

            // re-get and re-test
            VerifyCopy(await ContentEditingService.GetAsync(result.Result!.Key));

            void VerifyCopy(IContent? copiedContent)
            {
                Assert.IsNotNull(copiedContent);
                Assert.AreEqual(Constants.System.Root, copiedContent.ParentId);
                Assert.IsTrue(copiedContent.HasIdentity);
                Assert.AreNotEqual(child.Id, copiedContent.Id);
                Assert.AreNotEqual(child.Key, copiedContent.Key);
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
    public async Task Can_Copy_To_Another_Parent(bool allowedAtParent)
    {
        var contentType = await CreateTextPageContentTypeAsync();
        (IContent root1, IContent child1) = await CreateRootAndChildAsync(contentType, "Root 1", "Child 1");
        (IContent root2, IContent child2) = await CreateRootAndChildAsync(contentType, "Root 2", "Child 2");

        if (allowedAtParent is false)
        {
            contentType.AllowedContentTypes = Enumerable.Empty<ContentTypeSort>();
        }

        ContentTypeService.Save(contentType);

        var result = await ContentEditingService.CopyAsync(child1.Key, root2.Key, false, false, Constants.Security.SuperUserKey);

        if (allowedAtParent)
        {
            Assert.IsTrue(result.Success);
            Assert.AreEqual(ContentEditingOperationStatus.Success, result.Status);
            VerifyCopy(result.Result);

            // re-get and re-test
            VerifyCopy(await ContentEditingService.GetAsync(result.Result!.Key));

            void VerifyCopy(IContent? copiedContent)
            {
                Assert.IsNotNull(copiedContent);
                Assert.AreEqual(root2.Id, copiedContent.ParentId);
                Assert.IsTrue(copiedContent.HasIdentity);
                Assert.AreNotEqual(child1.Id, copiedContent.Id);
                Assert.AreNotEqual(child1.Key, copiedContent.Key);
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
    public async Task Can_Copy_Entire_Structure(bool includeDescendants)
    {
        var contentType = await CreateTextPageContentTypeAsync();
        (IContent root1, IContent child1) = await CreateRootAndChildAsync(contentType, "Root 1", "Child 1");
        (IContent root2, IContent child2) = await CreateRootAndChildAsync(contentType, "Root 2", "Child 2");

        var result = await ContentEditingService.CopyAsync(root1.Key, root2.Key, false, includeDescendants, Constants.Security.SuperUserKey);
        Assert.IsTrue(result.Success);
        Assert.AreEqual(ContentEditingOperationStatus.Success, result.Status);

        VerifyCopy(result.Result);

        // re-get and re-test
        VerifyCopy(await ContentEditingService.GetAsync(result.Result!.Key));

        void VerifyCopy(IContent? copiedRoot)
        {
            Assert.IsNotNull(copiedRoot);
            Assert.AreEqual(root2.Id, copiedRoot.ParentId);
            Assert.AreNotEqual(root1.Id, copiedRoot.Id);
            Assert.AreNotEqual(root1.Key, copiedRoot.Key);
            Assert.AreEqual(root1.Name, copiedRoot.Name);

            var copiedChildren = ContentService.GetPagedChildren(copiedRoot.Id, 0, 100, out var total).ToArray();

            if (includeDescendants)
            {
                Assert.AreEqual(1, copiedChildren.Length);
                Assert.AreEqual(1, total);
                var copiedChild = copiedChildren.First();
                Assert.AreNotEqual(child1.Id, copiedChild.Id);
                Assert.AreNotEqual(child1.Key, copiedChild.Key);
                Assert.AreEqual(child1.Name, copiedChild.Name);
            }
            else
            {
                Assert.AreEqual(0, copiedChildren.Length);
                Assert.AreEqual(0, total);
            }
        }
    }

    [Test]
    public async Task Can_Copy_To_Existing_Parent()
    {
        var contentType = await CreateTextPageContentTypeAsync();
        (IContent root, IContent child) = await CreateRootAndChildAsync(contentType);

        var result = await ContentEditingService.CopyAsync(child.Key, root.Key, false, false, Constants.Security.SuperUserKey);
        Assert.IsTrue(result.Success);
        Assert.AreEqual(ContentEditingOperationStatus.Success, result.Status);

        VerifyCopy(result.Result);

        // re-get and re-test
        VerifyCopy(await ContentEditingService.GetAsync(result.Result!.Key));

        void VerifyCopy(IContent? copiedContent)
        {
            Assert.IsNotNull(copiedContent);
            Assert.AreEqual(root.Id, copiedContent.ParentId);
            Assert.IsTrue(copiedContent.HasIdentity);
            Assert.AreNotEqual(child.Key, copiedContent.Key);
            Assert.AreNotEqual(child.Name, copiedContent.Name);
        }
    }

    [TestCase(true)]
    [TestCase(false)]
    public async Task Can_Copy_Beneath_Self(bool includeDescendants)
    {
        var contentType = await CreateTextPageContentTypeAsync();
        (IContent root, IContent child) = await CreateRootAndChildAsync(contentType);

        var result = await ContentEditingService.CopyAsync(root.Key, child.Key, false, includeDescendants, Constants.Security.SuperUserKey);
        Assert.IsTrue(result.Success);
        Assert.AreEqual(ContentEditingOperationStatus.Success, result.Status);

        VerifyCopy(result.Result);

        // re-get and re-test
        VerifyCopy(await ContentEditingService.GetAsync(result.Result!.Key));

        void VerifyCopy(IContent? copiedRoot)
        {
            Assert.IsNotNull(copiedRoot);
            Assert.AreEqual(child.Id, copiedRoot.ParentId);
            Assert.IsTrue(copiedRoot.HasIdentity);
            Assert.AreNotEqual(root.Key, copiedRoot.Key);
            Assert.AreEqual(root.Name, copiedRoot.Name);
            var copiedChildren = ContentService.GetPagedChildren(copiedRoot.Id, 0, 100, out var total).ToArray();

            if (includeDescendants)
            {
                Assert.AreEqual(1, copiedChildren.Length);
                Assert.AreEqual(1, total);
                var copiedChild = copiedChildren.First();
                Assert.AreNotEqual(child.Id, copiedChild.Id);
                Assert.AreNotEqual(child.Key, copiedChild.Key);
                Assert.AreEqual(child.Name, copiedChild.Name);
            }
            else
            {
                Assert.AreEqual(0, copiedChildren.Length);
                Assert.AreEqual(0, total);
            }
        }
    }

    [TestCase(true)]
    [TestCase(false)]
    public async Task Can_Copy_Onto_Self(bool includeDescendants)
    {
        var contentType = await CreateTextPageContentTypeAsync();
        (IContent root, IContent child) = await CreateRootAndChildAsync(contentType);

        var result = await ContentEditingService.CopyAsync(root.Key, root.Key, false, includeDescendants, Constants.Security.SuperUserKey);
        Assert.IsTrue(result.Success);
        Assert.AreEqual(ContentEditingOperationStatus.Success, result.Status);

        VerifyCopy(result.Result);

        // re-get and re-test
        VerifyCopy(await ContentEditingService.GetAsync(result.Result!.Key));

        void VerifyCopy(IContent? copiedRoot)
        {
            Assert.IsNotNull(copiedRoot);
            Assert.AreEqual(root.Id, copiedRoot.ParentId);
            Assert.IsTrue(copiedRoot.HasIdentity);
            Assert.AreNotEqual(root.Key, copiedRoot.Key);
            Assert.AreEqual(root.Name, copiedRoot.Name);
            var copiedChildren = ContentService.GetPagedChildren(copiedRoot.Id, 0, 100, out var total).ToArray();

            if (includeDescendants)
            {
                Assert.AreEqual(1, copiedChildren.Length);
                Assert.AreEqual(1, total);
                var copiedChild = copiedChildren.First();
                Assert.AreNotEqual(child.Id, copiedChild.Id);
                Assert.AreNotEqual(child.Key, copiedChild.Key);
                Assert.AreEqual(child.Name, copiedChild.Name);
            }
            else
            {
                Assert.AreEqual(0, copiedChildren.Length);
                Assert.AreEqual(0, total);
            }
        }
    }

    [Test]
    public async Task Can_Relate_Copy_To_Original()
    {
        var contentType = await CreateTextPageContentTypeAsync();
        (IContent root, IContent child) = await CreateRootAndChildAsync(contentType);

        var result = await ContentEditingService.CopyAsync(child.Key, root.Key, true, false, Constants.Security.SuperUserKey);
        Assert.IsTrue(result.Success);
        Assert.AreEqual(ContentEditingOperationStatus.Success, result.Status);

        var relationService = GetRequiredService<IRelationService>();
        var relations = relationService.GetByParentId(child.Id)!.ToArray();
        Assert.AreEqual(1, relations.Length);
        Assert.AreEqual(result.Result!.Id, relations.First().ChildId);
    }

    [Test]
    public async Task Cannot_Copy_Non_Existing_Content()
    {
        var result = await ContentEditingService.CopyAsync(Guid.NewGuid(), Constants.System.RootKey, false, false, Constants.Security.SuperUserKey);
        Assert.IsFalse(result.Success);
        Assert.AreEqual(ContentEditingOperationStatus.NotFound, result.Status);
    }

    [Test]
    public async Task Cannot_Copy_To_Non_Existing_Parent()
    {
        var contentType = await CreateTextPageContentTypeAsync();
        (IContent root, IContent child) = await CreateRootAndChildAsync(contentType);

        var result = await ContentEditingService.CopyAsync(child.Key, Guid.NewGuid(), false, false, Constants.Security.SuperUserKey);
        Assert.IsFalse(result.Success);
        Assert.AreEqual(ContentEditingOperationStatus.ParentNotFound, result.Status);
    }

    [Test]
    public async Task Cannot_Copy_To_Trashed_Parent()
    {
        var contentType = await CreateTextPageContentTypeAsync();
        (IContent root1, IContent child1) = await CreateRootAndChildAsync(contentType);
        (IContent root2, IContent child2) = await CreateRootAndChildAsync(contentType);
        await ContentEditingService.MoveToRecycleBinAsync(root1.Key, Constants.Security.SuperUserKey);

        var result = await ContentEditingService.CopyAsync(root2.Key, root1.Key, false, false, Constants.Security.SuperUserKey);
        Assert.IsFalse(result.Success);
        Assert.AreEqual(ContentEditingOperationStatus.InTrash, result.Status);
    }
}
