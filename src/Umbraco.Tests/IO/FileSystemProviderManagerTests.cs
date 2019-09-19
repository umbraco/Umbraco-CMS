using System;
using System.IO;
using System.Text;
using Moq;
using NUnit.Framework;
using Umbraco.Core;
using Umbraco.Core.IO;
using Umbraco.Core.Logging;
using Umbraco.Core.Profiling;
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

            // media fs wants this
            ApplicationContext.Current = new ApplicationContext(CacheHelper.CreateDisabledCacheHelper(), new ProfilingLogger(Mock.Of<ILogger>(), Mock.Of<IProfiler>()));

            // start clean
            // because some tests will create corrupt or weird filesystems
            FileSystemProviderManager.ResetCurrent();
        }

        [TearDown]
        public void TearDown()
        {
            // stay clean (see note in SetUp)
            FileSystemProviderManager.ResetCurrent();
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
        public void Media_Fs_Safe_Delete()
        {
            var fs = FileSystemProviderManager.Current.GetFileSystemProvider<MediaFileSystem>();
            var ms = new MemoryStream(Encoding.UTF8.GetBytes("test"));
            var virtPath = fs.GetMediaPath("file.txt", Guid.NewGuid(), Guid.NewGuid());
            fs.AddFile(virtPath, ms);

            // ~/media/1234/file.txt exists
            var physPath = IOHelper.MapPath(Path.Combine("media", virtPath));
            Assert.IsTrue(File.Exists(physPath));

            // ~/media/1234/file.txt is gone
            fs.DeleteMediaFiles(new [] { virtPath });
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
            var fs1 = FileSystemProviderManager.Current.GetFileSystemProvider<MediaFileSystem>();
            var fs2 = FileSystemProviderManager.Current.GetFileSystemProvider<MediaFileSystem>();

            Assert.AreSame(fs1, fs2);
        }

        [Test]
        public void Exception_Thrown_On_Invalid_Typed_File_System()
		{
			Assert.Throws<InvalidOperationException>(() => FileSystemProviderManager.Current.GetFileSystemProvider<InvalidTypedFileSystem>());
		}

        [Test]
        public void Exception_Thrown_On_NonConfigured_Typed_File_System()
        {
            // note: we need to reset the manager between tests else the Accept_Fallback test would corrupt that one
            Assert.Throws<ArgumentException>(() => FileSystemProviderManager.Current.GetFileSystemProvider<NonConfiguredTypeFileSystem>());
        }

        [Test]
        public void Accept_Fallback_On_NonConfigured_Typed_File_System()
        {
            var fs = FileSystemProviderManager.Current.GetFileSystemProvider<NonConfiguredTypeFileSystem>(() => new PhysicalFileSystem("~/App_Data/foo"));

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
