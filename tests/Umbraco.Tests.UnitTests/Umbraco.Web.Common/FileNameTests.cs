// Copyright (c) Umbraco.
// See LICENSE for more details.

using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using AutoFixture.NUnit3;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Options;
using Moq;
using NUnit.Framework;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Composing;
using Umbraco.Cms.Core.Configuration.Models;
using Umbraco.Cms.Core.Hosting;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Tests.UnitTests.AutoFixture;
using Umbraco.Cms.Web.BackOffice.Controllers;
using Umbraco.Cms.Web.BackOffice.Install;

namespace Umbraco.Cms.Tests.UnitTests.Umbraco.Web.Common;

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
        var root = TestContext.CurrentContext.TestDirectory.Split("tests")[0];
        var pathToFiles = Path.Combine(root, "src", "Umbraco.Cms.StaticAssets");
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

        var views = GetUiFiles(new[] { "umbraco", "UmbracoInstall" });
        Assert.True(views.Contains(fileName), $"Expected {fileName} to exist, but it didn't");
    }

    [Test]
    [AutoMoqData]
    public void PreviewViewExists(PreviewController sut)
    {
        var viewResult = sut.Index() as ViewResult;
        var fileName = GetViewName(viewResult);

        var views = GetUiFiles(new[] { "umbraco", "UmbracoBackOffice" });

        Assert.True(views.Contains(fileName), $"Expected {fileName} to exist, but it didn't");
    }

    [Test]
    [AutoMoqData]
    public async Task BackOfficeDefaultExists(
        [Frozen] IHostingEnvironment hostingEnvironment,
        [Frozen] ITempDataDictionary tempDataDictionary,
        [Frozen] IRuntimeState runtimeState,
        BackOfficeController sut)
    {
        Mock.Get(hostingEnvironment).Setup(x => x.ToAbsolute("/")).Returns("http://localhost/");
        Mock.Get(hostingEnvironment).SetupGet(x => x.ApplicationVirtualPath).Returns("/");
        Mock.Get(runtimeState).Setup(x => x.Level).Returns(RuntimeLevel.Run);

        sut.TempData = tempDataDictionary;

        var viewResult = await sut.Default() as ViewResult;
        var fileName = GetViewName(viewResult);
        var views = GetUiFiles(new[] { "umbraco", "UmbracoBackOffice" });

        Assert.True(views.Contains(fileName), $"Expected {fileName} to exist, but it didn't");
    }

    [Test]
    public void LanguageFilesAreLowerCase()
    {
        var languageProvider = new EmbeddedFileProvider(
            typeof(IAssemblyProvider).Assembly,
            "Umbraco.Cms.Core.EmbeddedResources.Lang");
        var files = languageProvider.GetDirectoryContents(string.Empty)
            .Where(x => !x.IsDirectory && x.Name.EndsWith(".xml"))
            .Select(x => x.Name);

        foreach (var fileName in files)
        {
            Assert.AreEqual(
                fileName.ToLower(),
                fileName,
                $"Language files must be all lowercase but {fileName} is not lowercase.");
        }
    }
}
