// Copyright (c) Umbraco.
// See LICENSE for more details.

using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Moq;
using NUnit.Framework;
using Umbraco.Cms.Api.Management.Services.FileSystem;
using Umbraco.Cms.Core.Configuration.Models;
using Umbraco.Cms.Core.Hosting;
using Umbraco.Cms.Core.IO;

namespace Umbraco.Cms.Tests.UnitTests.Umbraco.Cms.Api.Management.Services.FileSystem;

/// <summary>
///     Covers the file extension filtering applied by the tree services when listing files.
/// </summary>
[TestFixture]
public class FileSystemTreeServiceTests
{
    [Test]
    public void Can_Filter_PartialView_Files_Ignoring_Case()
    {
        var service = new PartialViewTreeService(CreateFileSystems("Upper.CSHTML", "lower.cshtml", "Mixed.CsHtml", "other.txt"));

        CollectionAssert.AreEquivalent(new[] { "Upper.CSHTML", "lower.cshtml", "Mixed.CsHtml" }, service.GetFiles("/"));
    }

    [Test]
    public void Can_Filter_Script_Files_Ignoring_Case()
    {
        var service = new ScriptTreeService(CreateFileSystems("Upper.JS", "lower.js", "other.txt"));

        CollectionAssert.AreEquivalent(new[] { "Upper.JS", "lower.js" }, service.GetFiles("/"));
    }

    [Test]
    public void Can_Filter_StyleSheet_Files_Ignoring_Case()
    {
        var service = new StyleSheetTreeService(CreateFileSystems("Upper.CSS", "lower.css", "other.txt"));

        CollectionAssert.AreEquivalent(new[] { "Upper.CSS", "lower.css" }, service.GetFiles("/"));
    }

    [Test]
    public void Cannot_Include_File_Whose_Extension_Is_Not_The_Last_Thing_In_The_Name()
    {
        // The extension is matched ordinally, so a trailing zero width joiner means the name does not
        // end with ".cshtml" - a culture sensitive comparison would treat the joiner as ignorable and match.
        var service = new PartialViewTreeService(CreateFileSystems("view.cshtml" + ZeroWidthJoiner, "view.cshtml"));

        CollectionAssert.AreEquivalent(new[] { "view.cshtml" }, service.GetFiles("/"));
    }

    private const string ZeroWidthJoiner = "\u200D";

    private static FileSystems CreateFileSystems(params string[] files)
    {
        var fileSystem = new Mock<IFileSystem>();
        fileSystem.Setup(x => x.GetFiles(It.IsAny<string>())).Returns(files);

        return new FileSystems(
            NullLoggerFactory.Instance,
            Mock.Of<IIOHelper>(),
            Options.Create(new GlobalSettings()),
            Mock.Of<IHostingEnvironment>(),
            fileSystem.Object,
            fileSystem.Object,
            fileSystem.Object,
            fileSystem.Object);
    }
}
