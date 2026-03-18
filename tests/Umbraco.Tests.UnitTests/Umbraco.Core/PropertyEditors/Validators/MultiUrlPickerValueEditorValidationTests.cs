using System.ComponentModel.DataAnnotations;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using Umbraco.Cms.Core.Cache;
using Umbraco.Cms.Core.IO;
using Umbraco.Cms.Core.Models.Validation;
using Umbraco.Cms.Core.PropertyEditors;
using Umbraco.Cms.Core.Routing;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Strings;
using Umbraco.Cms.Infrastructure.Serialization;

namespace Umbraco.Cms.Tests.UnitTests.Umbraco.Core.PropertyEditors.Validators;

[TestFixture]
internal class MultiUrlPickerValueEditorValidationTests
{
    /// <summary>
    /// Verifies that the MultiUrlPicker value editor correctly enforces the minimum number of URLs required, based on the specified configuration.
    /// </summary>
    /// <param name="min">The minimum number of URLs that must be present for validation to succeed.</param>
    /// <param name="succeed">True if the validation is expected to pass; otherwise, false.</param>
    /// <param name="value">A JSON string representing the collection of URLs to validate, or <c>null</c> if no value is provided.</param>
    [TestCase(1, true, "[{\"icon\":\"icon-document\",\"name\":\"Page 1\",\"published\":true,\"queryString\":null,\"target\":null,\"trashed\":false,\"type\":\"document\",\"unique\":\"7d285be2-7cd5-4c7b-a252-b064e31f049f\",\"url\":\"/\"}]")]
    [TestCase(2, false, "[{\"icon\":\"icon-document\",\"name\":\"Page 1\",\"published\":true,\"queryString\":null,\"target\":null,\"trashed\":false,\"type\":\"document\",\"unique\":\"7d285be2-7cd5-4c7b-a252-b064e31f049f\",\"url\":\"/\"}]")]
    [TestCase(1, true, "[{\"icon\":\"icon-document\",\"name\":\"Page 1\",\"published\":true,\"queryString\":null,\"target\":null,\"trashed\":false,\"type\":\"document\",\"unique\":\"7d285be2-7cd5-4c7b-a252-b064e31f049f\",\"url\":\"/\"},{\"icon\":\"icon-document\",\"name\":\"Page 1\",\"published\":true,\"queryString\":null,\"target\":null,\"trashed\":false,\"type\":\"document\",\"unique\":\"7d285be2-7cd5-4c7b-a252-b064e31f049f\",\"url\":\"/\"}]")]
    [TestCase(3, false, "[{\"icon\":\"icon-document\",\"name\":\"Page 1\",\"published\":true,\"queryString\":null,\"target\":null,\"trashed\":false,\"type\":\"document\",\"unique\":\"7d285be2-7cd5-4c7b-a252-b064e31f049f\",\"url\":\"/\"},{\"icon\":\"icon-document\",\"name\":\"Page 1\",\"published\":true,\"queryString\":null,\"target\":null,\"trashed\":false,\"type\":\"document\",\"unique\":\"7d285be2-7cd5-4c7b-a252-b064e31f049f\",\"url\":\"/\"}]")]
    [TestCase(1, false, "[]")]
    [TestCase(1, false, null)]
    public void Validates_Min_Limit(int min, bool succeed, string? value)
    {
        var picker = CreateValueEditor();

        picker.ConfigurationObject = new MultiUrlPickerConfiguration() { MinNumber = min };

        var result = picker.Validate(value, false, null, PropertyValidationContext.Empty());
        ValidateResult(succeed, result);
    }

    [TestCase(1, true, "[{\"icon\":\"icon-document\",\"name\":\"Page 1\",\"published\":true,\"queryString\":null,\"target\":null,\"trashed\":false,\"type\":\"document\",\"unique\":\"7d285be2-7cd5-4c7b-a252-b064e31f049f\",\"url\":\"/\"}]")]
    [TestCase(1, false, "[{\"icon\":\"icon-document\",\"name\":\"Page 1\",\"published\":true,\"queryString\":null,\"target\":null,\"trashed\":false,\"type\":\"document\",\"unique\":\"7d285be2-7cd5-4c7b-a252-b064e31f049f\",\"url\":\"/\"},{\"icon\":\"icon-document\",\"name\":\"Page 1\",\"published\":true,\"queryString\":null,\"target\":null,\"trashed\":false,\"type\":\"document\",\"unique\":\"7d285be2-7cd5-4c7b-a252-b064e31f049f\",\"url\":\"/\"}]")]
    [TestCase(3, true, "[{\"icon\":\"icon-document\",\"name\":\"Page 1\",\"published\":true,\"queryString\":null,\"target\":null,\"trashed\":false,\"type\":\"document\",\"unique\":\"7d285be2-7cd5-4c7b-a252-b064e31f049f\",\"url\":\"/\"},{\"icon\":\"icon-document\",\"name\":\"Page 1\",\"published\":true,\"queryString\":null,\"target\":null,\"trashed\":false,\"type\":\"document\",\"unique\":\"7d285be2-7cd5-4c7b-a252-b064e31f049f\",\"url\":\"/\"}]")]
    [TestCase(1, true, "[]")]
    [TestCase(1, true, null)]
    public void Validates_Max_Limit(int max, bool succeed, string? value)
    {
        var picker = CreateValueEditor();

        picker.ConfigurationObject = new MultiUrlPickerConfiguration() { MaxNumber = max };

        var result = picker.Validate(value, false, null, PropertyValidationContext.Empty());
        ValidateResult(succeed, result);
    }

    private static void ValidateResult(bool succeed, IEnumerable<ValidationResult> result)
    {
        if (succeed)
        {
            Assert.IsEmpty(result);
        }
        else
        {
            Assert.That(result.Count(), Is.EqualTo(1));
        }
    }

    private static MultiUrlPickerValueEditor CreateValueEditor() =>
        new(
            Mock.Of<ILogger<MultiUrlPickerValueEditor>>(),
            Mock.Of<ILocalizedTextService>(),
            Mock.Of<IShortStringHelper>(),
            new DataEditorAttribute("alias"),
            Mock.Of<IPublishedUrlProvider>(),
            new SystemTextJsonSerializer(new DefaultJsonSerializerEncoderFactory()),
            Mock.Of<IIOHelper>(),
            Mock.Of<IContentService>(),
            Mock.Of<IMediaService>(),
            AppCaches.Disabled)
        {
            ConfigurationObject = new MultiUrlPickerConfiguration(),
        };
}
