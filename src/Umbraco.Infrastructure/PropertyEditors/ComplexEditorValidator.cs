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

    /// <summary>
    /// Initializes a new instance of the <see cref="ComplexEditorValidator"/> class with the specified property validation service.
    /// </summary>
    /// <param name="propertyValidationService">An instance of <see cref="IPropertyValidationService"/> used to validate properties.</param>
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

    /// <summary>
    /// Encapsulates validation information for a property type used by the complex editor validator.
    /// </summary>
    public class PropertyTypeValidationModel
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="PropertyTypeValidationModel"/> class with the specified property type, posted value, and JSON path.
        /// </summary>
        /// <param name="propertyType">The <see cref="IPropertyType"/> representing the property type to be validated.</param>
        /// <param name="postedValue">The value submitted for validation, which may be <c>null</c>.</param>
        /// <param name="jsonPath">The JSON path indicating the location of the property within the data structure.</param>
        public PropertyTypeValidationModel(IPropertyType propertyType, object? postedValue, string jsonPath)
        {
            PostedValue = postedValue;
            PropertyType = propertyType ?? throw new ArgumentNullException(nameof(propertyType));
            JsonPath = jsonPath;
        }

        /// <summary>
        /// Gets the value posted for this property type during validation.
        /// </summary>
        public object? PostedValue { get; }

        /// <summary>
        /// Gets the <see cref="IPropertyType"/> that this validation model is associated with.
        /// </summary>
        public IPropertyType PropertyType { get; }

        /// <summary>
        /// Gets the path within the JSON structure used for validation.
        /// </summary>
        public string JsonPath { get; }
    }

    /// <summary>
    /// Validation model for an element type used in a complex editor.
    /// </summary>
    public class ElementTypeValidationModel
    {
        private readonly List<PropertyTypeValidationModel> _list = new();

        /// <summary>
        /// Initializes a new instance of the <see cref="ElementTypeValidationModel"/> class with the specified element type alias and unique identifier.
        /// </summary>
        /// <param name="elementTypeAlias">The alias of the element type to validate.</param>
        /// <param name="id">The unique identifier (GUID) of the element.</param>
        public ElementTypeValidationModel(string elementTypeAlias, Guid id)
        {
            ElementTypeAlias = elementTypeAlias;
            Id = id;
        }

        /// <summary>
        /// Gets a collection containing the validation results for each property type.
        /// </summary>
        public IEnumerable<PropertyTypeValidationModel> PropertyTypeValidation => _list;

        /// <summary>
        /// Gets the alias of the element type.
        /// </summary>
        public string ElementTypeAlias { get; }

        /// <summary>
        /// Gets the unique identifier of the element type.
        /// </summary>
        public Guid Id { get; }

        /// <summary>Adds a property type validation to the element type validation model.</summary>
        /// <param name="propValidation">The property type validation model to add.</param>
        public void AddPropertyTypeValidation(PropertyTypeValidationModel propValidation) => _list.Add(propValidation);
    }
}
