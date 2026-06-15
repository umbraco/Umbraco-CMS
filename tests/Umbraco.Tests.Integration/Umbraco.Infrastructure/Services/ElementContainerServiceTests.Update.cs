using NUnit.Framework;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Services.OperationStatus;

namespace Umbraco.Cms.Tests.Integration.Umbraco.Infrastructure.Services;

public partial class ElementContainerServiceTests
{
    [Test]
    public async Task Can_Update_Container_At_Root()
    {
        var key = (await ElementContainerService.CreateAsync(null, "Root Container", null, Constants.Security.SuperUserKey)).Result.Key;

        var result = await ElementContainerService.UpdateAsync(key, "Root Container UPDATED", Constants.Security.SuperUserKey);
        Assert.Multiple(() =>
        {
            Assert.That(result.Success, Is.True);
            Assert.That(result.Status, Is.EqualTo(EntityContainerOperationStatus.Success));
        });

        var updated = await ElementContainerService.GetAsync(key);
        Assert.That(updated, Is.Not.Null);
        Assert.Multiple(() =>
        {
            Assert.That(updated.Name, Is.EqualTo("Root Container UPDATED"));
            Assert.That(updated.ParentId, Is.EqualTo(Constants.System.Root));
        });
    }

    [Test]
    public async Task Can_Update_Child_Container()
    {
        EntityContainer root = (await ElementContainerService.CreateAsync(null, "Root Container", null, Constants.Security.SuperUserKey)).Result;
        EntityContainer child = (await ElementContainerService.CreateAsync(null, "Child Container", root.Key, Constants.Security.SuperUserKey)).Result;

        var result = await ElementContainerService.UpdateAsync(child.Key, "Child Container UPDATED", Constants.Security.SuperUserKey);
        Assert.Multiple(() =>
        {
            Assert.That(result.Success, Is.True);
            Assert.That(result.Status, Is.EqualTo(EntityContainerOperationStatus.Success));
        });

        EntityContainer updated = await ElementContainerService.GetAsync(child.Key);
        Assert.That(updated, Is.Not.Null);
        Assert.Multiple(() =>
        {
            Assert.That(updated.Name, Is.EqualTo("Child Container UPDATED"));
            Assert.That(updated.ParentId, Is.EqualTo(root.Id));
        });
    }
}
