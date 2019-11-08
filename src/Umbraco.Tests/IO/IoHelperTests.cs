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
            if (expectedExceptionType != null)
            {
                Assert.Throws(expectedExceptionType, () => Current.IOHelper.ResolveUrl(input));
            }
            else
            {
                var result = Current.IOHelper.ResolveUrl(input);
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

            Assert.AreEqual(Current.IOHelper.MapPath(SystemDirectories.Bin, true), Current.IOHelper.MapPath(SystemDirectories.Bin, false));
            Assert.AreEqual(Current.IOHelper.MapPath(SystemDirectories.Config, true), Current.IOHelper.MapPath(SystemDirectories.Config, false));
            Assert.AreEqual(Current.IOHelper.MapPath(SystemDirectories.Css, true), Current.IOHelper.MapPath(SystemDirectories.Css, false));
            Assert.AreEqual(Current.IOHelper.MapPath(SystemDirectories.Data, true), Current.IOHelper.MapPath(SystemDirectories.Data, false));
            Assert.AreEqual(Current.IOHelper.MapPath(SystemDirectories.Install, true), Current.IOHelper.MapPath(SystemDirectories.Install, false));
            Assert.AreEqual(Current.IOHelper.MapPath(SystemDirectories.Media, true), Current.IOHelper.MapPath(SystemDirectories.Media, false));
            Assert.AreEqual(Current.IOHelper.MapPath(SystemDirectories.Packages, true), Current.IOHelper.MapPath(SystemDirectories.Packages, false));
            Assert.AreEqual(Current.IOHelper.MapPath(SystemDirectories.Preview, true), Current.IOHelper.MapPath(SystemDirectories.Preview, false));
            Assert.AreEqual(Current.IOHelper.MapPath(SystemDirectories.Root, true), Current.IOHelper.MapPath(SystemDirectories.Root, false));
            Assert.AreEqual(Current.IOHelper.MapPath(SystemDirectories.Scripts, true), Current.IOHelper.MapPath(SystemDirectories.Scripts, false));
            Assert.AreEqual(Current.IOHelper.MapPath(SystemDirectories.Umbraco, true), Current.IOHelper.MapPath(SystemDirectories.Umbraco, false));
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
