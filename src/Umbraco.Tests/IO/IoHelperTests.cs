using System;
using NUnit.Framework;
using Umbraco.Core.IO;
using Umbraco.Core.Strings;
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

        [Test]
        public void EnsurePathIsApplicationRootPrefixed()
        {
            //Assert
            Assert.AreEqual("~/Views/Template.cshtml", PathUtility.EnsurePathIsApplicationRootPrefixed("Views/Template.cshtml"));
            Assert.AreEqual("~/Views/Template.cshtml", PathUtility.EnsurePathIsApplicationRootPrefixed("/Views/Template.cshtml"));
            Assert.AreEqual("~/Views/Template.cshtml", PathUtility.EnsurePathIsApplicationRootPrefixed("~/Views/Template.cshtml"));
        }
    }
}
