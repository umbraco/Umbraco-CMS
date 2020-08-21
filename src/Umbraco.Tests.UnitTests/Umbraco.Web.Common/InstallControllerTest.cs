using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Moq;
using NUnit.Framework;
using Umbraco.Core.Configuration;
using Umbraco.Tests.UnitTests.AutoFixture;
using Umbraco.Web.Common.Install;
using Umbraco.Core;
using AutoFixture.NUnit3;
using Umbraco.Core.Hosting;
using System.IO;
using System.Reflection;
using System.Linq;

namespace Umbraco.Tests.UnitTests.Umbraco.Web.Common
{
    [TestFixture]
    class InstallControllerTest
    {
        [Test, AutoMoqData]
        public async Task InstallViewExists(
            [Frozen] IHostingEnvironment hostingEnvironment,
            IHostingEnvironment environment,
            InstallController sut)
        {
            Mock.Get(hostingEnvironment).Setup(x => x.ToAbsolute(It.IsAny<string>())).Returns("/");
            var viewResult = await sut.Index() as ViewResult;
            var sections = viewResult.ViewName.Split("\\");
            var fileName = sections[sections.Length - 1];

            // TODO: Don't use DirectoryInfo to get contents of UmbracoInstall, use something that works everywhere.
            var views = new DirectoryInfo(@"..\\..\\..\\..\\Umbraco.Web.UI.NetCore\\umbraco\\UmbracoInstall").GetFiles()
                .Select(f => f.Name).ToArray();
            Assert.True(views.Contains(fileName), $"Expected {fileName} to exist, but it didn't");
        }
    }
}
