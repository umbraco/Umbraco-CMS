using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using NUnit.Framework;
using Umbraco.Core.IO;


namespace Umbraco.Tests.IO
{
    [TestFixture, RequiresSTA]
    public class PhysicalFileSystemTests : AbstractFileSystemTests
    {
        public PhysicalFileSystemTests()
            : base(new PhysicalFileSystem(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "FileSysTests"),
                "/Media/"))
        { }

        [SetUp]
        public void Setup()
        {
            
        }

        [TearDown]
        public void TearDown()
        {
            var path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "FileSysTests");
            if (Directory.Exists(path) == false) return;

            var files = Directory.GetFiles(path);
            foreach (var f in files)
            {
                File.Delete(f);
            }
            Directory.Delete(path, true);
        }

        protected override string ConstructUrl(string path)
        {
            return "/Media/" + path;
        }

        [Test]
        public void GetFullPathTest()
        {
            // outside of tests, one initializes the PhysicalFileSystem with eg ~/Dir
            // and then, rootPath = /path/to/Dir and rootUrl = /Dir/
            // here we initialize the PhysicalFileSystem with
            // rootPath = /path/to/FileSysTests
            // rootUrl = /Media/

            var basePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "FileSysTests");

            // ensure that GetFullPath
            // - does return the proper full path
            // - does properly normalize separators
            // - does throw on invalid paths

            var path = _fileSystem.GetFullPath("foo.tmp");
            Assert.AreEqual(Path.Combine(basePath, @"foo.tmp"), path);

            path = _fileSystem.GetFullPath("foo/bar.tmp");
            Assert.AreEqual(Path.Combine(basePath, @"foo\bar.tmp"), path);

            // that path is invalid as it would be outside the root directory
            Assert.Throws<FileSecurityException>(() => _fileSystem.GetFullPath("../../foo.tmp"));
        }
    }
}
