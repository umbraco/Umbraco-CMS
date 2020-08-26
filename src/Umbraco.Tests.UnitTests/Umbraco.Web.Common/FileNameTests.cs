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
using Umbraco.Web.BackOffice.Controllers;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using AutoFixture;
using Microsoft.AspNetCore.Http;

namespace Umbraco.Tests.UnitTests.Umbraco.Web.Common
{
    [TestFixture]
    class FileNameTests 
    {
        private string getViewName(ViewResult viewResult, string seperator = "/")
        {
            var sections = viewResult.ViewName.Split(seperator);
            return sections[sections.Length - 1];
        }

        private string[] getUiFiles(IEnumerable<string> pathFromNetCore)
        {
            var sourceRoot = TestContext.CurrentContext.TestDirectory.Split("Umbraco.Tests.UnitTests")[0];
            var pathToFiles = Path.Combine(sourceRoot, "Umbraco.Web.UI.NetCore");
            foreach(var pathSection in pathFromNetCore)
            {
                pathToFiles = Path.Combine(pathToFiles, pathSection);
            }

            return new DirectoryInfo(pathToFiles).GetFiles().Select(f => f.Name).ToArray();
        }

        [Test, AutoMoqData]
        public async Task InstallViewExists(
            [Frozen] IHostingEnvironment hostingEnvironment,
            InstallController sut)
        {
            Mock.Get(hostingEnvironment).Setup(x => x.ToAbsolute(It.IsAny<string>())).Returns("http://localhost/");
            var viewResult = await sut.Index() as ViewResult;
            var fileName = getViewName(viewResult, Path.DirectorySeparatorChar.ToString());

            var views = getUiFiles(new string[] { "Umbraco", "UmbracoInstall" });
            Assert.True(views.Contains(fileName), $"Expected {fileName} to exist, but it didn't");
        }

        [Test, AutoMoqData]
        public void PrviewViewExists(
            [Frozen] IGlobalSettings globalSettings,
            PreviewController sut)
        {
            Mock.Get(globalSettings).Setup(x => x.UmbracoPath).Returns("/");

            var viewResult = sut.Index() as ViewResult;
            var fileName = getViewName(viewResult);

            var views = getUiFiles(new string[] {"Umbraco", "UmbracoBackOffice" });

            Assert.True(views.Contains(fileName), $"Expected {fileName} to exist, but it didn't");
        }

        [Test, AutoMoqData]
        public async Task BackOfficeDefaultExists(
            [Frozen] IGlobalSettings globalSettings,
            [Frozen] IHostingEnvironment hostingEnvironment,
            [Frozen] ITempDataDictionary tempDataDictionary,
            BackOfficeController sut)
        {
            Mock.Get(globalSettings).Setup(x => x.UmbracoPath).Returns("/");
            Mock.Get(hostingEnvironment).Setup(x => x.ToAbsolute("/")).Returns("http://localhost/");
            Mock.Get(hostingEnvironment).SetupGet(x => x.ApplicationVirtualPath).Returns("/");


            sut.TempData = tempDataDictionary;

            var viewResult = await sut.Default() as ViewResult;
            var fileName = getViewName(viewResult);
            var views = getUiFiles(new string[] { "Umbraco", "UmbracoBackOffice" });

            Assert.True(views.Contains(fileName), $"Expected {fileName} to exist, but it didn't");
        }


        [Test]
        public void LanguageFilesAreLowercase()
        {
            
            var files = getUiFiles(new string[] { "Umbraco", "config", "lang" });
            foreach (var fileName in files)
            {
                Assert.AreEqual(fileName.ToLower(), fileName, $"Language files must be all lowercase but {fileName} is not lowercase.");
            }

        }
    }
}
