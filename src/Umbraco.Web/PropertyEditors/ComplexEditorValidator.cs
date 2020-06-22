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

        /// <summary>
        /// Return a single <see cref="NestedValidationResults"/> for all sub nested validation results in the complex editor
        /// </summary>
        /// <param name="value"></param>
        /// <param name="valueType"></param>
        /// <param name="dataTypeConfiguration"></param>
        /// <returns></returns>
        public IEnumerable<ValidationResult> Validate(object value, string valueType, object dataTypeConfiguration)
        {
            var elementTypeValues = GetElementTypeValidation(value).ToList();
            var rowResults = GetNestedValidationResults(elementTypeValues).ToList();

            if (rowResults.Count > 0)
            {
                var result = new NestedValidationResults();
                foreach(var rowResult in rowResults)
                {
                    result.AddElementTypeValidationResults(rowResult);
                }
                return result.Yield();
            }

            return Enumerable.Empty<ValidationResult>();
        }

       
        protected abstract IEnumerable<ElementTypeValidationModel> GetElementTypeValidation(object value);

        /// <summary>
        /// Return a nested validation result per row (Element Type)
        /// </summary>
        /// <param name="rawValue"></param>
        /// <returns></returns>
        protected IEnumerable<ValidationResultCollection> GetNestedValidationResults(IEnumerable<ElementTypeValidationModel> elements)
        {
            foreach (var row in elements)
            {
                var nestedValidation = new List<ValidationResult>();

                foreach (var prop in row.PropertyTypeValidation)
                {
                    foreach (var validationResult in _propertyValidationService.ValidatePropertyValue(prop.PropertyType, prop.PostedValue))
                    {
                        nestedValidation.Add(validationResult);
                    }
                }

                if (nestedValidation.Count > 0)
                {
                    yield return new ValidationResultCollection(nestedValidation.ToArray());
                }
            }
        }

        public class PropertyTypeValidationModel
        {
            public PropertyTypeValidationModel(object postedValue, PropertyType propertyType)
            {
                PostedValue = postedValue ?? throw new ArgumentNullException(nameof(postedValue));
                PropertyType = propertyType ?? throw new ArgumentNullException(nameof(propertyType));
            }

            public object PostedValue { get; }
            public PropertyType PropertyType { get; }
        }

        public class ElementTypeValidationModel
        {
            private List<PropertyTypeValidationModel> _list = new List<PropertyTypeValidationModel>();
            public IEnumerable<PropertyTypeValidationModel> PropertyTypeValidation => _list;

            public void AddPropertyTypeValidation(PropertyTypeValidationModel propValidation) => _list.Add(propValidation);
        }
    }
}
