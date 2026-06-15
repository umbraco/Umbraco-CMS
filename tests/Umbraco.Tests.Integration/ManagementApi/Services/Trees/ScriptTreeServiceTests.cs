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

        Assert.That($"file5{FileExtension}", Is.EqualTo(treeModel[index].Name));
        Assert.That($"file4{FileExtension}", Is.EqualTo(treeModel[index - 1].Name));
        Assert.That($"file6{FileExtension}", Is.EqualTo(treeModel[index + 1].Name));
        Assert.That(treeModel, Has.Length.EqualTo(3));
        Assert.That(after, Is.EqualTo(3));
        Assert.That(before, Is.EqualTo(4));
    }

    [Test]
    public void Can_Get_Ancestors()
    {
        var service = new ScriptTreeService(FileSystems);

        var path = Path.Join("tests", $"file5{FileExtension}");
        FileSystemTreeItemPresentationModel[] treeModels = service.GetAncestorModels(path, true);

        Assert.That(treeModels, Is.Not.Empty);
        Assert.That(treeModels, Has.Length.EqualTo(2));
        Assert.That(treeModels[0].Name, Is.EqualTo("tests"));
    }

    [Test]
    public void Can_Get_PathViewModels()
    {
        var service = new ScriptTreeService(FileSystems);

        FileSystemTreeItemPresentationModel[] treeModels = service.GetPathViewModels(string.Empty, 0, int.MaxValue, out var totalItems);

        Assert.That(treeModels, Is.Not.Empty);
        Assert.That(totalItems, Is.EqualTo(treeModels.Length));
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

        Assert.That(treeModels.Where(file => file.Name.Contains(".invalid")), Is.Empty);
        Assert.That(totalItems, Is.EqualTo(treeModels.Length));
    }
}
