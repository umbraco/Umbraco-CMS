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
    internal class PhysicalFileSystemTests : AbstractFileSystemTests
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
    }
}
