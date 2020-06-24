using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Umbraco.Web.PropertyEditors.Validation
{
    /// <summary>
    /// A collection of <see cref="ComplexEditorPropertyTypeValidationResult"/> for an element type within complex editor represented by an Element Type
    /// </summary>
    public class ComplexEditorElementTypeValidationResult : ValidationResult
    {
        public ComplexEditorElementTypeValidationResult(string elementTypeAlias)
            : base(string.Empty)
        {
            ElementTypeAlias = elementTypeAlias;
        }

        public IList<ComplexEditorPropertyTypeValidationResult> ValidationResults { get; } = new List<ComplexEditorPropertyTypeValidationResult>();
        public string ElementTypeAlias { get; }
    }
}
