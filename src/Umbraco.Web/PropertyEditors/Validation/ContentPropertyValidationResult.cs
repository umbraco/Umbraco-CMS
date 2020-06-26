using Newtonsoft.Json;
using System.ComponentModel.DataAnnotations;

namespace Umbraco.Web.PropertyEditors.Validation
{
    /// <summary>
    /// Custom <see cref="ValidationResult"/> for content properties
    /// </summary>
    /// <remarks>
    /// This clones the original result and then ensures the nested result if it's the correct type
    /// </remarks>
    public class ContentPropertyValidationResult : ValidationResult
    {
        public ContentPropertyValidationResult(ValidationResult nested)
            : base(nested.ErrorMessage, nested.MemberNames)
        {
            ComplexEditorResults = nested as ComplexEditorValidationResult;
        }

        /// <summary>
        /// Nested validation results for the content property
        /// </summary>
        /// <remarks>
        /// There can be nested results for complex editors that contain other editors
        /// </remarks>
        public ComplexEditorValidationResult ComplexEditorResults { get; }

        /// <summary>
        /// Return the <see cref="ValidationResult.ErrorMessage"/> if <see cref="ComplexEditorResults"/> is null, else the serialized
        /// complex validation results
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            if (ComplexEditorResults == null)
                return base.ToString();

            var json = JsonConvert.SerializeObject(this, new ValidationResultConverter());
            return json;
        }
    }
}
