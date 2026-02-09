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

    protected abstract string FileExtension { get; set; }

    protected abstract string FileSystemPath { get; }

    protected IHostingEnvironment HostingEnvironment => GetRequiredService<IHostingEnvironment>();

    [SetUp]
    public virtual void SetUpFileSystem()
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

        CreateFiles();
    }

    protected virtual void CreateFiles()
    {
        for (int i = 0; i < 10; i++)
        {
            using var stream = CreateStream();
            TestFileSystem.AddFile($"file{i}{FileExtension}", stream);
        }
    }

    protected static Stream CreateStream(string contents = null)
    {
        const string DefaultFileContent = "/* test */";
        var bytes = Encoding.UTF8.GetBytes(contents ?? DefaultFileContent);
        return new MemoryStream(bytes);
    }

    protected virtual IFileSystem? GetPartialViewsFileSystem() => null;

    protected virtual IFileSystem? GetStylesheetsFileSystem() => null;

    protected virtual IFileSystem? GetScriptsFileSystem() => null;

    [TearDown]
    public virtual void TearDownFileSystem()
    {
        Purge(TestFileSystem, string.Empty);
        FileSystems = null;
    }

    private static void Purge(IFileSystem fs, string path)
    {
        var files = fs.GetFiles(path, "*");
        foreach (var file in files)
        {
            try
            {
                fs.DeleteFile(file);
            }
            catch (IOException)
            {
                // Ignore locked files during cleanup
            }
        }

        var dirs = fs.GetDirectories(path);
        foreach (var dir in dirs)
        {
            Purge(fs, dir);
            try
            {
                fs.DeleteDirectory(dir);
            }
            catch (IOException)
            {
                // Ignore locked directories during cleanup
            }
        }
    }
}
