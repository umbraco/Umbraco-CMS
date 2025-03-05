using System.Globalization;
using System.Text.Json.Nodes;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using NUnit.Framework;
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
using static Umbraco.Cms.Core.PropertyEditors.BlockListPropertyEditorBase;

namespace Umbraco.Cms.Tests.UnitTests.Umbraco.Core.PropertyEditors;

[TestFixture]
public class BlockListEditorPropertyValueEditorTests
{
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

    private static JsonObject CreateBlocksJson(int numberOfBlocks)
    {
        var layoutItems = new JsonArray();
        var contentData = new JsonArray();
        for (int i = 0; i < numberOfBlocks; i++)
        {
            layoutItems.Add(CreateLayoutBlockJson());
            contentData.Add(CreateContentDataBlockJson());
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
            { "contentKey", Guid.NewGuid() },
        };

    private static JsonObject CreateContentDataBlockJson() =>
        new()
        {
            { "contentTypeKey", Guid.Parse("01935a73-c86b-4521-9dcb-ad7cea402215") },
            { "key", Guid.NewGuid() },
            {
                "values",
                new JsonArray
                {
                    new JsonObject
                    {
                        { "editorAlias", "Umbraco.TextBox" },
                        { "alias", "message" },
                        { "value", "Hello" },
                    },
                }
            }
        };

    private static BlockListEditorPropertyValueEditor CreateValueEditor()
    {
        var localizedTextServiceMock = new Mock<ILocalizedTextService>();
        localizedTextServiceMock.Setup(x => x.Localize(
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<CultureInfo>(),
        It.IsAny<IDictionary<string, string>>()))
            .Returns((string key, string alias, CultureInfo culture, IDictionary<string, string> args) => $"{key}_{alias}");

        var jsonSerializer = new SystemTextJsonSerializer();
        var languageService = Mock.Of<ILanguageService>();

        return new BlockListEditorPropertyValueEditor(
            new DataEditorAttribute("alias"),
            new BlockListEditorDataConverter(jsonSerializer),
            new(new DataEditorCollection(() => [])),
            new DataValueReferenceFactoryCollection(Enumerable.Empty<IDataValueReferenceFactory>),
            Mock.Of<IDataTypeConfigurationCache>(),
            Mock.Of<IBlockEditorElementTypeCache>(),
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
}
