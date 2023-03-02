using System.Text;
using Microsoft.Extensions.Logging;
using NUnit.Framework;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Hosting;
using Umbraco.Cms.Core.IO;
using Umbraco.Cms.Persistence.EFCore.Scoping;
using Umbraco.Cms.Tests.Common.Testing;
using Umbraco.Cms.Tests.Integration.Testing;

namespace Umbraco.Cms.Tests.Integration.Umbraco.Persistence.EFCore.Scoping;

[TestFixture]
[UmbracoTest(Database = UmbracoTestOptions.Database.NewSchemaPerTest, Logger = UmbracoTestOptions.Logger.Console)]
public class EfCoreScopedFileSystemsTests : UmbracoIntegrationTest
{
    [SetUp]
    public void SetUp() => ClearFiles(IOHelper);

    [TearDown]
    public void Teardown() => ClearFiles(IOHelper);

    private MediaFileManager MediaFileManager => GetRequiredService<MediaFileManager>();

    private IHostingEnvironment HostingEnvironment => GetRequiredService<IHostingEnvironment>();

    private IEfCoreScopeProvider EfCoreScopeProvider => GetRequiredService<IEfCoreScopeProvider>();

    private void ClearFiles(IIOHelper ioHelper)
    {
        TestHelper.DeleteDirectory(ioHelper.MapPath("media"));
        TestHelper.DeleteDirectory(ioHelper.MapPath("FileSysTests"));
        TestHelper.DeleteDirectory(ioHelper.MapPath(Constants.SystemDirectories.TempData.EnsureEndsWith('/') + "ShadowFs"));
    }

    [Test]
    public void MediaFileManager_does_not_write_to_physical_file_system_when_scoped_if_scope_does_not_complete()
    {
        var rootPath = HostingEnvironment.MapPathWebRoot(GlobalSettings.UmbracoMediaPhysicalRootPath);
        var rootUrl = HostingEnvironment.ToAbsolute(GlobalSettings.UmbracoMediaPath);
        var physMediaFileSystem = new PhysicalFileSystem(IOHelper, HostingEnvironment, GetRequiredService<ILogger<PhysicalFileSystem>>(), rootPath, rootUrl);
        var mediaFileManager = MediaFileManager;

        Assert.IsFalse(physMediaFileSystem.FileExists("f1.txt"));

        using (EfCoreScopeProvider.CreateScope(scopeFileSystems: true))
        {
            using (var ms = new MemoryStream(Encoding.UTF8.GetBytes("foo")))
            {
                MediaFileManager.FileSystem.AddFile("f1.txt", ms);
            }

            Assert.IsTrue(mediaFileManager.FileSystem.FileExists("f1.txt"));
            Assert.IsFalse(physMediaFileSystem.FileExists("f1.txt"));
        }

        // After scope is disposed ensure shadow wrapper didn't commit to physical
        Assert.IsFalse(mediaFileManager.FileSystem.FileExists("f1.txt"));
        Assert.IsFalse(physMediaFileSystem.FileExists("f1.txt"));
    }
}
