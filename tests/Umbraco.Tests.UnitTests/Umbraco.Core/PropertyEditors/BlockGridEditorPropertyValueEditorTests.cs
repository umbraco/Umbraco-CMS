using System.Globalization;
using System.Text.Json.Nodes;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using NUnit.Framework;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Cache;
using Umbraco.Cms.Core.Cache.PropertyEditors;
using Umbraco.Cms.Core.IO;
using Umbraco.Cms.Core.Models.Blocks;
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
using static Umbraco.Cms.Core.PropertyEditors.BlockGridPropertyEditorBase;

namespace Umbraco.Cms.Tests.UnitTests.Umbraco.Core.PropertyEditors;

[TestFixture]
public class BlockGridEditorPropertyValueEditorTests
{
    private static readonly Guid _contentTypeKey = Guid.NewGuid();
    private static readonly Guid _contentKey = Guid.NewGuid();
    private static readonly Guid _areaKey = Guid.NewGuid();

    [Test]
    public void Can_Validate_Null_When_Minimum_Configured_And_Not_Mandatory()
    {
        var editor = CreateValueEditor();

        var result = editor.Validate(null, false, null, PropertyValidationContext.Empty());

        Assert.IsEmpty(result);
    }

    [Test]
    public void Cannot_Validate_Null_When_Mandatory()
    {
        var editor = CreateValueEditor();

        var result = editor.Validate(null, true, null, PropertyValidationContext.Empty());

        Assert.AreEqual(1, result.Count());
        Assert.AreEqual(Constants.Validation.ErrorMessages.Properties.Missing, result.First().ErrorMessage);
    }

    [Test]
    public void Cannot_Validate_Empty_Blocks_When_Mandatory()
    {
        var editor = CreateValueEditor();

        var result = editor.Validate(CreateBlocksJson(0), true, null, PropertyValidationContext.Empty());

        Assert.AreEqual(1, result.Count());
        Assert.AreEqual(Constants.Validation.ErrorMessages.Properties.Empty, result.First().ErrorMessage);
    }

    [TestCase(0, true)]
    [TestCase(1, false)]
    [TestCase(2, true)]
    [TestCase(3, true)]
    public void Validates_Number_Of_Items_Is_Greater_Than_Or_Equal_To_Configured_Min(int numberOfBlocks, bool expectedSuccess)
    {
        var editor = CreateValueEditor();

        var result = editor.Validate(CreateBlocksJson(numberOfBlocks), false, null, PropertyValidationContext.Empty());

        if (expectedSuccess)
        {
            Assert.IsEmpty(result);
        }
        else
        {
            Assert.AreEqual(1, result.Count());
            Assert.AreEqual("validation_entriesShort", result.First().ErrorMessage);
        }
    }

    [TestCase(3, true)]
    [TestCase(4, true)]
    [TestCase(5, false)]
    public void Validates_Number_Of_Items_Is_Less_Than_Or_Equal_To_Configured_Max(int numberOfBlocks, bool expectedSuccess)
    {
        var editor = CreateValueEditor();

        var result = editor.Validate(CreateBlocksJson(numberOfBlocks), false, null, PropertyValidationContext.Empty());

        if (expectedSuccess)
        {
            Assert.IsEmpty(result);
        }
        else
        {
            Assert.AreEqual(1, result.Count());
            Assert.AreEqual("validation_entriesExceed", result.First().ErrorMessage);
        }
    }

    [TestCase(0, true)]
    [TestCase(1, false)]
    [TestCase(2, true)]
    [TestCase(3, true)]
    public void Validates_Number_Of_Items_In_Area_Is_Greater_Than_Or_Equal_To_Configured_Min(int numberOfItemsInArea, bool expectedSuccess)
    {
        var editor = CreateValueEditor(rootMin: null, rootMax: null, areaMinAllowed: 2);

        var result = editor.Validate(CreateBlocksJson(1, numberOfItemsInArea), false, null, PropertyValidationContext.Empty());

        if (expectedSuccess)
        {
            Assert.IsEmpty(result);
        }
        else
        {
            Assert.AreEqual(1, result.Count());
            Assert.AreEqual("validation_entriesAreasMismatch", result.First().ErrorMessage);
        }
    }

    private static JsonObject CreateBlocksJson(int numberOfBlocks, int numberOfItemsInArea = 0)
    {
        var layoutItems = new JsonArray();
        var contentData = new JsonArray();
        for (var i = 0; i < numberOfBlocks; i++)
        {
            layoutItems.Add(CreateLayoutBlockJson(numberOfItemsInArea));
            contentData.Add(CreateContentDataBlockJson());
        }

        return new JsonObject
        {
            {
                "layout", new JsonObject
                {
                    { Constants.PropertyEditors.Aliases.BlockGrid, layoutItems },
                }
            },
            { "contentData", contentData },
        };
    }

    private static JsonObject CreateLayoutBlockJson(int numberOfItemsInArea)
    {
        var areaItems = new JsonArray();
        for (var i = 0; i < numberOfItemsInArea; i++)
        {
            areaItems.Add(new JsonObject { { "contentKey", _contentKey } });
        }

        return new JsonObject
        {
            { "contentKey", _contentKey },
            {
                "areas", new JsonArray
                {
                    new JsonObject
                    {
                        { "key", _areaKey },
                        { "items", areaItems },
                    },
                }
            },
        };
    }

    private static JsonObject CreateContentDataBlockJson() =>
        new()
        {
            { "key", _contentKey },
            { "contentTypeKey", _contentTypeKey },
            { "values", new JsonArray() },
        };

    private static BlockGridEditorPropertyValueEditor CreateValueEditor(int? rootMin = 2, int? rootMax = 4, int? areaMinAllowed = null)
    {
        var localizedTextServiceMock = new Mock<ILocalizedTextService>();
        localizedTextServiceMock
            .Setup(x => x.Localize(
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<CultureInfo>(),
                It.IsAny<IDictionary<string, string>>()))
            .Returns((string key, string alias, CultureInfo culture, IDictionary<string, string> args) => $"{key}_{alias}");

        var jsonSerializer = new SystemTextJsonSerializer(new DefaultJsonSerializerEncoderFactory());
        var languageService = Mock.Of<ILanguageService>();

        var dataValueEditorFactoryMock = new Mock<IDataValueEditorFactory>();
        dataValueEditorFactoryMock
            .Setup(x => x.Create<TextOnlyValueEditor>(It.IsAny<object[]>()))
            .Returns(new TextOnlyValueEditor(
                new DataEditorAttribute("a"),
                Mock.Of<ILocalizedTextService>(),
                Mock.Of<IShortStringHelper>(),
                jsonSerializer,
                Mock.Of<IIOHelper>()));
        DataEditor textBoxEditor = new TextboxPropertyEditor(dataValueEditorFactoryMock.Object, Mock.Of<IIOHelper>());

        var propertyEditors = new PropertyEditorCollection(new DataEditorCollection(() => textBoxEditor.Yield()));

        var elementType = new ContentTypeBuilder()
            .WithKey(_contentTypeKey)
            .AddPropertyType()
                .WithAlias("message")
                .Done()
            .Build();
        var elementTypeCacheMock = new Mock<IBlockEditorElementTypeCache>();
        elementTypeCacheMock
            .Setup(x => x.GetMany(It.Is<IEnumerable<Guid>>(y => y.First() == _contentTypeKey)))
            .Returns([elementType]);

        return new BlockGridEditorPropertyValueEditor(
            new DataEditorAttribute("alias"),
            propertyEditors,
            new DataValueReferenceFactoryCollection(Enumerable.Empty<IDataValueReferenceFactory>, Mock.Of<ILogger<DataValueReferenceFactoryCollection>>()),
            Mock.Of<IDataTypeConfigurationCache>(),
            localizedTextServiceMock.Object,
            new NullLogger<BlockGridEditorPropertyValueEditor>(),
            Mock.Of<IShortStringHelper>(),
            jsonSerializer,
            elementTypeCacheMock.Object,
            Mock.Of<IPropertyValidationService>(),
            new BlockEditorVarianceHandler(languageService, Mock.Of<IContentTypeService>()),
            languageService,
            Mock.Of<IIOHelper>())
        {
            ConfigurationObject = new BlockGridConfiguration
            {
                ValidationLimit = new BlockGridConfiguration.NumberRange
                {
                    Min = rootMin,
                    Max = rootMax,
                },
                Blocks =
                [
                    new BlockGridConfiguration.BlockGridBlockConfiguration
                    {
                        ContentElementTypeKey = _contentTypeKey,
                        Areas =
                        [
                            new BlockGridConfiguration.BlockGridAreaConfiguration
                            {
                                Key = _areaKey,
                                MinAllowed = areaMinAllowed,
                            },
                        ],
                    },
                ],
            },
        };
    }
}
