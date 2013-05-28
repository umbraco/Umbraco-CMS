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
        /// <param name="value"></param>
        /// <param name="preValues">The current pre-values stored for the data type</param>
        /// <param name="editor">The property editor instance that we are validating for</param>
        /// <returns></returns>
        public abstract IEnumerable<ValidationResult> Validate(string value, string preValues, PropertyEditor editor);
    }
}