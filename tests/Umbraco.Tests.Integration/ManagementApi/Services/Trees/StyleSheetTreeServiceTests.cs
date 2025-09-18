using System.Text;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NUnit.Framework;
using Umbraco.Cms.Api.Management.Services.FileSystem;
using Umbraco.Cms.Api.Management.ViewModels.Tree;
using Umbraco.Cms.Core.Configuration.Models;
using Umbraco.Cms.Core.Hosting;
using Umbraco.Cms.Core.IO;
using Umbraco.Cms.Tests.Common.TestHelpers;
using Umbraco.Cms.Tests.Integration.Testing;

namespace Umbraco.Cms.Tests.Integration.ManagementApi.Services.Trees;

public class StyleSheetTreeServiceTests : UmbracoIntegrationTest
{
    private FileSystems _fileSystems;
    private IFileSystem _fileSystem;

    protected virtual T GetRequiredService<T>() => Services.GetRequiredService<T>();

    private IHostingEnvironment HostingEnvironment => GetRequiredService<IHostingEnvironment>();

    [SetUp]
    public void SetUpFileSystem()
    {
        var path = GlobalSettings.UmbracoCssPath;
        _fileSystem = new PhysicalFileSystem(IOHelper, HostingEnvironment, LoggerFactory.CreateLogger<PhysicalFileSystem>(), HostingEnvironment.MapPathWebRoot(path), HostingEnvironment.ToAbsolute(path));

        _fileSystems = FileSystemsCreator.CreateTestFileSystems(
            LoggerFactory,
            IOHelper,
            GetRequiredService<IOptions<GlobalSettings>>(),
            HostingEnvironment,
            null,
            _fileSystem,
            null,
            null);
        for (int i = 0; i < 10; i++)
        {
            using (var stream = CreateStream("/styleSheetTreeServiceTests/"))
            {
                _fileSystem.AddFile($"file{i}.css", stream);
            }
        }
    }

    [TearDown]
    public void TearDownFileSystem()
    {
        // Delete all files
        Purge(_fileSystems.StylesheetsFileSystem, string.Empty);

        _fileSystems = null;
    }

    [Test]
    public void Can_Get_Siblings_From_StyleSheet_Tree_Service()
    {
        var service = new StyleSheetTreeService(_fileSystems);

        FileSystemTreeItemPresentationModel[] treeModel = service.GetSiblingsViewModel("file5.css", 1, 1, out long before, out var after);
        int index = Array.FindIndex(treeModel, item => item.Name == "file5.css");

        Assert.AreEqual(treeModel[index].Name, "file5.css");
        Assert.AreEqual(treeModel[index - 1].Name, "file4.css");
        Assert.AreEqual(treeModel[index + 1].Name, "file6.css");
        Assert.That(treeModel.Length == 3);
        Assert.AreEqual(after, 3);
        Assert.AreEqual(before, 4);
    }

    private void Purge(IFileSystem fs, string path)
    {
        var files = fs.GetFiles(path, "*.css");
        foreach (var file in files)
        {
            fs.DeleteFile(file);
        }

        var dirs = fs.GetDirectories(path);
        foreach (var dir in dirs)
        {
            Purge(fs, dir);
            fs.DeleteDirectory(dir);
        }
    }

    protected Stream CreateStream(string contents = null)
    {
        if (string.IsNullOrEmpty(contents))
        {
            contents = "/* test */";
        }

        var bytes = Encoding.UTF8.GetBytes(contents);
        var stream = new MemoryStream(bytes);

        return stream;
    }
}
