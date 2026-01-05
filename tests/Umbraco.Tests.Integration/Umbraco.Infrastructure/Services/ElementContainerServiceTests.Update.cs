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
            Assert.IsTrue(result.Success);
            Assert.AreEqual(EntityContainerOperationStatus.Success, result.Status);
        });

        var updated = await ElementContainerService.GetAsync(key);
        Assert.NotNull(updated);
        Assert.Multiple(() =>
        {
            Assert.AreEqual("Root Container UPDATED", updated.Name);
            Assert.AreEqual(Constants.System.Root, updated.ParentId);
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
            Assert.IsTrue(result.Success);
            Assert.AreEqual(EntityContainerOperationStatus.Success, result.Status);
        });

        EntityContainer updated = await ElementContainerService.GetAsync(child.Key);
        Assert.NotNull(updated);
        Assert.Multiple(() =>
        {
            Assert.AreEqual("Child Container UPDATED", updated.Name);
            Assert.AreEqual(root.Id, updated.ParentId);
        });
    }
}
