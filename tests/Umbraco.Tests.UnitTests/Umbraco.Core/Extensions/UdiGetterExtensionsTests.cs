// Copyright (c) Umbraco.
// See LICENSE for more details.

using NUnit.Framework;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.Entities;
using Umbraco.Cms.Tests.Common.Builders;
using Umbraco.Cms.Tests.Common.Builders.Extensions;
using Umbraco.Extensions;

namespace Umbraco.Cms.Tests.UnitTests.Umbraco.Core.Extensions;

[TestFixture]
public class UdiGetterExtensionsTests
{
    [TestCase("style.css", "umb://stylesheet/style.css")]
    [TestCase("editor\\style.css", "umb://stylesheet/editor/style.css")]
    [TestCase("editor/style.css", "umb://stylesheet/editor/style.css")]
    public void GetUdiForStylesheet(string path, string expected)
    {
        var builder = new StylesheetBuilder();
        var stylesheet = builder.WithPath(path).Build();
        var result = stylesheet.GetUdi();
        Assert.AreEqual(expected, result.ToString());
    }

    [TestCase("script.js", "umb://script/script.js")]
    [TestCase("editor\\script.js", "umb://script/editor/script.js")]
    [TestCase("editor/script.js", "umb://script/editor/script.js")]
    public void GetUdiForScript(string path, string expected)
    {
        var builder = new ScriptBuilder();
        var script = builder.WithPath(path).Build();
        var result = script.GetUdi();
        Assert.AreEqual(expected, result.ToString());
    }

    [TestCase("test.cshtml", PartialViewType.PartialView, "umb://partial-view/test.cshtml")]
    [TestCase("editor\\test.cshtml", PartialViewType.PartialView, "umb://partial-view/editor/test.cshtml")]
    [TestCase("editor/test.cshtml", PartialViewType.PartialView, "umb://partial-view/editor/test.cshtml")]
    [TestCase("test.cshtml", PartialViewType.PartialViewMacro, "umb://partial-view-macro/test.cshtml")]
    [TestCase("editor\\test.cshtml", PartialViewType.PartialViewMacro, "umb://partial-view-macro/editor/test.cshtml")]
    [TestCase("editor/test.cshtml", PartialViewType.PartialViewMacro, "umb://partial-view-macro/editor/test.cshtml")]
    public void GetUdiForPartialView(string path, PartialViewType viewType, string expected)
    {
        var builder = new PartialViewBuilder();
        var partialView = builder
            .WithPath(path)
            .WithViewType(viewType)
            .Build();
        var result = partialView.GetUdi();
        Assert.AreEqual(expected, result.ToString());
    }

    [TestCase("6ad82c70-685c-4e04-9b36-d81bd779d16f", "umb://webhook/6ad82c70685c4e049b36d81bd779d16f")]
    public void GetUdiForWebhook(string key, string expected)
    {
        var builder = new WebhookBuilder();
        var webhook = builder
            .WithKey(Guid.Parse(key))
            .Build();

        Udi result = webhook.GetUdi();
        Assert.AreEqual(expected, result.ToString());

        result = ((IEntity)webhook).GetUdi();
        Assert.AreEqual(expected, result.ToString());
    }
}
