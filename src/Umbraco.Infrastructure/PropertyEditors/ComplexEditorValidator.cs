// Copyright (c) Umbraco.
// See LICENSE for more details.

using System.ComponentModel.DataAnnotations;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.Validation;
using Umbraco.Cms.Core.PropertyEditors.Validation;
using Umbraco.Cms.Core.Services;
using Umbraco.Extensions;

namespace Umbraco.Cms.Core.PropertyEditors;

/// <summary>
///     Used to validate complex editors that contain nested editors
/// </summary>
public abstract class ComplexEditorValidator : IValueValidator
{
    private readonly IPropertyValidationService _propertyValidationService;

    public ComplexEditorValidator(IPropertyValidationService propertyValidationService) =>
        _propertyValidationService = propertyValidationService;

    /// <inheritdoc/>
    public IEnumerable<ValidationResult> Validate(object? value, string? valueType, object? dataTypeConfiguration, PropertyValidationContext validationContext)
    {
        var elementTypeValues = GetElementTypeValidation(value, validationContext).ToList();
        var rowResults = GetNestedValidationResults(elementTypeValues, validationContext).ToList();

        if (rowResults.Count > 0)
        {
            var result = new NestedValidationResults();
            foreach (NestedValidationResults rowResult in rowResults)
            {
                result.ValidationResults.Add(rowResult);
            }

            return result.Yield();
        }

        return Enumerable.Empty<ValidationResult>();
    }

    protected abstract IEnumerable<ElementTypeValidationModel> GetElementTypeValidation(object? value, PropertyValidationContext validationContext);

    /// <summary>
    ///     Return a nested validation result per row (Element Type)
    /// </summary>
    protected IEnumerable<NestedValidationResults> GetNestedValidationResults(
        IEnumerable<ElementTypeValidationModel> elements,
        PropertyValidationContext validationContext)
    {
        foreach (ElementTypeValidationModel row in elements)
        {
            var elementTypeValidationResult = new NestedValidationResults();

            foreach (PropertyTypeValidationModel prop in row.PropertyTypeValidation)
            {
                var propValidationResult = new NestedJsonPathValidationResults(prop.JsonPath);

                foreach (ValidationResult validationResult in _propertyValidationService.ValidatePropertyValue(
                             prop.PropertyType, prop.PostedValue, validationContext))
                {
                    // add the result to the property results
                    propValidationResult.ValidationResults.Add(validationResult);
                }

                // add the property results to the element type results
                if (propValidationResult.ValidationResults.Count > 0)
                {
                    elementTypeValidationResult.ValidationResults.Add(propValidationResult);
                }
            }

            if (elementTypeValidationResult.ValidationResults.Count > 0)
            {
                yield return elementTypeValidationResult;
            }
        }
    }

    public class PropertyTypeValidationModel
    {
        public PropertyTypeValidationModel(IPropertyType propertyType, object? postedValue, string jsonPath)
        {
            PostedValue = postedValue;
            PropertyType = propertyType ?? throw new ArgumentNullException(nameof(propertyType));
            JsonPath = jsonPath;
        }

        public object? PostedValue { get; }

        public IPropertyType PropertyType { get; }

        public string JsonPath { get; }
    }

    public class ElementTypeValidationModel
    {
        private readonly List<PropertyTypeValidationModel> _list = new();

        public ElementTypeValidationModel(string elementTypeAlias, Guid id)
        {
            ElementTypeAlias = elementTypeAlias;
            Id = id;
        }

        public IEnumerable<PropertyTypeValidationModel> PropertyTypeValidation => _list;

        public string ElementTypeAlias { get; }

        public Guid Id { get; }

        public void AddPropertyTypeValidation(PropertyTypeValidationModel propValidation) => _list.Add(propValidation);
    }
}
