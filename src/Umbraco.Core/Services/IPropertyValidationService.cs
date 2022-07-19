using System.ComponentModel.DataAnnotations;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.PropertyEditors;

namespace Umbraco.Cms.Core.Services;

public interface IPropertyValidationService
{
    /// <summary>
    ///     Validates the content item's properties pass validation rules
    /// </summary>
    bool IsPropertyDataValid(IContent content, out IProperty[] invalidProperties, CultureImpact? impact);

    /// <summary>
    ///     Gets a value indicating whether the property has valid values.
    /// </summary>
    bool IsPropertyValid(IProperty property, string culture = "*", string segment = "*");

    /// <summary>
    ///     Validates a property value.
    /// </summary>
    IEnumerable<ValidationResult> ValidatePropertyValue(
        IDataEditor editor,
        IDataType dataType,
        object? postedValue,
        bool isRequired,
        string? validationRegExp,
        string? isRequiredMessage,
        string? validationRegExpMessage);

    /// <summary>
    ///     Validates a property value.
    /// </summary>
    IEnumerable<ValidationResult> ValidatePropertyValue(
        IPropertyType propertyType,
        object? postedValue);
}
