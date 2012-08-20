using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using Umbraco.Core.IO;

namespace Umbraco.Tests.IO
{
    [TestFixture]
    public class FileSystemProviderManagerTests
    {
        [Test]
        public void Can_Get_File_System()
        {
            var fs = FileSystemProviderManager.Current.GetFileSystemProvider(FileSystemProvider.Media);

            Assert.NotNull(fs);
        }

        [Test]
        public void Can_Get_Typed_File_System()
        {
            var fs = FileSystemProviderManager.Current.GetFileSystemProvider<IMediaFileSystem>();

            Assert.NotNull(fs);
        }
    }
}
