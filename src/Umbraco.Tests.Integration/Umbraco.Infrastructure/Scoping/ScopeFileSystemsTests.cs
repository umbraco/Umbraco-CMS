// Copyright (c) Umbraco.
// See LICENSE for more details.

using System;
using System.IO;
using System.Text;
using Microsoft.Extensions.Logging;
using NUnit.Framework;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Hosting;
using Umbraco.Cms.Core.IO;
using Umbraco.Core.Scoping;
using Umbraco.Extensions;
using Umbraco.Tests.Integration.Testing;
using Umbraco.Tests.Testing;
using Constants = Umbraco.Cms.Core.Constants;
using FileSystems = Umbraco.Cms.Core.IO.FileSystems;

namespace Umbraco.Tests.Integration.Umbraco.Infrastructure.Scoping
{
    [TestFixture]
    [UmbracoTest(Database = UmbracoTestOptions.Database.NewEmptyPerTest)]
    public class ScopeFileSystemsTests : UmbracoIntegrationTest
    {
        private IMediaFileSystem MediaFileSystem => GetRequiredService<IMediaFileSystem>();

        private IHostingEnvironment HostingEnvironment => GetRequiredService<IHostingEnvironment>();

        [SetUp]
        public void SetUp()
        {
            SafeCallContext.Clear();
            ClearFiles(IOHelper);
        }

        [TearDown]
        public void Teardown()
        {
            SafeCallContext.Clear();
            FileSystems.ResetShadowId();
            ClearFiles(IOHelper);
        }

        private void ClearFiles(IIOHelper ioHelper)
        {
            TestHelper.DeleteDirectory(ioHelper.MapPath("media"));
            TestHelper.DeleteDirectory(ioHelper.MapPath("FileSysTests"));
            TestHelper.DeleteDirectory(ioHelper.MapPath(Constants.SystemDirectories.TempData.EnsureEndsWith('/') + "ShadowFs"));
        }

        [Test]
        public void MediaFileSystem_does_not_write_to_physical_file_system_when_scoped_if_scope_does_not_complete()
        {
            string rootPath = HostingEnvironment.MapPathWebRoot(GlobalSettings.UmbracoMediaPath);
            string rootUrl = HostingEnvironment.ToAbsolute(GlobalSettings.UmbracoMediaPath);
            var physMediaFileSystem = new PhysicalFileSystem(IOHelper, HostingEnvironment, GetRequiredService<ILogger<PhysicalFileSystem>>(), rootPath, rootUrl);
            IMediaFileSystem mediaFileSystem = MediaFileSystem;

            Assert.IsFalse(physMediaFileSystem.FileExists("f1.txt"));

            using (ScopeProvider.CreateScope(scopeFileSystems: true))
            {
                using (var ms = new MemoryStream(Encoding.UTF8.GetBytes("foo")))
                {
                    mediaFileSystem.AddFile("f1.txt", ms);
                }

                Assert.IsTrue(mediaFileSystem.FileExists("f1.txt"));
                Assert.IsFalse(physMediaFileSystem.FileExists("f1.txt"));

                Assert.IsTrue(mediaFileSystem.FileExists("f1.txt"));
                Assert.IsFalse(physMediaFileSystem.FileExists("f1.txt"));
            }

            // After scope is disposed ensure shadow wrapper didn't commit to physical
            Assert.IsFalse(MediaFileSystem.FileExists("f1.txt"));
            Assert.IsFalse(physMediaFileSystem.FileExists("f1.txt"));
        }

        [Test]
        public void MediaFileSystem_writes_to_physical_file_system_when_scoped_and_scope_is_completed()
        {
            string rootPath = HostingEnvironment.MapPathWebRoot(GlobalSettings.UmbracoMediaPath);
            string rootUrl = HostingEnvironment.ToAbsolute(GlobalSettings.UmbracoMediaPath);
            var physMediaFileSystem = new PhysicalFileSystem(IOHelper, HostingEnvironment, GetRequiredService<ILogger<PhysicalFileSystem>>(), rootPath, rootUrl);
            IMediaFileSystem mediaFileSystem = MediaFileSystem;

            Assert.IsFalse(physMediaFileSystem.FileExists("f1.txt"));

            using (IScope scope = ScopeProvider.CreateScope(scopeFileSystems: true))
            {
                using (var ms = new MemoryStream(Encoding.UTF8.GetBytes("foo")))
                {
                    mediaFileSystem.AddFile("f1.txt", ms);
                }

                Assert.IsTrue(mediaFileSystem.FileExists("f1.txt"));
                Assert.IsFalse(physMediaFileSystem.FileExists("f1.txt"));

                scope.Complete();

                Assert.IsTrue(mediaFileSystem.FileExists("f1.txt"));
                Assert.IsFalse(physMediaFileSystem.FileExists("f1.txt"));
            }

            // After scope is disposed ensure shadow wrapper writes to physical file system
            Assert.IsTrue(MediaFileSystem.FileExists("f1.txt"));
            Assert.IsTrue(physMediaFileSystem.FileExists("f1.txt"));
        }

        [Test]
        public void MultiThread()
        {
            string rootPath = HostingEnvironment.MapPathWebRoot(GlobalSettings.UmbracoMediaPath);
            string rootUrl = HostingEnvironment.ToAbsolute(GlobalSettings.UmbracoMediaPath);
            var physMediaFileSystem = new PhysicalFileSystem(IOHelper, HostingEnvironment, GetRequiredService<ILogger<PhysicalFileSystem>>(), rootPath, rootUrl);
            IMediaFileSystem mediaFileSystem = MediaFileSystem;

            IScopeProvider scopeProvider = ScopeProvider;
            using (IScope scope = scopeProvider.CreateScope(scopeFileSystems: true))
            {
                using (var ms = new MemoryStream(Encoding.UTF8.GetBytes("foo")))
                {
                    mediaFileSystem.AddFile("f1.txt", ms);
                }

                Assert.IsTrue(mediaFileSystem.FileExists("f1.txt"));
                Assert.IsFalse(physMediaFileSystem.FileExists("f1.txt"));

                using (new SafeCallContext())
                {
                    Assert.IsFalse(mediaFileSystem.FileExists("f1.txt"));

                    using (var ms = new MemoryStream(Encoding.UTF8.GetBytes("foo")))
                    {
                        mediaFileSystem.AddFile("f2.txt", ms);
                    }

                    Assert.IsTrue(mediaFileSystem.FileExists("f2.txt"));
                    Assert.IsTrue(physMediaFileSystem.FileExists("f2.txt"));
                }

                Assert.IsTrue(mediaFileSystem.FileExists("f2.txt"));
                Assert.IsTrue(physMediaFileSystem.FileExists("f2.txt"));
            }
        }

        [Test]
        public void SingleShadow()
        {
            IScopeProvider scopeProvider = ScopeProvider;
            using (IScope scope = scopeProvider.CreateScope(scopeFileSystems: true))
            {
                using (new SafeCallContext()) // not nesting!
                {
                    // ok to create a 'normal' other scope
                    using (IScope other = scopeProvider.CreateScope())
                    {
                        other.Complete();
                    }

                    // not ok to create a 'scoped filesystems' other scope
                    // because at the moment we don't support concurrent scoped filesystems
                    Assert.Throws<InvalidOperationException>(() =>
                    {
                        IScope other = scopeProvider.CreateScope(scopeFileSystems: true);
                    });
                }
            }
        }

        [Test]
        public void SingleShadowEvenDetached()
        {
            var scopeProvider = (ScopeProvider)ScopeProvider;
            using (IScope scope = scopeProvider.CreateScope(scopeFileSystems: true))
            {
                using (new SafeCallContext()) // not nesting!
                {
                    // not ok to create a 'scoped filesystems' other scope
                    // because at the moment we don't support concurrent scoped filesystems
                    // even a detached one
                    Assert.Throws<InvalidOperationException>(() =>
                    {
                        IScope other = scopeProvider.CreateDetachedScope(scopeFileSystems: true);
                    });
                }
            }

            IScope detached = scopeProvider.CreateDetachedScope(scopeFileSystems: true);

            Assert.IsNull(scopeProvider.AmbientScope);

            Assert.Throws<InvalidOperationException>(() =>
            {
                // even if there is no ambient scope, there's a single shadow
                using (IScope other = scopeProvider.CreateScope(scopeFileSystems: true))
                {
                }
            });

            scopeProvider.AttachScope(detached);
            detached.Dispose();
        }
    }
}
