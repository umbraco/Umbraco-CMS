using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Umbraco.Web.PropertyEditors.Validation
{
    /// <summary>
    /// A collection of <see cref="ValidationResult"/> for a property type within a complex editor represented by an Element Type
    /// </summary>
    public class ComplexEditorPropertyTypeValidationResult : ValidationResult
    {
        public ComplexEditorPropertyTypeValidationResult(string propertyTypeAlias)
            : base(string.Empty)
        {
            PropertyTypeAlias = propertyTypeAlias;
        }

        public IList<ValidationResult> ValidationResults { get; } = new List<ValidationResult>();
        public string PropertyTypeAlias { get; }
    }
}
