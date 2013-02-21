using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using NUnit.Framework;
using Umbraco.Core.IO;
using Umbraco.Tests.BusinessLogic;

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
            Directory.Delete(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "FileSysTests"));
        }

        protected override string ConstructUrl(string path)
        {
            return "/Media/" + path;
        }
    }
}
