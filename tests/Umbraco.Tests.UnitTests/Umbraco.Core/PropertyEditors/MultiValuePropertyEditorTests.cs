// Copyright (c) Umbraco.
// See LICENSE for more details.

using Moq;
using NUnit.Framework;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.IO;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.PropertyEditors;
using Umbraco.Cms.Core.Serialization;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Strings;
using Umbraco.Cms.Infrastructure.Serialization;

namespace Umbraco.Cms.Tests.UnitTests.Umbraco.Core.PropertyEditors;

/// <summary>
///     Tests for the base classes of ValueEditors and PreValueEditors that are used for Property Editors that edit
///     multiple values such as the drop down list, check box list, color picker, etc....
/// </summary>
/// <remarks>
///     Some of these tests are to verify that the we'd store INT Ids in the Db but publish STRING values or sometimes the INT values
///     to cache. Now we always just deal with strings and we'll keep the tests that show that.
/// </remarks>
[TestFixture]
public class MultiValuePropertyEditorTests
{
    [Test]
    public void MultipleValueEditor_WithMultipleValues_Format_Data_For_Cache()
    {
        var dataValueEditorFactoryMock = new Mock<IDataValueEditorFactory>();
        var serializer = new SystemTextConfigurationEditorJsonSerializer();
        var checkBoxListPropertyEditor = new CheckBoxListPropertyEditor(
            dataValueEditorFactoryMock.Object,
            Mock.Of<IIOHelper>(),
            serializer);
        var dataType = CreateAndConfigureDataType(serializer, checkBoxListPropertyEditor);

        var configuration = dataType.ConfigurationObject as ValueListConfiguration;
        Assert.NotNull(configuration);
        Assert.AreEqual(3, configuration.Items.Count);

        var dataTypeServiceMock = new Mock<IDataTypeService>();
        dataTypeServiceMock
            .Setup(x => x.GetDataType(It.IsAny<int>()))
            .Returns(dataType);

        var multipleValueEditor = CreateValueEditor();
        dataValueEditorFactoryMock
            .Setup(x => x.Create<MultipleValueEditor>(It.IsAny<DataEditorAttribute>()))
            .Returns(multipleValueEditor);

        var prop = new Property(1, new PropertyType(Mock.Of<IShortStringHelper>(), dataType));
        prop.SetValue("Value 1,Value 2,Value 3");

        var valueEditor = dataType.Editor.GetValueEditor();
        ((DataValueEditor)valueEditor).ConfigurationObject = dataType.ConfigurationObject;
        var result = valueEditor.ConvertDbToString(prop.PropertyType, prop.GetValue());

        Assert.AreEqual("Value 1,Value 2,Value 3", result);
    }

    [Test]
    public void MultipleValueEditor_WithSingleValue_Format_Data_For_Cache()
    {
        var dataValueEditorFactoryMock = new Mock<IDataValueEditorFactory>();
        var serializer = new SystemTextConfigurationEditorJsonSerializer();
        var checkBoxListPropertyEditor = new CheckBoxListPropertyEditor(
            dataValueEditorFactoryMock.Object,
            Mock.Of<IIOHelper>(),
            serializer);
        var dataType = CreateAndConfigureDataType(serializer, checkBoxListPropertyEditor);

        var configuration = dataType.ConfigurationObject as ValueListConfiguration;
        Assert.NotNull(configuration);
        Assert.AreEqual(3, configuration.Items.Count);

        var dataTypeServiceMock = new Mock<IDataTypeService>();
        dataTypeServiceMock
            .Setup(x => x.GetDataType(It.IsAny<int>()))
            .Returns(dataType);

        var multipleValueEditor = CreateValueEditor();
        dataValueEditorFactoryMock
            .Setup(x => x.Create<MultipleValueEditor>(It.IsAny<DataEditorAttribute>()))
            .Returns(multipleValueEditor);

        var prop = new Property(1, new PropertyType(Mock.Of<IShortStringHelper>(), dataType));
        prop.SetValue("Value 2");

        var result = dataType.Editor.GetValueEditor().ConvertDbToString(prop.PropertyType, prop.GetValue());

        Assert.AreEqual("Value 2", result);
    }

    [Test]
    public void MultipleValueEditor_Format_Data_For_Editor()
    {
        var dataValueEditorFactoryMock = new Mock<IDataValueEditorFactory>();

        var serializer = new SystemTextConfigurationEditorJsonSerializer();
        var checkBoxListPropertyEditor = new CheckBoxListPropertyEditor(
            dataValueEditorFactoryMock.Object,
            Mock.Of<IIOHelper>(),
            serializer);
        var dataType = new DataType(checkBoxListPropertyEditor, serializer)
        {
            Id = 1,
        };
        var configurationEditor = dataType.Editor!.GetConfigurationEditor();
        dataType.ConfigurationData = configurationEditor.FromConfigurationObject(
            new ValueListConfiguration
            {
                Items = ["Item 1", "Item 2", "Item 3"]
            },
            serializer);

        var valueEditorConfiguration = configurationEditor.ToValueEditor(dataType.ConfigurationData);

        var result = configurationEditor.ToConfigurationObject(valueEditorConfiguration, serializer) as ValueListConfiguration;

        Assert.NotNull(result);
        Assert.AreEqual(3, result.Items.Count);
        Assert.AreEqual("Item 1", result.Items[0]);
        Assert.AreEqual("Item 2", result.Items[1]);
        Assert.AreEqual("Item 3", result.Items[2]);
    }

    private static MultipleValueEditor CreateValueEditor() =>
        new(
            Mock.Of<ILocalizedTextService>(),
            Mock.Of<IShortStringHelper>(),
            Mock.Of<IJsonSerializer>(),
            Mock.Of<IIOHelper>(),
            new DataEditorAttribute(Constants.PropertyEditors.Aliases.CheckBoxList));

    private static DataType CreateAndConfigureDataType(SystemTextConfigurationEditorJsonSerializer serializer, CheckBoxListPropertyEditor checkBoxListPropertyEditor)
    {
        var dataType = new DataType(checkBoxListPropertyEditor, serializer)
        {
            Id = 1,
        };
        dataType.ConfigurationData = dataType.Editor!.GetConfigurationEditor()
            .FromConfigurationObject(
                new ValueListConfiguration
                {
                    Items = ["Value 1", "Value 2", "Value 3"]
                },
                serializer);
        return dataType;
    }
}
