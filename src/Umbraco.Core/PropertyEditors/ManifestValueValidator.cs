using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Umbraco.Core.Models;

namespace Umbraco.Core.PropertyEditors
{
    /// <summary>
    /// A validator used to validate a value based on a validator name defined in a package manifest.
    /// </summary>
    public abstract class ManifestValueValidator
    {
        protected ManifestValueValidator()
        {
            var att = GetType().GetCustomAttribute<ValueValidatorAttribute>(false);
            if (att == null)
                throw new InvalidOperationException($"Class {GetType()} is not attributed with the {typeof(ValueValidatorAttribute)} attribute.");
            TypeName = att.TypeName;
        }

        public string TypeName { get; }

        /// <summary>
        /// Validates.
        /// </summary>
        /// <param name="value">The value to validate. Can be a json structure (JObject, JArray, etc...), could be a single string, representing an editor's model.</param>
        /// <param name="validatorConfiguration">The validator configuration, defined in the manifest.</param>
        /// <param name="dataTypeConfiguration">The data type configuration.</param>
        /// <param name="editor">The property editor.</param>
        /// <returns>Validation results.</returns>
        /// <remarks>If a validation result does not have a field name, then it applies to the entire property
        /// type being validated. Otherwise, it matches a field.</remarks>
        public abstract IEnumerable<ValidationResult> Validate(object value, string validatorConfiguration, object dataTypeConfiguration, PropertyEditor editor);
    }
}
