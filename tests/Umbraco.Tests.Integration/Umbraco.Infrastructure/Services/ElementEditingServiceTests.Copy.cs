using NUnit.Framework;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Services.OperationStatus;

namespace Umbraco.Cms.Tests.Integration.Umbraco.Infrastructure.Services;

public partial class ElementEditingServiceTests
{
    [Test]
    public async Task Can_Copy_To_Root()
    {
        var container = (await ElementContainerService.CreateAsync(null, "Root Container", null, Constants.Security.SuperUserKey)).Result;
        var original = await CreateInvariantElement(container.Key);

        Assert.AreEqual(1, GetFolderChildren(container.Key).Length);

        var copyResult = await ElementEditingService.CopyAsync(original.Key, null, Constants.Security.SuperUserKey);
        Assert.Multiple(() =>
        {
            Assert.IsTrue(copyResult.Success);
            Assert.AreEqual(ContentEditingOperationStatus.Success, copyResult.Status);
        });

        var copy = copyResult.Result;
        Assert.IsNotNull(copy);
        Assert.Multiple(() =>
        {
            Assert.IsTrue(copy.HasIdentity);
            Assert.AreEqual(Constants.System.Root, copy.ParentId);
            Assert.AreNotEqual(original.Key, copy.Key);
            Assert.AreEqual(original.Name, copy.Name);
        });
    }

    [Test]
    public async Task Can_Copy_To_Another_Parent()
    {
        var container1 = (await ElementContainerService.CreateAsync(null, "Root Container 1", null, Constants.Security.SuperUserKey)).Result;
        var container2 = (await ElementContainerService.CreateAsync(null, "Root Container 2", null, Constants.Security.SuperUserKey)).Result;
        var original = await CreateInvariantElement(container1.Key);

        Assert.AreEqual(1, GetFolderChildren(container1.Key).Length);
        Assert.AreEqual(0, GetFolderChildren(container2.Key).Length);

        var copyResult = await ElementEditingService.CopyAsync(original.Key, container2.Key, Constants.Security.SuperUserKey);
        Assert.Multiple(() =>
        {
            Assert.IsTrue(copyResult.Success);
            Assert.AreEqual(ContentEditingOperationStatus.Success, copyResult.Status);
        });

        Assert.AreEqual(1, GetFolderChildren(container2.Key).Length);

        var copy = copyResult.Result;
        Assert.IsNotNull(copy);
        Assert.Multiple(() =>
        {
            Assert.IsTrue(copy.HasIdentity);
            Assert.AreEqual(container2.Id, copy.ParentId);
            Assert.AreNotEqual(original.Key, copy.Key);
            Assert.AreEqual(original.Name, copy.Name);
        });
    }

    [Test]
    public async Task Can_Copy_To_Existing_Parent()
    {
        var container = (await ElementContainerService.CreateAsync(null, "Root Container", null, Constants.Security.SuperUserKey)).Result;
        var original = await CreateInvariantElement(container.Key);

        Assert.AreEqual(1, GetFolderChildren(container.Key).Length);

        var copyResult = await ElementEditingService.CopyAsync(original.Key, container.Key, Constants.Security.SuperUserKey);
        Assert.Multiple(() =>
        {
            Assert.IsTrue(copyResult.Success);
            Assert.AreEqual(ContentEditingOperationStatus.Success, copyResult.Status);
        });

        Assert.AreEqual(2, GetFolderChildren(container.Key).Length);

        var copy = copyResult.Result;
        Assert.IsNotNull(copy);
        Assert.Multiple(() =>
        {
            Assert.IsTrue(copy.HasIdentity);
            Assert.AreEqual(container.Id, copy.ParentId);
            Assert.AreNotEqual(original.Key, copy.Key);
            Assert.AreEqual($"{original.Name} (1)", copy.Name);
        });
    }

    [Test]
    public async Task Cannot_Copy_To_Non_Existing_Parent()
    {
        var original = await CreateInvariantElement();

        var copyResult = await ElementEditingService.CopyAsync(original.Key, Guid.NewGuid(), Constants.Security.SuperUserKey);
        Assert.Multiple(() =>
        {
            Assert.IsFalse(copyResult.Success);
            Assert.AreEqual(ContentEditingOperationStatus.ParentNotFound, copyResult.Status);
        });
    }
}
