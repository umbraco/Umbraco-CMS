using NUnit.Framework;
using Umbraco.Cms.Api.Management.Services.FileSystem;
using Umbraco.Cms.Api.Management.ViewModels.Tree;
using Umbraco.Cms.Core.IO;

namespace Umbraco.Cms.Tests.Integration.ManagementApi.Services.Trees;

public class StyleSheetTreeServiceTests : FileSystemTreeServiceTestsBase
{
    protected override string FileExtension { get; set; } = ".css";

    protected override string FileSystemPath => GlobalSettings.UmbracoCssPath;

    protected override IFileSystem? GetStylesheetsFileSystem() => TestFileSystem;

    [Test]
    public void Can_Get_Siblings_From_StyleSheet_Tree_Service()
    {
        var service = new StyleSheetTreeService(FileSystems);

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
    public void Can_Get_Ancestors_From_StyleSheet_Tree_Service()
    {
        var service = new StyleSheetTreeService(FileSystems);

        var path = Path.Join("tests", $"file5{FileExtension}");
        FileSystemTreeItemPresentationModel[] treeModel = service.GetAncestorModels(path, true);

        Assert.IsNotEmpty(treeModel);
        Assert.AreEqual(treeModel.Length, 2);
        Assert.AreEqual(treeModel[0].Name, "tests");
    }

    [Test]
    public void Can_Get_PathViewModels_From_StyleSheet_Tree_Service()
    {
        var service = new StyleSheetTreeService(FileSystems);

        FileSystemTreeItemPresentationModel[] treeModels = service.GetPathViewModels(string.Empty, 0, Int32.MaxValue, out var totalItems);

        Assert.IsNotEmpty(treeModels);
        Assert.AreEqual(treeModels.Length, totalItems);
    }

    [Test]
    public void Will_Hide_Unsupported_File_Extensions()
    {
        var service = new StyleSheetTreeService(FileSystems);
        for (int i = 0; i < 2; i++)
        {
            using var stream = CreateStream(Path.Join("tests"));
            TestFileSystem.AddFile($"file{i}.invalid", stream);
        }

        FileSystemTreeItemPresentationModel[] treeModels = service.GetPathViewModels(string.Empty, 0, Int32.MaxValue, out var totalItems);

        Assert.IsEmpty(treeModels.Where(file => file.Name.Contains(".invalid")));
        Assert.AreEqual(treeModels.Length, totalItems);
    }
}
