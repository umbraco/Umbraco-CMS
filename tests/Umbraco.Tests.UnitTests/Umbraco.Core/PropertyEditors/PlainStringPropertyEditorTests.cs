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
    [TestCase("{\"test\":\"test\"}")]
    [TestCase("{}")]
    [TestCase("[1,2,3]")]
    [TestCase("{\"nested\":{\"key\":\"value\"}}")]
    [TestCase("hello world")]
    [TestCase("123")]
    public void ToEditor_Returns_Raw_String_For_Json_Looking_Values(string storedValue)
    {
        var result = ToEditor(storedValue);

        Assert.IsNotNull(result);
        Assert.IsInstanceOf<string>(result);
        Assert.AreEqual(storedValue, result);
    }

    [Test]
    public void ToEditor_Returns_Null_Or_Empty_For_Null_Value()
    {
        var result = ToEditor(null);
        Assert.That(result is null or (string and ""), Is.True);
    }

    private static object? ToEditor(object? value)
    {
        var property = new Mock<IProperty>();
        property
            .Setup(x => x.GetValue(It.IsAny<string?>(), It.IsAny<string?>(), It.IsAny<bool>()))
            .Returns(value);

        return CreateValueEditor().ToEditor(property.Object);
    }

    private static IDataValueEditor CreateValueEditor()
    {
        var attribute = new DataEditorAttribute("Umbraco.Plain.String");
        return new TextOnlyValueEditor(
            attribute,
            Mock.Of<ILocalizedTextService>(),
            Mock.Of<IShortStringHelper>(),
            Mock.Of<IJsonSerializer>(),
            Mock.Of<IIOHelper>());
    }
}
