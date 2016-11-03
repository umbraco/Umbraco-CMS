using System;
using Moq;
using NUnit.Framework;
using Umbraco.Core;
using Umbraco.Core.IO;
using Umbraco.Core.Logging;
using Umbraco.Tests.TestHelpers;

namespace Umbraco.Tests.IO
{
    [TestFixture]
    public class FileSystemProviderManagerTests
    {
        [SetUp]
        public void Setup()
        {
            //init the config singleton
            var config = SettingsForTests.GetDefault();
            SettingsForTests.ConfigureSettings(config);
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
