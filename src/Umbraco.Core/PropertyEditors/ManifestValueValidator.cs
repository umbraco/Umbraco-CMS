using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Umbraco.Core.Models;

namespace Umbraco.Core.PropertyEditors
{
    /// <summary>
    /// A validator used to validate a value based on a validator name defined in a package manifest
    /// </summary>
    public abstract class ManifestValueValidator
    {
        protected ManifestValueValidator()
        {
            var att = this.GetType().GetCustomAttribute<ValueValidatorAttribute>(false);
            if (att == null)
            {
                throw new InvalidOperationException("The class " + GetType() + " is not attributed with the " + typeof(ValueValidatorAttribute) + " attribute");
            }
            TypeName = att.TypeName;
        }

        public string TypeName { get; private set; }

        /// <summary>
        /// Performs the validation against the value
        /// </summary>
        /// <param name="value">
        /// Depending on what is being validated, this value can be a json structure (JObject, JArray, etc...) representing an editor's model, it could be a single
        /// string representing an editor's model.
        /// </param>
        /// <param name="config">
        /// An object that is used to configure the validator. An example could be a regex 
        /// expression if the validator was a regex validator. This is defined in the manifest along with
        /// the definition of the validator.
        /// </param>
        /// <param name="preValues">The current pre-values stored for the data type</param>
        /// <param name="editor">The property editor instance that is being validated</param>
        /// <returns>
        /// Returns a list of validation results. If a result does not have a field name applied to it then then we assume that 
        /// the validation message applies to the entire property type being validated. If there is a field name applied to a 
        /// validation result we will try to match that field name up with a field name on the item itself.
        /// </returns>
        public abstract IEnumerable<ValidationResult> Validate(object value, string config, PreValueCollection preValues, PropertyEditor editor);
    }
}