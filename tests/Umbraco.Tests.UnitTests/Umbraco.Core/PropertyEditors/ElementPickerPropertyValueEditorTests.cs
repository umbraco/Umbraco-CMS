using Moq;
using NUnit.Framework;
using Umbraco.Cms.Core.IO;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.Editors;
using Umbraco.Cms.Core.PropertyEditors;
using Umbraco.Cms.Core.Scoping;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Strings;
using Umbraco.Cms.Infrastructure.Serialization;

namespace Umbraco.Cms.Tests.UnitTests.Umbraco.Core.PropertyEditors;

[TestFixture]
public class ElementPickerPropertyValueEditorTests
{
    [TestCase(null)]
    [TestCase("")]
    [TestCase("   ")]
    public void Empty_Stored_Value_To_Editor_Yields_Null(string? storedValue)
    {
        var result = ToEditor(storedValue);

        Assert.IsNull(result);
    }

    [Test]
    public void Stored_Selection_To_Editor_Yields_Selection()
    {
        var elementKey = Guid.NewGuid();

        var result = ToEditor($"[\"{elementKey}\"]");

        CollectionAssert.AreEqual(new[] { elementKey.ToString() }, result as IEnumerable<string>);
    }

    [Test]
    public void Empty_Selection_From_Editor_Yields_Null()
    {
        var result = CreateValueEditor().FromEditor(new ContentPropertyData(Array.Empty<string>(), null), null);

        Assert.IsNull(result);
    }

    private static object? ToEditor(string? storedValue)
    {
        var property = new Mock<IProperty>();
        property
            .Setup(p => p.GetValue(It.IsAny<string?>(), It.IsAny<string?>(), It.IsAny<bool>()))
            .Returns(storedValue);

        return CreateValueEditor().ToEditor(property.Object);
    }

    private static ElementPickerPropertyEditor.ElementPickerPropertyValueEditor CreateValueEditor()
        => new(
            Mock.Of<IShortStringHelper>(),
            new SystemTextJsonSerializer(new DefaultJsonSerializerEncoderFactory()),
            Mock.Of<IIOHelper>(),
            new DataEditorAttribute("alias") { ValueType = ValueTypes.Json },
            Mock.Of<ILocalizedTextService>(),
            Mock.Of<IElementService>(),
            Mock.Of<ICoreScopeProvider>())
        {
            ConfigurationObject = new ElementPickerConfiguration(),
        };
}
