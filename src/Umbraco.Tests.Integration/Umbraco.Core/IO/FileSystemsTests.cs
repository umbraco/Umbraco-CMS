// Copyright (c) Umbraco.
// See LICENSE for more details.

using System;
using System.IO;
using System.Text;
using NUnit.Framework;
using Umbraco.Cms.Core.Hosting;
using Umbraco.Cms.Core.IO;
using Umbraco.Cms.Core.IO.MediaPathSchemes;
using Umbraco.Extensions;
using Umbraco.Tests.Integration.Testing;
using Umbraco.Tests.Testing;

namespace Umbraco.Tests.IO
{
    [TestFixture]
    [UmbracoTest]
    public class FileSystemsTests : UmbracoIntegrationTest
    {
        [Test]
        public void Can_Get_MediaFileSystem()
        {
            IMediaFileSystem fileSystem = GetRequiredService<IMediaFileSystem>();
            Assert.NotNull(fileSystem);
        }

        [Test]
        public void Can_Get_IMediaFileSystem()
        {
            IMediaFileSystem fileSystem = GetRequiredService<IMediaFileSystem>();
            Assert.NotNull(fileSystem);
        }

        [Test]
        public void IMediaFileSystem_Is_Singleton()
        {
            IMediaFileSystem fileSystem1 = GetRequiredService<IMediaFileSystem>();
            IMediaFileSystem fileSystem2 = GetRequiredService<IMediaFileSystem>();
            Assert.AreSame(fileSystem1, fileSystem2);
        }

        [Test]
        public void Can_Unwrap_MediaFileSystem()
        {
            IMediaFileSystem fileSystem = GetRequiredService<IMediaFileSystem>();
            IFileSystem unwrapped = fileSystem.Unwrap();
            Assert.IsNotNull(unwrapped);
            var physical = unwrapped as PhysicalFileSystem;
            Assert.IsNotNull(physical);
        }

        [Test]
        public void Can_Delete_MediaFiles()
        {
            IMediaFileSystem fs = GetRequiredService<IMediaFileSystem>();
            var ms = new MemoryStream(Encoding.UTF8.GetBytes("test"));
            string virtPath = fs.GetMediaPath("file.txt", Guid.NewGuid(), Guid.NewGuid());
            fs.AddFile(virtPath, ms);

            // ~/media/1234/file.txt exists
            IHostingEnvironment hostingEnvironment = GetRequiredService<IHostingEnvironment>();
            string physPath = hostingEnvironment.MapPathWebRoot(Path.Combine("media", virtPath));
            Assert.IsTrue(File.Exists(physPath));

            // ~/media/1234/file.txt is gone
            fs.DeleteMediaFiles(new[] { virtPath });
            Assert.IsFalse(File.Exists(physPath));

            IMediaPathScheme scheme = GetRequiredService<IMediaPathScheme>();
            if (scheme is UniqueMediaPathScheme)
            {
                // ~/media/1234 is *not* gone
                physPath = Path.GetDirectoryName(physPath);
                Assert.IsTrue(Directory.Exists(physPath));
            }
            else
            {
                // ~/media/1234 is gone
                physPath = Path.GetDirectoryName(physPath);
                Assert.IsFalse(Directory.Exists(physPath));
            }

            // ~/media exists
            physPath = Path.GetDirectoryName(physPath);
            Assert.IsTrue(Directory.Exists(physPath));
        }

        // FIXME: don't make sense anymore
        /*
        [Test]
        public void Cannot_Get_InvalidFileSystem()
        {
            // throws because InvalidTypedFileSystem does not have the proper attribute with an alias
            Assert.Throws<InvalidOperationException>(() => FileSystems.GetFileSystem<InvalidFileSystem>());
        }

        [Test]
        public void Cannot_Get_NonConfiguredFileSystem()
        {
            // note: we need to reset the manager between tests else the Accept_Fallback test would corrupt that one
            // throws because NonConfiguredFileSystem has the proper attribute with an alias,
            // but then the container cannot find an IFileSystem implementation for that alias
            Assert.Throws<InvalidOperationException>(() => FileSystems.GetFileSystem<NonConfiguredFileSystem>());

            // all we'd need to pass is to register something like:
            //_container.Register<IFileSystem>("noconfig", factory => new PhysicalFileSystem("~/foo"));
        }

        internal class InvalidFileSystem : FileSystemWrapper
        {
            public InvalidFileSystem(IFileSystem innerFileSystem)
                : base(innerFileSystem)
            { }
        }

        [InnerFileSystem("noconfig")]
        internal class NonConfiguredFileSystem : FileSystemWrapper
        {
            public NonConfiguredFileSystem(IFileSystem innerFileSystem)
                : base(innerFileSystem)
            { }
        }
        */
    }
}
