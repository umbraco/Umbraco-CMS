using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;

namespace Umbraco.Core.PropertyEditors
{
    /// <summary>
    /// A validator that validates that the value against a Regex expression
    /// </summary>
    [ValueValidator("Regex")]
    internal sealed class RegexValueValidator : ManifestValueValidator
    {
        public override IEnumerable<ValidationResult> Validate(object value, string config, string preValues, PropertyEditor editor)
        {
            //TODO: localize these!

            if (!(value is string))
            {
                throw new InvalidOperationException("The value parameter must be a string for this validator type");
            }

            var regex = new Regex((string) config);
            
            if (!regex.IsMatch((string)value))
            {
                yield return new ValidationResult("Value is invalid, it does not match the correct pattern");
            }            
        }
    }
}