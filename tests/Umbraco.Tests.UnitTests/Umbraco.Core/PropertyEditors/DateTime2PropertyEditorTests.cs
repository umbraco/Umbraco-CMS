using System.Globalization;
using System.Text.Json;
using System.Text.Json.Nodes;
using Moq;
using NUnit.Framework;
using Umbraco.Cms.Core.Cache;
using Umbraco.Cms.Core.IO;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.Editors;
using Umbraco.Cms.Core.Models.Validation;
using Umbraco.Cms.Core.PropertyEditors;
using Umbraco.Cms.Core.PropertyEditors.ValueConverters;
using Umbraco.Cms.Core.Serialization;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Strings;
using Umbraco.Cms.Infrastructure.Serialization;

namespace Umbraco.Cms.Tests.UnitTests.Umbraco.Core.PropertyEditors;

[TestFixture]
public class DateTime2PropertyEditorTests
{
    private readonly IJsonSerializer _jsonSerializer =
        new SystemTextJsonSerializer(new DefaultJsonSerializerEncoderFactory());

    private readonly Mock<IDataTypeConfigurationCache> _dataTypeConfigurationCache = new(MockBehavior.Strict);

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
        new object[] { JsonNode.Parse("{\"date\": \"2025-08-20T14:30:00\"}"), DateTime2Configuration.TimeZoneMode.None, Array.Empty<string>(), true },
        new object[] { JsonNode.Parse("{\"date\": \"2025-08-20T14:30:00\"}"), DateTime2Configuration.TimeZoneMode.All, Array.Empty<string>(), true },
        new object[] { JsonNode.Parse("{\"date\": \"2025-08-20T14:30:00\"}"), DateTime2Configuration.TimeZoneMode.Local, Array.Empty<string>(), true },
        new object[] { JsonNode.Parse("{\"date\": \"2025-08-20T14:30:00\"}"), DateTime2Configuration.TimeZoneMode.Custom, new[] { "Europe/Copenhagen" }, false },
        new object[] { JsonNode.Parse("{\"date\": \"2025-08-20T14:30:00\", \"timeZone\": \"Europe/Copenhagen\"}"), DateTime2Configuration.TimeZoneMode.None, Array.Empty<string>(), true },
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
        new object[] { null, DateTime2Configuration.DateTimeFormat.DateOnly, null, null },
        new object[] { JsonNode.Parse("{}"), DateTime2Configuration.DateTimeFormat.DateTime, null, null },
        new object[] { JsonNode.Parse("{\"INVALID\": \"\"}"), DateTime2Configuration.DateTimeFormat.DateTime, null, null },
        new object[] { JsonNode.Parse("{\"date\": \"2025-08-20\"}"), DateTime2Configuration.DateTimeFormat.DateOnly, new DateTimeOffset(2025, 8, 20, 0, 0, 0, TimeSpan.Zero), null },
        new object[] { JsonNode.Parse("{\"date\": \"16:34\"}"), DateTime2Configuration.DateTimeFormat.TimeOnly, new DateTimeOffset(1, 1, 1, 16, 34, 0, TimeSpan.Zero), null },
        new object[] { JsonNode.Parse("{\"date\": \"2025-08-20T18:30:01\"}"), DateTime2Configuration.DateTimeFormat.DateTime, new DateTimeOffset(2025, 8, 20, 18, 30, 1, TimeSpan.Zero), null },
        new object[] { JsonNode.Parse("{\"date\": \"2025-08-20T18:30:01Z\"}"), DateTime2Configuration.DateTimeFormat.DateTime, new DateTimeOffset(2025, 8, 20, 18, 30, 1, TimeSpan.Zero), null },
        new object[] { JsonNode.Parse("{\"date\": \"2025-08-20T18:30:01-05:00\"}"), DateTime2Configuration.DateTimeFormat.DateTime, new DateTimeOffset(2025, 8, 20, 18, 30, 1, TimeSpan.FromHours(-5)), null },
    ];

    [TestCaseSource(nameof(_sourceList3))]
    public void Can_Parse_Values_From_Editor(
        object? value,
        DateTime2Configuration.DateTimeFormat format,
        DateTimeOffset? expectedDateTimeOffset,
        string? expectedTimeZone)
    {
        var expectedJson = expectedDateTimeOffset is null ? null : _jsonSerializer.Serialize(
            new DateTime2ValueConverter.DateTime2
            {
                Date = expectedDateTimeOffset.Value,
                TimeZone = expectedTimeZone,
            });
        var result = CreateValueEditor(format: format).FromEditor(
            new ContentPropertyData(
                value,
                new DateTime2Configuration
                {
                    Format = format,
                    TimeZones = new DateTime2Configuration.TimeZonesConfiguration
                    {
                        Mode = DateTime2Configuration.TimeZoneMode.None,
                        TimeZones = [],
                    },
                }),
            null);
        Assert.AreEqual(expectedJson, result);
    }

    private static readonly object[][] _sourceList4 =
    [
        [null, null, DateTime2Configuration.DateTimeFormat.DateOnly, DateTime2Configuration.TimeZoneMode.None, null],
        [null, null, DateTime2Configuration.DateTimeFormat.TimeOnly, DateTime2Configuration.TimeZoneMode.None, null],
        [null, null, DateTime2Configuration.DateTimeFormat.DateTime, DateTime2Configuration.TimeZoneMode.None, null],
        [0, null, DateTime2Configuration.DateTimeFormat.DateOnly, DateTime2Configuration.TimeZoneMode.None, new { date = "2025-08-20", timeZone = (string?)null }],
        [0, null, DateTime2Configuration.DateTimeFormat.DateOnly, DateTime2Configuration.TimeZoneMode.All, new { date = "2025-08-20", timeZone = (string?)null }],
        [0, null, DateTime2Configuration.DateTimeFormat.DateOnly, DateTime2Configuration.TimeZoneMode.Local, new { date = "2025-08-20", timeZone = (string?)null }],
        [0, null, DateTime2Configuration.DateTimeFormat.DateOnly, DateTime2Configuration.TimeZoneMode.Custom, new { date = "2025-08-20", timeZone = (string?)null }],
        [0, null, DateTime2Configuration.DateTimeFormat.TimeOnly, DateTime2Configuration.TimeZoneMode.None, new { date = "16:30:00.0000000", timeZone = (string?)null }],
        [0, null, DateTime2Configuration.DateTimeFormat.DateTime, DateTime2Configuration.TimeZoneMode.None, new { date = "2025-08-20T16:30:00.0000000", timeZone = (string?)null }],
        [0, null, DateTime2Configuration.DateTimeFormat.DateTime, DateTime2Configuration.TimeZoneMode.All, new { date = "2025-08-20T16:30:00.0000000+00:00", timeZone = (string?)null }],
        [0, null, DateTime2Configuration.DateTimeFormat.DateTime, DateTime2Configuration.TimeZoneMode.Local, new { date = "2025-08-20T16:30:00.0000000+00:00", timeZone = (string?)null }],
        [0, null, DateTime2Configuration.DateTimeFormat.DateTime, DateTime2Configuration.TimeZoneMode.Custom, new { date = "2025-08-20T16:30:00.0000000+00:00", timeZone = (string?)null }],
        [-5, null, DateTime2Configuration.DateTimeFormat.DateTime, DateTime2Configuration.TimeZoneMode.None, new { date = "2025-08-20T21:30:00.0000000", timeZone = (string?)null }],
        [-5, "Europe/Copenhagen", DateTime2Configuration.DateTimeFormat.DateTime, DateTime2Configuration.TimeZoneMode.None, new { date = "2025-08-20T21:30:00.0000000", timeZone = "Europe/Copenhagen" }],
        [-5, "Europe/Copenhagen", DateTime2Configuration.DateTimeFormat.DateTime, DateTime2Configuration.TimeZoneMode.All, new { date = "2025-08-20T16:30:00.0000000-05:00", timeZone = "Europe/Copenhagen" }],
    ];

    [TestCaseSource(nameof(_sourceList4))]
    public void Can_Parse_Values_To_Editor(
        int? offset,
        string? timeZone,
        DateTime2Configuration.DateTimeFormat format,
        DateTime2Configuration.TimeZoneMode timeZoneMode,
        object? expectedResult)
    {
        var storedValue = offset is null
            ? null
            : new DateTime2ValueConverter.DateTime2
            {
                Date = new DateTimeOffset(2025, 8, 20, 16, 30, 00, TimeSpan.FromHours(offset.Value)),
                TimeZone = timeZone,
            };
        var valueEditor = CreateValueEditor(format: format, timeZoneMode: timeZoneMode);
        _dataTypeConfigurationCache.Setup(dc => dc.GetConfigurationAs<DateTime2Configuration>(Guid.Empty))
            .Returns(valueEditor.ConfigurationObject as DateTime2Configuration);
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

        var test = JsonSerializer.SerializeToNode(expectedResult);

        Assert.IsNotNull(result);
        Assert.IsTrue(JsonNode.DeepEquals(JsonSerializer.SerializeToNode(expectedResult), result as JsonNode));
    }

    private DateTime2PropertyEditor.DateTime2DataValueEditor CreateValueEditor(
        DateTime2Configuration.DateTimeFormat format = DateTime2Configuration.DateTimeFormat.DateTime,
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
        var valueEditor = new DateTime2PropertyEditor.DateTime2DataValueEditor(
            Mock.Of<IShortStringHelper>(),
            _jsonSerializer,
            Mock.Of<IIOHelper>(),
            new DataEditorAttribute("alias"),
            localizedTextServiceMock.Object,
            _dataTypeConfigurationCache.Object)
        {
            ConfigurationObject = new DateTime2Configuration
            {
                Format = format,
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
