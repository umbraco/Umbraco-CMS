using System.Globalization;
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
        await ContentTypeService.CreateAsync(contentType, Constants.Security.SuperUserKey);

        var result = await ContentEditingService.MoveAsync(child.Key, Constants.System.RootKey, Constants.Security.SuperUserKey);

        if (allowedAtRoot)
        {
            Assert.That(result.Success, Is.True);
            Assert.That(result.Status, Is.EqualTo(ContentEditingOperationStatus.Success));
            VerifyMove(result.Result);

            // re-get and re-test
            VerifyMove(await ContentEditingService.GetAsync(result.Result!.Key));

            void VerifyMove(IContent? movedContent)
            {
                Assert.That(movedContent, Is.Not.Null);
                Assert.That(movedContent.ParentId, Is.EqualTo(Constants.System.Root));
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
    public async Task Can_Move_To_Another_Parent(bool allowedAtParent)
    {
        var contentType = await CreateTextPageContentTypeAsync();
        (IContent root1, IContent child1) = await CreateRootAndChildAsync(contentType, "Root 1", "Child 1");
        (IContent root2, IContent child2) = await CreateRootAndChildAsync(contentType, "Root 2", "Child 2");

        if (allowedAtParent is false)
        {
            contentType.AllowedContentTypes = Enumerable.Empty<ContentTypeSort>();
        }

        await ContentTypeService.CreateAsync(contentType, Constants.Security.SuperUserKey);

        var result = await ContentEditingService.MoveAsync(child1.Key, root2.Key, Constants.Security.SuperUserKey);

        if (allowedAtParent)
        {
            Assert.That(result.Success, Is.True);
            Assert.That(result.Status, Is.EqualTo(ContentEditingOperationStatus.Success));
            VerifyMove(result.Result);

            // re-get and re-test
            VerifyMove(await ContentEditingService.GetAsync(result.Result!.Key));

            void VerifyMove(IContent? movedContent)
            {
                Assert.That(movedContent, Is.Not.Null);
                Assert.That(movedContent.ParentId, Is.EqualTo(root2.Id));
            }
        }
        else
        {
            Assert.That(result.Success, Is.False);
            Assert.That(result.Status, Is.EqualTo(ContentEditingOperationStatus.NotAllowed));
        }
    }

    [Test]
    public async Task Can_Move_Entire_Structure()
    {
        var contentType = await CreateTextPageContentTypeAsync();
        (IContent root1, IContent child1) = await CreateRootAndChildAsync(contentType, "Root 1", "Child 1");
        (IContent root2, IContent child2) = await CreateRootAndChildAsync(contentType, "Root 2", "Child 2");

        var result = await ContentEditingService.MoveAsync(root1.Key, root2.Key, Constants.Security.SuperUserKey);
        Assert.That(result.Success, Is.True);
        Assert.That(result.Status, Is.EqualTo(ContentEditingOperationStatus.Success));

        VerifyMove(result.Result);

        // re-get and re-test
        VerifyMove(await ContentEditingService.GetAsync(result.Result!.Key));

        child1 = await ContentEditingService.GetAsync(child1.Key);
        Assert.That(child1, Is.Not.Null);
        var ancestorIds = child1.GetAncestorIds()!.ToArray();
        Assert.That(ancestorIds, Has.Length.EqualTo(2));
        Assert.That(ancestorIds.First(), Is.EqualTo(root2.Id));
        Assert.That(ancestorIds.Last(), Is.EqualTo(root1.Id));

        void VerifyMove(IContent? movedContent)
        {
            Assert.That(movedContent, Is.Not.Null);
            Assert.That(movedContent.ParentId, Is.EqualTo(root2.Id));
        }
    }

    [Test]
    public async Task Can_Move_To_Existing_Parent()
    {
        var contentType = await CreateTextPageContentTypeAsync();
        (IContent root, IContent child) = await CreateRootAndChildAsync(contentType);

        var result = await ContentEditingService.MoveAsync(child.Key, root.Key, Constants.Security.SuperUserKey);
        Assert.That(result.Success, Is.True);
        Assert.That(result.Status, Is.EqualTo(ContentEditingOperationStatus.Success));

        VerifyMove(result.Result);

        // re-get and re-test
        VerifyMove(await ContentEditingService.GetAsync(result.Result!.Key));

        void VerifyMove(IContent? movedContent)
        {
            Assert.That(movedContent, Is.Not.Null);
            Assert.That(movedContent.ParentId, Is.EqualTo(root.Id));
        }
    }

    [Test]
    public async Task Can_Move_From_Root_To_Root()
    {
        var contentType = await CreateTextPageContentTypeAsync();
        (IContent root, IContent child) = await CreateRootAndChildAsync(contentType);

        var result = await ContentEditingService.MoveAsync(root.Key, Constants.System.RootKey, Constants.Security.SuperUserKey);
        Assert.That(result.Success, Is.True);
        Assert.That(result.Status, Is.EqualTo(ContentEditingOperationStatus.Success));

        VerifyMove(result.Result);

        // re-get and re-test
        VerifyMove(await ContentEditingService.GetAsync(result.Result!.Key));

        void VerifyMove(IContent? movedContent)
        {
            Assert.That(movedContent, Is.Not.Null);
            Assert.That(movedContent.ParentId, Is.EqualTo(Constants.System.Root));
        }
    }

    [Test]
    public async Task Cannot_Move_Non_Existing_Content()
    {
        var result = await ContentEditingService.MoveAsync(Guid.NewGuid(), Constants.System.RootKey, Constants.Security.SuperUserKey);
        Assert.That(result.Success, Is.False);
        Assert.That(result.Status, Is.EqualTo(ContentEditingOperationStatus.NotFound));
    }

    [Test]
    public async Task Cannot_Move_To_Non_Existing_Parent()
    {
        var contentType = await CreateTextPageContentTypeAsync();
        (IContent root, IContent child) = await CreateRootAndChildAsync(contentType);

        var result = await ContentEditingService.MoveAsync(child.Key, Guid.NewGuid(), Constants.Security.SuperUserKey);
        Assert.That(result.Success, Is.False);
        Assert.That(result.Status, Is.EqualTo(ContentEditingOperationStatus.ParentNotFound));
    }

    [Test]
    public async Task Cannot_Move_To_Trashed_Parent()
    {
        var contentType = await CreateTextPageContentTypeAsync();
        (IContent root1, IContent child1) = await CreateRootAndChildAsync(contentType);
        (IContent root2, IContent child2) = await CreateRootAndChildAsync(contentType);
        await ContentEditingService.MoveToRecycleBinAsync(root1.Key, Constants.Security.SuperUserKey);

        var result = await ContentEditingService.MoveAsync(root2.Key, root1.Key, Constants.Security.SuperUserKey);
        Assert.That(result.Success, Is.False);
        Assert.That(result.Status, Is.EqualTo(ContentEditingOperationStatus.InTrash));
    }

    [Test]
    public async Task Cannot_Move_Beneath_Self()
    {
        var contentType = await CreateTextPageContentTypeAsync();
        (IContent root, IContent child) = await CreateRootAndChildAsync(contentType);

        var result = await ContentEditingService.MoveAsync(root.Key, child.Key, Constants.Security.SuperUserKey);
        Assert.That(result.Success, Is.False);
        Assert.That(result.Status, Is.EqualTo(ContentEditingOperationStatus.ParentInvalid));
    }

    /// <summary>
    /// Regression test for <see href="https://github.com/umbraco/Umbraco-CMS/issues/22610"/>:
    /// restoring a content item from the recycle bin under Arabic culture failed because the
    /// parent path's "-1" root marker could not be parsed under ar-EG's
    /// <see cref="NumberFormatInfo.NegativeSign"/> (U+061C + hyphen).
    /// </summary>
    [Test]
    public async Task Can_Restore_From_Recycle_Bin_Under_Arabic_Culture()
    {
        var contentType = await CreateTextPageContentTypeAsync();
        (IContent root, IContent child) = await CreateRootAndChildAsync(contentType);

        var moveToRecycleBinResult = await ContentEditingService.MoveToRecycleBinAsync(child.Key, Constants.Security.SuperUserKey);
        Assert.That(moveToRecycleBinResult.Success, Is.True);

        var savedCulture = CultureInfo.CurrentCulture;
        try
        {
            CultureInfo.CurrentCulture = new CultureInfo("ar-EG");

            // Sanity-check that the runtime's culture data actually triggers the bug. If a future
            // ICU/NLS update reverts ar-EG's NegativeSign to a plain ASCII hyphen, this test would
            // silently pass without exercising anything; skip rather than give false confidence.
            Assume.That(
                CultureInfo.CurrentCulture.NumberFormat.NegativeSign,
                Is.Not.EqualTo("-"),
                "ar-EG NegativeSign is plain ASCII hyphen on this host; cannot reproduce #22610.");

            var result = await ContentEditingService.RestoreAsync(child.Key, root.Key, Constants.Security.SuperUserKey);

            Assert.That(result.Success, Is.True);
            Assert.That(result.Status, Is.EqualTo(ContentEditingOperationStatus.Success));
        }
        finally
        {
            CultureInfo.CurrentCulture = savedCulture;
        }
    }
}
