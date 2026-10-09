using NUnit.Framework;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.PropertyEditors.ValueConverters;
using Umbraco.Cms.Core.Serialization;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Tests.Common.Builders;
using Umbraco.Cms.Tests.Common.Builders.Extensions;
using Umbraco.Cms.Tests.Integration.Testing.Search;

namespace Umbraco.Cms.Tests.Integration.Umbraco.Search.Core;

// NOTE:
// Covers the four newer date/time editors (Umbraco.DateOnly, Umbraco.TimeOnly, Umbraco.DateTimeUnspecified,
// Umbraco.DateTimeWithTimeZone), which persist their value as a JSON-serialized DateTimeDto rather than a raw
// DateTime. The legacy Umbraco.DateTime editor is covered elsewhere (SimplePropertyValueHandlerTests) but is
// re-verified here too, to guard the shared handler's legacy branch while extending it.
public class DateTimeEditorsPropertyValueHandlerTests : PropertyValueHandlerTestsBase
{
    private const string ContentTypeAlias = "dateTimeEditors";

    private IJsonSerializer JsonSerializer => GetRequiredService<IJsonSerializer>();

    [SetUp]
    public async Task SetUp()
    {
        IndexerAndSearcher.Reset();
        await CreateDateTimeEditorsContentType();
    }

    [Test]
    public void NewDateTimeEditors_CanBeIndexed()
    {
        Content content = new ContentBuilder()
            .WithContentType(GetContentType())
            .WithName("Date Time Editors")
            .WithPropertyValues(new
            {
                dateOnlyValue = JsonSerializer.Serialize(new DateTimeValueConverterBase.DateTimeDto
                {
                    Date = new DateTimeOffset(2001, 02, 03, 0, 0, 0, TimeSpan.Zero)
                }),
                timeOnlyValue = JsonSerializer.Serialize(new DateTimeValueConverterBase.DateTimeDto
                {
                    Date = new DateTimeOffset(1, 1, 1, 07, 08, 09, TimeSpan.Zero)
                }),
                dateTimeUnspecifiedValue = JsonSerializer.Serialize(new DateTimeValueConverterBase.DateTimeDto
                {
                    Date = new DateTimeOffset(2004, 05, 06, 07, 08, 09, TimeSpan.Zero)
                }),
                dateTimeWithTimeZoneValue = JsonSerializer.Serialize(new DateTimeValueConverterBase.DateTimeDto
                {
                    Date = new DateTimeOffset(2004, 05, 06, 07, 08, 09, TimeSpan.FromHours(2)),
                    TimeZone = "Europe/Copenhagen"
                }),
                legacyDateTimeValue = new DateTime(2001, 02, 03, 07, 08, 09)
            })
            .Build();

        ContentService.Save(content);
        ContentService.Publish(content, ["*"]);

        TestIndexDocument document = IndexerAndSearcher.Dump(IndexAliases.PublishedContent).Single();

        Assert.Multiple(() =>
        {
            AssertDateTimeOffset("dateOnlyValue", new DateTimeOffset(2001, 02, 03, 0, 0, 0, TimeSpan.Zero));
            AssertDateTimeOffset("timeOnlyValue", new DateTimeOffset(1, 1, 1, 07, 08, 09, TimeSpan.Zero));
            AssertDateTimeOffset("dateTimeUnspecifiedValue", new DateTimeOffset(2004, 05, 06, 07, 08, 09, TimeSpan.Zero));

            // stored with a +02:00 offset - the indexed value must be normalized to UTC (05:08:09Z)
            AssertDateTimeOffset("dateTimeWithTimeZoneValue", new DateTimeOffset(2004, 05, 06, 05, 08, 09, TimeSpan.Zero));

            // legacy DateTime editor must keep working after extending the handler's CanHandle
            AssertDateTimeOffset("legacyDateTimeValue", new DateTimeOffset(2001, 02, 03, 07, 08, 09, TimeSpan.Zero));
        });

        return;

        void AssertDateTimeOffset(string fieldName, DateTimeOffset expected)
        {
            DateTimeOffset? actual = document.Fields.FirstOrDefault(f => f.FieldName == fieldName)?.Value.DateTimeOffsets?.SingleOrDefault();
            Assert.That(actual, Is.EqualTo(expected));
        }
    }

    [Test]
    public void UnsetValue_IsNotIndexed()
    {
        Content content = new ContentBuilder()
            .WithContentType(GetContentType())
            .WithName("Date Time Editors")
            .Build();

        ContentService.Save(content);
        ContentService.Publish(content, ["*"]);

        TestIndexDocument document = IndexerAndSearcher.Dump(IndexAliases.PublishedContent).Single();
        Assert.That(document.Fields.Any(f => f.FieldName == "dateOnlyValue"), Is.False);
    }

    [Test]
    public void MalformedValue_IsNotIndexed()
    {
        Content content = new ContentBuilder()
            .WithContentType(GetContentType())
            .WithName("Date Time Editors")
            .WithPropertyValues(new
            {
                dateOnlyValue = "this is not valid JSON"
            })
            .Build();

        ContentService.Save(content);
        ContentService.Publish(content, ["*"]);

        TestIndexDocument document = IndexerAndSearcher.Dump(IndexAliases.PublishedContent).Single();
        Assert.That(document.Fields.Any(f => f.FieldName == "dateOnlyValue"), Is.False);
    }

    [Test]
    public void WrongShapeJsonValue_IsNotIndexed_AndDoesNotPreventDocumentIndexing()
    {
        // NOTE: only saved (draft index), not published - publishing a wrong-shape stored value fails
        //       property validation with an unrelated, pre-existing exception in TypedJsonValidatorRunner.
        Content content = new ContentBuilder()
            .WithContentType(GetContentType())
            .WithName("Date Time Editors")
            .WithPropertyValues(new
            {
                dateOnlyValue = "[1,2,3]",
                dateTimeUnspecifiedValue = JsonSerializer.Serialize(new DateTimeValueConverterBase.DateTimeDto
                {
                    Date = new DateTimeOffset(2004, 05, 06, 07, 08, 09, TimeSpan.Zero)
                })
            })
            .Build();

        ContentService.Save(content);

        TestIndexDocument document = IndexerAndSearcher.Dump(IndexAliases.DraftContent).Single();
        Assert.Multiple(() =>
        {
            Assert.That(document.Fields.Any(f => f.FieldName == "dateOnlyValue"), Is.False);

            DateTimeOffset? dateTimeUnspecifiedValue = document.Fields.FirstOrDefault(f => f.FieldName == "dateTimeUnspecifiedValue")?.Value.DateTimeOffsets?.SingleOrDefault();
            Assert.That(dateTimeUnspecifiedValue, Is.EqualTo(new DateTimeOffset(2004, 05, 06, 07, 08, 09, TimeSpan.Zero)));
        });
    }

    private IContentType GetContentType() => ContentTypeService.Get(ContentTypeAlias)
        ?? throw new InvalidOperationException($"Could not find the content type \"{ContentTypeAlias}\"");

    private async Task CreateDateTimeEditorsContentType()
    {
        IDataTypeService dataTypeService = GetRequiredService<IDataTypeService>();

        DataType dateOnlyDataType = BuildJsonDateDataType(Constants.PropertyEditors.Aliases.DateOnly, "Date Only");
        await dataTypeService.CreateAsync(dateOnlyDataType, Constants.Security.SuperUserKey);

        DataType timeOnlyDataType = BuildJsonDateDataType(Constants.PropertyEditors.Aliases.TimeOnly, "Time Only");
        await dataTypeService.CreateAsync(timeOnlyDataType, Constants.Security.SuperUserKey);

        DataType dateTimeUnspecifiedDataType = BuildJsonDateDataType(Constants.PropertyEditors.Aliases.DateTimeUnspecified, "Date Time Unspecified");
        await dataTypeService.CreateAsync(dateTimeUnspecifiedDataType, Constants.Security.SuperUserKey);

        DataType dateTimeWithTimeZoneDataType = BuildJsonDateDataType(Constants.PropertyEditors.Aliases.DateTimeWithTimeZone, "Date Time With Time Zone");
        await dataTypeService.CreateAsync(dateTimeWithTimeZoneDataType, Constants.Security.SuperUserKey);

        DataType legacyDateTimeDataType = new DataTypeBuilder()
            .WithId(0)
            .WithDatabaseType(ValueStorageType.Date)
            .WithName("Legacy Date Time")
            .AddEditor()
            .WithAlias(Constants.PropertyEditors.Aliases.DateTime)
            .Done()
            .Build();
        await dataTypeService.CreateAsync(legacyDateTimeDataType, Constants.Security.SuperUserKey);

        IContentType contentType = new ContentTypeBuilder()
            .WithAlias(ContentTypeAlias)
            .AddPropertyType()
            .WithAlias("dateOnlyValue")
            .WithDataTypeId(dateOnlyDataType.Id)
            .WithPropertyEditorAlias(Constants.PropertyEditors.Aliases.DateOnly)
            .Done()
            .AddPropertyType()
            .WithAlias("timeOnlyValue")
            .WithDataTypeId(timeOnlyDataType.Id)
            .WithPropertyEditorAlias(Constants.PropertyEditors.Aliases.TimeOnly)
            .Done()
            .AddPropertyType()
            .WithAlias("dateTimeUnspecifiedValue")
            .WithDataTypeId(dateTimeUnspecifiedDataType.Id)
            .WithPropertyEditorAlias(Constants.PropertyEditors.Aliases.DateTimeUnspecified)
            .Done()
            .AddPropertyType()
            .WithAlias("dateTimeWithTimeZoneValue")
            .WithDataTypeId(dateTimeWithTimeZoneDataType.Id)
            .WithPropertyEditorAlias(Constants.PropertyEditors.Aliases.DateTimeWithTimeZone)
            .Done()
            .AddPropertyType()
            .WithAlias("legacyDateTimeValue")
            .WithDataTypeId(legacyDateTimeDataType.Id)
            .WithPropertyEditorAlias(Constants.PropertyEditors.Aliases.DateTime)
            .Done()
            .Build();

        await ContentTypeService.CreateAsync(contentType, Constants.Security.SuperUserKey);
    }

    private static DataType BuildJsonDateDataType(string editorAlias, string name)
        => new DataTypeBuilder()
            .WithId(0)
            .WithDatabaseType(ValueStorageType.Nvarchar)
            .WithName(name)
            .AddEditor()
            .WithAlias(editorAlias)
            .Done()
            .Build();
}
