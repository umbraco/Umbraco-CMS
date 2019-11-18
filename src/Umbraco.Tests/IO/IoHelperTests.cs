using System;
using NUnit.Framework;
using Umbraco.Core.IO;
using Umbraco.Core;
using Umbraco.Core.Composing;

namespace Umbraco.Tests.IO
{
    [TestFixture]
    public class IoHelperTests
    {
        [TestCase("~/Scripts", "/Scripts", null)]
        [TestCase("/Scripts", "/Scripts", null)]
        [TestCase("../Scripts", "/Scripts", typeof(ArgumentException))]
        public void IOHelper_ResolveUrl(string input, string expected, Type expectedExceptionType)
        {
            var ioHelper = new IOHelper();

            if (expectedExceptionType != null)
            {
                Assert.Throws(expectedExceptionType, () =>ioHelper.ResolveUrl(input));
            }
            else
            {
                var result = ioHelper.ResolveUrl(input);
                Assert.AreEqual(expected, result);
            }
        }

        /// <summary>
        ///A test for MapPath verifying that HttpContext method (which includes vdirs) matches non-HttpContext method
        ///</summary>
        [Test]
        public void IOHelper_MapPathTestVDirTraversal()
        {
            //System.Diagnostics.Debugger.Break();

            var ioHelper = new IOHelper();


            Assert.AreEqual(ioHelper.MapPath(SystemDirectories.Bin, true), ioHelper.MapPath(SystemDirectories.Bin, false));
            Assert.AreEqual(ioHelper.MapPath(SystemDirectories.Config, true), ioHelper.MapPath(SystemDirectories.Config, false));
            //Assert.AreEqual(ioHelper.MapPath(SystemDirectories.Css, true), ioHelper.MapPath(SystemDirectories.Css, false));
            Assert.AreEqual(ioHelper.MapPath(SystemDirectories.Data, true), ioHelper.MapPath(SystemDirectories.Data, false));
            Assert.AreEqual(ioHelper.MapPath(SystemDirectories.Install, true), ioHelper.MapPath(SystemDirectories.Install, false));
            //Assert.AreEqual(ioHelper.MapPath(SystemDirectories.Media, true), ioHelper.MapPath(SystemDirectories.Media, false));
            Assert.AreEqual(ioHelper.MapPath(SystemDirectories.Packages, true), ioHelper.MapPath(SystemDirectories.Packages, false));
            Assert.AreEqual(ioHelper.MapPath(SystemDirectories.Preview, true), ioHelper.MapPath(SystemDirectories.Preview, false));
            Assert.AreEqual(ioHelper.MapPath(SystemDirectories.Root, true), ioHelper.MapPath(SystemDirectories.Root, false));
            //Assert.AreEqual(ioHelper.MapPath(SystemDirectories.Scripts, true), ioHelper.MapPath(SystemDirectories.Scripts, false));
            //Assert.AreEqual(ioHelper.MapPath(SystemDirectories.Umbraco, true), ioHelper.MapPath(SystemDirectories.Umbraco, false));
        }

        [Test]
        public void EnsurePathIsApplicationRootPrefixed()
        {
            //Assert
            Assert.AreEqual("~/Views/Template.cshtml", "Views/Template.cshtml".EnsurePathIsApplicationRootPrefixed());
            Assert.AreEqual("~/Views/Template.cshtml", "/Views/Template.cshtml".EnsurePathIsApplicationRootPrefixed());
            Assert.AreEqual("~/Views/Template.cshtml", "~/Views/Template.cshtml".EnsurePathIsApplicationRootPrefixed());
        }
    }
}
