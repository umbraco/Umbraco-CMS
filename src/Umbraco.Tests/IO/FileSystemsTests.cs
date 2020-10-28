using System;
using System.IO;
using System.Text;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using NUnit.Framework;
using Umbraco.Core;
using Umbraco.Core.Cache;
using Umbraco.Core.Composing;
using Umbraco.Core.Composing.CompositionExtensions;
using Umbraco.Core.Configuration.Models;
using Umbraco.Core.IO;
using Umbraco.Core.IO.MediaPathSchemes;
using Umbraco.Core.Logging;
using Umbraco.Core.Services;
using Umbraco.Infrastructure.Composing;
using Umbraco.Tests.TestHelpers;
using Umbraco.Web;
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
            _register = TestHelper.GetRegister();

            var composition = new Composition(
                (_register as ServiceCollectionRegistryAdapter).Services,
                TestHelper.GetMockedTypeLoader(),
                Mock.Of<IProfilingLogger>(),
                Mock.Of<IRuntimeState>(),
                TestHelper.IOHelper,
                AppCaches.NoCache
            );

            composition.Register(_ => Mock.Of<IDataTypeService>());
            composition.Services.AddTransient<ILoggerFactory, NullLoggerFactory>();
            composition.Register(typeof(ILogger<>), typeof(Logger<>));
            composition.Register(_ => TestHelper.ShortStringHelper);
            composition.Register(_ => TestHelper.IOHelper);
            composition.RegisterUnique<IMediaPathScheme, UniqueMediaPathScheme>();
            composition.RegisterUnique(TestHelper.IOHelper);
            composition.RegisterUnique(TestHelper.GetHostingEnvironment());

            var globalSettings = new GlobalSettings();
            composition.Register(x => Microsoft.Extensions.Options.Options.Create(globalSettings));


            composition.ComposeFileSystems();

            _factory = composition.CreateFactory();

            // make sure we start clean
            // because some tests will create corrupt or weird filesystems
            FileSystems.Reset();
        }

        [TearDown]
        public void TearDown()
        {
            // stay clean (see note in Setup)
            FileSystems.Reset();

            _register.DisposeIfDisposable();
        }

        private FileSystems FileSystems => _factory.GetRequiredService<FileSystems>();

        [Test]
        public void Can_Get_MediaFileSystem()
        {
            var fileSystem = _factory.GetRequiredService<IMediaFileSystem>();
            Assert.NotNull(fileSystem);
        }

        [Test]
        public void Can_Get_IMediaFileSystem()
        {
            var fileSystem = _factory.GetRequiredService<IMediaFileSystem>();
            Assert.NotNull(fileSystem);
        }

        [Test]
        public void IMediaFileSystem_Is_Singleton()
        {
            var fileSystem1 = _factory.GetRequiredService<IMediaFileSystem>();
            var fileSystem2 = _factory.GetRequiredService<IMediaFileSystem>();
            Assert.AreSame(fileSystem1, fileSystem2);
        }

        [Test]
        public void Can_Unwrap_MediaFileSystem()
        {
            var fileSystem = _factory.GetRequiredService<IMediaFileSystem>();
            var unwrapped = fileSystem.Unwrap();
            Assert.IsNotNull(unwrapped);
            var physical = unwrapped as PhysicalFileSystem;
            Assert.IsNotNull(physical);
        }

        [Test]
        public void Can_Delete_MediaFiles()
        {
            var fs = _factory.GetRequiredService<IMediaFileSystem>();
            var ms = new MemoryStream(Encoding.UTF8.GetBytes("test"));
            var virtPath = fs.GetMediaPath("file.txt", Guid.NewGuid(), Guid.NewGuid());
            fs.AddFile(virtPath, ms);

            // ~/media/1234/file.txt exists
            var ioHelper = _factory.GetRequiredService<IIOHelper>();
            var physPath = ioHelper.MapPath(Path.Combine("media", virtPath));
            Assert.IsTrue(File.Exists(physPath));

            // ~/media/1234/file.txt is gone
            fs.DeleteMediaFiles(new[] { virtPath });
            Assert.IsFalse(File.Exists(physPath));

            var scheme = _factory.GetRequiredService<IMediaPathScheme>();
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
