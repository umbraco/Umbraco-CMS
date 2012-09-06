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
    [TestFixture]
    internal class PhysicalFileSystemTests : AbstractFileSystemTests
    {
        public PhysicalFileSystemTests()
            : base(new PhysicalFileSystem(AppDomain.CurrentDomain.BaseDirectory,
                "/Media/"))
        { }

        protected override string ConstructUrl(string path)
        {
            return "/Media/" + path;
        }
    }
}
