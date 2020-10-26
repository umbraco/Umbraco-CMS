using System;
using AutoFixture.NUnit3;
using Moq;
using NUnit.Framework;
using Umbraco.Core.Hosting;
using Umbraco.Core.IO;
using Umbraco.Core.Strings;
using Umbraco.Tests.TestHelpers;
using Umbraco.Tests.UnitTests.AutoFixture;
using Umbraco.Web.Common.AspNetCore;

namespace Umbraco.Tests.UnitTests.Umbraco.Web.Website
{
    [TestFixture]
    public class AspNetCoreHostingEnvironmentTests
    {

        [InlineAutoMoqData("~/Scripts", "/Scripts", null)]
        [InlineAutoMoqData("/Scripts", "/Scripts", null)]
        [InlineAutoMoqData("../Scripts", "/Scripts", typeof(InvalidOperationException))]
        public void IOHelper_ResolveUrl(string input, string expected, Type expectedExceptionType, AspNetCoreHostingEnvironment sut)
        {

            if (expectedExceptionType != null)
            {
                Assert.Throws(expectedExceptionType, () =>sut.ToAbsolute(input));
            }
            else
            {

                var result = sut.ToAbsolute(input);
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
