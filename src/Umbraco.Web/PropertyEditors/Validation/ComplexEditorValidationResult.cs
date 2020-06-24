using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Umbraco.Web.PropertyEditors.Validation
{

    /// <summary>
    /// A collection of <see cref="ComplexEditorElementTypeValidationResult"/> for a complex editor represented by an Element Type
    /// </summary>
    /// <remarks>
    /// For example, each <see cref="ComplexEditorValidationResult"/> represents validation results for a row in Nested Content
    /// </remarks>
    public class ComplexEditorValidationResult : ValidationResult
    {
        public ComplexEditorValidationResult()
            : base(string.Empty)
        {
        }

        public IList<ComplexEditorElementTypeValidationResult> ValidationResults { get; } = new List<ComplexEditorElementTypeValidationResult>();
    }
}
