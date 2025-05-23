using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using NUnit.Framework;
using Umbraco.Cms.Core.Cache;
using Umbraco.Cms.Core.Cache.PropertyEditors;
using Umbraco.Cms.Core.IO;
using Umbraco.Cms.Core.Models.Blocks;
using Umbraco.Cms.Core.PropertyEditors;
using Umbraco.Cms.Core.PropertyEditors.ValueConverters;
using Umbraco.Cms.Core.Serialization;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Strings;

namespace Umbraco.Cms.Tests.UnitTests.Umbraco.Core.PropertyEditors;

[TestFixture]
public class DataValueEditorReuseTests
{
    private Mock<IDataValueEditorFactory> _dataValueEditorFactoryMock;
    private PropertyEditorCollection _propertyEditorCollection;
    private DataValueReferenceFactoryCollection _dataValueReferenceFactories;

    [SetUp]
    public void SetUp()
    {
        _dataValueEditorFactoryMock = new Mock<IDataValueEditorFactory>();

        _dataValueEditorFactoryMock
            .Setup(m => m.Create<TextOnlyValueEditor>(It.IsAny<DataEditorAttribute>()))
            .Returns(() => new TextOnlyValueEditor(
                new DataEditorAttribute("a"),
                Mock.Of<ILocalizedTextService>(),
                Mock.Of<IShortStringHelper>(),
                Mock.Of<IJsonSerializer>(),
                Mock.Of<IIOHelper>()));

        _propertyEditorCollection = new PropertyEditorCollection(new DataEditorCollection(Enumerable.Empty<IDataEditor>));
        _dataValueReferenceFactories = new DataValueReferenceFactoryCollection(Enumerable.Empty<IDataValueReferenceFactory>, new NullLogger<DataValueReferenceFactoryCollection>());

        var blockVarianceHandler = new BlockEditorVarianceHandler(Mock.Of<ILanguageService>(), Mock.Of<IContentTypeService>());
        _dataValueEditorFactoryMock
            .Setup(m =>
                m.Create<BlockListPropertyEditorBase.BlockListEditorPropertyValueEditor>(It.IsAny<DataEditorAttribute>(), It.IsAny<BlockEditorDataConverter<BlockListValue, BlockListLayoutItem>>()))
            .Returns(() => new BlockListPropertyEditorBase.BlockListEditorPropertyValueEditor(
                new DataEditorAttribute("a"),
                new BlockListEditorDataConverter(Mock.Of<IJsonSerializer>()),
                _propertyEditorCollection,
                _dataValueReferenceFactories,
                Mock.Of<IDataTypeConfigurationCache>(),
                Mock.Of<IBlockEditorElementTypeCache>(),
                Mock.Of<ILocalizedTextService>(),
                Mock.Of<ILogger<BlockListPropertyEditorBase.BlockListEditorPropertyValueEditor>>(),
                Mock.Of<IShortStringHelper>(),
                Mock.Of<IJsonSerializer>(),
                Mock.Of<IPropertyValidationService>(),
                blockVarianceHandler,
                Mock.Of<ILanguageService>(),
                Mock.Of<IIOHelper>()
                ));
    }

    [Test]
    public void GetValueEditor_Reusable_Value_Editor_Is_Reused_When_Created_Without_Configuration()
    {
        var textboxPropertyEditor = new TextboxPropertyEditor(
            _dataValueEditorFactoryMock.Object,
            Mock.Of<IIOHelper>());

        // textbox is set to reuse its data value editor when created *without* configuration
        var dataValueEditor1 = textboxPropertyEditor.GetValueEditor();
        Assert.NotNull(dataValueEditor1);
        var dataValueEditor2 = textboxPropertyEditor.GetValueEditor();
        Assert.NotNull(dataValueEditor2);
        Assert.AreSame(dataValueEditor1, dataValueEditor2);
        _dataValueEditorFactoryMock.Verify(
            m => m.Create<TextOnlyValueEditor>(It.IsAny<DataEditorAttribute>()),
            Times.Once);
    }

    [Test]
    public void GetValueEditor_Reusable_Value_Editor_Is_Not_Reused_When_Created_With_Configuration()
    {
        var textboxPropertyEditor = new TextboxPropertyEditor(
            _dataValueEditorFactoryMock.Object,
            Mock.Of<IIOHelper>());

        // no matter what, a property editor should never reuse its data value editor when created *with* configuration
        var dataValueEditor1 = textboxPropertyEditor.GetValueEditor("config");
        Assert.NotNull(dataValueEditor1);
        Assert.AreEqual("config", ((DataValueEditor)dataValueEditor1).ConfigurationObject);
        var dataValueEditor2 = textboxPropertyEditor.GetValueEditor("config");
        Assert.NotNull(dataValueEditor2);
        Assert.AreEqual("config", ((DataValueEditor)dataValueEditor2).ConfigurationObject);
        Assert.AreNotSame(dataValueEditor1, dataValueEditor2);
        _dataValueEditorFactoryMock.Verify(
            m => m.Create<TextOnlyValueEditor>(It.IsAny<DataEditorAttribute>()),
            Times.Exactly(2));
    }

    [Test]
    public void GetValueEditor_Not_Reusable_Value_Editor_Is_Not_Reused_When_Created_Without_Configuration()
    {
        var blockListPropertyEditor = new BlockListPropertyEditor(
            _dataValueEditorFactoryMock.Object,
            Mock.Of<IIOHelper>(),
            Mock.Of<IBlockValuePropertyIndexValueFactory>(),
            Mock.Of<IJsonSerializer>());

        // block list is *not* set to reuse its data value editor
        var dataValueEditor1 = blockListPropertyEditor.GetValueEditor();
        Assert.NotNull(dataValueEditor1);
        var dataValueEditor2 = blockListPropertyEditor.GetValueEditor();
        Assert.NotNull(dataValueEditor2);
        Assert.AreNotSame(dataValueEditor1, dataValueEditor2);
        _dataValueEditorFactoryMock.Verify(
            m => m.Create<BlockListPropertyEditorBase.BlockListEditorPropertyValueEditor>(It.IsAny<DataEditorAttribute>(), It.IsAny<BlockEditorDataConverter<BlockListValue, BlockListLayoutItem>>()),
            Times.Exactly(2));
    }

    [Test]
    public void GetValueEditor_Not_Reusable_Value_Editor_Is_Not_Reused_When_Created_With_Configuration()
    {
        var blockListPropertyEditor = new BlockListPropertyEditor(
            _dataValueEditorFactoryMock.Object,
            Mock.Of<IIOHelper>(),
            Mock.Of<IBlockValuePropertyIndexValueFactory>(),
            Mock.Of<IJsonSerializer>());

        // no matter what, a property editor should never reuse its data value editor when created *with* configuration
        var dataValueEditor1 = blockListPropertyEditor.GetValueEditor("config");
        Assert.NotNull(dataValueEditor1);
        Assert.AreEqual("config", ((DataValueEditor)dataValueEditor1).ConfigurationObject);
        var dataValueEditor2 = blockListPropertyEditor.GetValueEditor("config");
        Assert.NotNull(dataValueEditor2);
        Assert.AreEqual("config", ((DataValueEditor)dataValueEditor2).ConfigurationObject);
        Assert.AreNotSame(dataValueEditor1, dataValueEditor2);
        _dataValueEditorFactoryMock.Verify(
            m => m.Create<BlockListPropertyEditorBase.BlockListEditorPropertyValueEditor>(It.IsAny<DataEditorAttribute>(), It.IsAny<BlockEditorDataConverter<BlockListValue, BlockListLayoutItem>>()),
            Times.Exactly(2));
    }
}
