// Copyright (c) Umbraco.
// See LICENSE for more details.

using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Hosting;
using Umbraco.Cms.Core.IO;
using Umbraco.Cms.Infrastructure.Scoping;
using Umbraco.Cms.Tests.Common;
using Umbraco.Cms.Tests.Common.Testing;
using Umbraco.Cms.Tests.Integration.Testing;
using Umbraco.Extensions;

namespace Umbraco.Cms.Tests.Integration.Umbraco.Infrastructure.Scoping;

[TestFixture]
[UmbracoTest(Database = UmbracoTestOptions.Database.NewEmptyPerTest)]
public class ScopeFileSystemsTests : UmbracoIntegrationTest
{
    [SetUp]
    public void SetUp() => ClearFiles(IOHelper);

    [TearDown]
    public void Teardown() => ClearFiles(IOHelper);

    private MediaFileManager MediaFileManager => GetRequiredService<MediaFileManager>();

    private IHostingEnvironment HostingEnvironment => GetRequiredService<IHostingEnvironment>();

    private void ClearFiles(IIOHelper ioHelper)
    {
        TestHelper.DeleteDirectory(ioHelper.MapPath("media"));
        TestHelper.DeleteDirectory(ioHelper.MapPath("FileSysTests"));
        TestHelper.DeleteDirectory(ioHelper.MapPath(Constants.SystemDirectories.TempData.EnsureEndsWith('/') +
                                                    "ShadowFs"));
    }

    [Test]
    public void MediaFileManager_does_not_write_to_physical_file_system_when_scoped_if_scope_does_not_complete()
    {
        var rootPath = HostingEnvironment.MapPathWebRoot(GlobalSettings.UmbracoMediaPhysicalRootPath);
        var rootUrl = HostingEnvironment.ToAbsolute(GlobalSettings.UmbracoMediaPath);
        var physMediaFileSystem = new PhysicalFileSystem(IOHelper, HostingEnvironment, GetRequiredService<ILogger<PhysicalFileSystem>>(), rootPath, rootUrl);
        var mediaFileManager = MediaFileManager;

        Assert.IsFalse(physMediaFileSystem.FileExists("f1.txt"));

        using (ScopeProvider.CreateScope(scopeFileSystems: true))
        {
            using (var ms = new MemoryStream(Encoding.UTF8.GetBytes("foo")))
            {
                MediaFileManager.FileSystem.AddFile("f1.txt", ms);
            }

            Assert.IsTrue(mediaFileManager.FileSystem.FileExists("f1.txt"));
            Assert.IsFalse(physMediaFileSystem.FileExists("f1.txt"));

            Assert.IsTrue(mediaFileManager.FileSystem.FileExists("f1.txt"));
            Assert.IsFalse(physMediaFileSystem.FileExists("f1.txt"));
        }

        // After scope is disposed ensure shadow wrapper didn't commit to physical
        Assert.IsFalse(mediaFileManager.FileSystem.FileExists("f1.txt"));
        Assert.IsFalse(physMediaFileSystem.FileExists("f1.txt"));
    }

    [Test]
    public void MediaFileManager_writes_to_physical_file_system_when_scoped_and_scope_is_completed()
    {
        var rootPath = HostingEnvironment.MapPathWebRoot(GlobalSettings.UmbracoMediaPhysicalRootPath);
        var rootUrl = HostingEnvironment.ToAbsolute(GlobalSettings.UmbracoMediaPath);
        var physMediaFileSystem = new PhysicalFileSystem(IOHelper, HostingEnvironment, GetRequiredService<ILogger<PhysicalFileSystem>>(), rootPath, rootUrl);
        var mediaFileManager = MediaFileManager;

        Assert.IsFalse(physMediaFileSystem.FileExists("f1.txt"));

        using (var scope = ScopeProvider.CreateScope(scopeFileSystems: true))
        {
            using (var ms = new MemoryStream(Encoding.UTF8.GetBytes("foo")))
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
        var physMediaFileSystem = new PhysicalFileSystem(IOHelper, HostingEnvironment, GetRequiredService<ILogger<PhysicalFileSystem>>(), rootPath, rootUrl);
        var mediaFileManager = MediaFileManager;
        var taskHelper = new TaskHelper(Mock.Of<ILogger<TaskHelper>>());

        var scopeProvider = ScopeProvider;
        using (var scope = scopeProvider.CreateScope(scopeFileSystems: true))
        {
            using (var ms = new MemoryStream(Encoding.UTF8.GetBytes("foo")))
            {
                mediaFileManager.FileSystem.AddFile("f1.txt", ms);
            }

            Assert.IsTrue(mediaFileManager.FileSystem.FileExists("f1.txt"));
            Assert.IsFalse(physMediaFileSystem.FileExists("f1.txt"));

            // execute on another disconnected thread (execution context will not flow)
            var t = taskHelper.ExecuteBackgroundTask(() =>
            {
                Assert.IsFalse(mediaFileManager.FileSystem.FileExists("f1.txt"));

                using (var ms = new MemoryStream(Encoding.UTF8.GetBytes("foo")))
                {
                    mediaFileManager.FileSystem.AddFile("f2.txt", ms);
                }

                Assert.IsTrue(mediaFileManager.FileSystem.FileExists("f2.txt"));
                Assert.IsTrue(physMediaFileSystem.FileExists("f2.txt"));

                return Task.CompletedTask;
            });

            Task.WaitAll(t);

            Assert.IsTrue(mediaFileManager.FileSystem.FileExists("f2.txt"));
            Assert.IsTrue(physMediaFileSystem.FileExists("f2.txt"));
        }
    }

    [Test]
    public void SingleShadow()
    {
        var taskHelper = new TaskHelper(Mock.Of<ILogger<TaskHelper>>());
        var scopeProvider = ScopeProvider;
        var isThrown = false;
        using (var scope = scopeProvider.CreateScope(scopeFileSystems: true))
        {
            // This is testing when another thread concurrently tries to create a scoped file system
            // because at the moment we don't support concurrent scoped filesystems.
            var t = taskHelper.ExecuteBackgroundTask(() =>
            {
                // ok to create a 'normal' other scope
                using (var other = scopeProvider.CreateScope())
                {
                    other.Complete();
                }

                // not ok to create a 'scoped filesystems' other scope
                // we will get a "Already shadowing." exception.
                Assert.Throws<InvalidOperationException>(() =>
                {
                    using var other = scopeProvider.CreateScope(scopeFileSystems: true);
                });

                isThrown = true;

                return Task.CompletedTask;
            });

            Task.WaitAll(t);
        }

        Assert.IsTrue(isThrown);
    }

    [Test]
    public void SingleShadowEvenDetached()
    {
        var taskHelper = new TaskHelper(Mock.Of<ILogger<TaskHelper>>());
        var scopeProvider = (ScopeProvider)ScopeProvider;
        using (var scope = scopeProvider.CreateScope(scopeFileSystems: true))
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
                    using var other = scopeProvider.CreateDetachedScope(scopeFileSystems: true);
                });

                return Task.CompletedTask;
            });

            Task.WaitAll(t);
        }

        var detached = scopeProvider.CreateDetachedScope(scopeFileSystems: true);

        Assert.IsNull(scopeProvider.AmbientScope);

        Assert.Throws<InvalidOperationException>(() =>
        {
            // even if there is no ambient scope, there's a single shadow
            using var other = scopeProvider.CreateScope(scopeFileSystems: true);
        });

        scopeProvider.AttachScope(detached);
        detached.Dispose();
    }
}
