using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Umbraco.Web.PropertyEditors.Validation
{
    public class PropertyTypeValidationResult : ValidationResult
    {
        public PropertyTypeValidationResult(string propertyTypeAlias)
            : base(string.Empty)
        {
            PropertyTypeAlias = propertyTypeAlias;
        }

        public IList<ValidationResult> ValidationResults { get; } = new List<ValidationResult>();
        public string PropertyTypeAlias { get; }
    }

    public class ElementTypeValidationResult : ValidationResult
    {
        public ElementTypeValidationResult(string elementTypeAlias)
            : base(string.Empty)
        {
            ElementTypeAlias = elementTypeAlias;
        }

        public IList<PropertyTypeValidationResult> ValidationResults { get; } = new List<PropertyTypeValidationResult>();
        public string ElementTypeAlias { get; }
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

        public IList<ElementTypeValidationResult> ValidationResults { get; } = new List<ElementTypeValidationResult>();
    }
}
