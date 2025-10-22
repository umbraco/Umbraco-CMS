using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NUnit.Framework;
using Umbraco.Cms.Api.Management.Services.FileSystem;
using Umbraco.Cms.Api.Management.ViewModels.Tree;
using Umbraco.Cms.Core.Configuration.Models;
using Umbraco.Cms.Core.IO;
using Umbraco.Cms.Tests.Common.TestHelpers;

namespace Umbraco.Cms.Tests.Integration.ManagementApi.Services.Trees;

public class StyleSheetTreeServiceTests : FileSystemTreeServiceTestsBase
{
    protected override string FileExtension { get; set; } = ".css";

    protected override string FileSystemPath => GlobalSettings.UmbracoCssPath;

    protected FileSystems FileSystems { get; private set; }

    protected IFileSystem TestFileSystem { get; private set; }

    protected override IFileSystem? GetStylesheetsFileSystem() => TestFileSystem;


    [SetUp]
    public override void SetUpFileSystem()
    {
        TestFileSystem = new PhysicalFileSystem(IOHelper, HostingEnvironment, LoggerFactory.CreateLogger<PhysicalFileSystem>(), HostingEnvironment.MapPathWebRoot(FileSystemPath), HostingEnvironment.ToAbsolute(FileSystemPath));

        FileSystems = FileSystemsCreator.CreateTestFileSystems(
            LoggerFactory,
            IOHelper,
            GetRequiredService<IOptions<GlobalSettings>>(),
            HostingEnvironment,
            GetPartialViewsFileSystem(),
            GetStylesheetsFileSystem(),
            GetScriptsFileSystem(),
            null);
        for (int i = 0; i < 10; i++)
        {
            using var stream = CreateStream(Path.Join("tests"));
            TestFileSystem.AddFile($"file{i}{FileExtension}", stream);
        }
    }

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

    [TearDown]
    public override void TearDownFileSystem()
    {
        Purge(TestFileSystem, string.Empty);
        FileSystems = null;
    }
}
