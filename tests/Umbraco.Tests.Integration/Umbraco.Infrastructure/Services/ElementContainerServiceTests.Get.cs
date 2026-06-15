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
        Assert.That(created, Is.Not.Null);
        Assert.Multiple(() =>
        {
            Assert.That(created.Name, Is.EqualTo("Root Container"));
            Assert.That(created.ParentId, Is.EqualTo(Constants.System.Root));
        });
    }

    [Test]
    public async Task Can_Get_Child_Container()
    {
        EntityContainer root = (await ElementContainerService.CreateAsync(null,"Root Container", null, Constants.Security.SuperUserKey)).Result;
        EntityContainer child = (await ElementContainerService.CreateAsync(null, "Child Container", root.Key, Constants.Security.SuperUserKey)).Result;

        EntityContainer created = await ElementContainerService.GetAsync(child.Key);
        Assert.That(created, Is.Not.Null);
        Assert.Multiple(() =>
        {
            Assert.That(created.Name, Is.EqualTo("Child Container"));
            Assert.That(child.ParentId, Is.EqualTo(root.Id));
        });
    }
}
