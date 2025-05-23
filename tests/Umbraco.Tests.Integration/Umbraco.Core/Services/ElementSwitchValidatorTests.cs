using NUnit.Framework;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Extensions;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.PropertyEditors;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Services.ContentTypeEditing;
using Umbraco.Cms.Tests.Common.Attributes;
using Umbraco.Cms.Tests.Common.Builders;
using Umbraco.Cms.Tests.Common.Builders.Extensions;
using Umbraco.Cms.Tests.Common.Testing;
using Umbraco.Cms.Tests.Integration.Testing;

namespace Umbraco.Cms.Tests.Integration.Umbraco.Core.Services;

[TestFixture]
[UmbracoTest(Database = UmbracoTestOptions.Database.NewSchemaPerTest)]
internal sealed class ElementSwitchValidatorTests : UmbracoIntegrationTest
{
    private IElementSwitchValidator ElementSwitchValidator => GetRequiredService<IElementSwitchValidator>();

    private IContentTypeService ContentTypeService => GetRequiredService<IContentTypeService>();

    private IContentService ContentService => GetRequiredService<IContentService>();

    private IDataTypeService DataTypeService => GetRequiredService<IDataTypeService>();

    [TestCase(new[] { true }, 0, true, true, TestName = "E=>E No Ancestor or children")]
    [TestCase(new[] { false }, 0, false, true, TestName = "D=>D No Ancestor or children")]
    [TestCase(new[] { true }, 0, false, true, TestName = "E=>D No Ancestor or children")]
    [TestCase(new[] { false }, 0, true, true, TestName = "D=>E No Ancestor or children")]
    [TestCase(new[] { true, true }, 1, true, true, TestName = "E Valid Parent")]
    [TestCase(new[] { true, true }, 0, true, true, TestName = "E Valid Child")]
    [TestCase(new[] { false, false }, 1, false, true, TestName = "D Valid Parent")]
    [TestCase(new[] { false, false }, 0, false, true, TestName = "D Valid Child")]
    [TestCase(new[] { false, false }, 1, true, false, TestName = "E InValid Parent")]
    [TestCase(new[] { false, false }, 0, true, true, TestName = "E InValid Child")]
    [TestCase(new[] { true, true }, 1, false, false, TestName = "D InValid Parent")]
    [TestCase(new[] { true, true }, 0, false, true, TestName = "D InValid Child")]
    [TestCase(
        new[] { true, false, false, true, false },
        2,
        true,
        false,
        TestName = "D=>E InValid Child, Invalid Parent")]
    [TestCase(
        new[] { false, true, false, true, false },
        2,
        true,
        false,
        TestName = "D=>E InValid Child, Invalid Ancestor")]
    [TestCase(
        new[] { true, false, false, true, true },
        2,
        true,
        false,
        TestName = "D=>E Valid Children, Invalid Parent")]
    [TestCase(
        new[] { false, true, false, true, true },
        2,
        true,
        false,
        TestName = "D=>E Valid Children, Invalid Ancestor")]
    [TestCase(new[] { false, false, false, false, false }, 2, true, false, TestName = "D=>E mismatch")]
    [TestCase(new[] { false, false, true, false, false }, 2, false, true, TestName = "D=>E correction")]
    [TestCase(new[] { true, true, true, true, true }, 2, false, false, TestName = "E=>D mismatch")]
    [TestCase(new[] { true, true, false, true, true }, 2, true, true, TestName = "E=>D correction")]
    [LongRunning]
    public async Task AncestorsAreAligned(
        bool[] isElementDoctypeChain,
        int itemToTestIndex,
        bool itemToTestNewIsElementValue,
        bool validationShouldPass)
    {
        // Arrange
        IContentType? parentItem = null;
        IContentType? itemToTest = null;
        for (var index = 0; index < isElementDoctypeChain.Length; index++)
        {
            var itemIsElement = isElementDoctypeChain[index];
            var builder = new ContentTypeBuilder()
                .WithIsElement(itemIsElement);
            if (parentItem is not null)
            {
                builder.WithParentContentType(parentItem);
            }

            var contentType = builder.Build();
            await ContentTypeService.CreateAsync(contentType, Constants.Security.SuperUserKey);
            parentItem = contentType;
            if (index == itemToTestIndex)
            {
                itemToTest = contentType;
            }
        }

        // Act
        itemToTest!.IsElement = itemToTestNewIsElementValue;
        var result = await ElementSwitchValidator.AncestorsAreAlignedAsync(itemToTest);

        // Assert
        Assert.AreEqual(result, validationShouldPass);
    }

    [TestCase(new[] { true }, 0, true, true, TestName = "E=>E No Ancestor or children")]
    [TestCase(new[] { false }, 0, false, true, TestName = "D=>D No Ancestor or children")]
    [TestCase(new[] { true }, 0, false, true, TestName = "E=>D No Ancestor or children")]
    [TestCase(new[] { false }, 0, true, true, TestName = "D=>E No Ancestor or children")]
    [TestCase(new[] { true, true }, 1, true, true, TestName = "E Valid Parent")]
    [TestCase(new[] { true, true }, 0, true, true, TestName = "E Valid Child")]
    [TestCase(new[] { false, false }, 1, false, true, TestName = "D Valid Parent")]
    [TestCase(new[] { false, false }, 0, false, true, TestName = "D Valid Child")]
    [TestCase(new[] { false, false }, 1, true, true, TestName = "E InValid Parent")]
    [TestCase(new[] { false, false }, 0, true, false, TestName = "E InValid Child")]
    [TestCase(new[] { true, true }, 1, false, true, TestName = "D InValid Parent")]
    [TestCase(new[] { true, true }, 0, false, false, TestName = "D InValid Child")]
    [TestCase(
        new[] { true, false, false, true, false },
        2,
        true,
        false,
        TestName = "D=>E InValid Child, Invalid Parent")]
    [TestCase(
        new[] { false, true, false, true, false },
        2,
        true,
        false,
        TestName = "D=>E InValid Child, Invalid Ancestor")]
    [TestCase(
        new[] { true, false, false, true, true },
        2,
        true,
        true,
        TestName = "D=>E Valid Children, Invalid Parent")]
    [TestCase(new[] { false, true, false, true, true },
        2,
        true,
        true,
        TestName = "D=>E Valid Children, Invalid Ancestor")]
    [TestCase(new[] { false, false, false, false, false }, 2, true, false, TestName = "D=>E mismatch")]
    [TestCase(new[] { false, false, true, false, false }, 2, false, true, TestName = "D=>E correction")]
    [TestCase(new[] { true, true, true, true, true }, 2, false, false, TestName = "E=>D mismatch")]
    [TestCase(new[] { true, true, false, true, true }, 2, true, true, TestName = "E=>D correction")]
    [LongRunning]
    public async Task DescendantsAreAligned(
        bool[] isElementDoctypeChain,
        int itemToTestIndex,
        bool itemToTestNewIsElementValue,
        bool validationShouldPass)
    {
        // Arrange
        IContentType? parentItem = null;
        IContentType? itemToTest = null;
        for (var index = 0; index < isElementDoctypeChain.Length; index++)
        {
            var itemIsElement = isElementDoctypeChain[index];
            var builder = new ContentTypeBuilder()
                .WithIsElement(itemIsElement);
            if (parentItem is not null)
            {
                builder.WithParentContentType(parentItem);
            }

            var contentType = builder.Build();
            await ContentTypeService.CreateAsync(contentType, Constants.Security.SuperUserKey);
            parentItem = contentType;
            if (index == itemToTestIndex)
            {
                itemToTest = contentType;
            }
        }

        // Act
        itemToTest!.IsElement = itemToTestNewIsElementValue;
        var result = await ElementSwitchValidator.DescendantsAreAlignedAsync(itemToTest);

        // Assert
        Assert.AreEqual(result, validationShouldPass);
    }

    [TestCase(0, true, TestName = "No Content")]
    [TestCase(1, false, TestName = "One Content Item")]
    [TestCase(5, false, TestName = "Many Content Items")]
    public async Task DocumentToElementHasNoContent(int amountOfDocumentsCreated, bool validationShouldPass)
    {
        // Arrange
        var contentType = await SetupContentType(false);

        for (int i = 0; i < amountOfDocumentsCreated; i++)
        {
            var contentBuilder = new ContentBuilder().WithContentType(contentType);
            var content = contentBuilder.Build();
            ContentService.Save(content);
        }

        // Act
        contentType.IsElement = true;
        var result = await ElementSwitchValidator.DocumentToElementHasNoContentAsync(contentType);

        // Assert
        Assert.AreEqual(result, validationShouldPass);
    }

    // Since the full permutation table would result in 64 tests and more block editors might be added later,
    // we will at least test each single failure and a few combinations
    // used in none
    [TestCase(false, false, false, false, false, false, true)]
    // used in one
    [TestCase(true, false, false, false, false, false, false)]
    [TestCase(false, true, false, false, false, false, false)]
    [TestCase(false, false, true, false, false, false, false)]
    [TestCase(false, false, false, true, false, false, false)]
    [TestCase(false, false, false, false, true, false, false)]
    [TestCase(false, false, false, false, false, true, false)]
    // used in selection and setting
    [TestCase(true, true, false, false, false, false, false)]
    // used in 2 selections
    [TestCase(true, false, true, false, false, false, false)]
    // used in 2 settings
    [TestCase(false, true, false, false, false, true, false)]
    // used in all
    [TestCase(true, true, true, true, true, true, false)]
    public async Task ElementToDocumentNotUsedInBlockStructures(
        bool isUsedInBlockList,
        bool isUsedInBlockListBlockSetting,
        bool isUsedInBlockGrid,
        bool isUsedInBlockGridBlockSetting,
        bool isUsedInRte,
        bool isUsedInRteBlockSetting,
        bool validationShouldPass)
    {
        // Arrange
        var elementType = await SetupContentType(true);

        var otherElementType = await SetupContentType(true);

        if (isUsedInBlockList)
        {
            await SetupDataType(Constants.PropertyEditors.Aliases.BlockList, elementType.Key, null);
        }

        if (isUsedInBlockListBlockSetting)
        {
            await SetupDataType(Constants.PropertyEditors.Aliases.BlockList, otherElementType.Key, elementType.Key);
        }

        if (isUsedInBlockGrid)
        {
            await SetupDataType(Constants.PropertyEditors.Aliases.BlockGrid, elementType.Key, null);
        }

        if (isUsedInBlockGridBlockSetting)
        {
            await SetupDataType(Constants.PropertyEditors.Aliases.BlockGrid, otherElementType.Key, elementType.Key);
        }

        if (isUsedInRte)
        {
            await SetupDataType(Constants.PropertyEditors.Aliases.RichText, elementType.Key, null);
        }

        if (isUsedInRteBlockSetting)
        {
            await SetupDataType(Constants.PropertyEditors.Aliases.RichText, otherElementType.Key, elementType.Key);
        }

        // Act
        var result = await ElementSwitchValidator.ElementToDocumentNotUsedInBlockStructuresAsync(elementType);

        // Assert
        Assert.AreEqual(result, validationShouldPass);
    }

    private async Task<IContentType> SetupContentType(bool isElement)
    {
        var typeBuilder = new ContentTypeBuilder()
            .WithIsElement(isElement);
        var contentType = typeBuilder.Build();
        await ContentTypeService.CreateAsync(contentType, Constants.Security.SuperUserKey);
        return contentType;
    }

    // Constants.PropertyEditors.Aliases.BlockGrid
    private async Task SetupDataType(
        string editorAlias,
        Guid elementKey,
        Guid? elementSettingKey)
    {
        Dictionary<string, object> configuration;
        switch (editorAlias)
        {
            case Constants.PropertyEditors.Aliases.BlockGrid:
                configuration = GetBlockGridBaseConfiguration();
                break;
            case Constants.PropertyEditors.Aliases.RichText:
                configuration = GetRteBaseConfiguration();
                break;
            default:
                configuration = new Dictionary<string, object>();
                break;
        }

        SetBlockConfiguration(
            configuration,
            elementKey,
            elementSettingKey,
            editorAlias == Constants.PropertyEditors.Aliases.BlockGrid ? true : null);


        var dataTypeBuilder = new DataTypeBuilder()
            .WithId(0)
            .WithDatabaseType(ValueStorageType.Nvarchar)
            .AddEditor()
            .WithAlias(editorAlias);

        switch (editorAlias)
        {
            case Constants.PropertyEditors.Aliases.BlockGrid:
                dataTypeBuilder.WithConfigurationEditor(
                    new BlockGridConfigurationEditor(IOHelper) { DefaultConfiguration = configuration });
                break;
            case Constants.PropertyEditors.Aliases.BlockList:
                dataTypeBuilder.WithConfigurationEditor(
                    new BlockListConfigurationEditor(IOHelper) { DefaultConfiguration = configuration });
                break;
            case Constants.PropertyEditors.Aliases.RichText:
                dataTypeBuilder.WithConfigurationEditor(
                    new RichTextConfigurationEditor(IOHelper) { DefaultConfiguration = configuration });
                break;
        }

         var dataType = dataTypeBuilder.Done()
            .Build();

        await DataTypeService.CreateAsync(dataType, Constants.Security.SuperUserKey);
    }

    private void SetBlockConfiguration(
        Dictionary<string, object> dictionary,
        Guid? elementKey,
        Guid? elementSettingKey,
        bool? allowAtRoot)
    {
        if (elementKey is null)
        {
            return;
        }

        dictionary["blocks"] = new[] { BuildBlockConfiguration(elementKey.Value, elementSettingKey, allowAtRoot) };
    }

    private Dictionary<string, object> GetBlockGridBaseConfiguration()
        => new Dictionary<string, object> { ["gridColumns"] = 12 };

    private Dictionary<string, object> GetRteBaseConfiguration()
    {
        var dictionary = new Dictionary<string, object>
        {
            ["maxImageSize"] = 500,
            ["mode"] = "Classic",
            ["toolbar"] = new[]
            {
                "styles", "bold", "italic", "alignleft", "aligncenter", "alignright", "bullist", "numlist",
                "outdent", "indent", "sourcecode", "link", "umbmediapicker", "umbembeddialog"
            },
        };
        return dictionary;
    }

    private Dictionary<string, object> BuildBlockConfiguration(
        Guid? elementKey,
        Guid? elementSettingKey,
        bool? allowAtRoot)
    {
        var dictionary = new Dictionary<string, object>();
        if (allowAtRoot is not null)
        {
            dictionary.Add("allowAtRoot", allowAtRoot.Value);
        }

        dictionary.Add("contentElementTypeKey", elementKey.ToString());
        if (elementSettingKey is not null)
        {
            dictionary.Add("settingsElementTypeKey", elementSettingKey.ToString());
        }

        return dictionary;
    }
}
