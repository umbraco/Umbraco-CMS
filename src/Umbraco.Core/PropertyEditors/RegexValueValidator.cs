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
        public override IEnumerable<ValidationResult> Validate(string value, string config, string preValues, PropertyEditor editor)
        {
            //TODO: localize these!
            
            var regex = new Regex(config);
            
            if (!regex.IsMatch(value))
            {
                yield return new ValidationResult("Value is invalid, it does not match the correct pattern");
            }            
        }
    }
}