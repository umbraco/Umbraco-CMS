using NUnit.Framework;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.PropertyEditors;
using Umbraco.Cms.Core.Serialization;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Search.Core.Models.Indexing;
using Umbraco.Cms.Tests.Common.Builders;
using Umbraco.Cms.Tests.Common.Builders.Extensions;
using Umbraco.Cms.Tests.Integration.Testing.Search;

namespace Umbraco.Cms.Tests.Integration.Umbraco.Search.Core;

public class MultipleDocumentPickerPropertyValueHandlerTests : ContentTestBase
{
    private IContentType _contentType;

    [Test]
    public void PickedDocuments_CanBeIndexed()
    {
        Content content = CreateContent(
            "[\"7c7ad126-bdbc-46c1-8cc1-c281bf575d97\",\"b9cc8a2e-9a02-4bbe-b0ca-38e197316517\"]");

        IEnumerable<string>? keywords = IndexedKeywords(content);
        Assert.That(
            keywords,
            Is.EqualTo(new[] { "7c7ad126-bdbc-46c1-8cc1-c281bf575d97", "b9cc8a2e-9a02-4bbe-b0ca-38e197316517" }).AsCollection);
    }

    [Test]
    public void ASinglePickedDocument_CanBeIndexed()
    {
        Content content = CreateContent("[\"7c7ad126-bdbc-46c1-8cc1-c281bf575d97\"]");

        IEnumerable<string>? keywords = IndexedKeywords(content);
        Assert.That(keywords, Is.EqualTo(new[] { "7c7ad126-bdbc-46c1-8cc1-c281bf575d97" }).AsCollection);
    }

    [TestCase("[]")]
    [TestCase("")]
    public void AnEmptyValue_YieldsNoField(string value)
    {
        Content content = CreateContent(value);

        Assert.That(IndexedField(content), Is.Null);
    }

    private Content CreateContent(string pickerValue)
    {
        Content content = new ContentBuilder()
            .WithContentType(_contentType)
            .WithName("Multiple Document Picker")
            .WithPropertyValues(new { pickerValue })
            .Build();

        ContentService.Save(content);
        ContentService.Publish(content, ["*"]);

        return content;
    }

    private IndexField? IndexedField(Content content)
    {
        IReadOnlyList<TestIndexDocument> documents = IndexerAndSearcher.Dump(IndexAliases.PublishedContent);
        Assert.That(documents, Has.Count.EqualTo(1));

        return documents.Single().Fields.FirstOrDefault(f => f.FieldName == "pickerValue");
    }

    private IEnumerable<string>? IndexedKeywords(Content content) => IndexedField(content)?.Value.Keywords;

    [SetUp]
    protected async Task CreateMultipleDocumentPickerContentType()
    {
        IDataTypeService dataTypeService = GetRequiredService<IDataTypeService>();
        PropertyEditorCollection propertyEditorCollection = GetRequiredService<PropertyEditorCollection>();
        IConfigurationEditorJsonSerializer configurationEditorJsonSerializer = GetRequiredService<IConfigurationEditorJsonSerializer>();

        var pickerDataType = new DataType(
            propertyEditorCollection[Constants.PropertyEditors.Aliases.MultipleDocumentPicker],
            configurationEditorJsonSerializer)
        {
            Name = "Multiple document picker",
            DatabaseType = ValueStorageType.Ntext,
            ParentId = Constants.System.Root,
            CreateDate = DateTime.UtcNow,
        };
        await dataTypeService.CreateAsync(pickerDataType, Constants.Security.SuperUserKey);

        _contentType = new ContentTypeBuilder()
            .WithAlias("multipleDocumentPickerEditor")
            .AddPropertyType()
            .WithAlias("pickerValue")
            .WithDataTypeId(pickerDataType.Id)
            .WithPropertyEditorAlias(Constants.PropertyEditors.Aliases.MultipleDocumentPicker)
            .Done()
            .Build();
        await ContentTypeService.CreateAsync(_contentType, Constants.Security.SuperUserKey);

        IndexerAndSearcher.Reset();
    }
}
