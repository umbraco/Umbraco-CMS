using Umbraco.Core.Models;

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
    }
}
