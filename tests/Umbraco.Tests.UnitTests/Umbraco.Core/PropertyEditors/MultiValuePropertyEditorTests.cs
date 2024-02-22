// Copyright (c) Umbraco.
// See LICENSE for more details.

using System.Globalization;
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
///     Mostly this used to test the we'd store INT Ids in the Db but publish STRING values or sometimes the INT values
///     to cache. Now we always just deal with strings and we'll keep the tests that show that.
/// </remarks>
[TestFixture]
public class MultiValuePropertyEditorTests
{
    [Test]
    public void DropDownMultipleValueEditor_Format_Data_For_Cache()
    {
        var dataValueEditorFactoryMock = new Mock<IDataValueEditorFactory>();
        var serializer = new SystemTextConfigurationEditorJsonSerializer();
        var checkBoxListPropertyEditor = new CheckBoxListPropertyEditor(
            dataValueEditorFactoryMock.Object,
            Mock.Of<IIOHelper>(), serializer);
        var dataType = new DataType(checkBoxListPropertyEditor, serializer)
        {
            Id = 1,
        };
        dataType.ConfigurationData = dataType.Editor!.GetConfigurationEditor()
            .FromConfigurationObject(
                new ValueListConfiguration
                {
                    Items = new List<ValueListConfiguration.ValueListItem>
                    {
                        new() { Value = "Value 1" },
                        new() { Value = "Value 2" },
                        new() { Value = "Value 3" },
                    },
                },
                serializer);

        var configuration = dataType.ConfigurationObject as ValueListConfiguration;
        Assert.NotNull(configuration);
        Assert.AreEqual(3, configuration.Items.Count);

        var dataTypeServiceMock = new Mock<IDataTypeService>();
        dataTypeServiceMock
            .Setup(x => x.GetDataType(It.IsAny<int>()))
            .Returns(dataType);

        // TODO use builders instead of this mess
        var multipleValueEditor = new MultipleValueEditor(
            Mock.Of<ILocalizedTextService>(),
            Mock.Of<IShortStringHelper>(),
            Mock.Of<IJsonSerializer>(),
            Mock.Of<IIOHelper>(),
            new DataEditorAttribute(Constants.PropertyEditors.Aliases.TextBox));
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
    public void DropDownValueEditor_Format_Data_For_Cache()
    {
        var dataValueEditorFactoryMock = new Mock<IDataValueEditorFactory>();

        var serializer = new SystemTextConfigurationEditorJsonSerializer();
        var checkBoxListPropertyEditor = new CheckBoxListPropertyEditor(
            dataValueEditorFactoryMock.Object,
            Mock.Of<IIOHelper>(), serializer);
        var dataType = new DataType(checkBoxListPropertyEditor, serializer)
        {
            Id = 1,
        };
        dataType.ConfigurationData = dataType.Editor!.GetConfigurationEditor()
            .FromConfigurationObject(
                new ValueListConfiguration
                {
                    Items = new List<ValueListConfiguration.ValueListItem>
                    {
                        new() { Value = "Value 1" },
                        new() { Value = "Value 2" },
                        new() { Value = "Value 3" },
                    },
                },
                serializer);

        var configuration = dataType.ConfigurationObject as ValueListConfiguration;
        Assert.NotNull(configuration);
        Assert.AreEqual(3, configuration.Items.Count);

        var dataTypeServiceMock = new Mock<IDataTypeService>();
        dataTypeServiceMock
            .Setup(x => x.GetDataType(It.IsAny<int>()))
            .Returns(dataType);

        // TODO use builders instead of this mess
        var multipleValueEditor = new MultipleValueEditor(
            Mock.Of<ILocalizedTextService>(),
            Mock.Of<IShortStringHelper>(),
            Mock.Of<IJsonSerializer>(),
            Mock.Of<IIOHelper>(),
            new DataEditorAttribute(Constants.PropertyEditors.Aliases.TextBox));
        dataValueEditorFactoryMock
            .Setup(x => x.Create<MultipleValueEditor>(It.IsAny<DataEditorAttribute>()))
            .Returns(multipleValueEditor);

        var prop = new Property(1, new PropertyType(Mock.Of<IShortStringHelper>(), dataType));
        prop.SetValue("Value 2");

        var result = dataType.Editor.GetValueEditor().ConvertDbToString(prop.PropertyType, prop.GetValue());

        Assert.AreEqual("Value 2", result);
    }

    [Test]
    public void DropDownPreValueEditor_Format_Data_For_Editor()
    {
        var dataValueEditorFactoryMock = new Mock<IDataValueEditorFactory>();

        var serializer = new SystemTextConfigurationEditorJsonSerializer();
        var checkBoxListPropertyEditor = new CheckBoxListPropertyEditor(
            dataValueEditorFactoryMock.Object,
            Mock.Of<IIOHelper>(), serializer);
        var dataType = new DataType(checkBoxListPropertyEditor, serializer)
        {
            Id = 1,
        };
        var configurationEditor = dataType.Editor!.GetConfigurationEditor();
        dataType.ConfigurationData = configurationEditor.FromConfigurationObject(
            new ValueListConfiguration
            {
                Items = new List<ValueListConfiguration.ValueListItem>
                {
                    new() { Value = "Item 1" },
                    new() { Value = "Item 2" },
                    new() { Value = "Item 3" },
                },
            },
            serializer);

        var valueEditorConfiguration = configurationEditor.ToValueEditor(dataType.ConfigurationData);

        var result = configurationEditor.ToConfigurationObject(valueEditorConfiguration, serializer) as ValueListConfiguration;

        Assert.NotNull(result);
        Assert.AreEqual(3, result.Items.Count);
        Assert.AreEqual("Item 1", result.Items[0].Value);
        Assert.AreEqual("Item 2", result.Items[1].Value);
        Assert.AreEqual("Item 3", result.Items[2].Value);
    }
}
