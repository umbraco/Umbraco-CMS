using System;
using System.IO;
using System.Text;
using LightInject;
using Moq;
using NUnit.Framework;
using Umbraco.Core;
using Umbraco.Core.Configuration.UmbracoSettings;
using Umbraco.Core.Composing;
using Umbraco.Core.IO;
using Umbraco.Core.IO.MediaPathSchemes;
using Umbraco.Core.Logging;
using Umbraco.Core.Services;
using Umbraco.Tests.TestHelpers;

namespace Umbraco.Tests.IO
{
    [TestFixture]
    public class FileSystemsTests
    {
        private ServiceContainer _container;

        [SetUp]
        public void Setup()
        {
            //init the config singleton
            var config = SettingsForTests.GetDefaultUmbracoSettings();
            SettingsForTests.ConfigureSettings(config);

            _container = new ServiceContainer();
            _container.ConfigureUmbracoCore();
            _container.Register(_ => Mock.Of<ILogger>());
            _container.Register<FileSystems>();
            _container.Register(_ => Mock.Of<IDataTypeService>());
            _container.Register(_ => Mock.Of<IContentSection>());
            _container.RegisterSingleton<IMediaPathScheme, OriginalMediaPathScheme>();

            // make sure we start clean
            // because some tests will create corrupt or weird filesystems
            FileSystems.Reset();
        }

        [TearDown]
        public void TearDown()
        {
            // stay clean (see note in Setup)
            FileSystems.Reset();

            Current.Reset();
            _container.Dispose();
        }

        private FileSystems FileSystems => _container.GetInstance<FileSystems>();

        [Test]
        public void Can_Get_Base_File_System()
        {
            var fileSystem = FileSystems.GetUnderlyingFileSystemProvider("media");

            Assert.NotNull(fileSystem);
        }

        [Test]
        public void Can_Get_Typed_File_System()
        {
            var fileSystem = FileSystems.GetFileSystemProvider<MediaFileSystem>();

            Assert.NotNull(fileSystem);
        }

        [Test]
        public void Media_Fs_Safe_Delete()
        {
            var fs = FileSystems.GetFileSystemProvider<MediaFileSystem>();
            var ms = new MemoryStream(Encoding.UTF8.GetBytes("test"));
            var virtPath = fs.GetMediaPath("file.txt", Guid.NewGuid(), Guid.NewGuid());
            fs.AddFile(virtPath, ms);

            // ~/media/1234/file.txt exists
            var physPath = IOHelper.MapPath(Path.Combine("media", virtPath));
            Assert.IsTrue(File.Exists(physPath));

            // ~/media/1234/file.txt is gone
            fs.DeleteMediaFiles(new[] { virtPath });
            Assert.IsFalse(File.Exists(physPath));

            // ~/media/1234 is gone
            physPath = Path.GetDirectoryName(physPath);
            Assert.IsFalse(Directory.Exists(physPath));

            // ~/media exists
            physPath = Path.GetDirectoryName(physPath);
            Assert.IsTrue(Directory.Exists(physPath));
        }

        public void Singleton_Typed_File_System()
        {
            var fs1 = FileSystems.GetFileSystemProvider<MediaFileSystem>();
            var fs2 = FileSystems.GetFileSystemProvider<MediaFileSystem>();

            Assert.AreSame(fs1, fs2);
        }

        [Test]
        public void Exception_Thrown_On_Invalid_Typed_File_System()
        {
            Assert.Throws<InvalidOperationException>(() => FileSystems.GetFileSystemProvider<InvalidTypedFileSystem>());
        }

        [Test]
        public void Exception_Thrown_On_NonConfigured_Typed_File_System()
        {
            // note: we need to reset the manager between tests else the Accept_Fallback test would corrupt that one
            Assert.Throws<ArgumentException>(() => FileSystems.GetFileSystemProvider<NonConfiguredTypeFileSystem>());
        }

        [Test]
        public void Accept_Fallback_On_NonConfigured_Typed_File_System()
        {
            var fs = FileSystems.GetFileSystemProvider<NonConfiguredTypeFileSystem>(() => new PhysicalFileSystem("~/App_Data/foo"));

            Assert.NotNull(fs);
        }

        /// <summary>
        /// Used in unit tests, for a typed file system we need to inherit from FileSystemWrapper and they MUST have a ctor
        /// that only accepts a base IFileSystem object
        /// </summary>
        internal class InvalidTypedFileSystem : FileSystemWrapper
        {
            public InvalidTypedFileSystem(IFileSystem wrapped, string invalidParam)
                : base(wrapped)
            { }
        }

        [FileSystemProvider("noconfig")]
        internal class NonConfiguredTypeFileSystem : FileSystemWrapper
        {
            public NonConfiguredTypeFileSystem(IFileSystem wrapped)
                : base(wrapped)
            { }
        }
    }
}
