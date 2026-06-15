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
        await ContentTypeService.CreateAsync(contentType, Constants.Security.SuperUserKey);

        var result = await ContentEditingService.CopyAsync(child.Key, Constants.System.RootKey, false, false, Constants.Security.SuperUserKey);

        if (allowedAtRoot)
        {
            Assert.That(result.Success, Is.True);
            Assert.That(result.Status, Is.EqualTo(ContentEditingOperationStatus.Success));
            VerifyCopy(result.Result);

            // re-get and re-test
            VerifyCopy(await ContentEditingService.GetAsync(result.Result!.Key));

            void VerifyCopy(IContent? copiedContent)
            {
                Assert.That(copiedContent, Is.Not.Null);
                Assert.That(copiedContent.ParentId, Is.EqualTo(Constants.System.Root));
                Assert.That(copiedContent.HasIdentity, Is.True);
                Assert.That(copiedContent.Id, Is.Not.EqualTo(child.Id));
                Assert.That(copiedContent.Key, Is.Not.EqualTo(child.Key));
            }
        }
        else
        {
            Assert.That(result.Success, Is.False);
            Assert.That(result.Status, Is.EqualTo(ContentEditingOperationStatus.NotAllowed));
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

        await ContentTypeService.CreateAsync(contentType, Constants.Security.SuperUserKey);

        var result = await ContentEditingService.CopyAsync(child1.Key, root2.Key, false, false, Constants.Security.SuperUserKey);

        if (allowedAtParent)
        {
            Assert.That(result.Success, Is.True);
            Assert.That(result.Status, Is.EqualTo(ContentEditingOperationStatus.Success));
            VerifyCopy(result.Result);

            // re-get and re-test
            VerifyCopy(await ContentEditingService.GetAsync(result.Result!.Key));

            void VerifyCopy(IContent? copiedContent)
            {
                Assert.That(copiedContent, Is.Not.Null);
                Assert.That(copiedContent.ParentId, Is.EqualTo(root2.Id));
                Assert.That(copiedContent.HasIdentity, Is.True);
                Assert.That(copiedContent.Id, Is.Not.EqualTo(child1.Id));
                Assert.That(copiedContent.Key, Is.Not.EqualTo(child1.Key));
            }
        }
        else
        {
            Assert.That(result.Success, Is.False);
            Assert.That(result.Status, Is.EqualTo(ContentEditingOperationStatus.NotAllowed));
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
        Assert.That(result.Success, Is.True);
        Assert.That(result.Status, Is.EqualTo(ContentEditingOperationStatus.Success));

        VerifyCopy(result.Result);

        // re-get and re-test
        VerifyCopy(await ContentEditingService.GetAsync(result.Result!.Key));

        void VerifyCopy(IContent? copiedRoot)
        {
            Assert.That(copiedRoot, Is.Not.Null);
            Assert.That(copiedRoot.ParentId, Is.EqualTo(root2.Id));
            Assert.That(copiedRoot.Id, Is.Not.EqualTo(root1.Id));
            Assert.That(copiedRoot.Key, Is.Not.EqualTo(root1.Key));
            Assert.That(copiedRoot.Name, Is.EqualTo(root1.Name));

            var copiedChildren = ContentService.GetPagedChildren(copiedRoot.Id, 0, 100, out var total, propertyAliases: null, filter: null, ordering: null).ToArray();

            if (includeDescendants)
            {
                Assert.That(copiedChildren, Has.Length.EqualTo(1));
                Assert.That(total, Is.EqualTo(1));
                var copiedChild = copiedChildren.First();
                Assert.That(copiedChild.Id, Is.Not.EqualTo(child1.Id));
                Assert.That(copiedChild.Key, Is.Not.EqualTo(child1.Key));
                Assert.That(copiedChild.Name, Is.EqualTo(child1.Name));
            }
            else
            {
                Assert.That(copiedChildren, Is.Empty);
                Assert.That(total, Is.EqualTo(0));
            }
        }
    }

    [Test]
    public async Task Can_Copy_To_Existing_Parent()
    {
        var contentType = await CreateTextPageContentTypeAsync();
        (IContent root, IContent child) = await CreateRootAndChildAsync(contentType);

        var result = await ContentEditingService.CopyAsync(child.Key, root.Key, false, false, Constants.Security.SuperUserKey);
        Assert.That(result.Success, Is.True);
        Assert.That(result.Status, Is.EqualTo(ContentEditingOperationStatus.Success));

        VerifyCopy(result.Result);

        // re-get and re-test
        VerifyCopy(await ContentEditingService.GetAsync(result.Result!.Key));

        void VerifyCopy(IContent? copiedContent)
        {
            Assert.That(copiedContent, Is.Not.Null);
            Assert.That(copiedContent.ParentId, Is.EqualTo(root.Id));
            Assert.That(copiedContent.HasIdentity, Is.True);
            Assert.That(copiedContent.Key, Is.Not.EqualTo(child.Key));
            Assert.That(copiedContent.Name, Is.Not.EqualTo(child.Name));
        }
    }

    [TestCase(true)]
    [TestCase(false)]
    public async Task Can_Copy_Beneath_Self(bool includeDescendants)
    {
        var contentType = await CreateTextPageContentTypeAsync();
        (IContent root, IContent child) = await CreateRootAndChildAsync(contentType);

        var result = await ContentEditingService.CopyAsync(root.Key, child.Key, false, includeDescendants, Constants.Security.SuperUserKey);
        Assert.That(result.Success, Is.True);
        Assert.That(result.Status, Is.EqualTo(ContentEditingOperationStatus.Success));

        VerifyCopy(result.Result);

        // re-get and re-test
        VerifyCopy(await ContentEditingService.GetAsync(result.Result!.Key));

        void VerifyCopy(IContent? copiedRoot)
        {
            Assert.That(copiedRoot, Is.Not.Null);
            Assert.That(copiedRoot.ParentId, Is.EqualTo(child.Id));
            Assert.That(copiedRoot.HasIdentity, Is.True);
            Assert.That(copiedRoot.Key, Is.Not.EqualTo(root.Key));
            Assert.That(copiedRoot.Name, Is.EqualTo(root.Name));
            var copiedChildren = ContentService.GetPagedChildren(copiedRoot.Id, 0, 100, out var total, propertyAliases: null, filter: null, ordering: null).ToArray();

            if (includeDescendants)
            {
                Assert.That(copiedChildren, Has.Length.EqualTo(1));
                Assert.That(total, Is.EqualTo(1));
                var copiedChild = copiedChildren.First();
                Assert.That(copiedChild.Id, Is.Not.EqualTo(child.Id));
                Assert.That(copiedChild.Key, Is.Not.EqualTo(child.Key));
                Assert.That(copiedChild.Name, Is.EqualTo(child.Name));
            }
            else
            {
                Assert.That(copiedChildren, Is.Empty);
                Assert.That(total, Is.EqualTo(0));
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
        Assert.That(result.Success, Is.True);
        Assert.That(result.Status, Is.EqualTo(ContentEditingOperationStatus.Success));

        VerifyCopy(result.Result);

        // re-get and re-test
        VerifyCopy(await ContentEditingService.GetAsync(result.Result!.Key));

        void VerifyCopy(IContent? copiedRoot)
        {
            Assert.That(copiedRoot, Is.Not.Null);
            Assert.That(copiedRoot.ParentId, Is.EqualTo(root.Id));
            Assert.That(copiedRoot.HasIdentity, Is.True);
            Assert.That(copiedRoot.Key, Is.Not.EqualTo(root.Key));
            Assert.That(copiedRoot.Name, Is.EqualTo(root.Name));
            var copiedChildren = ContentService.GetPagedChildren(copiedRoot.Id, 0, 100, out var total, propertyAliases: null, filter: null, ordering: null).ToArray();

            if (includeDescendants)
            {
                Assert.That(copiedChildren, Has.Length.EqualTo(1));
                Assert.That(total, Is.EqualTo(1));
                var copiedChild = copiedChildren.First();
                Assert.That(copiedChild.Id, Is.Not.EqualTo(child.Id));
                Assert.That(copiedChild.Key, Is.Not.EqualTo(child.Key));
                Assert.That(copiedChild.Name, Is.EqualTo(child.Name));
            }
            else
            {
                Assert.That(copiedChildren, Is.Empty);
                Assert.That(total, Is.EqualTo(0));
            }
        }
    }

    [Test]
    public async Task Can_Relate_Copy_To_Original()
    {
        var contentType = await CreateTextPageContentTypeAsync();
        (IContent root, IContent child) = await CreateRootAndChildAsync(contentType);

        var result = await ContentEditingService.CopyAsync(child.Key, root.Key, true, false, Constants.Security.SuperUserKey);
        Assert.That(result.Success, Is.True);
        Assert.That(result.Status, Is.EqualTo(ContentEditingOperationStatus.Success));

        var relationService = GetRequiredService<IRelationService>();
        var relations = relationService.GetByParentId(child.Id)!.ToArray();
        Assert.That(relations, Has.Length.EqualTo(1));
        Assert.That(relations.First().ChildId, Is.EqualTo(result.Result!.Id));
    }

    [Test]
    public async Task Cannot_Copy_Non_Existing_Content()
    {
        var result = await ContentEditingService.CopyAsync(Guid.NewGuid(), Constants.System.RootKey, false, false, Constants.Security.SuperUserKey);
        Assert.That(result.Success, Is.False);
        Assert.That(result.Status, Is.EqualTo(ContentEditingOperationStatus.NotFound));
    }

    [Test]
    public async Task Cannot_Copy_To_Non_Existing_Parent()
    {
        var contentType = await CreateTextPageContentTypeAsync();
        (IContent root, IContent child) = await CreateRootAndChildAsync(contentType);

        var result = await ContentEditingService.CopyAsync(child.Key, Guid.NewGuid(), false, false, Constants.Security.SuperUserKey);
        Assert.That(result.Success, Is.False);
        Assert.That(result.Status, Is.EqualTo(ContentEditingOperationStatus.ParentNotFound));
    }

    [Test]
    public async Task Cannot_Copy_To_Trashed_Parent()
    {
        var contentType = await CreateTextPageContentTypeAsync();
        (IContent root1, IContent child1) = await CreateRootAndChildAsync(contentType);
        (IContent root2, IContent child2) = await CreateRootAndChildAsync(contentType);
        await ContentEditingService.MoveToRecycleBinAsync(root1.Key, Constants.Security.SuperUserKey);

        var result = await ContentEditingService.CopyAsync(root2.Key, root1.Key, false, false, Constants.Security.SuperUserKey);
        Assert.That(result.Success, Is.False);
        Assert.That(result.Status, Is.EqualTo(ContentEditingOperationStatus.InTrash));
    }
}
