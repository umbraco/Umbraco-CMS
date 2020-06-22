using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Umbraco.Web.PropertyEditors.Validation
{
    public class ValidationResultCollection : ValidationResult
    {
        public ValidationResultCollection(params ValidationResult[] nested)
            : base(string.Empty)
        {
            ValidationResults = new List<ValidationResult>(nested);
        }

        public IList<ValidationResult> ValidationResults { get; }
    }

    /// <summary>
    /// Custom <see cref="ValidationResult"/> that contains a list of nested validation results
    /// </summary>
    /// <remarks>
    /// For example, each <see cref="NestedValidationResults"/> represents validation results for a row in Nested Content
    /// </remarks>
    public class NestedValidationResults : ValidationResult
    {
        public NestedValidationResults()
            : base(string.Empty)
        {
        }

        public void AddElementTypeValidationResults(ValidationResultCollection resultCollection)
        {
            ValidationResults.Add(resultCollection);
        }

        public IList<ValidationResultCollection> ValidationResults { get; } = new List<ValidationResultCollection>();
    }
}
