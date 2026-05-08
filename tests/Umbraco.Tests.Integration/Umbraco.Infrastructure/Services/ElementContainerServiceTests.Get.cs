using NUnit.Framework;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Models;

namespace Umbraco.Cms.Tests.Integration.Umbraco.Infrastructure.Services;

public partial class ElementContainerServiceTests
{
    [Test]
    public async Task Can_Get_Container_At_Root()
    {
        EntityContainer root = (await ElementContainerService.CreateAsync(null,"Root Container", null, Constants.Security.SuperUserKey)).Result;

        EntityContainer created = await ElementContainerService.GetAsync(root.Key);
        Assert.NotNull(created);
        Assert.Multiple(() =>
        {
            Assert.AreEqual("Root Container", created.Name);
            Assert.AreEqual(Constants.System.Root, created.ParentId);
        });
    }

    [Test]
    public async Task Can_Get_Child_Container()
    {
        EntityContainer root = (await ElementContainerService.CreateAsync(null,"Root Container", null, Constants.Security.SuperUserKey)).Result;
        EntityContainer child = (await ElementContainerService.CreateAsync(null, "Child Container", root.Key, Constants.Security.SuperUserKey)).Result;

        EntityContainer created = await ElementContainerService.GetAsync(child.Key);
        Assert.IsNotNull(created);
        Assert.Multiple(() =>
        {
            Assert.AreEqual("Child Container", created.Name);
            Assert.AreEqual(root.Id, child.ParentId);
        });
    }
}
