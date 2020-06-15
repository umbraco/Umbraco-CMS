using System.ComponentModel.DataAnnotations;

namespace Umbraco.Web.PropertyEditors.Validation
{
    /// <summary>
    /// Custom <see cref="ValidationResult"/> for content properties
    /// </summary>
    /// <remarks>
    /// This clones the original result and then ensures the nested result if it's the correct type
    /// </remarks>
    public class PropertyValidationResult : ValidationResult
    {
        public PropertyValidationResult(ValidationResult nested)
            : base(nested.ErrorMessage, nested.MemberNames)
        {
            NestedResuls = nested as NestedValidationResults;
        }

        /// <summary>
        /// Nested validation results for the content property
        /// </summary>
        /// <remarks>
        /// There can be nested results for complex editors that contain other editors
        /// </remarks>
        public NestedValidationResults NestedResuls { get; }
    }
}
