using System.Globalization;
using System.Text.Json.Nodes;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.IO;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.Editors;
using Umbraco.Cms.Core.Models.Validation;
using Umbraco.Cms.Core.PropertyEditors;
using Umbraco.Cms.Core.PropertyEditors.ValueConverters;
using Umbraco.Cms.Core.Serialization;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Strings;
using Umbraco.Cms.Infrastructure.Models;
using Umbraco.Cms.Infrastructure.Serialization;

namespace Umbraco.Cms.Tests.UnitTests.Umbraco.Core.PropertyEditors;

/// <summary>
/// Contains unit tests for the <see cref="DateTimeWithTimeZonePropertyEditor"/> class in Umbraco.
/// </summary>
[TestFixture]
public class DateTimeWithTimeZonePropertyEditorTests
{
    private static readonly IJsonSerializer _jsonSerializer =
        new SystemTextJsonSerializer(new DefaultJsonSerializerEncoderFactory());

    private static readonly object[] _validateDateReceivedTestCases =
    [
        new object[] { null, true },
        new object[] { JsonNode.Parse("{}"), false },
        new object[] { JsonNode.Parse("{\"test\": \"\"}"), false },
        new object[] { JsonNode.Parse("{\"date\": \"\"}"), false },
        new object[] { JsonNode.Parse("{\"date\": \"INVALID\"}"), false },
        new object[] { JsonNode.Parse("{\"date\": \"2025-08-20T14:30:00\"}"), true }
    ];

    private static readonly object[] _sourceList2 =
    [
        new object[] { JsonNode.Parse("{\"date\": \"2025-08-20T14:30:00\"}"), DateTimeConfiguration.TimeZoneMode.All, Array.Empty<string>(), true },
        new object[] { JsonNode.Parse("{\"date\": \"2025-08-20T14:30:00\"}"), DateTimeConfiguration.TimeZoneMode.Local, Array.Empty<string>(), true },
        new object[] { JsonNode.Parse("{\"date\": \"2025-08-20T14:30:00\"}"), DateTimeConfiguration.TimeZoneMode.Custom, new[] { "Europe/Copenhagen" }, false },
        new object[] { JsonNode.Parse("{\"date\": \"2025-08-20T14:30:00\", \"timeZone\": \"Europe/Copenhagen\"}"), DateTimeConfiguration.TimeZoneMode.All, Array.Empty<string>(), true },
        new object[] { JsonNode.Parse("{\"date\": \"2025-08-20T14:30:00\", \"timeZone\": \"Europe/Copenhagen\"}"), DateTimeConfiguration.TimeZoneMode.Local, Array.Empty<string>(), true },
        new object[] { JsonNode.Parse("{\"date\": \"2025-08-20T14:30:00\", \"timeZone\": \"Europe/Copenhagen\"}"), DateTimeConfiguration.TimeZoneMode.Custom, Array.Empty<string>(), false },
        new object[] { JsonNode.Parse("{\"date\": \"2025-08-20T14:30:00\", \"timeZone\": \"Europe/Copenhagen\"}"), DateTimeConfiguration.TimeZoneMode.Custom, new[] { "Europe/Copenhagen" }, true },
        new object[] { JsonNode.Parse("{\"date\": \"2025-08-20T14:30:00\", \"timeZone\": \"Europe/Copenhagen\"}"), DateTimeConfiguration.TimeZoneMode.Custom, new[] { "Europe/Amsterdam", "Europe/Copenhagen" }, true },
        new object[] { JsonNode.Parse("{\"date\": \"2025-08-20T14:30:00\", \"timeZone\": \"Europe/Copenhagen\"}"), DateTimeConfiguration.TimeZoneMode.Custom, new[] { "Europe/Amsterdam" }, false },
    ];

    /// <summary>
    /// Tests that the value is validated against the provided time zone options and verifies whether validation succeeds or fails as expected.
    /// </summary>
    /// <param name="value">The value to be validated as a time zone.</param>
    /// <param name="timeZoneMode">The configuration mode for time zone selection.</param>
    /// <param name="timeZones">The array of valid time zone identifiers.</param>
    /// <param name="expectedSuccess">True if validation is expected to succeed; otherwise, false.</param>
    [TestCaseSource(nameof(_sourceList2))]
    public void Validates_TimeZone_Received(
        object value,
        DateTimeConfiguration.TimeZoneMode timeZoneMode,
        string[] timeZones,
        bool expectedSuccess)
    {
        var editor = CreateValueEditor(timeZoneMode: timeZoneMode, timeZones: timeZones);
        var result = editor.Validate(value, false, null, PropertyValidationContext.Empty()).ToList();
        if (expectedSuccess)
        {
            Assert.IsEmpty(result);
        }
        else
        {
            Assert.AreEqual(1, result.Count);

            var validationResult = result.First();
            Assert.AreEqual("validation_notOneOfOptions", validationResult.ErrorMessage);
        }
    }

    [TestCaseSource(nameof(_validateDateReceivedTestCases))]
    public void Validates_Date_Received(object? value, bool expectedSuccess)
    {
        var editor = CreateValueEditor();
        var result = editor.Validate(value, false, null, PropertyValidationContext.Empty()).ToList();
        if (expectedSuccess)
        {
            Assert.IsEmpty(result);
        }
        else
        {
            Assert.AreEqual(1, result.Count);

            var validationResult = result.First();
            Assert.AreEqual("validation_invalidDate", validationResult.ErrorMessage);
        }
    }

    private static readonly object[] _dateTimeWithTimeZoneParseValuesFromEditorTestCases =
    [
        new object[] { null, null, null },
        new object[] { JsonNode.Parse("{}"), null, null },
        new object[] { JsonNode.Parse("{\"INVALID\": \"\"}"), null, null },
        new object[] { JsonNode.Parse("{\"date\": \"2025-08-20T18:30:01\"}"), new DateTimeOffset(2025, 8, 20, 18, 30, 1, TimeSpan.Zero), null },
        new object[] { JsonNode.Parse("{\"date\": \"2025-08-20T18:30:01Z\"}"), new DateTimeOffset(2025, 8, 20, 18, 30, 1, TimeSpan.Zero), null },
        new object[] { JsonNode.Parse("{\"date\": \"2025-08-20T18:30:01-05:00\"}"), new DateTimeOffset(2025, 8, 20, 18, 30, 1, TimeSpan.FromHours(-5)), null },
    ];

    /// <summary>
    /// Tests that values from the editor can be parsed correctly into the expected DateTimeOffset and time zone.
    /// </summary>
    /// <param name="value">The input value from the editor.</param>
    /// <param name="expectedDateTimeOffset">The expected parsed DateTimeOffset result.</param>
    /// <param name="expectedTimeZone">The expected time zone string.</param>
    [TestCaseSource(nameof(_dateTimeWithTimeZoneParseValuesFromEditorTestCases))]
    public void Can_Parse_Values_From_Editor(
        object? value,
        DateTimeOffset? expectedDateTimeOffset,
        string? expectedTimeZone)
    {
        var expectedJson = expectedDateTimeOffset is null ? null : _jsonSerializer.Serialize(
            new DateTimeValueConverterBase.DateTimeDto
            {
                Date = expectedDateTimeOffset.Value,
                TimeZone = expectedTimeZone,
            });
        var result = CreateValueEditor().FromEditor(
            new ContentPropertyData(
                value,
                new DateTimeConfiguration
                {
                    TimeZones = null,
                }),
            null);
        Assert.AreEqual(expectedJson, result);
    }

    private static readonly object[][] _dateTimeWithTimeZoneParseValuesToEditorTestCases =
    [
        [null, null, DateTimeConfiguration.TimeZoneMode.All, null],
        [0, null, DateTimeConfiguration.TimeZoneMode.All, new DateTimeEditorValue { Date = "2025-08-20T16:30:00+00:00", TimeZone = null }],
        [0, null, DateTimeConfiguration.TimeZoneMode.Local, new DateTimeEditorValue { Date = "2025-08-20T16:30:00+00:00", TimeZone = null }],
        [0, null, DateTimeConfiguration.TimeZoneMode.Custom, new DateTimeEditorValue { Date = "2025-08-20T16:30:00+00:00", TimeZone = null }],
        [-5, "Europe/Copenhagen", DateTimeConfiguration.TimeZoneMode.All, new DateTimeEditorValue { Date = "2025-08-20T16:30:00-05:00", TimeZone = "Europe/Copenhagen" }],
    ];

    /// <summary>
    /// Verifies that various combinations of offset and time zone values are correctly parsed and mapped to the editor model.
    /// </summary>
    /// <param name="offset">The nullable offset in hours used to construct the <see cref="DateTimeOffset"/> for the test case.</param>
    /// <param name="timeZone">The optional time zone identifier string to associate with the value, or <c>null</c> if not specified.</param>
    /// <param name="timeZoneMode">The <see cref="DateTimeConfiguration.TimeZoneMode"/> indicating how time zones are handled by the editor.</param>
    /// <param name="expectedResult">The expected <see cref="DateTimeEditorValue"/> result from the editor, or <c>null</c> if no value is expected.</param>
    [TestCaseSource(nameof(_dateTimeWithTimeZoneParseValuesToEditorTestCases))]
    public void Can_Parse_Values_To_Editor(
        int? offset,
        string? timeZone,
        DateTimeConfiguration.TimeZoneMode timeZoneMode,
        object? expectedResult)
    {
        var storedValue = offset is null
            ? null
            : new DateTimeValueConverterBase.DateTimeDto
            {
                Date = new DateTimeOffset(2025, 8, 20, 16, 30, 00, TimeSpan.FromHours(offset.Value)),
                TimeZone = timeZone,
            };
        var valueEditor = CreateValueEditor(timeZoneMode: timeZoneMode);
        var storedValueJson = storedValue is null ? null : _jsonSerializer.Serialize(storedValue);
        var result = valueEditor.ToEditor(
            new Property(new PropertyType(Mock.Of<IShortStringHelper>(), "dataType", ValueStorageType.Ntext))
            {
                Values =
                [
                    new Property.PropertyValue
                    {
                        EditedValue = storedValueJson,
                        PublishedValue = storedValueJson,
                    }
                ],
            });

        if (expectedResult is null)
        {
            Assert.IsNull(result);
            return;
        }

        Assert.IsNotNull(result);
        Assert.IsInstanceOf<DateTimeEditorValue>(result);
        var apiModel = (DateTimeEditorValue)result;
        Assert.AreEqual(((DateTimeEditorValue)expectedResult).Date, apiModel.Date);
        Assert.AreEqual(((DateTimeEditorValue)expectedResult).TimeZone, apiModel.TimeZone);
    }

    private DateTimePropertyEditorBase.DateTimeDataValueEditor CreateValueEditor(
        DateTimeConfiguration.TimeZoneMode timeZoneMode = DateTimeConfiguration.TimeZoneMode.All,
        string[]? timeZones = null)
    {
        var localizedTextServiceMock = new Mock<ILocalizedTextService>();
        localizedTextServiceMock.Setup(x => x.Localize(
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<CultureInfo>(),
                It.IsAny<IDictionary<string, string>>()))
            .Returns((string key, string alias, CultureInfo _, IDictionary<string, string> _) => $"{key}_{alias}");
        var valueEditor = new DateTimePropertyEditorBase.DateTimeDataValueEditor(
            Mock.Of<IShortStringHelper>(),
            _jsonSerializer,
            Mock.Of<IIOHelper>(),
            new DataEditorAttribute(Constants.PropertyEditors.Aliases.DateTimeWithTimeZone),
            localizedTextServiceMock.Object,
            Mock.Of<ILogger<DateTimePropertyEditorBase.DateTimeDataValueEditor>>(),
            dt => dt.Date.ToString("yyyy-MM-ddTHH:mm:sszzz"))
        {
            ConfigurationObject = new DateTimeConfiguration
            {
                TimeZones = new DateTimeConfiguration.TimeZonesConfiguration
                {
                    Mode = timeZoneMode,
                    TimeZones = timeZones?.ToList() ?? [],
                },
            },
        };
        return valueEditor;
    }
}
