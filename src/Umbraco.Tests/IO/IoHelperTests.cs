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
        private IIOHelper _ioHelper => TestHelper.IOHelper;

        [TestCase("~/Scripts", "/Scripts", null)]
        [TestCase("/Scripts", "/Scripts", null)]
        [TestCase("../Scripts", "/Scripts", typeof(ArgumentException))]
        public void IOHelper_ResolveUrl(string input, string expected, Type expectedExceptionType)
        {

            if (expectedExceptionType != null)
            {
                Assert.Throws(expectedExceptionType, () =>_ioHelper.ResolveUrl(input));
            }
            else
            {
                var result = _ioHelper.ResolveUrl(input);
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

            Assert.AreEqual(_ioHelper.MapPath(Constants.SystemDirectories.Bin, true), _ioHelper.MapPath(Constants.SystemDirectories.Bin, false));
            Assert.AreEqual(_ioHelper.MapPath(Constants.SystemDirectories.Config, true), _ioHelper.MapPath(Constants.SystemDirectories.Config, false));
            Assert.AreEqual(_ioHelper.MapPath(globalSettings.UmbracoCssPath, true), _ioHelper.MapPath(globalSettings.UmbracoCssPath, false));
            Assert.AreEqual(_ioHelper.MapPath(Constants.SystemDirectories.Data, true), _ioHelper.MapPath(Constants.SystemDirectories.Data, false));
            Assert.AreEqual(_ioHelper.MapPath(Constants.SystemDirectories.Install, true), _ioHelper.MapPath(Constants.SystemDirectories.Install, false));
            Assert.AreEqual(_ioHelper.MapPath(globalSettings.UmbracoMediaPath, true), _ioHelper.MapPath(globalSettings.UmbracoMediaPath, false));
            Assert.AreEqual(_ioHelper.MapPath(Constants.SystemDirectories.Packages, true), _ioHelper.MapPath(Constants.SystemDirectories.Packages, false));
            Assert.AreEqual(_ioHelper.MapPath(Constants.SystemDirectories.Preview, true), _ioHelper.MapPath(Constants.SystemDirectories.Preview, false));
            Assert.AreEqual(_ioHelper.MapPath(_ioHelper.Root, true), _ioHelper.MapPath(_ioHelper.Root, false));
            Assert.AreEqual(_ioHelper.MapPath(globalSettings.UmbracoScriptsPath, true), _ioHelper.MapPath(globalSettings.UmbracoScriptsPath, false));
            Assert.AreEqual(_ioHelper.MapPath(globalSettings.UmbracoPath, true), _ioHelper.MapPath(globalSettings.UmbracoPath, false));
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
