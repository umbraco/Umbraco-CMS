using Microsoft.AspNetCore.Html;
using NUnit.Framework;
using System.Text.Encodings.Web;
using Umbraco.Cms.Core.Models.PublishedContent;
using Umbraco.Extensions;

namespace Umbraco.Cms.Tests.UnitTests.Umbraco.Web.Common.Extensions;

[TestFixture]
public class BlockEmptyStateTests
{
    [TearDown]
    public void TearDown() => VisualEditorPropertyTracker.Disable();

    private static string Render(IHtmlContent content)
    {
        using var writer = new StringWriter();
        content.WriteTo(writer, HtmlEncoder.Default);
        return writer.ToString();
    }

    [Test]
    public void Returns_Annotated_Container_When_Enabled_And_Editable()
    {
        VisualEditorPropertyTracker.Enable();
        IHtmlContent result = BlockEmptyState.Container("umb-block-list", "bodyText", editableInVisualEditor: true);
        var html = Render(result);
        Assert.That(html, Does.Contain("class=\"umb-block-list\""));
        Assert.That(html, Does.Contain("data-umb-block-property=\"bodyText\""));
    }

    [Test]
    public void Returns_Empty_When_Tracker_Disabled()
    {
        VisualEditorPropertyTracker.Disable();
        IHtmlContent result = BlockEmptyState.Container("umb-block-list", "bodyText", editableInVisualEditor: true);
        Assert.That(Render(result), Is.Empty);
    }

    [Test]
    public void Returns_Empty_When_Not_Editable()
    {
        VisualEditorPropertyTracker.Enable();
        IHtmlContent result = BlockEmptyState.Container("umb-block-grid", "bodyText", editableInVisualEditor: false);
        Assert.That(Render(result), Is.Empty);
    }

    [Test]
    public void Returns_Empty_When_Alias_Missing()
    {
        VisualEditorPropertyTracker.Enable();
        IHtmlContent result = BlockEmptyState.Container("umb-block-list", string.Empty, editableInVisualEditor: true);
        Assert.That(Render(result), Is.Empty);
    }

    [Test]
    public void Encodes_Alias_And_Class()
    {
        VisualEditorPropertyTracker.Enable();
        IHtmlContent result = BlockEmptyState.Container("umb-block-list", "a\"b", editableInVisualEditor: true);
        var html = Render(result);
        Assert.That(html, Does.Not.Contain("a\"b"));
        Assert.That(html, Does.Contain("a&quot;b").Or.Contain("a&#x22;b"));
    }
}
