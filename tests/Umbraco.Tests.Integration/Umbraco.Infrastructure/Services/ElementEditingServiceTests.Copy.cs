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

        Assert.That(GetFolderChildren(container.Key), Has.Length.EqualTo(1));

        var copyResult = await ElementEditingService.CopyAsync(original.Key, null, Constants.Security.SuperUserKey);
        Assert.Multiple(() =>
        {
            Assert.That(copyResult.Success, Is.True);
            Assert.That(copyResult.Status, Is.EqualTo(ContentEditingOperationStatus.Success));
        });

        var copy = copyResult.Result;
        Assert.That(copy, Is.Not.Null);
        Assert.Multiple(() =>
        {
            Assert.That(copy.HasIdentity, Is.True);
            Assert.That(copy.ParentId, Is.EqualTo(Constants.System.Root));
            Assert.That(copy.Key, Is.Not.EqualTo(original.Key));
            Assert.That(copy.Name, Is.EqualTo(original.Name));
        });
    }

    [Test]
    public async Task Can_Copy_To_Another_Parent()
    {
        var container1 = (await ElementContainerService.CreateAsync(null, "Root Container 1", null, Constants.Security.SuperUserKey)).Result;
        var container2 = (await ElementContainerService.CreateAsync(null, "Root Container 2", null, Constants.Security.SuperUserKey)).Result;
        var original = await CreateInvariantElement(container1.Key);

        Assert.That(GetFolderChildren(container1.Key), Has.Length.EqualTo(1));
        Assert.That(GetFolderChildren(container2.Key), Is.Empty);

        var copyResult = await ElementEditingService.CopyAsync(original.Key, container2.Key, Constants.Security.SuperUserKey);
        Assert.Multiple(() =>
        {
            Assert.That(copyResult.Success, Is.True);
            Assert.That(copyResult.Status, Is.EqualTo(ContentEditingOperationStatus.Success));
        });

        Assert.That(GetFolderChildren(container2.Key), Has.Length.EqualTo(1));

        var copy = copyResult.Result;
        Assert.That(copy, Is.Not.Null);
        Assert.Multiple(() =>
        {
            Assert.That(copy.HasIdentity, Is.True);
            Assert.That(copy.ParentId, Is.EqualTo(container2.Id));
            Assert.That(copy.Key, Is.Not.EqualTo(original.Key));
            Assert.That(copy.Name, Is.EqualTo(original.Name));
        });
    }

    [Test]
    public async Task Can_Copy_To_Existing_Parent()
    {
        var container = (await ElementContainerService.CreateAsync(null, "Root Container", null, Constants.Security.SuperUserKey)).Result;
        var original = await CreateInvariantElement(container.Key);

        Assert.That(GetFolderChildren(container.Key), Has.Length.EqualTo(1));

        var copyResult = await ElementEditingService.CopyAsync(original.Key, container.Key, Constants.Security.SuperUserKey);
        Assert.Multiple(() =>
        {
            Assert.That(copyResult.Success, Is.True);
            Assert.That(copyResult.Status, Is.EqualTo(ContentEditingOperationStatus.Success));
        });

        Assert.That(GetFolderChildren(container.Key), Has.Length.EqualTo(2));

        var copy = copyResult.Result;
        Assert.That(copy, Is.Not.Null);
        Assert.Multiple(() =>
        {
            Assert.That(copy.HasIdentity, Is.True);
            Assert.That(copy.ParentId, Is.EqualTo(container.Id));
            Assert.That(copy.Key, Is.Not.EqualTo(original.Key));
            Assert.That(copy.Name, Is.EqualTo($"{original.Name} (1)"));
        });
    }

    [Test]
    public async Task Cannot_Copy_To_Non_Existing_Parent()
    {
        var original = await CreateInvariantElement();

        var copyResult = await ElementEditingService.CopyAsync(original.Key, Guid.NewGuid(), Constants.Security.SuperUserKey);
        Assert.Multiple(() =>
        {
            Assert.That(copyResult.Success, Is.False);
            Assert.That(copyResult.Status, Is.EqualTo(ContentEditingOperationStatus.ParentNotFound));
        });
    }
}
