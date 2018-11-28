using System;
using System.IO;
using System.Text;
using Moq;
using NUnit.Framework;
using Umbraco.Core;
using Umbraco.Core.Cache;
using Umbraco.Core.Components;
using Umbraco.Core.Configuration.UmbracoSettings;
using Umbraco.Core.Composing;
using Umbraco.Core.Composing.Composers;
using Umbraco.Core.Configuration;
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
        private IContainer _container;

        [SetUp]
        public void Setup()
        {
            //init the config singleton
            var config = SettingsForTests.GetDefaultUmbracoSettings();
            SettingsForTests.ConfigureSettings(config);

            _container = ContainerFactory.Create();
            Current.Factory = _container;
            var composition = new Composition(_container, new TypeLoader(), Mock.Of<IProfilingLogger>(), RuntimeLevel.Run);

            _container.Register(_ => Mock.Of<ILogger>());
            _container.Register(_ => Mock.Of<IDataTypeService>());
            _container.Register(_ => Mock.Of<IContentSection>());
            _container.RegisterSingleton<IMediaPathScheme, OriginalMediaPathScheme>();

            composition.ComposeFileSystems();

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
        public void Can_Get_MediaFileSystem()
        {
            var fileSystem = _container.GetInstance<IMediaFileSystem>();
            Assert.NotNull(fileSystem);
        }

        [Test]
        public void Can_Get_IMediaFileSystem()
        {
            var fileSystem = _container.GetInstance<IMediaFileSystem>();
            Assert.NotNull(fileSystem);
        }

        [Test]
        public void IMediaFileSystem_Is_Singleton()
        {
            var fileSystem1 = _container.GetInstance<IMediaFileSystem>();
            var fileSystem2 = _container.GetInstance<IMediaFileSystem>();
            Assert.AreSame(fileSystem1, fileSystem2);
        }

        [Test]
        public void Can_Delete_MediaFiles()
        {
            var fs = _container.GetInstance<IMediaFileSystem>();
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


        // fixme - don't make sense anymore
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
