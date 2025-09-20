using System.Globalization;
using System.Text.Json.Nodes;
using Moq;
using NUnit.Framework;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.IO;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.Editors;
using Umbraco.Cms.Core.Models.Validation;
using Umbraco.Cms.Core.PropertyEditors;
using Umbraco.Cms.Core.PropertyEditors.ValueConverters;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Strings;

namespace Umbraco.Cms.Tests.UnitTests.Umbraco.Core.PropertyEditors;

public partial class DateTime2PropertyEditorTests
{
    private readonly DateTimeWithTimeZoneValueConverter _dateTimeWithTimeZoneValueConverter = new(_jsonSerializer);

    private static readonly object[] _sourceList2 =
    [
        new object[] { JsonNode.Parse("{\"date\": \"2025-08-20T14:30:00\"}"), DateTime2Configuration.TimeZoneMode.All, Array.Empty<string>(), true },
        new object[] { JsonNode.Parse("{\"date\": \"2025-08-20T14:30:00\"}"), DateTime2Configuration.TimeZoneMode.Local, Array.Empty<string>(), true },
        new object[] { JsonNode.Parse("{\"date\": \"2025-08-20T14:30:00\"}"), DateTime2Configuration.TimeZoneMode.Custom, new[] { "Europe/Copenhagen" }, false },
        new object[] { JsonNode.Parse("{\"date\": \"2025-08-20T14:30:00\", \"timeZone\": \"Europe/Copenhagen\"}"), DateTime2Configuration.TimeZoneMode.All, Array.Empty<string>(), true },
        new object[] { JsonNode.Parse("{\"date\": \"2025-08-20T14:30:00\", \"timeZone\": \"Europe/Copenhagen\"}"), DateTime2Configuration.TimeZoneMode.Local, Array.Empty<string>(), true },
        new object[] { JsonNode.Parse("{\"date\": \"2025-08-20T14:30:00\", \"timeZone\": \"Europe/Copenhagen\"}"), DateTime2Configuration.TimeZoneMode.Custom, Array.Empty<string>(), false },
        new object[] { JsonNode.Parse("{\"date\": \"2025-08-20T14:30:00\", \"timeZone\": \"Europe/Copenhagen\"}"), DateTime2Configuration.TimeZoneMode.Custom, new[] { "Europe/Copenhagen" }, true },
        new object[] { JsonNode.Parse("{\"date\": \"2025-08-20T14:30:00\", \"timeZone\": \"Europe/Copenhagen\"}"), DateTime2Configuration.TimeZoneMode.Custom, new[] { "Europe/Amsterdam", "Europe/Copenhagen" }, true },
        new object[] { JsonNode.Parse("{\"date\": \"2025-08-20T14:30:00\", \"timeZone\": \"Europe/Copenhagen\"}"), DateTime2Configuration.TimeZoneMode.Custom, new[] { "Europe/Amsterdam" }, false },
    ];

    [TestCaseSource(nameof(_sourceList2))]
    public void Validates_TimeZone_Received(
        object value,
        DateTime2Configuration.TimeZoneMode timeZoneMode,
        string[] timeZones,
        bool expectedSuccess)
    {
        var editor = CreateDateTimeWithTimeZoneValueEditor(timeZoneMode: timeZoneMode, timeZones: timeZones);
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
    public void DateTimeWithTimeZone_Validates_Date_Received(object? value, bool expectedSuccess)
    {
        var editor = CreateDateTimeWithTimeZoneValueEditor();
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

    [TestCaseSource(nameof(_dateTimeWithTimeZoneParseValuesFromEditorTestCases))]
    public void DateTimeWithTimeZone_Can_Parse_Values_From_Editor(
        object? value,
        DateTimeOffset? expectedDateTimeOffset,
        string? expectedTimeZone)
    {
        var expectedJson = expectedDateTimeOffset is null ? null : _jsonSerializer.Serialize(
            new DateTime2ValueConverterBase.DateTime2Dto
            {
                Date = expectedDateTimeOffset.Value,
                TimeZone = expectedTimeZone,
            });
        var result = CreateDateTimeUnspecifiedValueEditor().FromEditor(
            new ContentPropertyData(
                value,
                new DateTime2Configuration
                {
                    TimeZones = null,
                }),
            null);
        Assert.AreEqual(expectedJson, result);
    }

    private static readonly object[][] _dateTimeWithTimeZoneParseValuesToEditorTestCases =
    [
        [null, null, DateTime2Configuration.TimeZoneMode.All, null],
        [0, null, DateTime2Configuration.TimeZoneMode.All, new DateTime2PropertyEditorBase.DateTime2ApiModel { Date = "2025-08-20T16:30:00.0000000+00:00", TimeZone = null }],
        [0, null, DateTime2Configuration.TimeZoneMode.Local, new DateTime2PropertyEditorBase.DateTime2ApiModel { Date = "2025-08-20T16:30:00.0000000+00:00", TimeZone = null }],
        [0, null, DateTime2Configuration.TimeZoneMode.Custom, new DateTime2PropertyEditorBase.DateTime2ApiModel { Date = "2025-08-20T16:30:00.0000000+00:00", TimeZone = null }],
        [-5, "Europe/Copenhagen", DateTime2Configuration.TimeZoneMode.All, new DateTime2PropertyEditorBase.DateTime2ApiModel { Date = "2025-08-20T16:30:00.0000000-05:00", TimeZone = "Europe/Copenhagen" }],
    ];

    [TestCaseSource(nameof(_dateTimeWithTimeZoneParseValuesToEditorTestCases))]
    public void DateTimeWithTimeZone_Can_Parse_Values_To_Editor(
        int? offset,
        string? timeZone,
        DateTime2Configuration.TimeZoneMode timeZoneMode,
        object? expectedResult)
    {
        var storedValue = offset is null
            ? null
            : new DateTime2ValueConverterBase.DateTime2Dto
            {
                Date = new DateTimeOffset(2025, 8, 20, 16, 30, 00, TimeSpan.FromHours(offset.Value)),
                TimeZone = timeZone,
            };
        var valueEditor = CreateDateTimeWithTimeZoneValueEditor(timeZoneMode: timeZoneMode);
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
        Assert.IsInstanceOf<DateTime2PropertyEditorBase.DateTime2ApiModel>(result);
        var apiModel = (DateTime2PropertyEditorBase.DateTime2ApiModel)result;
        Assert.AreEqual(((DateTime2PropertyEditorBase.DateTime2ApiModel)expectedResult).Date, apiModel.Date);
        Assert.AreEqual(((DateTime2PropertyEditorBase.DateTime2ApiModel)expectedResult).TimeZone, apiModel.TimeZone);
    }

    private DateTime2PropertyEditorBase.DateTime2DataValueEditor<DateTimeWithTimeZoneValueConverter> CreateDateTimeWithTimeZoneValueEditor(
        DateTime2Configuration.TimeZoneMode timeZoneMode = DateTime2Configuration.TimeZoneMode.All,
        string[]? timeZones = null)
    {
        var localizedTextServiceMock = new Mock<ILocalizedTextService>();
        localizedTextServiceMock.Setup(x => x.Localize(
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<CultureInfo>(),
                It.IsAny<IDictionary<string, string>>()))
            .Returns((string key, string alias, CultureInfo _, IDictionary<string, string> _) => $"{key}_{alias}");
        var valueEditor = new DateTime2PropertyEditorBase.DateTime2DataValueEditor<DateTimeWithTimeZoneValueConverter>(
            Mock.Of<IShortStringHelper>(),
            _jsonSerializer,
            Mock.Of<IIOHelper>(),
            new DataEditorAttribute(Constants.PropertyEditors.Aliases.DateTimeWithTimeZone),
            localizedTextServiceMock.Object,
            _dateTimeWithTimeZoneValueConverter)
        {
            ConfigurationObject = new DateTime2Configuration
            {
                TimeZones = new DateTime2Configuration.TimeZonesConfiguration
                {
                    Mode = timeZoneMode,
                    TimeZones = timeZones?.ToList() ?? [],
                },
            },
        };
        return valueEditor;
    }
}
