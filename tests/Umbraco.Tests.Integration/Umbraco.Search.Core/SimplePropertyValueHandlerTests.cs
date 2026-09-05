using NUnit.Framework;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.PropertyEditors;
using Umbraco.Cms.Core.PropertyEditors.ValueConverters;
using Umbraco.Cms.Core.Serialization;
using Umbraco.Cms.Search.Core.PropertyValueHandlers;
using Umbraco.Cms.Search.Core.PropertyValueHandlers.Collection;
using Umbraco.Cms.Tests.Common.Builders;
using Umbraco.Cms.Tests.Common.Builders.Extensions;
using Umbraco.Cms.Tests.Integration.Testing.Search;

namespace Umbraco.Cms.Tests.Integration.Umbraco.Search.Core;

public class SimplePropertyValueHandlerTests : PropertyValueHandlerTestsBase
{
    [Test]
    public void AllSupportedEditors_CanBeIndexed()
    {
        IJsonSerializer jsonSerializer = GetRequiredService<IJsonSerializer>();

        Content content = new ContentBuilder()
            .WithContentType(GetAllSimpleEditorsContentType())
            .WithName("All Supported Editors")
            .WithPropertyValues(
                new
                {
                    textBoxValue = "The TextBox value",
                    textAreaValue = "The TextArea value",
                    integerValue = 1234,
                    decimalValue = 56.78m,
                    dateValue = new DateTime(2001, 02, 03),
                    dateAndTimeValue = new DateTime(2004, 05, 06, 07, 08, 09),
                    dateOnlyValue = jsonSerializer.Serialize(new DateTimeValueConverterBase.DateTimeDto
                    {
                        Date = new DateTimeOffset(new DateOnly(2010, 03, 15), TimeOnly.MinValue, TimeSpan.Zero),
                    }),
                    timeOnlyValue = jsonSerializer.Serialize(new DateTimeValueConverterBase.DateTimeDto
                    {
                        Date = new DateTimeOffset(DateOnly.MinValue, new TimeOnly(13, 45, 30), TimeSpan.Zero),
                    }),
                    dateTimeWithTimeZoneValue = jsonSerializer.Serialize(new DateTimeValueConverterBase.DateTimeDto
                    {
                        Date = new DateTimeOffset(new DateOnly(2012, 07, 20), new TimeOnly(9, 15, 0), TimeSpan.FromHours(2)),
                        TimeZone = "Europe/Copenhagen",
                    }),
                    dateTimeUnspecifiedValue = jsonSerializer.Serialize(new DateTimeValueConverterBase.DateTimeDto
                    {
                        Date = new DateTimeOffset(new DateOnly(2015, 11, 05), new TimeOnly(18, 30, 0), TimeSpan.Zero),
                    }),
                    tagsAsJsonValue = "[\"One\",\"Two\",\"Three\"]",
                    tagsAsCsvValue = "Four,Five,Six",
                    multipleTextstringsValue = "First\nSecond\nThird",
                    contentPickerValue = "udi://document/55bf7f6d-acd2-4f1e-92bd-f0b5c41dbfed",
                    booleanAsBooleanValue = true,
                    booleanAsIntegerValue = 1,
                    booleanAsStringValue = "1",
                    sliderSingleValue = "123.45",
                    sliderRangeValue = "123.45,567.89",
                    multiUrlPickerValue = jsonSerializer.Serialize(new[]
                    {
                        new MultiUrlPickerValueEditor.LinkDto
                        {
                            Name = "Link One"
                        },
                        new MultiUrlPickerValueEditor.LinkDto
                        {
                            Name = "Link Two"
                        },
                        new MultiUrlPickerValueEditor.LinkDto
                        {
                            // should be ignored - but make sure we test it all the same
                            Name = null
                        }
                    }),
                    dropdownSingleValue = "[\"One\"]",
                    dropdownMultipleValue = "[\"One\", \"Two\", \"Three\"]",
                    radioButtonListValue = "One",
                    checkBoxListValue = "Two"
                })
            .Build();

        ContentService.Save(content);
        ContentService.Publish(content, ["*"]);

        IReadOnlyList<TestIndexDocument> documents = IndexerAndSearcher.Dump(IndexAliases.PublishedContent);
        Assert.That(documents, Has.Count.EqualTo(1));

        TestIndexDocument document = documents.Single();
        Assert.Multiple(() =>
        {
            var textBoxValue = document.Fields.FirstOrDefault(f => f.FieldName == "textBoxValue")?.Value.Texts?.SingleOrDefault();
            Assert.That(textBoxValue, Is.EqualTo("The TextBox value"));

            var textAreaValue = document.Fields.FirstOrDefault(f => f.FieldName == "textAreaValue")?.Value.Texts?.SingleOrDefault();
            Assert.That(textAreaValue, Is.EqualTo("The TextArea value"));

            var integerValue = document.Fields.FirstOrDefault(f => f.FieldName == "integerValue")?.Value.Integers?.SingleOrDefault();
            Assert.That(integerValue, Is.EqualTo(1234));

            var decimalValue = document.Fields.FirstOrDefault(f => f.FieldName == "decimalValue")?.Value.Decimals?.SingleOrDefault();
            Assert.That(decimalValue, Is.EqualTo(56.78m));

            DateTimeOffset? dateValue = document.Fields.FirstOrDefault(f => f.FieldName == "dateValue")?.Value.DateTimeOffsets?.SingleOrDefault();
            Assert.That(dateValue, Is.EqualTo(new DateTimeOffset(new DateOnly(2001, 02, 03), new TimeOnly(), TimeSpan.Zero)));

            DateTimeOffset? dateAndTimeValue = document.Fields.FirstOrDefault(f => f.FieldName == "dateAndTimeValue")?.Value.DateTimeOffsets?.SingleOrDefault();
            Assert.That(dateAndTimeValue, Is.EqualTo(new DateTimeOffset(new DateOnly(2004, 05, 06), new TimeOnly(07, 08, 09), TimeSpan.Zero)));

            DateTimeOffset? dateOnlyValue = document.Fields.FirstOrDefault(f => f.FieldName == "dateOnlyValue")?.Value.DateTimeOffsets?.SingleOrDefault();
            Assert.That(dateOnlyValue, Is.EqualTo(new DateTimeOffset(new DateOnly(2010, 03, 15), TimeOnly.MinValue, TimeSpan.Zero)));

            DateTimeOffset? timeOnlyValue = document.Fields.FirstOrDefault(f => f.FieldName == "timeOnlyValue")?.Value.DateTimeOffsets?.SingleOrDefault();
            Assert.That(timeOnlyValue, Is.EqualTo(new DateTimeOffset(DateOnly.MinValue, new TimeOnly(13, 45, 30), TimeSpan.Zero)));

            DateTimeOffset? dateTimeWithTimeZoneValue = document.Fields.FirstOrDefault(f => f.FieldName == "dateTimeWithTimeZoneValue")?.Value.DateTimeOffsets?.SingleOrDefault();
            Assert.That(dateTimeWithTimeZoneValue, Is.EqualTo(new DateTimeOffset(new DateOnly(2012, 07, 20), new TimeOnly(9, 15, 0), TimeSpan.FromHours(2))));

            DateTimeOffset? dateTimeUnspecifiedValue = document.Fields.FirstOrDefault(f => f.FieldName == "dateTimeUnspecifiedValue")?.Value.DateTimeOffsets?.SingleOrDefault();
            Assert.That(dateTimeUnspecifiedValue, Is.EqualTo(new DateTimeOffset(new DateOnly(2015, 11, 05), new TimeOnly(18, 30, 0), TimeSpan.Zero)));

            var tagsAsJsonValue = document.Fields.FirstOrDefault(f => f.FieldName == "tagsAsJsonValue")?.Value.Keywords?.ToArray();
            Assert.That(new[] { "One", "Two", "Three" }, Is.EqualTo(tagsAsJsonValue).AsCollection);

            var tagsAsCsvValue = document.Fields.FirstOrDefault(f => f.FieldName == "tagsAsCsvValue")?.Value.Keywords?.ToArray();
            Assert.That(new[] { "Four", "Five", "Six" }, Is.EqualTo(tagsAsCsvValue).AsCollection);

            var allTagsValue = document.Fields.FirstOrDefault(f => f.FieldName == Cms.Search.Core.Constants.FieldNames.Tags)?.Value.Keywords?.ToArray();
            Assert.That(new[] { "One", "Two", "Three", "Four", "Five", "Six" }, Is.EquivalentTo(allTagsValue));

            var multipleTextstringsValue = document.Fields.FirstOrDefault(f => f.FieldName == "multipleTextstringsValue")?.Value.Texts?.ToArray();
            Assert.That(new[] { "First", "Second", "Third" }, Is.EqualTo(multipleTextstringsValue).AsCollection);

            var contentPickerValue = document.Fields.FirstOrDefault(f => f.FieldName == "contentPickerValue")?.Value.Keywords?.SingleOrDefault();
            Assert.That("55bf7f6d-acd2-4f1e-92bd-f0b5c41dbfed", Is.EqualTo(contentPickerValue));

            var booleanAsBooleanValue = document.Fields.FirstOrDefault(f => f.FieldName == "booleanAsBooleanValue")?.Value.Integers?.SingleOrDefault();
            Assert.That(booleanAsBooleanValue, Is.EqualTo(1));

            var booleanAsIntegerValue = document.Fields.FirstOrDefault(f => f.FieldName == "booleanAsIntegerValue")?.Value.Integers?.SingleOrDefault();
            Assert.That(booleanAsIntegerValue, Is.EqualTo(1));

            var booleanAsStringValue = document.Fields.FirstOrDefault(f => f.FieldName == "booleanAsStringValue")?.Value.Integers?.SingleOrDefault();
            Assert.That(booleanAsStringValue, Is.EqualTo(1));

            var sliderSingleValue = document.Fields.FirstOrDefault(f => f.FieldName == "sliderSingleValue")?.Value.Decimals?.SingleOrDefault();
            Assert.That(sliderSingleValue, Is.EqualTo(123.45m));

            var sliderRangeValue = document.Fields.FirstOrDefault(f => f.FieldName == "sliderRangeValue")?.Value.Decimals?.ToArray();
            Assert.That(new[] { 123.45m, 567.89m }, Is.EqualTo(sliderRangeValue).AsCollection);

            var multiUrlPickerValue = document.Fields.FirstOrDefault(f => f.FieldName == "multiUrlPickerValue")?.Value.Texts?.ToArray();
            Assert.That(new[] { "Link One", "Link Two" }, Is.EqualTo(multiUrlPickerValue).AsCollection);

            var dropdownSingleValue = document.Fields.FirstOrDefault(f => f.FieldName == "dropdownSingleValue")?.Value.Keywords?.ToArray();
            Assert.That(new[] { "One" }, Is.EqualTo(dropdownSingleValue).AsCollection);

            var dropdownMultipleValue = document.Fields.FirstOrDefault(f => f.FieldName == "dropdownMultipleValue")?.Value.Keywords?.ToArray();
            Assert.That(new[] { "One", "Two", "Three" }, Is.EqualTo(dropdownMultipleValue).AsCollection);

            var radioButtonListValue = document.Fields.FirstOrDefault(f => f.FieldName == "radioButtonListValue")?.Value.Keywords?.ToArray();
            Assert.That(new[] { "One" }, Is.EqualTo(radioButtonListValue).AsCollection);

            var checkBoxListValue = document.Fields.FirstOrDefault(f => f.FieldName == "checkBoxListValue")?.Value.Keywords?.ToArray();
            Assert.That(new[] { "Two" }, Is.EqualTo(checkBoxListValue).AsCollection);
        });
    }

    [Test]
    public void AllCorePropertyValueHandlers_HaveTheCorePropertyValueHandlerMarkerInterface()
    {
        IPropertyValueHandler[] handlers = GetRequiredService<PropertyValueHandlerCollection>().ToArray();
        Assert.That(handlers, Is.Not.Empty);
        Assert.That(handlers.All(handler => handler is ICorePropertyValueHandler), Is.True);
    }

    [SetUp]
    public async Task SetupTest()
        => await CreateAllSimpleEditorsContentType();
}
