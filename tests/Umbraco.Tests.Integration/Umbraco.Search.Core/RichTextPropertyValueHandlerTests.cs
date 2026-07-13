using NUnit.Framework;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Search.Core.Models.Indexing;
using Umbraco.Cms.Tests.Common.Builders;
using Umbraco.Cms.Tests.Common.Builders.Extensions;
using Umbraco.Cms.Tests.Integration.Testing.Search;

namespace Umbraco.Cms.Tests.Integration.Umbraco.Search.Core;

public class RichTextPropertyValueHandlerTests : ContentTestBase
{
    private IContentType _contentType;

    [Test]
    public void RichTextEditor_CanIndexAllTextRelevanceLevels()
    {
        Content content = new ContentBuilder()
            .WithContentType(_contentType)
            .WithName("RichText Editor")
            .WithPropertyValues(
                new
                {
                    richTextValue = """
                                    <h1>H1 Heading #1</h1>
                                    <p>Paragraph #1<br/>Paragraph #2</p>
                                    <h2>H2 Heading #1</h2>
                                    <h3>H3 Heading #1</h3>
                                    <p>Paragraph #3<br/>Paragraph #4</p>
                                    <h2>H2 Heading #2</h2>
                                    <h3>H3 Heading #2</h3>
                                    <p>Paragraph #5</p>
                                    <h3>H3 Heading #3</h3>
                                    <p>Paragraph #6</p>
                                    <ul><li>List Item #1</li><li>List Item #2</li></ul>
                                    """
                })
            .Build();

        ContentService.Save(content);

        IReadOnlyList<TestIndexDocument> documents = IndexerAndSearcher.Dump(IndexAliases.DraftContent);
        Assert.That(documents, Has.Count.EqualTo(1));

        TestIndexDocument document = documents.Single();
        IndexValue? richTextValue = document.Fields.FirstOrDefault(f => f.FieldName == "richTextValue")?.Value;
        Assert.That(richTextValue, Is.Not.Null);

        Assert.Multiple(() =>
        {
            CollectionAssert.AreEqual(new [] { "H1 Heading #1" }, richTextValue.TextsR1);
            CollectionAssert.AreEqual(new [] { "H2 Heading #1", "H2 Heading #2" }, richTextValue.TextsR2);
            CollectionAssert.AreEqual(new [] { "H3 Heading #1", "H3 Heading #2", "H3 Heading #3" }, richTextValue.TextsR3);

            CollectionAssert.AreEqual(new [] { "Paragraph #1 Paragraph #2", "Paragraph #3 Paragraph #4", "Paragraph #5", "Paragraph #6", "List Item #1 List Item #2" }, richTextValue.Texts);
        });
    }

    [Test]
    public void RichTextEditor_IncludesTextFromNestedTags()
    {
        Content content = new ContentBuilder()
            .WithContentType(_contentType)
            .WithName("RichText Editor")
            .WithPropertyValues(
                new
                {
                    richTextValue = """
                               <p>Some <strong>bold</strong> text</p>
                               <p><a href="https://some.where">A link to somewhere</a></p>
                               """
                })
            .Build();

        ContentService.Save(content);

        IReadOnlyList<TestIndexDocument> documents = IndexerAndSearcher.Dump(IndexAliases.DraftContent);
        Assert.That(documents, Has.Count.EqualTo(1));

        TestIndexDocument document = documents.Single();
        IndexValue? richTextValue = document.Fields.FirstOrDefault(f => f.FieldName == "richTextValue")?.Value;
        Assert.That(richTextValue, Is.Not.Null);
        CollectionAssert.AreEqual(new [] { "Some bold text", "A link to somewhere" }, richTextValue.Texts);
    }

    [TestCase(null)]
    [TestCase("")]
    public void RichTextEditor_IgnoresEmptyAndInvalidValues(object? value)
    {
        Content content = new ContentBuilder()
            .WithContentType(_contentType)
            .WithName("RichText Editor")
            .WithPropertyValues(
                new
                {
                    richTextValue = value
                })
            .Build();

        ContentService.Save(content);

        IReadOnlyList<TestIndexDocument> documents = IndexerAndSearcher.Dump(IndexAliases.DraftContent);
        Assert.That(documents, Has.Count.EqualTo(1));

        TestIndexDocument document = documents.Single();
        Assert.That(document.Fields.Any(field => field.FieldName == "richTextValue"), Is.False);
    }

    [TestCase(true)]
    [TestCase(123)]
    [TestCase("This is not valid HTML")]
    public void RichTextEditor_IncludesSimpleValues(object value)
    {
        Content content = new ContentBuilder()
            .WithContentType(_contentType)
            .WithName("RichText Editor")
            .WithPropertyValues(
                new
                {
                    richTextValue = value
                })
            .Build();

        ContentService.Save(content);

        IReadOnlyList<TestIndexDocument> documents = IndexerAndSearcher.Dump(IndexAliases.DraftContent);
        Assert.That(documents, Has.Count.EqualTo(1));

        TestIndexDocument document = documents.Single();
        IndexValue? richTextValue = document.Fields.FirstOrDefault(f => f.FieldName == "richTextValue")?.Value;
        Assert.That(richTextValue, Is.Not.Null);
        CollectionAssert.AreEqual(new [] { value.ToString() }, richTextValue.Texts);
    }

    [SetUp]
    public async Task SetupTest()
    {
        DataType richTextDataType = new DataTypeBuilder()
            .WithId(0)
            .WithDatabaseType(ValueStorageType.Ntext)
            .WithName("RichText Editor")
            .AddEditor()
            .WithAlias(Constants.PropertyEditors.Aliases.RichText)
            .Done()
            .Build();
        await GetRequiredService<IDataTypeService>().CreateAsync(richTextDataType, Constants.Security.SuperUserKey);

        _contentType = new ContentTypeBuilder()
            .WithAlias("richTextEditor")
            .AddPropertyType()
            .WithAlias("richTextValue")
            .WithDataTypeId(richTextDataType.Id)
            .WithPropertyEditorAlias(Constants.PropertyEditors.Aliases.RichText)
            .Done()
            .Build();

        await ContentTypeService.CreateAsync(_contentType, Constants.Security.SuperUserKey);

        IndexerAndSearcher.Reset();
    }
}
