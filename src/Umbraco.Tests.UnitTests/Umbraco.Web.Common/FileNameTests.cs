// Copyright (c) Umbraco.
// See LICENSE for more details.

using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using AutoFixture.NUnit3;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.Extensions.Options;
using Moq;
using NUnit.Framework;
using Umbraco.Cms.Core.Configuration.Models;
using Umbraco.Cms.Core.Hosting;
using Umbraco.Cms.Web.BackOffice.Controllers;
using Umbraco.Cms.Web.Common.Install;
using Umbraco.Tests.UnitTests.AutoFixture;

namespace Umbraco.Tests.UnitTests.Umbraco.Web.Common
{
    [TestFixture]
    internal class FileNameTests
    {
        private string GetViewName(ViewResult viewResult, string separator = "/")
        {
            var sections = viewResult.ViewName.Split(separator);
            return sections[^1];
        }

        private IEnumerable<string> GetUiFiles(IEnumerable<string> pathFromNetCore)
        {
            var sourceRoot = TestContext.CurrentContext.TestDirectory.Split("Umbraco.Tests.UnitTests")[0];
            var pathToFiles = Path.Combine(sourceRoot, "Umbraco.Web.UI.NetCore");
            foreach (var pathSection in pathFromNetCore)
            {
                pathToFiles = Path.Combine(pathToFiles, pathSection);
            }

            return new DirectoryInfo(pathToFiles).GetFiles().Select(f => f.Name).ToArray();
        }

        [Test]
        [AutoMoqData]
        public async Task InstallViewExists(
            [Frozen] IHostingEnvironment hostingEnvironment,
            InstallController sut)
        {
            Mock.Get(hostingEnvironment).Setup(x => x.ToAbsolute(It.IsAny<string>())).Returns("http://localhost/");
            var viewResult = await sut.Index() as ViewResult;
            var fileName = GetViewName(viewResult, Path.DirectorySeparatorChar.ToString());

            IEnumerable<string> views = GetUiFiles(new[] { "umbraco", "UmbracoInstall" });
            Assert.True(views.Contains(fileName), $"Expected {fileName} to exist, but it didn't");
        }

        [Test]
        [AutoMoqData]
        public void PreviewViewExists(
            [Frozen] IOptions<GlobalSettings> globalSettings,
            PreviewController sut)
        {
            globalSettings.Value.UmbracoPath = "/";

            var viewResult = sut.Index() as ViewResult;
            var fileName = GetViewName(viewResult);

            IEnumerable<string> views = GetUiFiles(new[] { "umbraco", "UmbracoBackOffice" });

            Assert.True(views.Contains(fileName), $"Expected {fileName} to exist, but it didn't");
        }

        [Test]
        [AutoMoqData]
        public async Task BackOfficeDefaultExists(
            [Frozen] IOptions<GlobalSettings> globalSettings,
            [Frozen] IHostingEnvironment hostingEnvironment,
            [Frozen] ITempDataDictionary tempDataDictionary,
            BackOfficeController sut)
        {
            globalSettings.Value.UmbracoPath = "/";
            Mock.Get(hostingEnvironment).Setup(x => x.ToAbsolute("/")).Returns("http://localhost/");
            Mock.Get(hostingEnvironment).SetupGet(x => x.ApplicationVirtualPath).Returns("/");

            sut.TempData = tempDataDictionary;

            var viewResult = await sut.Default() as ViewResult;
            var fileName = GetViewName(viewResult);
            IEnumerable<string> views = GetUiFiles(new[] { "umbraco", "UmbracoBackOffice" });

            Assert.True(views.Contains(fileName), $"Expected {fileName} to exist, but it didn't");
        }

        [Test]
        public void LanguageFilesAreLowercase()
        {
            IEnumerable<string> files = GetUiFiles(new[] { "umbraco", "config", "lang" });
            foreach (var fileName in files)
            {
                Assert.AreEqual(
                    fileName.ToLower(),
                    fileName,
                    $"Language files must be all lowercase but {fileName} is not lowercase.");
            }
        }
    }
}
