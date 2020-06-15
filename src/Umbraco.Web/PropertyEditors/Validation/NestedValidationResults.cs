using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Umbraco.Web.PropertyEditors.Validation
{
    /// <summary>
    /// Custom <see cref="ValidationResult"/> that contains a list of nested validation results
    /// </summary>
    /// <remarks>
    /// For example, each <see cref="NestedValidationResults"/> represents validation results for a row in Nested Content
    /// </remarks>
    public class NestedValidationResults : ValidationResult
    {
        public NestedValidationResults(IEnumerable<ValidationResult> nested)
            : base(string.Empty)
        {
            ValidationResults = new List<ValidationResult>(nested);
        }

        public IList<ValidationResult> ValidationResults { get; }
    }
}
