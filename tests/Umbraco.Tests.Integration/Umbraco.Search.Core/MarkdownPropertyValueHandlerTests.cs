using NUnit.Framework;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Search.Core.Models.Indexing;
using Umbraco.Cms.Tests.Common.Builders;
using Umbraco.Cms.Tests.Common.Builders.Extensions;
using Umbraco.Cms.Tests.Integration.Testing.Search;

namespace Umbraco.Cms.Tests.Integration.Umbraco.Search.Core;

public class MarkdownPropertyValueHandlerTests : ContentTestBase
{
    private IContentType _contentType;

    [Test]
    public void MarkdownEditor_CanIndexAllTextRelevanceLevels()
    {
        Content content = new ContentBuilder()
            .WithContentType(_contentType)
            .WithName("Markdown Editor")
            .WithPropertyValues(
                new
                {
                    markdownValue = """
                                    # H1 Heading #1

                                    Paragraph #1
                                    Paragraph #2

                                    ## H2 Heading #1

                                    ### H3 Heading #1

                                    Paragraph #3
                                    Paragraph #4

                                    ## H2 Heading #2

                                    ### H3 Heading #2

                                    Paragraph #5

                                    ### H3 Heading #3

                                    Paragraph #6
                                    """

                })
            .Build();

        ContentService.Save(content);

        IReadOnlyList<TestIndexDocument> documents = IndexerAndSearcher.Dump(IndexAliases.DraftContent);
        Assert.That(documents, Has.Count.EqualTo(1));

        TestIndexDocument document = documents.Single();
        IndexValue? markdownValue = document.Fields.FirstOrDefault(f => f.FieldName == "markdownValue")?.Value;
        Assert.That(markdownValue, Is.Not.Null);

        Assert.Multiple(() =>
        {
            CollectionAssert.AreEqual(new[] { "H1 Heading #1" }, markdownValue.TextsR1);
            CollectionAssert.AreEqual(new[] { "H2 Heading #1", "H2 Heading #2" }, markdownValue.TextsR2);
            CollectionAssert.AreEqual(new[] { "H3 Heading #1", "H3 Heading #2", "H3 Heading #3" }, markdownValue.TextsR3);

            CollectionAssert.AreEqual(new[] { "Paragraph #1 Paragraph #2", "Paragraph #3 Paragraph #4", "Paragraph #5", "Paragraph #6" }, markdownValue.Texts);
        });
    }

    [Test]
    public void MarkdownEditor_IncludesTextFromNestedTags()
    {
        Content content = new ContentBuilder()
            .WithContentType(_contentType)
            .WithName("Markdown Editor")
            .WithPropertyValues(
                new
                {
                    markdownValue = """
                                    Some <strong>bold</strong> text

                                    <a href="https://some.where">A link to somewhere</a>
                                    """

                })
            .Build();

        ContentService.Save(content);

        IReadOnlyList<TestIndexDocument> documents = IndexerAndSearcher.Dump(IndexAliases.DraftContent);
        Assert.That(documents, Has.Count.EqualTo(1));

        TestIndexDocument document = documents.Single();
        IndexValue? markdownValue = document.Fields.FirstOrDefault(f => f.FieldName == "markdownValue")?.Value;
        Assert.That(markdownValue, Is.Not.Null);
        CollectionAssert.AreEqual(new[] { "Some bold text", "A link to somewhere" }, markdownValue.Texts);
    }

    [TestCase(null)]
    [TestCase("")]
    public void MarkdownEditor_IgnoresEmptyValues(object? value)
    {
        Content content = new ContentBuilder()
            .WithContentType(_contentType)
            .WithName("Markdown Editor")
            .WithPropertyValues(
                new
                {
                    markdownValue = value
                })
            .Build();

        ContentService.Save(content);

        IReadOnlyList<TestIndexDocument> documents = IndexerAndSearcher.Dump(IndexAliases.DraftContent);
        Assert.That(documents, Has.Count.EqualTo(1));

        TestIndexDocument document = documents.Single();
        Assert.That(document.Fields.Any(field => field.FieldName == "markdownValue"), Is.False);
    }

    [TestCase(true)]
    [TestCase(123)]
    [TestCase("A text string")]
    public void MarkdownEditor_IncludesSimpleValues(object value)
    {
        Content content = new ContentBuilder()
            .WithContentType(_contentType)
            .WithName("Markdown Editor")
            .WithPropertyValues(
                new
                {
                    markdownValue = value
                })
            .Build();

        ContentService.Save(content);

        IReadOnlyList<TestIndexDocument> documents = IndexerAndSearcher.Dump(IndexAliases.DraftContent);
        Assert.That(documents, Has.Count.EqualTo(1));


        TestIndexDocument document = documents.Single();
        IndexValue? markdownValue = document.Fields.FirstOrDefault(f => f.FieldName == "markdownValue")?.Value;
        Assert.That(markdownValue, Is.Not.Null);
        CollectionAssert.AreEqual(new[] { value.ToString() }, markdownValue.Texts);
    }

    [SetUp]
    public async Task SetupTest()
    {
        DataType markdownDataType = new DataTypeBuilder()
            .WithId(0)
            .WithDatabaseType(ValueStorageType.Ntext)
            .WithName("Markdown Editor")
            .AddEditor()
            .WithAlias(Constants.PropertyEditors.Aliases.MarkdownEditor)
            .Done()
            .Build();
        await GetRequiredService<IDataTypeService>().CreateAsync(markdownDataType, Constants.Security.SuperUserKey);

        _contentType = new ContentTypeBuilder()
            .WithAlias("markdownEditor")
            .AddPropertyType()
            .WithAlias("markdownValue")
            .WithDataTypeId(markdownDataType.Id)
            .WithPropertyEditorAlias(Constants.PropertyEditors.Aliases.MarkdownEditor)
            .Done()
            .Build();

        await ContentTypeService.CreateAsync(_contentType, Constants.Security.SuperUserKey);

        IndexerAndSearcher.Reset();
    }
}
