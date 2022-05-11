// Copyright (c) Umbraco.
// See LICENSE for more details.

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.PropertyEditors.Validation;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Services.Implement;
using Umbraco.Extensions;

namespace Umbraco.Cms.Core.PropertyEditors
{
    /// <summary>
    /// Used to validate complex editors that contain nested editors
    /// </summary>
    public abstract class ComplexEditorValidator : IValueValidator
    {
        private readonly IPropertyValidationService _propertyValidationService;

        public ComplexEditorValidator(IPropertyValidationService propertyValidationService)
        {
            _propertyValidationService = propertyValidationService;
        }

        /// <summary>
        /// Return a single <see cref="ComplexEditorValidationResult"/> for all sub nested validation results in the complex editor
        /// </summary>
        /// <param name="value"></param>
        /// <param name="valueType"></param>
        /// <param name="dataTypeConfiguration"></param>
        /// <returns></returns>
        public IEnumerable<ValidationResult> Validate(object? value, string? valueType, object? dataTypeConfiguration)
        {
            var elementTypeValues = GetElementTypeValidation(value).ToList();
            var rowResults = GetNestedValidationResults(elementTypeValues).ToList();

            if (rowResults.Count > 0)
            {
                var result = new ComplexEditorValidationResult();
                foreach(var rowResult in rowResults)
                {
                    result.ValidationResults.Add(rowResult);
                }
                return result.Yield();
            }

            return Enumerable.Empty<ValidationResult>();
        }


        protected abstract IEnumerable<ElementTypeValidationModel> GetElementTypeValidation(object? value);

        /// <summary>
        /// Return a nested validation result per row (Element Type)
        /// </summary>
        /// <param name="rawValue"></param>
        /// <returns></returns>
        protected IEnumerable<ComplexEditorElementTypeValidationResult> GetNestedValidationResults(IEnumerable<ElementTypeValidationModel> elements)
        {
            foreach (var row in elements)
            {
                var elementTypeValidationResult = new ComplexEditorElementTypeValidationResult(row.ElementTypeAlias, row.Id);

                foreach (var prop in row.PropertyTypeValidation)
                {
                    var propValidationResult = new ComplexEditorPropertyTypeValidationResult(prop.PropertyType.Alias);

                    foreach (var validationResult in _propertyValidationService.ValidatePropertyValue(prop.PropertyType, prop.PostedValue))
                    {
                        // add the result to the property results
                        propValidationResult.AddValidationResult(validationResult);
                    }

                    // add the property results to the element type results
                    if (propValidationResult.ValidationResults.Count > 0)
                        elementTypeValidationResult.ValidationResults.Add(propValidationResult);
                }

                if (elementTypeValidationResult.ValidationResults.Count > 0)
                {
                    yield return elementTypeValidationResult;
                }
            }
        }

        public class PropertyTypeValidationModel
        {
            public PropertyTypeValidationModel(IPropertyType propertyType, object? postedValue)
            {
                PostedValue = postedValue;
                PropertyType = propertyType ?? throw new ArgumentNullException(nameof(propertyType));
            }

            public object? PostedValue { get; }
            public IPropertyType PropertyType { get; }
        }

        public class ElementTypeValidationModel
        {
            public ElementTypeValidationModel(string elementTypeAlias, Guid id)
            {
                ElementTypeAlias = elementTypeAlias;
                Id = id;
            }

            private List<PropertyTypeValidationModel> _list = new List<PropertyTypeValidationModel>();

            public IEnumerable<PropertyTypeValidationModel> PropertyTypeValidation => _list;

            public string ElementTypeAlias { get; }
            public Guid Id { get; }

            public void AddPropertyTypeValidation(PropertyTypeValidationModel propValidation) => _list.Add(propValidation);
        }
    }
}
