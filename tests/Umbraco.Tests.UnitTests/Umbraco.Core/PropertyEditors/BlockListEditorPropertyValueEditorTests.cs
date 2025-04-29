using System.Globalization;
using System.Text.Json.Nodes;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using NUnit.Framework;
using Umbraco.Cms.Core.Cache;
using Umbraco.Cms.Core.Cache.PropertyEditors;
using Umbraco.Cms.Core.IO;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.Blocks;
using Umbraco.Cms.Core.Models.Editors;
using Umbraco.Cms.Core.Models.Validation;
using Umbraco.Cms.Core.PropertyEditors;
using Umbraco.Cms.Core.PropertyEditors.ValueConverters;
using Umbraco.Cms.Core.Serialization;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Strings;
using Umbraco.Cms.Infrastructure.Serialization;
using Umbraco.Cms.Tests.Common.Builders;
using Umbraco.Cms.Tests.Common.Builders.Extensions;
using Umbraco.Extensions;
using static Umbraco.Cms.Core.PropertyEditors.BlockListPropertyEditorBase;

namespace Umbraco.Cms.Tests.UnitTests.Umbraco.Core.PropertyEditors;

[TestFixture]
public class BlockListEditorPropertyValueEditorTests
{
    private static readonly Guid _contentTypeKey = Guid.NewGuid();
    private static readonly Guid _contentKey = Guid.NewGuid();

    [Test]
    public void Validates_Null_As_Below_Configured_Min()
    {
        var editor = CreateValueEditor();
        var result = editor.Validate(null, false, null, PropertyValidationContext.Empty());
        Assert.AreEqual(1, result.Count());

        var validationResult = result.First();
        Assert.AreEqual($"validation_entriesShort", validationResult.ErrorMessage);
    }

    [TestCase(0, false)]
    [TestCase(1, false)]
    [TestCase(2, true)]
    [TestCase(3, true)]
    public void Validates_Number_Of_Items_Is_Greater_Than_Or_Equal_To_Configured_Min(int numberOfBlocks, bool expectedSuccess)
    {
        var value = CreateBlocksJson(numberOfBlocks);
        var editor = CreateValueEditor();
        var result = editor.Validate(value, false, null, PropertyValidationContext.Empty());
        if (expectedSuccess)
        {
            Assert.IsEmpty(result);
        }
        else
        {
            Assert.AreEqual(1, result.Count());

            var validationResult = result.First();
            Assert.AreEqual("validation_entriesShort", validationResult.ErrorMessage);
        }
    }

    [TestCase(3, true)]
    [TestCase(4, true)]
    [TestCase(5, false)]
    public void Validates_Number_Of_Items_Is_Less_Than_Or_Equal_To_Configured_Max(int numberOfBlocks, bool expectedSuccess)
    {
        var value = CreateBlocksJson(numberOfBlocks);
        var editor = CreateValueEditor();
        var result = editor.Validate(value, false, null, PropertyValidationContext.Empty());
        if (expectedSuccess)
        {
            Assert.IsEmpty(result);
        }
        else
        {
            Assert.AreEqual(1, result.Count());

            var validationResult = result.First();
            Assert.AreEqual("validation_entriesExceed", validationResult.ErrorMessage);
        }
    }

    [Test]
    public void MergeVariantInvariantPropertyValue_Can_Merge_Null_Values()
    {
        var editor = CreateValueEditor();
        var result = editor.MergeVariantInvariantPropertyValue(null, null, true, ["en-US"]);
        Assert.IsNull(result);
    }

    [Test]
    public void FromEditor_With_Null_Current_Value_Returns_Expected_Json_Value()
    {
        var editedValue = CreateBlocksJson(1);
        var editor = CreateValueEditor();

        var contentPropertyData = new ContentPropertyData(editedValue, null);

        var result = editor.FromEditor(contentPropertyData, null);
        AssertResultValue(result, 0, "A");
    }

    [Test]
    public void FromEditor_With_Current_Value_Returns_Expected_Json_Value()
    {
        var editedValue = CreateBlocksJson(1, "B");
        var currentValue = CreateBlocksJson(1);
        var editor = CreateValueEditor();

        var contentPropertyData = new ContentPropertyData(editedValue, null);

        var result = editor.FromEditor(contentPropertyData, currentValue);
        AssertResultValue(result, 0, "B");
    }

    [Test]
    public void FromEditor_With_Block_Item_Editor_That_Uses_Current_Value_With_Edited_Property_Returns_Expected_Json_Value()
    {
        var editedValue = CreateBlocksJson(1, "B");
        var currentValue = CreateBlocksJson(1);
        var editor = CreateValueEditor(ValueEditorSetup.ConcatenatingTextValueEditor);

        var contentPropertyData = new ContentPropertyData(editedValue, null);

        var result = editor.FromEditor(contentPropertyData, currentValue);
        AssertResultValue(result, 0, "A, B");
    }

    [Test]
    public void FromEditor_With_Block_Item_Editor_That_Uses_Current_Value_With_Edited_And_Added_Property_Returns_Expected_Json_Value()
    {
        var editedValue = CreateBlocksJson(1, "B", "C");
        var currentValue = CreateBlocksJson(1);
        var editor = CreateValueEditor(ValueEditorSetup.ConcatenatingTextValueEditor);

        var contentPropertyData = new ContentPropertyData(editedValue, null);

        var result = editor.FromEditor(contentPropertyData, currentValue);
        AssertResultValue(result, 0, "A, B");
        AssertResultValue(result, 1, "C");
    }

    [Test]
    public void FromEditor_With_Block_Item_Editor_That_Uses_Current_Value_With_Edited_And_Removed_Property_Returns_Expected_Json_Value()
    {
        var editedValue = CreateBlocksJson(1, "B", "C");
        var currentValue = CreateBlocksJson(1, null);
        var editor = CreateValueEditor(ValueEditorSetup.ConcatenatingTextValueEditor);

        var contentPropertyData = new ContentPropertyData(editedValue, null);

        var result = editor.FromEditor(contentPropertyData, currentValue);
        AssertResultValue(result, 0, "B");
        AssertResultValue(result, 1, "C");
    }

    private static JsonObject CreateBlocksJson(int numberOfBlocks, string? blockMessagePropertyValue = "A", string? blockMessage2PropertyValue = null)
    {
        var layoutItems = new JsonArray();
        var contentData = new JsonArray();
        for (int i = 0; i < numberOfBlocks; i++)
        {
            layoutItems.Add(CreateLayoutBlockJson());
            contentData.Add(CreateContentDataBlockJson(blockMessagePropertyValue, blockMessage2PropertyValue));
        }

        return new JsonObject
        {
            {
                "layout", new JsonObject
                {
                    { "Umbraco.BlockList", layoutItems },
                }
            },
            { "contentData", contentData },
        };
    }

    private static JsonObject CreateLayoutBlockJson() =>
        new()
        {
            { "$type", "BlockListLayoutItem" },
            { "contentKey", _contentKey },
        };

    private static JsonObject CreateContentDataBlockJson(string? blockMessagePropertyValue, string? blockMessage2PropertyValue)
    {
        var values = new JsonArray();
        if (!string.IsNullOrEmpty(blockMessagePropertyValue))
        {
            values.Add(new JsonObject
            {
                { "editorAlias", "Umbraco.TextBox" },
                { "alias", "message" },
                { "value", blockMessagePropertyValue },
            });
        }

        if (!string.IsNullOrEmpty(blockMessage2PropertyValue))
        {
            values.Add(new JsonObject
            {
                { "editorAlias", "Umbraco.TextBox" },
                { "alias", "message2" },
                { "value", blockMessage2PropertyValue },
            });
        }

        return new()
        {
            { "contentTypeKey", _contentTypeKey },
            { "key", _contentKey },
            { "values", values }
        };
    }

    private enum ValueEditorSetup
    {
        TextOnlyValueEditor,
        ConcatenatingTextValueEditor,
    }

    private static BlockListEditorPropertyValueEditor CreateValueEditor(ValueEditorSetup valueEditorSetup = ValueEditorSetup.TextOnlyValueEditor)
    {
        var localizedTextServiceMock = new Mock<ILocalizedTextService>();
        localizedTextServiceMock
            .Setup(x => x.Localize(
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<CultureInfo>(),
                It.IsAny<IDictionary<string, string>>()))
            .Returns((string key, string alias, CultureInfo culture, IDictionary<string, string> args) => $"{key}_{alias}");

        var jsonSerializer = new SystemTextJsonSerializer();
        var languageService = Mock.Of<ILanguageService>();

        var dataValueEditorFactoryMock = new Mock<IDataValueEditorFactory>();

        DataEditor textBoxEditor;
        switch (valueEditorSetup)
        {
            case ValueEditorSetup.ConcatenatingTextValueEditor:
                dataValueEditorFactoryMock
                    .Setup(x => x.Create<ConcatenatingTextValueEditor>(It.IsAny<object[]>()))
                    .Returns(new ConcatenatingTextValueEditor(
                        Mock.Of<IShortStringHelper>(),
                        new SystemTextJsonSerializer()));
                textBoxEditor = new ConcatenatingTextboxPropertyEditor(
                    dataValueEditorFactoryMock.Object);

                break;
            default:
                dataValueEditorFactoryMock
                    .Setup(x => x.Create<TextOnlyValueEditor>(It.IsAny<object[]>()))
                    .Returns(new TextOnlyValueEditor(
                        new DataEditorAttribute("a"),
                        Mock.Of<ILocalizedTextService>(),
                        Mock.Of<IShortStringHelper>(),
                        new SystemTextJsonSerializer(),
                        Mock.Of<IIOHelper>()));
                textBoxEditor = new TextboxPropertyEditor(
                    dataValueEditorFactoryMock.Object,
                    Mock.Of<IIOHelper>());
                break;
        }

        var propertyEditors = new PropertyEditorCollection(new DataEditorCollection(() => textBoxEditor.Yield()));

        var elementType = new ContentTypeBuilder()
            .WithKey(_contentTypeKey)
            .AddPropertyType()
                .WithAlias("message")
                .Done()
            .AddPropertyType()
                .WithAlias("message2")
                .Done()
            .Build();
        var elementTypeCacheMock = new Mock<IBlockEditorElementTypeCache>();
        elementTypeCacheMock
            .Setup(x => x.GetMany(It.Is<IEnumerable<Guid>>(y => y.First() == _contentTypeKey)))
            .Returns([elementType]);

        return new BlockListEditorPropertyValueEditor(
            new DataEditorAttribute("alias"),
            new BlockListEditorDataConverter(jsonSerializer),
            propertyEditors,
            new DataValueReferenceFactoryCollection(Enumerable.Empty<IDataValueReferenceFactory>),
            Mock.Of<IDataTypeConfigurationCache>(),
            elementTypeCacheMock.Object,
            localizedTextServiceMock.Object,
            new NullLogger<BlockListEditorPropertyValueEditor>(),
            Mock.Of<IShortStringHelper>(),
            jsonSerializer,
            Mock.Of<IPropertyValidationService>(),
            new BlockEditorVarianceHandler(languageService, Mock.Of<IContentTypeService>()),
            languageService,
            Mock.Of<IIOHelper>())
        {
            ConfigurationObject = new BlockListConfiguration
            {
                ValidationLimit = new BlockListConfiguration.NumberRange
                {
                    Min = 2,
                    Max = 4
                },
            },
        };
    }

    /// <summary>
    /// An illustrative property editor that uses the edited and current value when returning a result from the FromEditor calls.
    /// </summary>
    /// <remarks>
    /// This is used to simulate a real-world editor that needs to use this value from within the block editor and verify
    /// that it receives and processes the value.
    /// </remarks>
    [DataEditor(
        global::Umbraco.Cms.Core.Constants.PropertyEditors.Aliases.TextBox)]
    private class ConcatenatingTextboxPropertyEditor : DataEditor
    {
        public ConcatenatingTextboxPropertyEditor(IDataValueEditorFactory dataValueEditorFactory)
            : base(dataValueEditorFactory)
        {
        }

        protected override IDataValueEditor CreateValueEditor() =>
            DataValueEditorFactory.Create<ConcatenatingTextValueEditor>(Attribute!);
    }

    /// <summary>
    /// An illustrative value editor that uses the edited and current value when returning a result from the FromEditor calls.
    /// </summary>
    /// <remarks>
    /// See notes on <see cref="ConcatenatingTextboxPropertyEditor"/>.
    /// </remarks>
    private class ConcatenatingTextValueEditor : DataValueEditor
    {
        public ConcatenatingTextValueEditor(IShortStringHelper shortStringHelper, IJsonSerializer? jsonSerializer)
            : base(shortStringHelper, jsonSerializer)
        {
        }

        public override object FromEditor(ContentPropertyData propertyData, object? currentValue)
        {
            var values = new List<string>();
            if (currentValue is not null)
            {
                values.Add(currentValue.ToString());
            }

            var editedValue = propertyData.Value;
            if (editedValue is not null)
            {
                values.Add(editedValue.ToString());
            }

            return string.Join(", ", values);
        }
    }

    private static void AssertResultValue(object? result, int valueIndex, string expectedValue)
    {
        Assert.IsNotNull(result);
        var resultAsJson = (JsonObject)JsonNode.Parse(result.ToString());
        Assert.AreEqual(expectedValue, resultAsJson["contentData"][0]["values"][valueIndex]["value"].ToString());
    }
}
