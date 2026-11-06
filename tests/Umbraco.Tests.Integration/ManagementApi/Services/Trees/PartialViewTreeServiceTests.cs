using NUnit.Framework;
using Umbraco.Cms.Api.Management.Services.FileSystem;
using Umbraco.Cms.Api.Management.ViewModels.Tree;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.IO;

namespace Umbraco.Cms.Tests.Integration.ManagementApi.Services.Trees;

public class PartialViewTreeServiceTests : FileSystemTreeServiceTestsBase
{
    protected override string FileExtension { get; set; } = ".cshtml";

    protected override string FileSystemPath => Constants.SystemDirectories.PartialViews;

    protected override IFileSystem? GetPartialViewsFileSystem() => TestFileSystem;

    [Test]
    public void Can_Get_Siblings()
    {
        var service = new PartialViewTreeService(FileSystems);

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
        var service = new PartialViewTreeService(FileSystems);

        var path = Path.Join("tests", $"file5{FileExtension}");
        FileSystemTreeItemPresentationModel[] treeModels = service.GetAncestorModels(path, true);

        Assert.IsNotEmpty(treeModels);
        Assert.AreEqual(treeModels.Length, 2);
        Assert.AreEqual(treeModels[0].Name, "tests");
    }

    [Test]
    public void Can_Get_Ancestors_Of_Rooted_Path_Starting_At_Top_Level_Item()
    {
        using var stream = CreateStream();
        TestFileSystem.AddFile(Path.Join("blockgrid", $"area{FileExtension}"), stream);
        var service = new PartialViewTreeService(FileSystems);

        var path = $"{Path.DirectorySeparatorChar}{Path.Join("blockgrid", $"area{FileExtension}")}";
        FileSystemTreeItemPresentationModel[] treeModels = service.GetAncestorModels(path, true);

        Assert.AreEqual(2, treeModels.Length);
        Assert.AreEqual("blockgrid", treeModels[0].Name);
        Assert.AreEqual("/blockgrid", treeModels[0].Path);
        Assert.IsNull(treeModels[0].Parent);
        Assert.AreEqual($"area{FileExtension}", treeModels[1].Name);
        Assert.AreEqual("/blockgrid", treeModels[1].Parent?.Path);
    }

    [Test]
    public void Can_Get_Ancestors_Of_Rooted_Top_Level_File_Without_Parent()
    {
        var service = new PartialViewTreeService(FileSystems);

        var path = $"{Path.DirectorySeparatorChar}file5{FileExtension}";
        FileSystemTreeItemPresentationModel[] treeModels = service.GetAncestorModels(path, true);

        Assert.AreEqual(1, treeModels.Length);
        Assert.AreEqual($"file5{FileExtension}", treeModels[0].Name);
        Assert.IsNull(treeModels[0].Parent);
    }

    [Test]
    public void Can_Get_PathViewModels()
    {
        var service = new PartialViewTreeService(FileSystems);

        FileSystemTreeItemPresentationModel[] treeModels = service.GetPathViewModels(string.Empty, 0, int.MaxValue, out var totalItems);

        Assert.IsNotEmpty(treeModels);
        Assert.AreEqual(treeModels.Length, totalItems);
    }

    [Test]
    public void Will_Hide_Unsupported_File_Extensions()
    {
        var service = new PartialViewTreeService(FileSystems);
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
