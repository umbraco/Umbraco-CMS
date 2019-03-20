﻿using System;
using System.IO;
using System.Text;
using Moq;
using NUnit.Framework;
using Umbraco.Core;
using Umbraco.Core.Configuration.UmbracoSettings;
using Umbraco.Core.Composing;
using Umbraco.Core.IO;
using Umbraco.Core.IO.MediaPathSchemes;
using Umbraco.Core.Logging;
using Umbraco.Core.Services;
using Umbraco.Tests.Components;
using Umbraco.Tests.TestHelpers;
using Umbraco.Core.Composing.CompositionExtensions;
using FileSystems = Umbraco.Core.IO.FileSystems;

namespace Umbraco.Tests.IO
{
    [TestFixture]
    public class FileSystemsTests
    {
        private IRegister _register;
        private IFactory _factory;

        [SetUp]
        public void Setup()
        {
            _register = RegisterFactory.Create();

            var composition = new Composition(_register, new TypeLoader(), Mock.Of<IProfilingLogger>(), ComponentTests.MockRuntimeState(RuntimeLevel.Run));

            composition.Register(_ => Mock.Of<ILogger>());
            composition.Register(_ => Mock.Of<IDataTypeService>());
            composition.Register(_ => Mock.Of<IContentSection>());
            composition.RegisterUnique<IMediaPathScheme, UniqueMediaPathScheme>();

            composition.Configs.Add(SettingsForTests.GetDefaultGlobalSettings);
            composition.Configs.Add(SettingsForTests.GetDefaultUmbracoSettings);

            composition.ComposeFileSystems();

            composition.Configs.Add(SettingsForTests.GetDefaultUmbracoSettings);

            _factory = composition.CreateFactory();

            Current.Reset();
            Current.Factory = _factory;

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
            _register.DisposeIfDisposable();
        }

        private FileSystems FileSystems => _factory.GetInstance<FileSystems>();

        [Test]
        public void Can_Get_MediaFileSystem()
        {
            var fileSystem = _factory.GetInstance<IMediaFileSystem>();
            Assert.NotNull(fileSystem);
        }

        [Test]
        public void Can_Get_IMediaFileSystem()
        {
            var fileSystem = _factory.GetInstance<IMediaFileSystem>();
            Assert.NotNull(fileSystem);
        }

        [Test]
        public void IMediaFileSystem_Is_Singleton()
        {
            var fileSystem1 = _factory.GetInstance<IMediaFileSystem>();
            var fileSystem2 = _factory.GetInstance<IMediaFileSystem>();
            Assert.AreSame(fileSystem1, fileSystem2);
        }

        [Test]
        public void Can_Unwrap_MediaFileSystem()
        {
            var fileSystem = _factory.GetInstance<IMediaFileSystem>();
            var unwrapped = fileSystem.Unwrap();
            Assert.IsNotNull(unwrapped);
            var physical = unwrapped as PhysicalFileSystem;
            Assert.IsNotNull(physical);
        }

        [Test]
        public void Can_Delete_MediaFiles()
        {
            var fs = _factory.GetInstance<IMediaFileSystem>();
            var ms = new MemoryStream(Encoding.UTF8.GetBytes("test"));
            var virtPath = fs.GetMediaPath("file.txt", Guid.NewGuid(), Guid.NewGuid());
            fs.AddFile(virtPath, ms);

            // ~/media/1234/file.txt exists
            var physPath = IOHelper.MapPath(Path.Combine("media", virtPath));
            Assert.IsTrue(File.Exists(physPath));

            // ~/media/1234/file.txt is gone
            fs.DeleteMediaFiles(new[] { virtPath });
            Assert.IsFalse(File.Exists(physPath));

            var scheme = Current.Factory.GetInstance<IMediaPathScheme>();
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
