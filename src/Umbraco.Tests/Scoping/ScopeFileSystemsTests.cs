using System;
using System.IO;
using System.Text;
using NUnit.Framework;
using Umbraco.Core;
using Umbraco.Core.IO;
using Umbraco.Core.Scoping;
using Umbraco.Tests.TestHelpers;

namespace Umbraco.Tests.Scoping
{
    [TestFixture]
    [DatabaseTestBehavior(DatabaseBehavior.EmptyDbFilePerTest)]
    public class ScopeFileSystemsTests : BaseDatabaseFactoryTest
    {
        [SetUp]
        public override void Initialize()
        {
            base.Initialize();

            SafeCallContext.Clear();
            ClearFiles();
        }

        [TearDown]
        public override void TearDown()
        {
            base.TearDown();
            SafeCallContext.Clear();
            ClearFiles();
        }

        private static void ClearFiles()
        {
            TestHelper.DeleteDirectory(IOHelper.MapPath("media"));
            TestHelper.DeleteDirectory(IOHelper.MapPath("FileSysTests"));
            TestHelper.DeleteDirectory(IOHelper.MapPath("App_Data"));
        }

        [TestCase(true)]
        [TestCase(false)]
        public void CreateMediaTest(bool complete)
        {
            var physMediaFileSystem = new PhysicalFileSystem(IOHelper.MapPath("media"), "ignore");
            var mediaFileSystem = FileSystemProviderManager.Current.MediaFileSystem;

            Assert.IsFalse(physMediaFileSystem.FileExists("f1.txt"));

            var scopeProvider = ApplicationContext.ScopeProvider;
            using (var scope = scopeProvider.CreateScope(scopeFileSystems: true))
            {
                using (var ms = new MemoryStream(Encoding.UTF8.GetBytes("foo")))
                    mediaFileSystem.AddFile("f1.txt", ms);
                Assert.IsTrue(mediaFileSystem.FileExists("f1.txt"));
                Assert.IsFalse(physMediaFileSystem.FileExists("f1.txt"));
                if (complete)
                    scope.Complete();
                Assert.IsTrue(mediaFileSystem.FileExists("f1.txt"));
                Assert.IsFalse(physMediaFileSystem.FileExists("f1.txt"));
            }

            if (complete)
            {
                Assert.IsTrue(FileSystemProviderManager.Current.MediaFileSystem.FileExists("f1.txt"));
                Assert.IsTrue(physMediaFileSystem.FileExists("f1.txt"));
            }
            else
            {
                Assert.IsFalse(FileSystemProviderManager.Current.MediaFileSystem.FileExists("f1.txt"));
                Assert.IsFalse(physMediaFileSystem.FileExists("f1.txt"));
            }
        }

        [Test]
        public void MultiThread()
        {
            var physMediaFileSystem = new PhysicalFileSystem(IOHelper.MapPath("media"), "ignore");
            var mediaFileSystem = FileSystemProviderManager.Current.MediaFileSystem;

            var scopeProvider = ApplicationContext.ScopeProvider;
            using (var scope = scopeProvider.CreateScope(scopeFileSystems: true))
            {
                using (var ms = new MemoryStream(Encoding.UTF8.GetBytes("foo")))
                    mediaFileSystem.AddFile("f1.txt", ms);
                Assert.IsTrue(mediaFileSystem.FileExists("f1.txt"));
                Assert.IsFalse(physMediaFileSystem.FileExists("f1.txt"));

                using (new SafeCallContext())
                {
                    Assert.IsFalse(mediaFileSystem.FileExists("f1.txt"));

                    using (var ms = new MemoryStream(Encoding.UTF8.GetBytes("foo")))
                        mediaFileSystem.AddFile("f2.txt", ms);
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
            var scopeProvider = ApplicationContext.ScopeProvider;
            using (var scope = scopeProvider.CreateScope(scopeFileSystems: true))
            {
                using (new SafeCallContext()) // not nesting!
                {
                    // ok to create a 'normal' other scope
                    using (var other = scopeProvider.CreateScope())
                    {
                        other.Complete();
                    }

                    // not ok to create a 'scoped filesystems' other scope
                    // because at the moment we don't support concurrent scoped filesystems
                    Assert.Throws<InvalidOperationException>(() =>
                    {
                        var other = scopeProvider.CreateScope(scopeFileSystems: true);
                    });
                }
            }
        }

        [Test]
        public void SingleShadowEvenDetached()
        {
            var scopeProvider = ApplicationContext.ScopeProvider as IScopeProviderInternal;
            using (var scope = scopeProvider.CreateScope(scopeFileSystems: true))
            {
                using (new SafeCallContext()) // not nesting!
                {
                    // not ok to create a 'scoped filesystems' other scope
                    // because at the moment we don't support concurrent scoped filesystems
                    // even a detached one
                    Assert.Throws<InvalidOperationException>(() =>
                    {
                        var other = scopeProvider.CreateDetachedScope(scopeFileSystems: true);
                    });
                }
            }

            var detached = scopeProvider.CreateDetachedScope(scopeFileSystems: true);

            Assert.IsNull(scopeProvider.AmbientScope);

            Assert.Throws<InvalidOperationException>(() =>
            {
                // even if there is no ambient scope, there's a single shadow
                using (var other = scopeProvider.CreateScope(scopeFileSystems: true))
                { }
            });

            scopeProvider.AttachScope(detached);
            detached.Dispose();
        }
    }
}
