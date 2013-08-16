using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Umbraco.Core.PropertyEditors
{
    /// <summary>
    /// An abstract class defining a validator
    /// </summary>
    public abstract class ValidatorBase
    {
        /// <summary>
        /// Validates the object with the resolved ValueValidator found for this type
        /// </summary>
        /// <param name="value">
        /// Depending on what is being validated, this value can be a json structure representing an editor's model, it could be a single
        /// string representing an editor's model, this class structure is also used to validate pre-values and in that case this value
        /// could be a json structure representing the entire pre-value model or it could ba a single value representing a pre-value field.
        /// </param>
        /// <param name="preValues">The current pre-values stored for the data type</param>
        /// <param name="editor">The property editor instance that we are validating for</param>
        /// <returns></returns>
        public abstract IEnumerable<ValidationResult> Validate(string value, string preValues, PropertyEditor editor);
    }
}