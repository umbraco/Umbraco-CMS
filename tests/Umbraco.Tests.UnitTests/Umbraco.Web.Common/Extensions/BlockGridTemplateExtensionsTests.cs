using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Mvc.Rendering;
using Moq;
using NUnit.Framework;
using Umbraco.Cms.Core.Models.Blocks;
using Umbraco.Cms.Core.Models.PublishedContent;
using Umbraco.Extensions;

namespace Umbraco.Cms.Tests.UnitTests.Umbraco.Web.Common.Extensions;

[TestFixture]
public class BlockGridTemplateExtensionsTests
{
    [TearDown]
    public void TearDown() => VisualEditorPropertyTracker.Disable();

    private static string Render(IHtmlContent content)
    {
        using var writer = new StringWriter();
        content.WriteTo(writer, HtmlEncoder.Default);
        return writer.ToString();
    }

    private static IPublishedProperty EmptyEditableProperty(string alias, bool editable)
    {
        var emptyGrid = new BlockGridModel(new List<BlockGridItem>(), null);

        var propertyType = new Mock<IPublishedPropertyType>();
        propertyType.SetupGet(x => x.Alias).Returns(alias);
        propertyType.SetupGet(x => x.EditableInVisualEditor).Returns(editable);

        var property = new Mock<IPublishedProperty>();
        property.SetupGet(x => x.Alias).Returns(alias);
        property.SetupGet(x => x.PropertyType).Returns(propertyType.Object);
        property.Setup(x => x.GetValue(null, null)).Returns(emptyGrid);
        return property.Object;
    }

    [Test]
    public async Task Empty_Editable_Property_In_VisualEditor_Emits_Annotated_Container()
    {
        VisualEditorPropertyTracker.Enable();
        IHtmlContent result = await Mock.Of<IHtmlHelper>().GetBlockGridHtmlAsync(EmptyEditableProperty("bodyText", editable: true));
        var html = Render(result);
        Assert.That(html, Does.Contain("class=\"umb-block-grid\""));
        Assert.That(html, Does.Contain("data-umb-block-property=\"bodyText\""));
    }

    [Test]
    public async Task Empty_NonEditable_Property_Emits_Nothing()
    {
        VisualEditorPropertyTracker.Enable();
        IHtmlContent result = await Mock.Of<IHtmlHelper>().GetBlockGridHtmlAsync(EmptyEditableProperty("bodyText", editable: false));
        Assert.That(Render(result), Is.Empty);
    }

    [Test]
    public async Task Empty_Property_Outside_VisualEditor_Emits_Nothing()
    {
        VisualEditorPropertyTracker.Disable();
        IHtmlContent result = await Mock.Of<IHtmlHelper>().GetBlockGridHtmlAsync(EmptyEditableProperty("bodyText", editable: true));
        Assert.That(Render(result), Is.Empty);
    }
}
