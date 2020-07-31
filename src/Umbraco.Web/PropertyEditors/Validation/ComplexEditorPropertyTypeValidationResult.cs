using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;

namespace Umbraco.Web.PropertyEditors.Validation
{
    /// <summary>
    /// A collection of <see cref="ValidationResult"/> for a property type within a complex editor represented by an Element Type
    /// </summary>
    /// <remarks>
    /// For a more indepth explanation of how server side validation works with the angular app, see this GitHub PR: 
    /// https://github.com/umbraco/Umbraco-CMS/pull/8339
    /// </remarks>
    public class ComplexEditorPropertyTypeValidationResult : ValidationResult
    {
        public ComplexEditorPropertyTypeValidationResult(string propertyTypeAlias)
            : base(string.Empty)
        {
            PropertyTypeAlias = propertyTypeAlias;
        }

        private readonly List<ValidationResult> _validationResults = new List<ValidationResult>();

        public void AddValidationResult(ValidationResult validationResult)
        {
            if (validationResult is ComplexEditorValidationResult && _validationResults.Any(x => x is ComplexEditorValidationResult))
                throw new InvalidOperationException($"Cannot add more than one {typeof(ComplexEditorValidationResult)}");

            _validationResults.Add(validationResult);
        }

        public IReadOnlyList<ValidationResult> ValidationResults => _validationResults;
        public string PropertyTypeAlias { get; }
    }
}
