using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Tests.Common.Builders;
using Umbraco.Cms.Tests.Common.Builders.Extensions;
using Umbraco.Tests.Search.Integration.Services;

namespace Umbraco.Tests.Search.Integration.Tests;

public class LabelPropertyValueHandlerTests : ContentTestBase
{
    private IContentType _contentType;

    [Test]
    public void AllLabelEditors_CanBeIndexed()
    {
        Content content = new ContentBuilder()
            .WithContentType(_contentType)
            .WithName("All Label Editors")
            .WithPropertyValues(
                new
                {
                    bigIntValue = 123456789L,
                    decimalValue = 56.78m,
                    integerValue = 1234,
                    stringValue = "The label value",
                    timeValue = DateTime.MinValue.Add(new TimeSpan(1, 2, 3, 4)),
                    dateTimeValue = new DateTime(2004, 05, 06, 07, 08, 09)
                })
            .Build();

        ContentService.Save(content);
        ContentService.Publish(content, ["*"]);

        IReadOnlyList<TestIndexDocument> documents = IndexerAndSearcher.Dump(IndexAliases.PublishedContent);
        Assert.That(documents, Has.Count.EqualTo(1));

        TestIndexDocument document = documents.Single();
        Assert.Multiple(() =>
        {
            var bigIntValue = document.Fields.FirstOrDefault(f => f.FieldName == "bigIntValue")?.Value.Integers?.SingleOrDefault();
            Assert.That(bigIntValue, Is.EqualTo(123456789));

            var decimalValue = document.Fields.FirstOrDefault(f => f.FieldName == "decimalValue")?.Value.Decimals?.SingleOrDefault();
            Assert.That(decimalValue, Is.EqualTo(56.78m));

            var integerValue = document.Fields.FirstOrDefault(f => f.FieldName == "integerValue")?.Value.Integers?.SingleOrDefault();
            Assert.That(integerValue, Is.EqualTo(1234));

            var stringValue = document.Fields.FirstOrDefault(f => f.FieldName == "stringValue")?.Value.Texts?.SingleOrDefault();
            Assert.That(stringValue, Is.EqualTo("The label value"));

            DateTimeOffset? dateTimeValue = document.Fields.FirstOrDefault(f => f.FieldName == "dateTimeValue")?.Value.DateTimeOffsets?.SingleOrDefault();
            Assert.That(dateTimeValue, Is.EqualTo(new DateTimeOffset(new DateOnly(2004, 05, 06), new TimeOnly(07, 08, 09), TimeSpan.Zero)));

            // time is unsupported
            Assert.That(document.Fields.Any(f => f.FieldName == "timeValue"), Is.False);
        });
    }

    [TestCase(long.MaxValue)]
    [TestCase(long.MinValue)]
    public void LongValueWithOverflowAsInteger_IsNotIndexed(long value)
    {
        Content content = new ContentBuilder()
            .WithContentType(_contentType)
            .WithName("All Label Editors")
            .WithPropertyValues(
                new
                {
                    bigIntValue = value
                })
            .Build();

        ContentService.Save(content);
        ContentService.Publish(content, ["*"]);

        IReadOnlyList<TestIndexDocument> documents = IndexerAndSearcher.Dump(IndexAliases.PublishedContent);
        Assert.That(documents, Has.Count.EqualTo(1));

        TestIndexDocument document = documents.Single();
        Assert.That(document.Fields.Any(f => f.FieldName == "bigIntValue"), Is.False);
    }

    [TestCase(null)]
    [TestCase("")]
    public void EmptyValues_AreNotIndexed(string? value)
    {
        Content content = new ContentBuilder()
            .WithContentType(_contentType)
            .WithName("All Label Editors")
            .WithPropertyValues(
                new
                {
                    bigIntValue = value,
                    decimalValue = value,
                    integerValue = value,
                    stringValue = value,
                    timeValue = value,
                    dateTimeValue = value
                })
            .Build();

        ContentService.Save(content);
        ContentService.Publish(content, ["*"]);

        IReadOnlyList<TestIndexDocument> documents = IndexerAndSearcher.Dump(IndexAliases.PublishedContent);
        Assert.That(documents, Has.Count.EqualTo(1));

        TestIndexDocument document = documents.Single();
        Assert.Multiple(() =>
        {
            Assert.That(document.Fields.Any(f => f.FieldName == "bigIntValue"), Is.False);
            Assert.That(document.Fields.Any(f => f.FieldName == "decimalValue"), Is.False);
            Assert.That(document.Fields.Any(f => f.FieldName == "integerValue"), Is.False);
            Assert.That(document.Fields.Any(f => f.FieldName == "stringValue"), Is.False);
            Assert.That(document.Fields.Any(f => f.FieldName == "timeValue"), Is.False);
            Assert.That(document.Fields.Any(f => f.FieldName == "dateTimeValue"), Is.False);
        });
    }

    [SetUp]
    protected async Task CreateAllLabelEditorsContentType()
    {
        _contentType = new ContentTypeBuilder()
            .WithAlias("allLabelEditors")
            .AddPropertyType()
            .WithAlias("bigIntValue")
            .WithDataTypeId(Constants.DataTypes.LabelBigint)
            .WithPropertyEditorAlias(Constants.PropertyEditors.Aliases.Label)
            .Done()
            .AddPropertyType()
            .WithAlias("decimalValue")
            .WithDataTypeId(Constants.DataTypes.LabelDecimal)
            .WithPropertyEditorAlias(Constants.PropertyEditors.Aliases.Label)
            .Done()
            .AddPropertyType()
            .WithAlias("integerValue")
            .WithDataTypeId(Constants.DataTypes.LabelInt)
            .WithPropertyEditorAlias(Constants.PropertyEditors.Aliases.Label)
            .Done()
            .AddPropertyType()
            .WithAlias("stringValue")
            .WithDataTypeId(Constants.DataTypes.LabelString)
            .WithPropertyEditorAlias(Constants.PropertyEditors.Aliases.Label)
            .Done()
            .AddPropertyType()
            .WithAlias("timeValue")
            .WithDataTypeId(Constants.DataTypes.LabelTime)
            .WithPropertyEditorAlias(Constants.PropertyEditors.Aliases.Label)
            .Done()
            .AddPropertyType()
            .WithAlias("dateTimeValue")
            .WithDataTypeId(Constants.DataTypes.LabelDateTime)
            .WithPropertyEditorAlias(Constants.PropertyEditors.Aliases.Label)
            .Done()
            .Build();

        await ContentTypeService.CreateAsync(_contentType, Constants.Security.SuperUserKey);

        IndexerAndSearcher.Reset();
    }
}
