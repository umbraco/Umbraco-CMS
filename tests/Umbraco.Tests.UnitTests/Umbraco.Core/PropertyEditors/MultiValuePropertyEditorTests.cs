// Copyright (c) Umbraco.
// See LICENSE for more details.

using System.Collections.Generic;
using System.Globalization;
using Moq;
using Newtonsoft.Json;
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
        var serializer = new ConfigurationEditorJsonSerializer();
        var checkBoxListPropertyEditor = new CheckBoxListPropertyEditor(
            dataValueEditorFactoryMock.Object,
            Mock.Of<ILocalizedTextService>(),
            Mock.Of<IIOHelper>(),
            Mock.Of<IEditorConfigurationParser>());
        var dataType = new DataType(checkBoxListPropertyEditor, serializer)
        {
            Configuration = new ValueListConfiguration
            {
                Items = new List<ValueListConfiguration.ValueListItem>
                {
                    new() { Id = 4567, Value = "Value 1" },
                    new() { Id = 1234, Value = "Value 2" },
                    new() { Id = 8910, Value = "Value 3" },
                },
            },
            Id = 1,
        };

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
            new DataEditorAttribute(Constants.PropertyEditors.Aliases.TextBox, "Test Textbox", "textbox"));
        dataValueEditorFactoryMock
            .Setup(x => x.Create<MultipleValueEditor>(It.IsAny<DataEditorAttribute>()))
            .Returns(multipleValueEditor);

        var prop = new Property(1, new PropertyType(Mock.Of<IShortStringHelper>(), dataType));
        prop.SetValue("Value 1,Value 2,Value 3");

        var valueEditor = dataType.Editor.GetValueEditor();
        ((DataValueEditor)valueEditor).Configuration = dataType.Configuration;
        var result = valueEditor.ConvertDbToString(prop.PropertyType, prop.GetValue());

        Assert.AreEqual("Value 1,Value 2,Value 3", result);
    }

    [Test]
    public void DropDownValueEditor_Format_Data_For_Cache()
    {
        var dataValueEditorFactoryMock = new Mock<IDataValueEditorFactory>();

        var serializer = new ConfigurationEditorJsonSerializer();
        var checkBoxListPropertyEditor = new CheckBoxListPropertyEditor(
            dataValueEditorFactoryMock.Object,
            Mock.Of<ILocalizedTextService>(),
            Mock.Of<IIOHelper>(),
            Mock.Of<IEditorConfigurationParser>());
        var dataType = new DataType(checkBoxListPropertyEditor, serializer)
        {
            Configuration = new ValueListConfiguration
            {
                Items = new List<ValueListConfiguration.ValueListItem>
                {
                    new() { Id = 10, Value = "Value 1" },
                    new() { Id = 1234, Value = "Value 2" },
                    new() { Id = 11, Value = "Value 3" },
                },
            },
            Id = 1,
        };

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
            new DataEditorAttribute(Constants.PropertyEditors.Aliases.TextBox, "Test Textbox", "textbox"));
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
        // editor wants ApplicationContext.Current.Services.TextService
        // (that should be fixed with proper injection)
        var textService = new Mock<ILocalizedTextService>();
        textService.Setup(x => x.Localize(
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<CultureInfo>(),
                It.IsAny<IDictionary<string, string>>()))
            .Returns("blah");

        //// var appContext = new ApplicationContext(
        ////    new DatabaseContext(TestObjects.GetIDatabaseFactoryMock(), logger, Mock.Of<IRuntimeState>(), Mock.Of<IMigrationEntryService>()),
        ////    new ServiceContext(
        ////        localizedTextService: textService.Object
        ////    ),
        ////    Mock.Of<CacheHelper>(),
        ////    new ProfilingLogger(logger, Mock.Of<IProfiler>()))
        //// {
        ////    //IsReady = true
        //// };
        //// Current.ApplicationContext = appContext;

        var configuration = new ValueListConfiguration
        {
            Items = new List<ValueListConfiguration.ValueListItem>
            {
                new() { Id = 1, Value = "Item 1" },
                new() { Id = 2, Value = "Item 2" },
                new() { Id = 3, Value = "Item 3" },
            },
        };

        var editor = new ValueListConfigurationEditor(Mock.Of<ILocalizedTextService>(), Mock.Of<IIOHelper>(), Mock.Of<IEditorConfigurationParser>());

        var result = editor.ToConfigurationEditor(configuration);

        // 'result' is meant to be serialized, is built with anonymous objects
        // so we cannot really test what's in it - but by serializing it
        var json = JsonConvert.SerializeObject(result);
        Assert.AreEqual(
            "{\"items\":{\"1\":{\"value\":\"Item 1\",\"sortOrder\":1},\"2\":{\"value\":\"Item 2\",\"sortOrder\":2},\"3\":{\"value\":\"Item 3\",\"sortOrder\":3}}}",
            json);
    }
}
