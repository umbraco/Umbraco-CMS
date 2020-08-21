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
            var location = new FileInfo(typeof(InstallController).Assembly.Location).Directory;
        }
    }
}
