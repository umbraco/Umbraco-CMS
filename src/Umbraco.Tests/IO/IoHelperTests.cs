using System;
using NUnit.Framework;
using Umbraco.Core.IO;
using Umbraco.Core;
using Umbraco.Core.Composing;
using Umbraco.Core.Configuration;
using Umbraco.Tests.TestHelpers;

namespace Umbraco.Tests.IO
{
    [TestFixture]
    public class IoHelperTests
    {
        private IIOHelper _ioHelper => IOHelper.Default;

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
            var globalSettings = SettingsForTests.GenerateMockGlobalSettings();

            Assert.AreEqual(Current.IOHelper.MapPath(Constants.SystemDirectories.Bin, true), Current.IOHelper.MapPath(Constants.SystemDirectories.Bin, false));
            Assert.AreEqual(Current.IOHelper.MapPath(Constants.SystemDirectories.Config, true), Current.IOHelper.MapPath(Constants.SystemDirectories.Config, false));
            Assert.AreEqual(Current.IOHelper.MapPath(globalSettings.UmbracoCssPath, true), Current.IOHelper.MapPath(globalSettings.UmbracoCssPath, false));
            Assert.AreEqual(Current.IOHelper.MapPath(Constants.SystemDirectories.Data, true), Current.IOHelper.MapPath(Constants.SystemDirectories.Data, false));
            Assert.AreEqual(Current.IOHelper.MapPath(Constants.SystemDirectories.Install, true), Current.IOHelper.MapPath(Constants.SystemDirectories.Install, false));
            Assert.AreEqual(Current.IOHelper.MapPath(globalSettings.UmbracoMediaPath, true), Current.IOHelper.MapPath(globalSettings.UmbracoMediaPath, false));
            Assert.AreEqual(Current.IOHelper.MapPath(Constants.SystemDirectories.Packages, true), Current.IOHelper.MapPath(Constants.SystemDirectories.Packages, false));
            Assert.AreEqual(Current.IOHelper.MapPath(Constants.SystemDirectories.Preview, true), Current.IOHelper.MapPath(Constants.SystemDirectories.Preview, false));
            Assert.AreEqual(Current.IOHelper.MapPath(_ioHelper.Root, true), Current.IOHelper.MapPath(_ioHelper.Root, false));
            Assert.AreEqual(Current.IOHelper.MapPath(globalSettings.UmbracoScriptsPath, true), Current.IOHelper.MapPath(globalSettings.UmbracoScriptsPath, false));
            Assert.AreEqual(Current.IOHelper.MapPath(globalSettings.UmbracoPath, true), Current.IOHelper.MapPath(globalSettings.UmbracoPath, false));
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
