using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Umbraco.Core.Models;
using Umbraco.Core.PropertyEditors;

namespace Umbraco.Core.Services
{
    public interface IPropertyValidationService
    {
        /// <summary>
        /// Validates the content item's properties pass validation rules
        /// </summary>
        bool IsPropertyDataValid(IContent content, out IProperty[] invalidProperties, CultureImpact impact);

        /// <summary>
        /// Gets a value indicating whether the property has valid values.
        /// </summary>
        bool IsPropertyValid(IProperty property, string culture = "*", string segment = "*");

        IEnumerable<ValidationResult> ValidatePropertyValue(
            IDataEditor editor,
            IDataType dataType,
            object postedValue,
            bool isRequired,
            string validationRegExp,
            string isRequiredMessage,
            string validationRegExpMessage);
    }
}
