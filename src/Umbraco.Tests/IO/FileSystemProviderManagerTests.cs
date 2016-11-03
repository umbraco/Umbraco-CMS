using System;
using LightInject;
using Moq;
using NUnit.Framework;
using Umbraco.Core;
using Umbraco.Core.Configuration.UmbracoSettings;
using Umbraco.Core.DI;
using Umbraco.Core.IO;
using Umbraco.Core.Logging;
using Umbraco.Core.Services;
using Umbraco.Tests.TestHelpers;

namespace Umbraco.Tests.IO
{
    [TestFixture]
    public class FileSystemProviderManagerTests
    {
        private ServiceContainer _container;

        [SetUp]
        public void Setup()
        {
            //init the config singleton
            var config = SettingsForTests.GetDefault();
            SettingsForTests.ConfigureSettings(config);

            _container = new ServiceContainer();
            _container.ConfigureUmbracoCore();
            _container.Register(_ => Mock.Of<ILogger>());
            _container.Register<FileSystems>();
            _container.Register(_ => Mock.Of<IDataTypeService>());
            _container.Register(_ => Mock.Of<IContentSection>());
        }

        [TearDown]
        public void TearDown()
        {
            Current.Reset();
            _container.Dispose();
        }

        [Test]
        public void Can_Get_Base_File_System()
        {
            var fileSystems = new FileSystems(Mock.Of<ILogger>());
            var fileSystem = fileSystems.GetUnderlyingFileSystemProvider(Constants.IO.MediaFileSystemProvider);

            Assert.NotNull(fileSystem);
        }

        [Test]
        public void Can_Get_Typed_File_System()
        {
            var fileSystems = new FileSystems(Mock.Of<ILogger>());
            var fileSystem = fileSystems.GetFileSystem<MediaFileSystem>();

            Assert.NotNull(fileSystem);
        }

		[Test]
        public void Exception_Thrown_On_Invalid_Typed_File_System()
		{
            var fileSystems = new FileSystems(Mock.Of<ILogger>());
            Assert.Throws<InvalidOperationException>(() => fileSystems.GetFileSystem<InvalidTypedFileSystem>());
		}


	    /// <summary>
	    /// Used in unit tests, for a typed file system we need to inherit from FileSystemWrapper and they MUST have a ctor
	    /// that only accepts a base IFileSystem object
	    /// </summary>
	    internal class InvalidTypedFileSystem : FileSystemWrapper
	    {
		    public InvalidTypedFileSystem(IFileSystem wrapped, string invalidParam) : base(wrapped)
		    {
		    }
	    }
	}
}
