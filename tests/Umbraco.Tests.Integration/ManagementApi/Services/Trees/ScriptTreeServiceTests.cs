using NUnit.Framework;
using Umbraco.Cms.Api.Management.Services.FileSystem;
using Umbraco.Cms.Api.Management.ViewModels.Tree;
using Umbraco.Cms.Core.IO;

namespace Umbraco.Cms.Tests.Integration.ManagementApi.Services.Trees;

public class ScriptTreeServiceTests : FileSystemTreeServiceTestsBase
{
    protected override string FileExtension { get; set; } = ".js";

    protected override string FileSystemPath => GlobalSettings.UmbracoScriptsPath;

    protected override IFileSystem? GetScriptsFileSystem() => TestFileSystem;

    [Test]
    public void Can_Get_Siblings()
    {
        var service = new ScriptTreeService(FileSystems);

        FileSystemTreeItemPresentationModel[] treeModel = service.GetSiblingsViewModels($"file5{FileExtension}", 1, 1, out long before, out var after);
        int index = Array.FindIndex(treeModel, item => item.Name == $"file5{FileExtension}");

        Assert.AreEqual(treeModel[index].Name, $"file5{FileExtension}");
        Assert.AreEqual(treeModel[index - 1].Name, $"file4{FileExtension}");
        Assert.AreEqual(treeModel[index + 1].Name, $"file6{FileExtension}");
        Assert.That(treeModel.Length == 3);
        Assert.AreEqual(after, 3);
        Assert.AreEqual(before, 4);
    }

    [Test]
    public void Can_Get_Ancestors()
    {
        var service = new ScriptTreeService(FileSystems);

        var path = Path.Join("tests", $"file5{FileExtension}");
        FileSystemTreeItemPresentationModel[] treeModels = service.GetAncestorModels(path, true);

        Assert.IsNotEmpty(treeModels);
        Assert.AreEqual(treeModels.Length, 2);
        Assert.AreEqual(treeModels[0].Name, "tests");
    }

    [Test]
    public void Can_Get_PathViewModels()
    {
        var service = new ScriptTreeService(FileSystems);

        FileSystemTreeItemPresentationModel[] treeModels = service.GetPathViewModels(string.Empty, 0, int.MaxValue, out var totalItems);

        Assert.IsNotEmpty(treeModels);
        Assert.AreEqual(treeModels.Length, totalItems);
    }

    [Test]
    public void Will_Hide_Unsupported_File_Extensions()
    {
        var service = new ScriptTreeService(FileSystems);
        for (int i = 0; i < 2; i++)
        {
            using var stream = CreateStream();
            TestFileSystem.AddFile($"file{i}.invalid", stream);
        }

        FileSystemTreeItemPresentationModel[] treeModels = service.GetPathViewModels(string.Empty, 0, int.MaxValue, out var totalItems);

        Assert.IsEmpty(treeModels.Where(file => file.Name.Contains(".invalid")));
        Assert.AreEqual(treeModels.Length, totalItems);
    }
}
