using System.Text;
using Microsoft.Extensions.DependencyInjection;
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
    public FileSystems _fileSystems;
    public IFileSystem _fileSystem;

    protected abstract string path { get; }

    public IHostingEnvironment HostingEnvironment => GetRequiredService<IHostingEnvironment>();

    protected virtual T GetRequiredService<T>() => Services.GetRequiredService<T>();

    [SetUp]
    public void SetUpFileSystem()
    {
        _fileSystem = new PhysicalFileSystem(IOHelper, HostingEnvironment, LoggerFactory.CreateLogger<PhysicalFileSystem>(), HostingEnvironment.MapPathWebRoot(path), HostingEnvironment.ToAbsolute(path));

        _fileSystems = FileSystemsCreator.CreateTestFileSystems(
            LoggerFactory,
            IOHelper,
            GetRequiredService<IOptions<GlobalSettings>>(),
            HostingEnvironment,
            _fileSystem,
            _fileSystem,
            _fileSystem,
            _fileSystem);
        for (int i = 0; i < 10; i++)
        {
            using (var stream = CreateStream(Path.Join("tests")))
            {
                _fileSystem.AddFile($"file{i}", stream);
            }
        }
    }

    [TearDown]
    public void TearDownFileSystem()
    {
        // Delete all files
        Purge(_fileSystems.StylesheetsFileSystem, string.Empty);
        Purge(_fileSystems.ScriptsFileSystem, string.Empty);
        Purge(_fileSystems.PartialViewsFileSystem, string.Empty);
        Purge(_fileSystems.MvcViewsFileSystem, string.Empty);

        _fileSystems = null;
    }

    private void Purge(IFileSystem fs, string path)
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
