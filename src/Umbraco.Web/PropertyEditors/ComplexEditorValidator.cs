using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using Umbraco.Core;
using Umbraco.Core.Models;
using Umbraco.Core.PropertyEditors;
using Umbraco.Core.Services;
using Umbraco.Web.PropertyEditors.Validation;

namespace Umbraco.Web.PropertyEditors
{
    /// <summary>
    /// Used to validate complex editors that contain nested editors
    /// </summary>
    public abstract class ComplexEditorValidator : IValueValidator
    {
        private readonly PropertyValidationService _propertyValidationService;

        public ComplexEditorValidator(PropertyEditorCollection propertyEditors, IDataTypeService dataTypeService, ILocalizedTextService textService)
        {
            _propertyValidationService = new PropertyValidationService(propertyEditors, dataTypeService, textService);
        }

        public IEnumerable<ValidationResult> Validate(object value, string valueType, object dataTypeConfiguration)
        {
            var elements = GetElementsFromValue(value);
            var rowResults = GetNestedValidationResults(elements).ToList();

            return rowResults.Count > 0
                ? new NestedValidationResults(rowResults).Yield()
                : Enumerable.Empty<ValidationResult>();
        }

        protected abstract IEnumerable<ElementTypeValidationModel> GetElementsFromValue(object value);

        /// <summary>
        /// Return a nested validation result per row
        /// </summary>
        /// <param name="rawValue"></param>
        /// <returns></returns>
        protected IEnumerable<NestedValidationResults> GetNestedValidationResults(IEnumerable<ElementTypeValidationModel> elements)
        {
            foreach (var row in elements)
            {
                var nestedValidation = new List<ValidationResult>();

                foreach(var validationResult in _propertyValidationService.ValidatePropertyValue(row.PropertyType, row.PostedValue))
                {
                    nestedValidation.Add(validationResult);
                }

                if (nestedValidation.Count > 0)
                {
                    yield return new NestedValidationResults(nestedValidation);
                }
            }
        }

        public class ElementTypeValidationModel
        {
            public ElementTypeValidationModel(object postedValue, PropertyType propertyType)
            {
                PostedValue = postedValue ?? throw new ArgumentNullException(nameof(postedValue));
                PropertyType = propertyType ?? throw new ArgumentNullException(nameof(propertyType));
            }

            public object PostedValue { get; }
            public PropertyType PropertyType { get; }

        }
    }
}
