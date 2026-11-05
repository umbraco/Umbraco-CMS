// Copyright (c) Umbraco.
// See LICENSE for more details.

using NUnit.Framework;
using Umbraco.Cms.Web.BackOffice.Controllers;

namespace Umbraco.Cms.Tests.UnitTests.Umbraco.Web.BackOffice.Controllers;

[TestFixture]
public class CodeFileControllerTests
{
    [TestCase("MyPartial.cshtml", ".cshtml", true)]
    [TestCase("/Views/Partials/MyPartial.cshtml", ".cshtml", true)]
    [TestCase("Folder/MyPartial.cshtml", ".cshtml", true)]
    [TestCase("MyPartial.CSHTML", ".cshtml", true)]
    [TestCase("myScript.js", ".js", true)]
    [TestCase("Folder/myScript.js", ".js", true)]
    [TestCase("web.config", ".cshtml", false)]
    [TestCase("Folder/web.config", ".cshtml", false)]
    [TestCase("evil.dll", ".cshtml", false)]
    [TestCase("MyPartial", ".cshtml", false)]
    [TestCase("MyPartial.cshtml.config", ".cshtml", false)]
    [TestCase("cshtml", ".cshtml", false)]
    [TestCase("web.config", ".js", false)]
    [TestCase("myScript.js.html", ".js", false)]
    [TestCase("myScript.cshtml", ".js", false)]
    [TestCase("", ".cshtml", false)]
    [TestCase("   ", ".cshtml", false)]
    [TestCase(null, ".cshtml", false)]
    public void HasValidFileExtension_Returns_Expected_Result(string? path, string extension, bool expected)
        => Assert.AreEqual(expected, CodeFileController.HasValidFileExtension(path, extension));
}
