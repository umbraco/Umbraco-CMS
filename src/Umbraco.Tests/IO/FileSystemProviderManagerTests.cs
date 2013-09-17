using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using NUnit.Framework;
using Umbraco.Core.IO;
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
            var fs = FileSystemProviderManager.Current.GetUnderlyingFileSystemProvider(FileSystemProviderConstants.Media);

            Assert.NotNull(fs);
        }

        [Test]
        public void Can_Get_Typed_File_System()
        {
            var fs = FileSystemProviderManager.Current.GetFileSystemProvider<MediaFileSystem>();

            Assert.NotNull(fs);
        }

		[Test]
        public void Exception_Thrown_On_Invalid_Typed_File_System()
		{
			Assert.Throws<InvalidOperationException>(() => FileSystemProviderManager.Current.GetFileSystemProvider<InvalidTypedFileSystem>());
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
