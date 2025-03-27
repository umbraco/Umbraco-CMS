using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.IO;
using Umbraco.Cms.Persistence.EFCore.Scoping;
using Umbraco.Cms.Tests.Common;
using Umbraco.Cms.Tests.Common.Testing;
using Umbraco.Cms.Tests.Integration.Testing;
using Umbraco.Cms.Tests.Integration.Umbraco.Persistence.EFCore.DbContext;
using IHostingEnvironment = Umbraco.Cms.Core.Hosting.IHostingEnvironment;

namespace Umbraco.Cms.Tests.Integration.Umbraco.Persistence.EFCore.Scoping;

[TestFixture]
[UmbracoTest(Database = UmbracoTestOptions.Database.NewSchemaPerTest, Logger = UmbracoTestOptions.Logger.Console)]
internal sealed class EFCoreScopedFileSystemsTests : UmbracoIntegrationTest
{
    [SetUp]
    public void SetUp() => ClearFiles(IOHelper);

    [TearDown]
    public void Teardown() => ClearFiles(IOHelper);

    private MediaFileManager MediaFileManager => GetRequiredService<MediaFileManager>();

    private IHostingEnvironment HostingEnvironment => GetRequiredService<IHostingEnvironment>();
    private IHostEnvironment HostEnvironment => GetRequiredService<IHostEnvironment>();

    private IEFCoreScopeProvider<TestUmbracoDbContext> EfCoreScopeProvider => GetRequiredService<IEFCoreScopeProvider<TestUmbracoDbContext>>();
    private IEFCoreScopeAccessor<TestUmbracoDbContext> EfCoreScopeAccessor => GetRequiredService<IEFCoreScopeAccessor<TestUmbracoDbContext>>();

    private void ClearFiles(IIOHelper ioHelper)
    {
        TestHelper.DeleteDirectory(ioHelper.MapPath("media"));
        TestHelper.DeleteDirectory(ioHelper.MapPath("FileSysTests"));
        TestHelper.DeleteDirectory(ioHelper.MapPath(Constants.SystemDirectories.TempData.EnsureEndsWith('/') + "ShadowFs"));
    }

    [Test]
    public void MediaFileManager_Does_Not_Write_To_Physical_File_System_When_Scoped_If_Scope_Does_Not_Complete()
    {
        var rootPath = HostingEnvironment.MapPathWebRoot(GlobalSettings.UmbracoMediaPhysicalRootPath);
        var rootUrl = HostingEnvironment.ToAbsolute(GlobalSettings.UmbracoMediaPath);
        var physMediaFileSystem = new PhysicalFileSystem(IOHelper, HostEnvironment, GetRequiredService<ILogger<PhysicalFileSystem>>(), rootPath, rootUrl);
        var mediaFileManager = MediaFileManager;

        Assert.IsFalse(physMediaFileSystem.FileExists("f1.txt"));

        using (EfCoreScopeProvider.CreateScope(scopeFileSystems: true))
        {
            using (var ms = new MemoryStream("foo"u8.ToArray()))
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

    [Test]
    public void MediaFileManager_Writes_To_Physical_File_System_When_Scoped_And_Scope_Is_Completed()
    {
        var rootPath = HostingEnvironment.MapPathWebRoot(GlobalSettings.UmbracoMediaPhysicalRootPath);
        var rootUrl = HostingEnvironment.ToAbsolute(GlobalSettings.UmbracoMediaPath);
        var physMediaFileSystem = new PhysicalFileSystem(IOHelper, HostEnvironment, GetRequiredService<ILogger<PhysicalFileSystem>>(), rootPath, rootUrl);
        var mediaFileManager = MediaFileManager;

        Assert.IsFalse(physMediaFileSystem.FileExists("f1.txt"));

        using (var scope = EfCoreScopeProvider.CreateScope(scopeFileSystems: true))
        {
            using (var ms = new MemoryStream("foo"u8.ToArray()))
            {
                mediaFileManager.FileSystem.AddFile("f1.txt", ms);
            }

            Assert.IsTrue(mediaFileManager.FileSystem.FileExists("f1.txt"));
            Assert.IsFalse(physMediaFileSystem.FileExists("f1.txt"));

            scope.Complete();

            Assert.IsTrue(mediaFileManager.FileSystem.FileExists("f1.txt"));
            Assert.IsFalse(physMediaFileSystem.FileExists("f1.txt"));
        }

        // After scope is disposed ensure shadow wrapper writes to physical file system
        Assert.IsTrue(mediaFileManager.FileSystem.FileExists("f1.txt"));
        Assert.IsTrue(physMediaFileSystem.FileExists("f1.txt"));
    }

    [Test]
    public void MultiThread()
    {
        var rootPath = HostingEnvironment.MapPathWebRoot(GlobalSettings.UmbracoMediaPhysicalRootPath);
        var rootUrl = HostingEnvironment.ToAbsolute(GlobalSettings.UmbracoMediaPath);
        var physMediaFileSystem = new PhysicalFileSystem(IOHelper, HostEnvironment, GetRequiredService<ILogger<PhysicalFileSystem>>(), rootPath, rootUrl);
        var mediaFileManager = MediaFileManager;
        var taskHelper = new TaskHelper(Mock.Of<ILogger<TaskHelper>>());

        using (EfCoreScopeProvider.CreateScope(scopeFileSystems: true))
        {
            using (var ms = new MemoryStream("foo"u8.ToArray()))
            {
                mediaFileManager.FileSystem.AddFile("f1.txt", ms);
            }

            Assert.IsTrue(mediaFileManager.FileSystem.FileExists("f1.txt"));
            Assert.IsFalse(physMediaFileSystem.FileExists("f1.txt"));

            // execute on another disconnected thread (execution context will not flow)
            var t = taskHelper.ExecuteBackgroundTask(() =>
            {
                Assert.IsFalse(mediaFileManager.FileSystem.FileExists("f1.txt"));

                using (var ms = new MemoryStream("foo"u8.ToArray()))
                {
                    mediaFileManager.FileSystem.AddFile("f2.txt", ms);
                }

                Assert.IsTrue(mediaFileManager.FileSystem.FileExists("f2.txt"));
                Assert.IsTrue(physMediaFileSystem.FileExists("f2.txt"));

                return Task.CompletedTask;
            });

            t.Wait();

            Assert.IsTrue(mediaFileManager.FileSystem.FileExists("f2.txt"));
            Assert.IsTrue(physMediaFileSystem.FileExists("f2.txt"));
        }
    }

    [Test]
    public void SingleShadow()
    {
        var taskHelper = new TaskHelper(Mock.Of<ILogger<TaskHelper>>());
        var isThrown = false;
        using (EfCoreScopeProvider.CreateScope(scopeFileSystems: true))
        {
            // This is testing when another thread concurrently tries to create a scoped file system
            // because at the moment we don't support concurrent scoped filesystems.
            var t = taskHelper.ExecuteBackgroundTask(() =>
            {
                // ok to create a 'normal' other scope
                using (var other = EfCoreScopeProvider.CreateScope())
                {
                    other.Complete();
                }

                // not ok to create a 'scoped filesystems' other scope
                // we will get a "Already shadowing." exception.
                Assert.Throws<InvalidOperationException>(() =>
                {
                    using var other = EfCoreScopeProvider.CreateScope(scopeFileSystems: true);
                });

                isThrown = true;

                return Task.CompletedTask;
            });

            t.Wait();
        }

        Assert.IsTrue(isThrown);
    }

    [Test]
    public void SingleShadowEvenDetached()
    {
        var taskHelper = new TaskHelper(Mock.Of<ILogger<TaskHelper>>());
        using (var scope = EfCoreScopeProvider.CreateScope(scopeFileSystems: true))
        {
            // This is testing when another thread concurrently tries to create a scoped file system
            // because at the moment we don't support concurrent scoped filesystems.
            var t = taskHelper.ExecuteBackgroundTask(() =>
            {
                // not ok to create a 'scoped filesystems' other scope
                // because at the moment we don't support concurrent scoped filesystems
                // even a detached one
                // we will get a "Already shadowing." exception.
                Assert.Throws<InvalidOperationException>(() =>
                {
                    using var other = EfCoreScopeProvider.CreateDetachedScope(scopeFileSystems: true);
                });

                return Task.CompletedTask;
            });

            t.Wait();
        }

        var detached = EfCoreScopeProvider.CreateDetachedScope(scopeFileSystems: true);

        Assert.IsNull(EfCoreScopeAccessor.AmbientScope);

        Assert.Throws<InvalidOperationException>(() =>
        {
            // even if there is no ambient scope, there's a single shadow
            using var other = EfCoreScopeProvider.CreateScope(scopeFileSystems: true);
        });

        EfCoreScopeProvider.AttachScope(detached);
        detached.Dispose();
    }
}
