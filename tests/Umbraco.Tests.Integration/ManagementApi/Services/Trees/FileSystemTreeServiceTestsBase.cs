using System.Text;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NUnit.Framework;
using Umbraco.Cms.Core.Configuration.Models;
using Umbraco.Cms.Core.Hosting;
using Umbraco.Cms.Core.IO;
using Umbraco.Cms.Tests.Common.TestHelpers;
using Umbraco.Cms.Tests.Integration.Testing;

namespace Umbraco.Cms.Tests.Integration.ManagementApi.Services.Trees;

public abstract class FileSystemTreeServiceTestsBase : UmbracoIntegrationTest
{
    protected FileSystems FileSystems { get; private set; }

    protected IFileSystem TestFileSystem { get; private set; }

    protected abstract string FileSystemPath { get; }

    protected IHostingEnvironment HostingEnvironment => GetRequiredService<IHostingEnvironment>();

    [SetUp]
    public void SetUpFileSystem()
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
            TestFileSystem.AddFile($"file{i}", stream);
        }
    }

    private static Stream CreateStream(string contents = null)
    {
        if (string.IsNullOrEmpty(contents))
        {
            contents = "/* test */";
        }

        var bytes = Encoding.UTF8.GetBytes(contents);
        return new MemoryStream(bytes);
    }

    protected virtual IFileSystem? GetPartialViewsFileSystem() => null;

    protected virtual IFileSystem? GetStylesheetsFileSystem() => null;

    protected virtual IFileSystem? GetScriptsFileSystem() => null;

    [TearDown]
    public void TearDownFileSystem()
    {
        Purge(TestFileSystem, string.Empty);
        FileSystems = null;
    }

    private static void Purge(IFileSystem fs, string path)
    {
        var files = fs.GetFiles(path, "*");
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
}
