using System.Globalization;
using System.Text.Json.Nodes;
using Moq;
using NUnit.Framework;
using Umbraco.Cms.Core.Cache;
using Umbraco.Cms.Core.IO;
using Umbraco.Cms.Core.Models.Editors;
using Umbraco.Cms.Core.Models.Validation;
using Umbraco.Cms.Core.PropertyEditors;
using Umbraco.Cms.Core.PropertyEditors.ValueConverters;
using Umbraco.Cms.Core.Serialization;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Strings;

namespace Umbraco.Cms.Tests.UnitTests.Umbraco.Core.PropertyEditors;

[TestFixture]
public class DateWithTimeZonePropertyEditorTests
{
    private readonly Mock<IJsonSerializer> _jsonSerializerMock = new(MockBehavior.Strict);

    private static readonly object[] _sourceList1 =
    [
        new object[] { null, true },
        new object[] { "INVALID", false },
        new object[] { JsonNode.Parse("{}"), false },
        new object[] { JsonNode.Parse("{\"test\": \"\"}"), false },
        new object[] { JsonNode.Parse("{\"date\": \"\"}"), false },
        new object[] { JsonNode.Parse("{\"date\": \"INVALID\"}"), false },
        new object[] { JsonNode.Parse("{\"date\": \"2025-08-20T14:30:00\"}"), true }
    ];

    [TestCaseSource(nameof(_sourceList1))]
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

    private static readonly object[] _sourceList2 =
    [
        new object[] { JsonNode.Parse("{\"date\": \"2025-08-20T14:30:00\"}"), DateTimeWithTimeZoneMode.None, Array.Empty<string>(), true },
        new object[] { JsonNode.Parse("{\"date\": \"2025-08-20T14:30:00\"}"), DateTimeWithTimeZoneMode.All, Array.Empty<string>(), true },
        new object[] { JsonNode.Parse("{\"date\": \"2025-08-20T14:30:00\"}"), DateTimeWithTimeZoneMode.Local, Array.Empty<string>(), true },
        new object[] { JsonNode.Parse("{\"date\": \"2025-08-20T14:30:00\"}"), DateTimeWithTimeZoneMode.Custom, new[] { "Europe/Copenhagen" }, false },
        new object[] { JsonNode.Parse("{\"date\": \"2025-08-20T14:30:00\", \"timeZone\": \"Europe/Copenhagen\"}"), DateTimeWithTimeZoneMode.None, Array.Empty<string>(), true },
        new object[] { JsonNode.Parse("{\"date\": \"2025-08-20T14:30:00\", \"timeZone\": \"Europe/Copenhagen\"}"), DateTimeWithTimeZoneMode.All, Array.Empty<string>(), true },
        new object[] { JsonNode.Parse("{\"date\": \"2025-08-20T14:30:00\", \"timeZone\": \"Europe/Copenhagen\"}"), DateTimeWithTimeZoneMode.Local, Array.Empty<string>(), true },
        new object[] { JsonNode.Parse("{\"date\": \"2025-08-20T14:30:00\", \"timeZone\": \"Europe/Copenhagen\"}"), DateTimeWithTimeZoneMode.Custom, Array.Empty<string>(), false },
        new object[] { JsonNode.Parse("{\"date\": \"2025-08-20T14:30:00\", \"timeZone\": \"Europe/Copenhagen\"}"), DateTimeWithTimeZoneMode.Custom, new[] { "Europe/Copenhagen" }, true },
        new object[] { JsonNode.Parse("{\"date\": \"2025-08-20T14:30:00\", \"timeZone\": \"Europe/Copenhagen\"}"), DateTimeWithTimeZoneMode.Custom, new[] { "Europe/Amsterdam", "Europe/Copenhagen" }, true },
        new object[] { JsonNode.Parse("{\"date\": \"2025-08-20T14:30:00\", \"timeZone\": \"Europe/Copenhagen\"}"), DateTimeWithTimeZoneMode.Custom, new[] { "Europe/Amsterdam" }, false },
    ];

    [TestCaseSource(nameof(_sourceList2))]
    public void Validates_TimeZone_Received(
        object value,
        DateTimeWithTimeZoneMode timeZoneMode,
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

    private static readonly object[] _sourceList3 =
    [
        new object[] { null, DateTimeWithTimeZoneFormat.DateOnly, null, null },
        new object[] { JsonNode.Parse("{}"), DateTimeWithTimeZoneFormat.DateTime, null, null },
        new object[] { JsonNode.Parse("{\"INVALID\": \"\"}"), DateTimeWithTimeZoneFormat.DateTime, null, null },
        new object[] { JsonNode.Parse("{\"date\": \"2025-08-20\"}"), DateTimeWithTimeZoneFormat.DateOnly, new DateTimeOffset(2025, 8, 20, 0, 0, 0, TimeSpan.Zero), null },
        new object[] { JsonNode.Parse("{\"date\": \"16:34\"}"), DateTimeWithTimeZoneFormat.TimeOnly, new DateTimeOffset(1, 1, 1, 16, 34, 0, TimeSpan.Zero), null },
        new object[] { JsonNode.Parse("{\"date\": \"2025-08-20T18:30:01\"}"), DateTimeWithTimeZoneFormat.DateTime, new DateTimeOffset(2025, 8, 20, 18, 30, 1, TimeSpan.Zero), null },
        new object[] { JsonNode.Parse("{\"date\": \"2025-08-20T18:30:01Z\"}"), DateTimeWithTimeZoneFormat.DateTime, new DateTimeOffset(2025, 8, 20, 18, 30, 1, TimeSpan.Zero), null },
        new object[] { JsonNode.Parse("{\"date\": \"2025-08-20T18:30:01-05:00\"}"), DateTimeWithTimeZoneFormat.DateTime, new DateTimeOffset(2025, 8, 20, 18, 30, 1, TimeSpan.FromHours(-5)), null },
    ];

    [TestCaseSource(nameof(_sourceList3))]
    public void Can_Parse_Values_From_Editor(
        object? value,
        DateTimeWithTimeZoneFormat format,
        DateTimeOffset? expectedDateTimeOffset,
        string? expectedTimeZone)
    {
        var expectedJson = expectedDateTimeOffset is null ? null : "RESULT_JSON";
        _jsonSerializerMock.Setup(s => s.Serialize(It.IsAny<DateTimeWithTimeZoneValueConverter.DateTimeWithTimeZone>()))
            .Returns((object _) => expectedJson)
            .Callback<object?>(o =>
            {
                Assert.IsNotNull(o);
                Assert.IsInstanceOf<DateTimeWithTimeZoneValueConverter.DateTimeWithTimeZone>(o);
                var dateWithTimeZone = (DateTimeWithTimeZoneValueConverter.DateTimeWithTimeZone)o;
                Assert.AreEqual(expectedDateTimeOffset, dateWithTimeZone.Date);
                Assert.AreEqual(expectedTimeZone, dateWithTimeZone.TimeZone);
            })
            .Verifiable(Times.Exactly(expectedJson is null ? 0 : 1));
        var result = CreateValueEditor(format: format).FromEditor(
            new ContentPropertyData(
                value,
                new DateTimeWithTimeZoneConfiguration
                {
                    Format = format,
                    TimeZones = new DateTimeWithTimeZoneTimeZones
                    {
                        Mode = DateTimeWithTimeZoneMode.None,
                        TimeZones = [],
                    },
                }),
            null);
        Assert.AreEqual(expectedJson, result);
    }

    private DateTimeWithTimeZonePropertyEditor.DateTimeWithTimeZoneDataValueEditor CreateValueEditor(
        DateTimeWithTimeZoneFormat format = DateTimeWithTimeZoneFormat.DateTime,
        DateTimeWithTimeZoneMode timeZoneMode = DateTimeWithTimeZoneMode.All,
        string[]? timeZones = null)
    {
        var localizedTextServiceMock = new Mock<ILocalizedTextService>();
        localizedTextServiceMock.Setup(x => x.Localize(
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<CultureInfo>(),
                It.IsAny<IDictionary<string, string>>()))
            .Returns((string key, string alias, CultureInfo _, IDictionary<string, string> _) => $"{key}_{alias}");
        var valueEditor = new DateTimeWithTimeZonePropertyEditor.DateTimeWithTimeZoneDataValueEditor(
            Mock.Of<IShortStringHelper>(),
            _jsonSerializerMock.Object,
            Mock.Of<IIOHelper>(),
            new DataEditorAttribute("alias"),
            localizedTextServiceMock.Object,
            Mock.Of<IDataTypeConfigurationCache>())
        {
            ConfigurationObject = new DateTimeWithTimeZoneConfiguration
            {
                Format = format,
                TimeZones = new DateTimeWithTimeZoneTimeZones
                {
                    Mode = timeZoneMode,
                    TimeZones = timeZones?.ToList() ?? [],
                },
            },
        };
        return valueEditor;
    }
}
