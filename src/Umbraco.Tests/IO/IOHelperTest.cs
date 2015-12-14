using System;
using NUnit.Framework;
using Umbraco.Core.IO;

namespace Umbraco.Tests.IO
{
    
    
    /// <summary>
    ///This is a test class for IOHelperTest and is intended
    ///to contain all IOHelperTest Unit Tests
    ///</summary>
    [TestFixture()]
    public class IOHelperTest
    {

        [TestCase("~/Scripts", "/Scripts", null)]
        [TestCase("/Scripts", "/Scripts", null)]
        [TestCase("../Scripts", "/Scripts", typeof(ArgumentException))]
		public void IOHelper_ResolveUrl(string input, string expected, Type expectedExceptionType)
		{
            if (expectedExceptionType != null)
            {
                Assert.Throws(expectedExceptionType, () => IOHelper.ResolveUrl(input));
            }
            else
            {
                var result = IOHelper.ResolveUrl(input);
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

            Assert.AreEqual(IOHelper.MapPath(SystemDirectories.Bin, true), IOHelper.MapPath(SystemDirectories.Bin, false));
            Assert.AreEqual(IOHelper.MapPath(SystemDirectories.Config, true), IOHelper.MapPath(SystemDirectories.Config, false));
            Assert.AreEqual(IOHelper.MapPath(SystemDirectories.Css, true), IOHelper.MapPath(SystemDirectories.Css, false));
            Assert.AreEqual(IOHelper.MapPath(SystemDirectories.Data, true), IOHelper.MapPath(SystemDirectories.Data, false));
            Assert.AreEqual(IOHelper.MapPath(SystemDirectories.Install, true), IOHelper.MapPath(SystemDirectories.Install, false));
            Assert.AreEqual(IOHelper.MapPath(SystemDirectories.Masterpages, true), IOHelper.MapPath(SystemDirectories.Masterpages, false));
            Assert.AreEqual(IOHelper.MapPath(SystemDirectories.Media, true), IOHelper.MapPath(SystemDirectories.Media, false));
            Assert.AreEqual(IOHelper.MapPath(SystemDirectories.Packages, true), IOHelper.MapPath(SystemDirectories.Packages, false));
            Assert.AreEqual(IOHelper.MapPath(SystemDirectories.Preview, true), IOHelper.MapPath(SystemDirectories.Preview, false));
            Assert.AreEqual(IOHelper.MapPath(SystemDirectories.MacroScripts, true), IOHelper.MapPath(SystemDirectories.MacroScripts, false));
            Assert.AreEqual(IOHelper.MapPath(SystemDirectories.Root, true), IOHelper.MapPath(SystemDirectories.Root, false));
            Assert.AreEqual(IOHelper.MapPath(SystemDirectories.Scripts, true), IOHelper.MapPath(SystemDirectories.Scripts, false));
            Assert.AreEqual(IOHelper.MapPath(SystemDirectories.Umbraco, true), IOHelper.MapPath(SystemDirectories.Umbraco, false));
            Assert.AreEqual(IOHelper.MapPath(SystemDirectories.UmbracoClient, true), IOHelper.MapPath(SystemDirectories.UmbracoClient, false));
            Assert.AreEqual(IOHelper.MapPath(SystemDirectories.UserControls, true), IOHelper.MapPath(SystemDirectories.UserControls, false));
            Assert.AreEqual(IOHelper.MapPath(SystemDirectories.WebServices, true), IOHelper.MapPath(SystemDirectories.WebServices, false));
            Assert.AreEqual(IOHelper.MapPath(SystemDirectories.Xslt, true), IOHelper.MapPath(SystemDirectories.Xslt, false));
        }
    }
}
