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

[TestFixture]
public class TimeOnlyPropertyEditorTests
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

    private static readonly object[] _timeOnlyParseValuesFromEditorTestCases =
    [
        new object[] { null, null, null },
        new object[] { JsonNode.Parse("{\"date\": \"16:34\"}"), new DateTimeOffset(1, 1, 1, 16, 34, 0, TimeSpan.Zero), null },
    ];

    [TestCaseSource(nameof(_timeOnlyParseValuesFromEditorTestCases))]
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

    private static readonly object[][] _timeOnlyParseValuesToEditorTestCases =
    [
        [null, null, null],
        [0, null, new DateTimeEditorValue { Date = "16:30:00", TimeZone = null }],
    ];

    [TestCaseSource(nameof(_timeOnlyParseValuesToEditorTestCases))]
    public void Can_Parse_Values_To_Editor(
        int? offset,
        string? timeZone,
        object? expectedResult)
    {
        var storedValue = offset is null
            ? null
            : new DateTimeValueConverterBase.DateTimeDto
            {
                Date = new DateTimeOffset(2025, 8, 20, 16, 30, 00, TimeSpan.FromHours(offset.Value)),
                TimeZone = timeZone,
            };
        var valueEditor = CreateValueEditor(timeZoneMode: null);
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
        DateTimeConfiguration.TimeZoneMode? timeZoneMode = null,
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
            new DataEditorAttribute(Constants.PropertyEditors.Aliases.TimeOnly),
            localizedTextServiceMock.Object,
            Mock.Of<ILogger<DateTimePropertyEditorBase.DateTimeDataValueEditor>>(),
            dt => dt.Date.ToString("HH:mm:ss"))
        {
            ConfigurationObject = new DateTimeConfiguration
            {
                TimeZones = timeZoneMode is null
                    ? null
                    : new DateTimeConfiguration.TimeZonesConfiguration
                    {
                        Mode = timeZoneMode.Value,
                        TimeZones = timeZones?.ToList() ?? [],
                    },
            },
        };
        return valueEditor;
    }
}
