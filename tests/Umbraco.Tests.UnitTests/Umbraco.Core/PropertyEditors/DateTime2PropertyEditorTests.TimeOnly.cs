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
    private readonly TimeOnlyValueConverter _timeOnlyValueConverter = new(_jsonSerializer);

    [TestCaseSource(nameof(_validateDateReceivedTestCases))]
    public void TimeOnly_Validates_Date_Received(object? value, bool expectedSuccess)
    {
        var editor = CreateTimeOnlyValueEditor();
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
    public void TimeOnly_Can_Parse_Values_From_Editor(
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
        var result = CreateTimeOnlyValueEditor().FromEditor(
            new ContentPropertyData(
                value,
                new DateTime2Configuration
                {
                    TimeZones = new DateTime2Configuration.TimeZonesConfiguration
                    {
                        Mode = DateTime2Configuration.TimeZoneMode.None,
                        TimeZones = [],
                    },
                }),
            null);
        Assert.AreEqual(expectedJson, result);
    }

    private static readonly object[][] _timeOnlyParseValuesToEditorTestCases =
    [
        [null, null, DateTime2Configuration.TimeZoneMode.None, null],
        [0, null, DateTime2Configuration.TimeZoneMode.None, new DateTime2PropertyEditorBase.DateTime2ApiModel { Date = "16:30:00.0000000", TimeZone = null }],
        [0, null, DateTime2Configuration.TimeZoneMode.All, new DateTime2PropertyEditorBase.DateTime2ApiModel { Date = "16:30:00.0000000", TimeZone = null }],
        [0, null, DateTime2Configuration.TimeZoneMode.Local, new DateTime2PropertyEditorBase.DateTime2ApiModel { Date = "16:30:00.0000000", TimeZone = null }],
        [0, null, DateTime2Configuration.TimeZoneMode.Custom, new DateTime2PropertyEditorBase.DateTime2ApiModel { Date = "16:30:00.0000000", TimeZone = null }],
    ];

    [TestCaseSource(nameof(_timeOnlyParseValuesToEditorTestCases))]
    public void TimeOnly_Can_Parse_Values_To_Editor(
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
        var valueEditor = CreateTimeOnlyValueEditor(timeZoneMode: timeZoneMode);
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

    private DateTime2PropertyEditorBase.DateTime2DataValueEditor<TimeOnlyValueConverter> CreateTimeOnlyValueEditor(
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
        var valueEditor = new DateTime2PropertyEditorBase.DateTime2DataValueEditor<TimeOnlyValueConverter>(
            Mock.Of<IShortStringHelper>(),
            _jsonSerializer,
            Mock.Of<IIOHelper>(),
            new DataEditorAttribute(Constants.PropertyEditors.Aliases.TimeOnly),
            localizedTextServiceMock.Object,
            _timeOnlyValueConverter)
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
