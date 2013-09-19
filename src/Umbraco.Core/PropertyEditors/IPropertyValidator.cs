using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Umbraco.Core.Models;

namespace Umbraco.Core.PropertyEditors
{


    /// <summary>
    /// An interface defining a validator
    /// </summary>
    public interface IPropertyValidator
    {
        /// <summary>
        /// Validates the object with the resolved ValueValidator found for this type
        /// </summary>
        /// <param name="value">
        /// Depending on what is being validated, this value can be a json structure (JObject, JArray, etc...) representing an editor's model, it could be a single
        /// string representing an editor's model, this class structure is also used to validate pre-values and in that case this value
        /// could be a json structure or a single value representing a pre-value field.
        /// </param>
        /// <param name="preValues">
        /// When validating a property editor value (not a pre-value), this is the current pre-values stored for the data type.
        /// When validating a pre-value field this will be null.
        /// </param>
        /// <param name="editor">The property editor instance that we are validating for</param>
        /// <returns></returns>
        IEnumerable<ValidationResult> Validate(object value, PreValueCollection preValues, PropertyEditor editor);
    }
}