// Copyright (c) Umbraco.
// See LICENSE for more details.

using System.Globalization;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.IO;
using Umbraco.Cms.Core.PropertyEditors;
using Umbraco.Cms.Core.PropertyEditors.ValueConverters;
using Umbraco.Cms.Core.Serialization;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Strings;
using Umbraco.Cms.Infrastructure.Serialization;

namespace Umbraco.Cms.Tests.UnitTests.Umbraco.Core.PropertyEditors;

/// <summary>
/// Unit tests for <see cref="IDataValueSortable.GetSortableValue"/> implementation
/// in <see cref="DateTimePropertyEditorBase.DateTimeDataValueEditor"/>.
/// </summary>
[TestFixture]
public class DateTimeDataValueEditorSortableTests
{
    private static readonly IJsonSerializer _jsonSerializer =
        new SystemTextJsonSerializer(new DefaultJsonSerializerEncoderFactory());

    [Test]
    public void GetSortableValue_Returns_Null_For_Null_Value()
    {
        var editor = CreateValueEditor(Constants.PropertyEditors.Aliases.DateTimeWithTimeZone);

        var result = editor.GetSortableValue(null, null);

        Assert.IsNull(result);
    }

    [Test]
    public void GetSortableValue_Returns_Null_For_Empty_String()
    {
        var editor = CreateValueEditor(Constants.PropertyEditors.Aliases.DateTimeWithTimeZone);

        var result = editor.GetSortableValue(string.Empty, null);

        Assert.IsNull(result);
    }

    [Test]
    public void GetSortableValue_Returns_Null_For_Whitespace_String()
    {
        var editor = CreateValueEditor(Constants.PropertyEditors.Aliases.DateTimeWithTimeZone);

        var result = editor.GetSortableValue("   ", null);

        Assert.IsNull(result);
    }

    [Test]
    public void GetSortableValue_Returns_Null_For_Non_String_Value()
    {
        var editor = CreateValueEditor(Constants.PropertyEditors.Aliases.DateTimeWithTimeZone);

        var result = editor.GetSortableValue(12345, null);

        Assert.IsNull(result);
    }

    [Test]
    public void GetSortableValue_Returns_Null_For_Invalid_Json()
    {
        var editor = CreateValueEditor(Constants.PropertyEditors.Aliases.DateTimeWithTimeZone);

        var result = editor.GetSortableValue("not valid json", null);

        Assert.IsNull(result);
    }

    [Test]
    public void GetSortableValue_Returns_Default_Date_For_Json_Without_Date_Field()
    {
        // When the JSON is valid but lacks a date field, the deserializer creates a DateTimeDto
        // with a default DateTimeOffset value (0001-01-01T00:00:00+00:00).
        // This is edge case behavior - in practice, the editor always provides a date.
        var editor = CreateValueEditor(Constants.PropertyEditors.Aliases.DateTimeWithTimeZone);
        var json = _jsonSerializer.Serialize(new { timeZone = "UTC" });

        var result = editor.GetSortableValue(json, null);

        Assert.IsNotNull(result);
        Assert.That(result, Does.StartWith("0001-01-01T00:00:00"));
    }

    [Test]
    public void GetSortableValue_Returns_Default_Date_For_Empty_Json_Object()
    {
        // When the JSON is an empty object, the deserializer creates a DateTimeDto
        // with default values. This is edge case behavior.
        var editor = CreateValueEditor(Constants.PropertyEditors.Aliases.DateTimeWithTimeZone);

        var result = editor.GetSortableValue("{}", null);

        Assert.IsNotNull(result);
        Assert.That(result, Does.StartWith("0001-01-01T00:00:00"));
    }

    [Test]
    public void GetSortableValue_Returns_Utc_Normalized_Value_For_Utc_Date()
    {
        var editor = CreateValueEditor(Constants.PropertyEditors.Aliases.DateTimeWithTimeZone);
        var storedValue = CreateStoredValue(new DateTimeOffset(2024, 6, 15, 10, 30, 0, TimeSpan.Zero), "UTC");

        var result = editor.GetSortableValue(storedValue, null);

        Assert.IsNotNull(result);
        Assert.That(result, Does.EndWith("+00:00").Or.EndWith("Z"));
        Assert.That(result, Does.StartWith("2024-06-15T10:30:00"));
    }

    [Test]
    public void GetSortableValue_Converts_Positive_Offset_To_Utc()
    {
        var editor = CreateValueEditor(Constants.PropertyEditors.Aliases.DateTimeWithTimeZone);

        // 14:30 at +02:00 should become 12:30 UTC
        var storedValue = CreateStoredValue(new DateTimeOffset(2024, 6, 15, 14, 30, 0, TimeSpan.FromHours(2)), "Europe/Paris");

        var result = editor.GetSortableValue(storedValue, null);

        Assert.IsNotNull(result);
        Assert.That(result, Does.Contain("2024-06-15T12:30:00"));
        Assert.That(result, Does.EndWith("+00:00").Or.EndWith("Z"));
    }

    [Test]
    public void GetSortableValue_Converts_Negative_Offset_To_Utc()
    {
        var editor = CreateValueEditor(Constants.PropertyEditors.Aliases.DateTimeWithTimeZone);

        // 08:00 at -05:00 should become 13:00 UTC
        var storedValue = CreateStoredValue(new DateTimeOffset(2024, 6, 15, 8, 0, 0, TimeSpan.FromHours(-5)), "America/New_York");

        var result = editor.GetSortableValue(storedValue, null);

        Assert.IsNotNull(result);
        Assert.That(result, Does.Contain("2024-06-15T13:00:00"));
        Assert.That(result, Does.EndWith("+00:00").Or.EndWith("Z"));
    }

    [Test]
    public void GetSortableValue_Handles_Date_Crossing_Day_Boundary()
    {
        var editor = CreateValueEditor(Constants.PropertyEditors.Aliases.DateTimeWithTimeZone);

        // 23:00 at +05:00 should become 18:00 UTC (same day)
        var storedValue = CreateStoredValue(new DateTimeOffset(2024, 6, 15, 23, 0, 0, TimeSpan.FromHours(5)), null);

        var result = editor.GetSortableValue(storedValue, null);

        Assert.IsNotNull(result);
        Assert.That(result, Does.Contain("2024-06-15T18:00:00"));
    }

    [Test]
    public void GetSortableValue_Handles_Date_Crossing_To_Previous_Day()
    {
        var editor = CreateValueEditor(Constants.PropertyEditors.Aliases.DateTimeWithTimeZone);

        // 01:00 at +05:00 should become 20:00 UTC on the previous day
        var storedValue = CreateStoredValue(new DateTimeOffset(2024, 6, 15, 1, 0, 0, TimeSpan.FromHours(5)), null);

        var result = editor.GetSortableValue(storedValue, null);

        Assert.IsNotNull(result);
        Assert.That(result, Does.Contain("2024-06-14T20:00:00"));
    }

    [Test]
    public void GetSortableValue_Produces_Lexicographically_Sortable_Values()
    {
        var editor = CreateValueEditor(Constants.PropertyEditors.Aliases.DateTimeWithTimeZone);

        // Create dates that would sort incorrectly without UTC normalization
        var dates = new[]
        {
            CreateStoredValue(new DateTimeOffset(2024, 1, 15, 10, 0, 0, TimeSpan.Zero), null),      // Jan 15
            CreateStoredValue(new DateTimeOffset(2024, 6, 20, 14, 0, 0, TimeSpan.FromHours(2)), null), // Jun 20
            CreateStoredValue(new DateTimeOffset(2024, 12, 25, 8, 0, 0, TimeSpan.FromHours(-5)), null), // Dec 25
        };

        var sortableValues = dates
            .Select(d => editor.GetSortableValue(d, null))
            .ToList();

        // All should be non-null
        Assert.That(sortableValues, Has.All.Not.Null);

        // Lexicographic sort should match chronological order
        var sorted = sortableValues.OrderBy(x => x).ToList();
        Assert.AreEqual(sortableValues[0], sorted[0], "January date should be first");
        Assert.AreEqual(sortableValues[1], sorted[1], "June date should be second");
        Assert.AreEqual(sortableValues[2], sorted[2], "December date should be third");
    }

    [Test]
    public void GetSortableValue_Returns_Valid_Iso8601_Format()
    {
        var editor = CreateValueEditor(Constants.PropertyEditors.Aliases.DateTimeWithTimeZone);
        var storedValue = CreateStoredValue(new DateTimeOffset(2024, 6, 15, 10, 30, 45, TimeSpan.Zero), null);

        var result = editor.GetSortableValue(storedValue, null);

        Assert.IsNotNull(result);

        // Should be parseable as DateTimeOffset
        Assert.That(DateTimeOffset.TryParse(result, CultureInfo.InvariantCulture, DateTimeStyles.None, out _), Is.True);
    }

    [Test]
    public void GetSortableValue_TimeOnly_Uses_MinDate_Component()
    {
        var editor = CreateValueEditor(Constants.PropertyEditors.Aliases.TimeOnly);

        // TimeOnly stores time with DateOnly.MinValue (0001-01-01)
        var storedValue = CreateStoredValue(new DateTimeOffset(1, 1, 1, 14, 30, 0, TimeSpan.Zero), null);

        var result = editor.GetSortableValue(storedValue, null);

        Assert.IsNotNull(result);
        Assert.That(result, Does.StartWith("0001-01-01T14:30:00"));
    }

    [Test]
    public void GetSortableValue_TimeOnly_Produces_Sortable_Times()
    {
        var editor = CreateValueEditor(Constants.PropertyEditors.Aliases.TimeOnly);

        var times = new[]
        {
            CreateStoredValue(new DateTimeOffset(1, 1, 1, 8, 0, 0, TimeSpan.Zero), null),  // 08:00
            CreateStoredValue(new DateTimeOffset(1, 1, 1, 12, 30, 0, TimeSpan.Zero), null), // 12:30
            CreateStoredValue(new DateTimeOffset(1, 1, 1, 18, 45, 0, TimeSpan.Zero), null), // 18:45
        };

        var sortableValues = times
            .Select(t => editor.GetSortableValue(t, null))
            .ToList();

        var sorted = sortableValues.OrderBy(x => x).ToList();
        CollectionAssert.AreEqual(sortableValues, sorted, "Times should already be in chronological order");
    }

    [Test]
    public void GetSortableValue_DateOnly_Produces_Sortable_Dates()
    {
        var editor = CreateValueEditor(Constants.PropertyEditors.Aliases.DateOnly);

        var dates = new[]
        {
            CreateStoredValue(new DateTimeOffset(2024, 3, 15, 0, 0, 0, TimeSpan.Zero), null),
            CreateStoredValue(new DateTimeOffset(2024, 7, 20, 0, 0, 0, TimeSpan.Zero), null),
            CreateStoredValue(new DateTimeOffset(2024, 11, 25, 0, 0, 0, TimeSpan.Zero), null),
        };

        var sortableValues = dates
            .Select(d => editor.GetSortableValue(d, null))
            .ToList();

        var sorted = sortableValues.OrderBy(x => x).ToList();
        CollectionAssert.AreEqual(sortableValues, sorted, "Dates should already be in chronological order");
    }

    private static string CreateStoredValue(DateTimeOffset date, string? timeZone)
    {
        var dto = new DateTimeValueConverterBase.DateTimeDto
        {
            Date = date,
            TimeZone = timeZone,
        };
        return _jsonSerializer.Serialize(dto);
    }

    private static DateTimePropertyEditorBase.DateTimeDataValueEditor CreateValueEditor(string editorAlias)
    {
        var localizedTextServiceMock = new Mock<ILocalizedTextService>();
        localizedTextServiceMock.Setup(x => x.Localize(
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<CultureInfo>(),
                It.IsAny<IDictionary<string, string>>()))
            .Returns((string key, string alias, CultureInfo _, IDictionary<string, string> _) => $"{key}_{alias}");

        return new DateTimePropertyEditorBase.DateTimeDataValueEditor(
            Mock.Of<IShortStringHelper>(),
            _jsonSerializer,
            Mock.Of<IIOHelper>(),
            new DataEditorAttribute(editorAlias),
            localizedTextServiceMock.Object,
            Mock.Of<ILogger<DateTimePropertyEditorBase.DateTimeDataValueEditor>>(),
            dt => dt.Date.ToString("O", CultureInfo.InvariantCulture));
    }
}
