using System.IO;
using Microsoft.Extensions.Logging;
using NUnit.Framework;
using Umbraco.Cms.Api.Management.Services.FileSystem;
using Umbraco.Cms.Api.Management.ViewModels.Tree;
using Umbraco.Cms.Core.IO;

namespace Umbraco.Cms.Tests.Integration.ManagementApi.Services.Trees;

public class PhysicalFileSystemTreeServiceTests : FileSystemTreeServiceTestsBase
{
    protected override string FileExtension { get; set; } = string.Empty;

    protected override string FileSystemPath => "/";

    protected override void CreateFiles()
    {
        var paths = new[]
        {
            Path.Join("App_Plugins", "test-extension", "test.js"),
            Path.Join("wwwroot", "css", "test.css"),
            Path.Join("wwwroot", "css", "test2.css"),
            Path.Join("wwwroot", "css", "test3.css"),
            Path.Join("wwwroot", "css", "test4.css"),
            Path.Join("Program.cs"),
        };
        foreach (var path in paths)
        {
            var stream = CreateStream();
            TestFileSystem.AddFile(path, stream);
        }
    }

    [Test]
    public void Can_Get_Siblings()
    {
        var service = CreateService();

        FileSystemTreeItemPresentationModel[] treeModels = service.GetSiblingsViewModels("wwwroot/css/test2.css", 1, 1, out long before, out var after);

        Assert.AreEqual(3, treeModels.Length);
        Assert.AreEqual(treeModels[0].Name, "test.css");
        Assert.AreEqual(treeModels[1].Name, "test2.css");
        Assert.AreEqual(treeModels[2].Name, "test3.css");
        Assert.AreEqual(before, 0);
        Assert.AreEqual(after, 1);
    }

    [Test]
    public void Can_Get_Ancestors()
    {
        var service = CreateService();

        FileSystemTreeItemPresentationModel[] treeModels = service.GetAncestorModels(Path.Join("wwwroot", "css", "test.css"), true);

        Assert.IsNotEmpty(treeModels);
        Assert.AreEqual(treeModels.Length, 3);
        Assert.AreEqual(treeModels[0].Name, "wwwroot");
        Assert.AreEqual(treeModels[1].Name, "css");
        Assert.AreEqual(treeModels[2].Name, "test.css");
    }

    [Test]
    public void Can_Get_Root_PathViewModels()
    {
        var service = CreateService();

        FileSystemTreeItemPresentationModel[] treeModels = service.GetPathViewModels(string.Empty, 0, int.MaxValue, out var totalItems);

        Assert.IsNotEmpty(treeModels);
        Assert.AreEqual(totalItems, 2);
        Assert.AreEqual(treeModels.Length, totalItems);
        Assert.AreEqual(treeModels[0].Name, "App_Plugins");
        Assert.AreEqual(treeModels[1].Name, "wwwroot");
    }

    [Test]
    public void Can_Get_Child_PathViewModels()
    {
        var service = CreateService();

        FileSystemTreeItemPresentationModel[] treeModels = service.GetPathViewModels("App_Plugins/test-extension", 0, int.MaxValue, out var totalItems);

        Assert.IsNotEmpty(treeModels);
        Assert.AreEqual(totalItems, 1);
        Assert.AreEqual(treeModels.Length, totalItems);
        Assert.AreEqual(treeModels[0].Name, "test.js");
    }

    private PhysicalFileSystemTreeService CreateService()
    {
        var physicalFileSystem = new PhysicalFileSystem(IOHelper, HostingEnvironment, LoggerFactory.CreateLogger<PhysicalFileSystem>(), HostingEnvironment.MapPathWebRoot(FileSystemPath), HostingEnvironment.ToAbsolute(FileSystemPath));
        return new PhysicalFileSystemTreeService(physicalFileSystem);
    }
}
