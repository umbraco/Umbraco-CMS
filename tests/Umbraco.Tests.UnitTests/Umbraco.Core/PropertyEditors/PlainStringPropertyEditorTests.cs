using Moq;
using NUnit.Framework;
using Umbraco.Cms.Core.IO;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.PropertyEditors;
using Umbraco.Cms.Core.Serialization;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Strings;

namespace Umbraco.Cms.Tests.UnitTests.Umbraco.Core.PropertyEditors;

[TestFixture]
public class PlainStringPropertyEditorTests
{
    [Test]
    public void Can_Create_Text_Only_Value_Editor()
    {
        IDataValueEditor valueEditor = CreatePropertyEditor().GetValueEditor();

        Assert.IsInstanceOf<TextOnlyValueEditor>(valueEditor);
    }

    [TestCase("{\"test\":\"test\"}")]
    [TestCase("{}")]
    [TestCase("[1,2,3]")]
    [TestCase("{\"nested\":{\"key\":\"value\"}}")]
    [TestCase("hello world")]
    [TestCase("123")]
    public void Can_Parse_Json_Looking_Value_To_Editor_As_Raw_String(string storedValue)
    {
        var result = ToEditor(storedValue);

        Assert.That(result, Is.EqualTo(storedValue));
    }

    [Test]
    public void Null_To_Editor_Yields_Empty_String()
    {
        var result = ToEditor(null);

        Assert.That(result, Is.Empty);
    }

    private static object? ToEditor(object? value)
    {
        var property = new Mock<IProperty>();
        property
            .Setup(x => x.GetValue(It.IsAny<string?>(), It.IsAny<string?>(), It.IsAny<bool>()))
            .Returns(value);

        return CreatePropertyEditor().GetValueEditor().ToEditor(property.Object);
    }

    private static PlainStringPropertyEditor CreatePropertyEditor()
    {
        var factory = new Mock<IDataValueEditorFactory>();

        factory
            .Setup(x => x.Create<TextOnlyValueEditor>(It.IsAny<DataEditorAttribute>()))
            .Returns((object[] args) => new TextOnlyValueEditor(
                (DataEditorAttribute)args[0],
                Mock.Of<ILocalizedTextService>(),
                Mock.Of<IShortStringHelper>(),
                Mock.Of<IJsonSerializer>(),
                Mock.Of<IIOHelper>()));

        return new PlainStringPropertyEditor(factory.Object);
    }
}
