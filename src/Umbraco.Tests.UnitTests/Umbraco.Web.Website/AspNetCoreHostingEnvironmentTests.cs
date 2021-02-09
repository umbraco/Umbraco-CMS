// Copyright (c) Umbraco.
// See LICENSE for more details.

using System;
using NUnit.Framework;
using Umbraco.Cms.Core.Strings;
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
                Assert.Throws(expectedExceptionType, () => sut.ToAbsolute(input));
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
            // Assert
            Assert.AreEqual("~/Views/Template.cshtml", PathUtility.EnsurePathIsApplicationRootPrefixed("Views/Template.cshtml"));
            Assert.AreEqual("~/Views/Template.cshtml", PathUtility.EnsurePathIsApplicationRootPrefixed("/Views/Template.cshtml"));
            Assert.AreEqual("~/Views/Template.cshtml", PathUtility.EnsurePathIsApplicationRootPrefixed("~/Views/Template.cshtml"));
        }
    }
}
